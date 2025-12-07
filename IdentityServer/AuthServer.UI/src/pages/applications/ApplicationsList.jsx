import { useState, useEffect, useCallback } from 'react';
import { Table, Button, Badge, Spinner, Card, Toast, ToastContainer } from 'react-bootstrap';
import applicationService from '../../services/applicationService';
import tenantService from '../../services/tenantService';
import { useAuth } from '../../contexts/AuthContext';
import { isSystemAdmin } from '../../utils/roleUtils';
import ApplicationFormDialog from '../../components/applications/ApplicationFormDialog';

const ApplicationsList = () => {
  const { user } = useAuth();
  const [applications, setApplications] = useState([]);
  const [tenants, setTenants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadingDetails, setLoadingDetails] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingApplication, setEditingApplication] = useState(null);
  const [toast, setToast] = useState({ show: false, message: '', variant: 'success' });

  const fetchApplications = useCallback(async () => {
    try {
      setLoading(true);
      const response = await applicationService.getByTenant(user?.tenantId || '');
      setApplications(response || []);
    } catch (err) {
      setToast({ show: true, message: 'Failed to load applications', variant: 'danger' });
      console.error('Failed to load applications', err);
    } finally {
      setLoading(false);
    }
  }, [user?.tenantId]);

  const fetchTenants = useCallback(async () => {
    if (isSystemAdmin(user)) {
      try {
        const response = await tenantService.getAll();
        setTenants(response || []);
      } catch (err) {
        console.error('Failed to load tenants', err);
      }
    }
  }, [user]);

  useEffect(() => {
    fetchApplications();
    fetchTenants();
  }, [fetchApplications, fetchTenants]);

  const handleOpenDialog = async (application = null) => {
    if (application) {
      // Fetch complete application details for editing
      try {
        setLoadingDetails(true);
        const fullApplication = await applicationService.getById(application.id);
        setEditingApplication(fullApplication);
        setOpenDialog(true);
      } catch (err) {
        setToast({ 
          show: true, 
          message: 'Failed to load application details', 
          variant: 'danger' 
        });
        console.error('Failed to load application details', err);
      } finally {
        setLoadingDetails(false);
      }
    } else {
      // Creating new application
      setEditingApplication(null);
      setOpenDialog(true);
    }
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingApplication(null);
  };

  const handleSubmit = async (formData) => {
    try {
      if (editingApplication) {
        await applicationService.update(editingApplication.id, formData);
        setToast({ show: true, message: 'Application updated successfully', variant: 'success' });
      } else {
        await applicationService.create(formData);
        setToast({ show: true, message: 'Application created successfully', variant: 'success' });
      }
      fetchApplications();
      handleCloseDialog();
    } catch (err) {
      setToast({
        show: true,
        message: err.response?.data?.error || 'Operation failed',
        variant: 'danger'
      });
      console.error('Submit error:', err);
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('Delete this application? This action cannot be undone.')) {
      try {
        await applicationService.delete(id);
        setToast({ show: true, message: 'Application deleted successfully', variant: 'success' });
        fetchApplications();
      } catch (err) {
        setToast({ show: true, message: 'Failed to delete application', variant: 'danger' });
        console.error('Delete error:', err);
      }
    }
  };

  const handleRegenerateSecret = async (id) => {
    if (window.confirm('Regenerate secret for this application? The old secret will stop working.')) {
      try {
        const response = await applicationService.regenerateSecret(id);
        setToast({
          show: true,
          message: `New secret: ${response.clientSecret} - Please save it securely!`,
          variant: 'warning'
        });
        fetchApplications();
      } catch (err) {
        setToast({ show: true, message: 'Failed to regenerate secret', variant: 'danger' });
        console.error('Regenerate error:', err);
      }
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" className="spinner-border-custom" />
        <p className="mt-3 text-muted">Loading applications...</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
        <div>
          <h1 className="page-title text-gradient mb-2">
            <i className="bi bi-app me-2"></i>
            Applications Management
          </h1>
          <p className="text-muted mb-0">Manage OAuth/OIDC applications and their configurations</p>
        </div>
        <Button
          variant="primary"
          size="lg"
          onClick={() => handleOpenDialog()}
          className="d-flex align-items-center gap-2"
        >
          <i className="bi bi-plus-circle"></i>
          Add Application
        </Button>
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-0">
          <div className="table-responsive">
            <Table hover className="mb-0">
              <thead>
                <tr>
                  <th><i className="bi bi-app me-2"></i>Name</th>
                  <th className="d-none d-md-table-cell"><i className="bi bi-key me-2"></i>Client ID</th>
                  <th className="d-none d-lg-table-cell"><i className="bi bi-gear me-2"></i>Type</th>
                  <th><i className="bi bi-toggle-on me-2"></i>Status</th>
                  <th className="text-end"><i className="bi bi-tools me-2"></i>Actions</th>
                </tr>
              </thead>
              <tbody>
                {applications.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="text-center py-5">
                      <i className="bi bi-inbox fs-1 text-muted d-block mb-3"></i>
                      <p className="text-muted mb-0">No applications found</p>
                    </td>
                  </tr>
                ) : (
                  applications.map((app) => (
                    <tr key={app.id}>
                      <td className="fw-semibold">
                        <i className="bi bi-app-indicator text-primary me-2"></i>
                        {app.name}
                      </td>
                      <td className="d-none d-md-table-cell text-muted font-monospace small">
                        {app.clientId}
                      </td>
                      <td className="d-none d-lg-table-cell">
                        <Badge bg="secondary" className="px-3 py-2">
                          {app.applicationType}
                        </Badge>
                      </td>
                      <td>
                        <Badge
                          bg={app.isActive ? 'success' : 'secondary'}
                          className="px-3 py-2"
                        >
                          {app.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </td>
                      <td className="text-end">
                        <div className="d-flex gap-2 justify-content-end">
                          <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={() => handleOpenDialog(app)}
                            disabled={loadingDetails}
                            title="Edit"
                          >
                            {loadingDetails ? (
                              <Spinner animation="border" size="sm" />
                            ) : (
                              <i className="bi bi-pencil"></i>
                            )}
                          </Button>
                          <Button
                            variant="outline-warning"
                            size="sm"
                            onClick={() => handleRegenerateSecret(app.id)}
                            title="Regenerate Secret"
                          >
                            <i className="bi bi-arrow-clockwise"></i>
                          </Button>
                          <Button
                            variant="outline-danger"
                            size="sm"
                            onClick={() => handleDelete(app.id)}
                            title="Delete"
                          >
                            <i className="bi bi-trash"></i>
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </Table>
          </div>
        </Card.Body>
      </Card>

      <ApplicationFormDialog
        open={openDialog}
        onClose={handleCloseDialog}
        onSubmit={handleSubmit}
        application={editingApplication}
        tenants={tenants}
      />

      <ToastContainer position="bottom-end" className="p-3">
        <Toast
          show={toast.show}
          onClose={() => setToast({ ...toast, show: false })}
          delay={5000}
          autohide
          bg={toast.variant}
        >
          <Toast.Header>
            <strong className="me-auto">
              {toast.variant === 'success' ? 'Success' : toast.variant === 'warning' ? 'Warning' : 'Error'}
            </strong>
          </Toast.Header>
          <Toast.Body className="text-white">{toast.message}</Toast.Body>
        </Toast>
      </ToastContainer>
    </div>
  );
};

export default ApplicationsList;