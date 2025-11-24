import { useState, useEffect, useCallback } from 'react';
import { Table, Button, Badge, Spinner, Card, Toast, ToastContainer, Form, Row, Col } from 'react-bootstrap';
import userService from '../../services/userService';
import tenantService from '../../services/tenantService';
import { useAuth } from '../../contexts/AuthContext';
import { isSystemAdmin, canViewAllUsers } from '../../utils/roleUtils';
import UserFormDialog from '../../components/users/UserFormDialog';

const UsersList = () => {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [tenants, setTenants] = useState([]);
  const [selectedTenantId, setSelectedTenantId] = useState(currentUser?.tenantId || '');
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [toast, setToast] = useState({ show: false, message: '', variant: 'success' });

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      const tenantIdToFetch = canViewAllUsers(currentUser) ? selectedTenantId : currentUser?.tenantId;

      if (!tenantIdToFetch) {
        setUsers([]);
        setLoading(false);
        return;
      }

      const response = await userService.getAll(tenantIdToFetch, 1, 100);
      setUsers(response.data || []);
    } catch (error) {
      setToast({ show: true, message: 'Failed to load users', variant: 'danger' });
      console.error('Failed to load users', error);
    } finally {
      setLoading(false);
    }
  }, [currentUser, selectedTenantId]);

  const fetchTenants = useCallback(async () => {
    if (canViewAllUsers(currentUser)) {
      try {
        const response = await tenantService.getAll();
        setTenants(response || []);
        if (response && response.length > 0 && !selectedTenantId) {
          setSelectedTenantId(response[0].id);
        }
      } catch (err) {
        console.error('Failed to load tenants', err);
      }
    }
  }, [currentUser, selectedTenantId]);

  useEffect(() => {
    fetchTenants();
  }, [fetchTenants]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const handleOpenDialog = (user = null) => {
    setEditingUser(user);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingUser(null);
  };

  const handleSubmit = async (formData) => {
    try {
      if (editingUser) {
        await userService.update(editingUser.id, formData);
        setToast({ show: true, message: 'User updated successfully', variant: 'success' });
      } else {
        await userService.create(formData);
        setToast({ show: true, message: 'User created successfully', variant: 'success' });
      }
      fetchUsers();
      handleCloseDialog();
    } catch (error) {
      setToast({
        show: true,
        message: error.response?.data?.error || 'Operation failed',
        variant: 'danger'
      });
      console.error('Submit error:', error);
    }
  };

  const handleDelete = async (userId) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await userService.delete(userId);
        setToast({ show: true, message: 'User deleted successfully', variant: 'success' });
        fetchUsers();
      } catch (error) {
        setToast({ show: true, message: 'Failed to delete user', variant: 'danger' });
        console.error('Delete error:', error);
      }
    }
  };

  const handleToggleLock = async (userId, isLocked) => {
    try {
      if (isLocked) {
        await userService.unlock(userId);
        setToast({ show: true, message: 'User unlocked successfully', variant: 'success' });
      } else {
        await userService.lock(userId, 30);
        setToast({ show: true, message: 'User locked successfully', variant: 'success' });
      }
      fetchUsers();
    } catch (error) {
      setToast({ show: true, message: 'Operation failed', variant: 'danger' });
      console.error('Toggle lock error:', error);
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" className="spinner-border-custom" />
        <p className="mt-3 text-muted">Loading users...</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
        <div>
          <h1 className="page-title text-gradient mb-2">
            <i className="bi bi-people me-2"></i>
            Users Management
          </h1>
          <p className="text-muted mb-0">Manage user accounts and permissions</p>
        </div>
        <div className="d-flex gap-3 align-items-center flex-wrap">
          {canViewAllUsers(currentUser) && tenants.length > 0 && (
            <Form.Select
              value={selectedTenantId}
              onChange={(e) => setSelectedTenantId(e.target.value)}
              style={{ minWidth: '200px' }}
            >
              {tenants.map((tenant) => (
                <option key={tenant.id} value={tenant.id}>
                  {tenant.name}
                </option>
              ))}
            </Form.Select>
          )}
          <Button
            variant="primary"
            size="lg"
            onClick={() => handleOpenDialog()}
            className="d-flex align-items-center gap-2"
          >
            <i className="bi bi-plus-circle"></i>
            Add User
          </Button>
        </div>
      </div>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-0">
          <div className="table-responsive">
            <Table hover className="mb-0">
              <thead>
                <tr>
                  <th><i className="bi bi-person me-2"></i>Name</th>
                  <th className="d-none d-md-table-cell"><i className="bi bi-envelope me-2"></i>Email</th>
                  {canViewAllUsers(currentUser) && (
                    <th className="d-none d-lg-table-cell"><i className="bi bi-building me-2"></i>Tenant</th>
                  )}
                  <th><i className="bi bi-toggle-on me-2"></i>Status</th>
                  <th className="text-end"><i className="bi bi-gear me-2"></i>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.length === 0 ? (
                  <tr>
                    <td colSpan={canViewAllUsers(currentUser) ? 5 : 4} className="text-center py-5">
                      <i className="bi bi-inbox fs-1 text-muted d-block mb-3"></i>
                      <p className="text-muted mb-0">No users found</p>
                    </td>
                  </tr>
                ) : (
                  users.map((user) => (
                    <tr key={user.id}>
                      <td className="fw-semibold">
                        <i className="bi bi-person-circle text-primary me-2"></i>
                        {`${user.firstName} ${user.lastName}`}
                      </td>
                      <td className="d-none d-md-table-cell text-muted">
                        {user.email}
                      </td>
                      {canViewAllUsers(currentUser) && (
                        <td className="d-none d-lg-table-cell">
                          {tenants.find(t => t.id === user.tenantId)?.name || 'N/A'}
                        </td>
                      )}
                      <td>
                        <Badge bg={user.isActive ? 'success' : 'secondary'} className="px-3 py-2">
                          {user.isActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </td>
                      <td className="text-end">
                        <div className="d-flex gap-2 justify-content-end">
                          <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={() => handleOpenDialog(user)}
                            title="Edit"
                          >
                            <i className="bi bi-pencil"></i>
                          </Button>
                          <Button
                            variant={user.isLocked ? 'outline-danger' : 'outline-warning'}
                            size="sm"
                            onClick={() => handleToggleLock(user.id, user.isLocked)}
                            title={user.isLocked ? 'Unlock' : 'Lock'}
                          >
                            <i className={`bi bi-${user.isLocked ? 'unlock' : 'lock'}`}></i>
                          </Button>
                          <Button
                            variant="outline-danger"
                            size="sm"
                            onClick={() => handleDelete(user.id)}
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

      <UserFormDialog
        open={openDialog}
        onClose={handleCloseDialog}
        onSubmit={handleSubmit}
        user={editingUser}
        tenants={tenants}
        currentUser={currentUser}
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

export default UsersList;
