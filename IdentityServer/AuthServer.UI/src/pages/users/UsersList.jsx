import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  Chip,
  Alert,
  Snackbar,
  Card,
  Skeleton,
  CardContent,
  Fade,
} from '@mui/material';
import { Add, Edit, Delete, Lock, LockOpen } from '@mui/icons-material';
import userService from '../../services/userService';
import { useAuth } from '../../contexts/AuthContext';

const UsersList = () => {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [formData, setFormData] = useState({
    email: '',
    firstName: '',
    lastName: '',
    password: '',
    tenantId: currentUser?.tenantId || '',
  });

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      const response = await userService.getAll(currentUser?.tenantId || '', 1, 100);
      setUsers(response.data || []);
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to load users', severity: 'error' });
      console.error('Failed to load users', error);
    } finally {
      setLoading(false);
    }
  }, [currentUser?.tenantId]);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const handleOpenDialog = (user = null) => {
    if (user) {
      setEditingUser(user);
      setFormData({
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        password: '',
        tenantId: user.tenantId,
      });
    } else {
      setEditingUser(null);
      setFormData({
        email: '',
        firstName: '',
        lastName: '',
        password: '',
        tenantId: currentUser?.tenantId || '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingUser(null);
  };

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (editingUser) {
        await userService.update(editingUser.id, formData);
        setSnackbar({ open: true, message: 'User updated successfully', severity: 'success' });
      } else {
        await userService.create(formData);
        setSnackbar({ open: true, message: 'User created successfully', severity: 'success' });
      }
      fetchUsers();
      handleCloseDialog();
    } catch (error) {
      setSnackbar({
        open: true,
        message: error.response?.data?.error || 'Operation failed',
        severity: 'error'
      });
      console.error('Submit error:', error);
    }
  };

  const handleDelete = async (userId) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await userService.delete(userId);
        setSnackbar({ open: true, message: 'User deleted successfully', severity: 'success' });
        fetchUsers();
      } catch (error) {
        setSnackbar({ open: true, message: 'Failed to delete user', severity: 'error' });
        console.error('Delete error:', error);
      }
    }
  };

  const handleToggleLock = async (userId, isLocked) => {
    try {
      if (isLocked) {
        await userService.unlock(userId);
        setSnackbar({ open: true, message: 'User unlocked successfully', severity: 'success' });
      } else {
        await userService.lock(userId, 30);
        setSnackbar({ open: true, message: 'User locked successfully', severity: 'success' });
      }
      fetchUsers();
    } catch (error) {
      setSnackbar({ open: true, message: 'Operation failed', severity: 'error' });
      console.error('Toggle lock error:', error);
    }
  };

  if (loading) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={60} sx={{ mb: 3 }} />
        <Card>
          <CardContent>
            <Skeleton variant="rectangular" height={400} />
          </CardContent>
        </Card>
      </Box>
    );
  }

  return (
    <Fade in timeout={500}>
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" fontWeight="bold" color="primary">
            Users Management
          </Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            onClick={() => handleOpenDialog()}
            sx={{
              borderRadius: 2,
              textTransform: 'none',
              boxShadow: 3,
              '&:hover': { boxShadow: 6 }
            }}
          >
            Add User
          </Button>
        </Box>

        <Card elevation={3} sx={{ borderRadius: 2, overflow: 'hidden' }}>
          <TableContainer>
            <Table sx={{ minWidth: { xs: 300, sm: 650 } }}>
              <TableHead sx={{ bgcolor: 'primary.light' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', sm: 'table-cell' } }}>
                    Email
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }} align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={4} align="center" sx={{ py: 4 }}>
                      <Typography color="text.secondary">No users found</Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  users.map((user) => (
                    <TableRow
                      key={user.id}
                      hover
                      sx={{
                        '&:hover': { bgcolor: 'action.hover' },
                        transition: 'background-color 0.3s'
                      }}
                    >
                      <TableCell sx={{ fontWeight: 'medium' }}>
                        {`${user.firstName} ${user.lastName}`}
                      </TableCell>
                      <TableCell sx={{ display: { xs: 'none', sm: 'table-cell' }, fontFamily: 'monospace' }}>
                        {user.email}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={user.isActive ? 'Active' : 'Inactive'}
                          color={user.isActive ? 'success' : 'default'}
                          size="small"
                          sx={{ fontWeight: 'bold' }}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" color="primary" sx={{ mr: 0.5 }} onClick={() => handleOpenDialog(user)}>
                          <Edit fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          color={user.isLocked ? 'error' : 'warning'}
                          sx={{ mr: 0.5 }}
                          onClick={() => handleToggleLock(user.id, user.isLocked)}
                        >
                          {user.isLocked ? <LockOpen fontSize="small" /> : <Lock fontSize="small" />}
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleDelete(user.id)}>
                          <Delete fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Card>

        <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
          <DialogTitle sx={{ bgcolor: 'primary.main', color: 'white', fontWeight: 'bold' }}>
            {editingUser ? 'Edit User' : 'Add User'}
          </DialogTitle>
          <form onSubmit={handleSubmit}>
            <DialogContent sx={{ mt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="First Name"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleChange}
                    required
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Last Name"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleChange}
                    required
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Email"
                    name="email"
                    type="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                  />
                </Grid>
                {!editingUser && (
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Password"
                      name="password"
                      type="password"
                      value={formData.password}
                      onChange={handleChange}
                      required
                    />
                  </Grid>
                )}
              </Grid>
            </DialogContent>
            <DialogActions sx={{ px: 3, pb: 2 }}>
              <Button onClick={handleCloseDialog} sx={{ textTransform: 'none' }}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" sx={{ textTransform: 'none' }}>
                {editingUser ? 'Update' : 'Create'}
              </Button>
            </DialogActions>
          </form>
        </Dialog>

        <Snackbar
          open={snackbar.open}
          autoHideDuration={6000}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert severity={snackbar.severity} sx={{ width: '100%' }}>
            {snackbar.message}
          </Alert>
        </Snackbar>
      </Box>
    </Fade>
  );
};

export default UsersList;
