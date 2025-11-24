import { useState, useEffect, useCallback } from 'react';
import { Table, Button, Badge, Spinner, Card, Toast, ToastContainer } from 'react-bootstrap';
import tenantService from '../../services/tenantService';
import { useAuth } from '../../contexts/AuthContext';
import { isSystemAdmin } from '../../utils/roleUtils';
import TenantFormDialog from '../../components/tenants/TenantFormDialog';
import { useNavigate } from 'react-router-dom';

const TenantsList = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [tenants, setTenants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingTenant, setEditingTenant] = useState(null);
  const [toast, setToast] = useState({ show: false, message: '', variant: 'success' });

  useEffect(() => {
    if (!isSystemAdmin(user)) {
      setToast({ show: true, message: 'Access denied. Only System Admins can manage tenants.', variant: 'danger' });
      navigate('/dashboard');
    }
  }, [user, navigate]);

  const fetchTenants = useCallback(async () => {
    try {
      setLoading(true);
      const response = await tenantService.getAll();
      setTenants(response || []);
    } catch (err) {
      setToast({ show: true, message: 'Failed to load tenants', variant: 'danger' });
      console.error('Failed to load tenants', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (isSystemAdmin(user)) {
      fetchTenants();
    }
  }, [fetchTenants, user]);

  const handleOpenDialog = (tenant = null) => {
    setEditingTenant(tenant);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingTenant(null);
  };

  const handleSubmit = async (formData) => {
    try {
      if (editingTenant) {
        await tenantService.update(editingTenant.id, formData);
        setToast({ show: true, message: 'Tenant updated successfully', variant: 'success' });
      } else {
        await tenantService.create(formData);
        setToast({ show: true, message: 'Tenant created successfully', variant: 'success' });
      }
      fetchTenants();
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
    if (window.confirm('Delete this tenant? This action cannot be undone.')) {
      try {
        await tenantService.delete(id);
        setToast({ show: true, message: 'Tenant deleted successfully', variant: 'success' });
        fetchTenants();
      } catch (err) {
        setToast({ show: true, message: 'Failed to delete tenant', variant: 'danger' });
        console.error('Delete error:', err);
      }
    }
  };

  const handleToggleStatus = async (id, status) => {
    try {
      if (status === 'Active') {
        await tenantService.suspend(id);
        setToast({ show: true, message: 'Tenant suspended', variant: 'success' });
      } else {
        await tenantService.activate(id);
        setToast({ show: true, message: 'Tenant activated', variant: 'success' });
      }
      fetchTenants();
    } catch (err) {
      setToast({ show: true, message: 'Failed to update status', variant: 'danger' });
      console.error('Toggle status error:', err);
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" className="spinner-border-custom" />
        <p className="mt-3 text-muted">Loading tenants...</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
        <div>
          <h1 className="page-title text-gradient mb-2">
            <i className="bi bi-building me-2"></i>
            Tenants Management
          </h1>
          <p className="text-muted mb-0">Manage tenant organizations and their settings</p>
        </div>
        <Button
          variant="primary"
          size="lg"
          onClick={() => handleOpenDialog()}
          className="d-flex align-items-center gap-2"
        >
          <i className="bi bi-plus-circle"></i>
          Add Tenant
        </Button>
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-0">
          <div className="table-responsive">
            <Table hover className="mb-0">
              <thead>
                <tr>
                  <th><i className="bi bi-building me-2"></i>Name</th>
                  <th className="d-none d-md-table-cell"><i className="bi bi-link-45deg me-2"></i>Subdomain</th>
                  <th className="d-none d-lg-table-cell"><i className="bi bi-star me-2"></i>Subscription</th>
                  <th><i className="bi bi-toggle-on me-2"></i>Status</th>
                  <th className="text-end"><i className="bi bi-gear me-2"></i>Actions</th>
                </tr>
              </thead>
              <tbody>
                {tenants.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="text-center py-5">
                      <i className="bi bi-inbox fs-1 text-muted d-block mb-3"></i>
                      <p className="text-muted mb-0">No tenants found</p>
                    </td>
                  </tr>
                ) : (
                  tenants.map((tenant) => (
                    <tr key={tenant.id}>
                      <td className="fw-semibold">
                        <i className="bi bi-building-fill text-primary me-2"></i>
                        {tenant.name}
                      </td>
                      <td className="d-none d-md-table-cell text-muted font-monospace">
                        {tenant.subdomain}
                      </td>
                      <td className="d-none d-lg-table-cell">
                        <Badge bg="info" className="px-3 py-2">{tenant.subscriptionPlan}</Badge>
                      </td>
                      <td>
                        <Badge
                          bg={tenant.status === 'Active' ? 'success' : 'secondary'}
                          className="px-3 py-2"
                        >
                          {tenant.status}
                        </Badge>
                      </td>
                      <td className="text-end">
                        <div className="d-flex gap-2 justify-content-end">
                          <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={() => handleOpenDialog(tenant)}
                            title="Edit"
                          >
                            <i className="bi bi-pencil"></i>
                          </Button>
                          <Button
                            variant={tenant.status === 'Active' ? 'outline-warning' : 'outline-success'}
                            size="sm"
                            onClick={() => handleToggleStatus(tenant.id, tenant.status)}
                            title={tenant.status === 'Active' ? 'Suspend' : 'Activate'}
                          >
                            <i className={`bi bi-${tenant.status === 'Active' ? 'pause-circle' : 'play-circle'}`}></i>
                          </Button>
                          <Button
                            variant="outline-danger"
                            size="sm"
                            onClick={() => handleDelete(tenant.id)}
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

      <TenantFormDialog
        open={openDialog}
        onClose={handleCloseDialog}
        onSubmit={handleSubmit}
        tenant={editingTenant}
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
              {toast.variant === 'success' ? 'Success' : 'Error'}
            </strong>
          </Toast.Header>
          <Toast.Body className="text-white">{toast.message}</Toast.Body>
        </Toast>
      </ToastContainer>
    </div>
  );
};

export default TenantsList;
