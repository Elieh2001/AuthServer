import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Typography, IconButton, Chip, Card,
  Snackbar, Alert, Skeleton, CardContent, Fade,
} from '@mui/material';
import { Add, Edit, Delete, Block, CheckCircle } from '@mui/icons-material';
import tenantService from '../../services/tenantService';

const TenantsList = () => {
  const [tenants, setTenants] = useState([]);
  const [loading, setLoading] = useState(true);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const fetchTenants = useCallback(async () => {
    try {
      setLoading(true);
      const response = await tenantService.getAll();
      setTenants(response || []);
    } catch (err) {
      setSnackbar({ open: true, message: 'Failed to load tenants', severity: 'error' });
      console.error('Failed to load tenants', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchTenants();
  }, [fetchTenants]);

  const handleDelete = async (id) => {
    if (window.confirm('Delete this tenant?')) {
      try {
        await tenantService.delete(id);
        setSnackbar({ open: true, message: 'Tenant deleted successfully', severity: 'success' });
        fetchTenants();
      } catch (err) {
        setSnackbar({ open: true, message: 'Failed to delete tenant', severity: 'error' });
        console.error('Delete error:', err);
      }
    }
  };

  const handleToggleStatus = async (id, status) => {
    try {
      if (status === 'Active') {
        await tenantService.suspend(id);
        setSnackbar({ open: true, message: 'Tenant suspended', severity: 'success' });
      } else {
        await tenantService.activate(id);
        setSnackbar({ open: true, message: 'Tenant activated', severity: 'success' });
      }
      fetchTenants();
    } catch (err) {
      setSnackbar({ open: true, message: 'Failed to update status', severity: 'error' });
      console.error('Toggle status error:', err);
    }
  };

  if (loading) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={60} sx={{ mb: 3 }} />
        <Card><CardContent><Skeleton variant="rectangular" height={400} /></CardContent></Card>
      </Box>
    );
  }

  return (
    <Fade in timeout={500}>
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" fontWeight="bold" color="primary">Tenants Management</Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            sx={{ borderRadius: 2, textTransform: 'none', boxShadow: 3, '&:hover': { boxShadow: 6 } }}
          >
            Add Tenant
          </Button>
        </Box>
        <Card elevation={3} sx={{ borderRadius: 2, overflow: 'hidden' }}>
          <TableContainer>
            <Table sx={{ minWidth: { xs: 300, sm: 650 } }}>
              <TableHead sx={{ bgcolor: 'primary.light' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', sm: 'table-cell' } }}>
                    Subdomain
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', md: 'table-cell' } }}>
                    Subscription
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }} align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {tenants.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                      <Typography color="text.secondary">No tenants found</Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  tenants.map((tenant) => (
                    <TableRow key={tenant.id} hover sx={{ '&:hover': { bgcolor: 'action.hover' }, transition: 'background-color 0.3s' }}>
                      <TableCell sx={{ fontWeight: 'medium' }}>{tenant.name}</TableCell>
                      <TableCell sx={{ display: { xs: 'none', sm: 'table-cell' }, fontFamily: 'monospace' }}>
                        {tenant.subdomain}
                      </TableCell>
                      <TableCell sx={{ display: { xs: 'none', md: 'table-cell' } }}>{tenant.subscriptionPlan}</TableCell>
                      <TableCell>
                        <Chip
                          label={tenant.status}
                          color={tenant.status === 'Active' ? 'success' : 'default'}
                          size="small"
                          sx={{ fontWeight: 'bold' }}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" color="primary" sx={{ mr: 0.5 }}><Edit fontSize="small" /></IconButton>
                        <IconButton size="small" color="warning" sx={{ mr: 0.5 }} onClick={() => handleToggleStatus(tenant.id, tenant.status)}>
                          {tenant.status === 'Active' ? <Block fontSize="small" /> : <CheckCircle fontSize="small" />}
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleDelete(tenant.id)}>
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
        <Snackbar
          open={snackbar.open}
          autoHideDuration={6000}
          onClose={() => setSnackbar({ ...snackbar, open: false })}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert severity={snackbar.severity} sx={{ width: '100%' }}>{snackbar.message}</Alert>
        </Snackbar>
      </Box>
    </Fade>
  );
};

export default TenantsList;
