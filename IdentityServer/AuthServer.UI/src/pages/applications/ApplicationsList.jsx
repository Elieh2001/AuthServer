import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Typography, IconButton, Chip, CircularProgress,
  Alert, Snackbar, Skeleton, Card, CardContent, Fade,
} from '@mui/material';
import { Add, Edit, Delete, Refresh } from '@mui/icons-material';
import applicationService from '../../services/applicationService';
import { useAuth } from '../../contexts/AuthContext';

const ApplicationsList = () => {
  const { user } = useAuth();
  const [applications, setApplications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const fetchApplications = useCallback(async () => {
    try {
      setLoading(true);
      const response = await applicationService.getByTenant(user?.tenantId || '');
      setApplications(response || []);
    } catch (err) {
      setSnackbar({ open: true, message: 'Failed to load applications', severity: 'error' });
      console.error('Failed to load applications', err);
    } finally {
      setLoading(false);
    }
  }, [user?.tenantId]);

  useEffect(() => {
    fetchApplications();
  }, [fetchApplications]);

  const handleDelete = async (id) => {
    if (window.confirm('Delete this application?')) {
      try {
        await applicationService.delete(id);
        setSnackbar({ open: true, message: 'Application deleted successfully', severity: 'success' });
        fetchApplications();
      } catch (err) {
        setSnackbar({ open: true, message: 'Failed to delete application', severity: 'error' });
        console.error('Delete error:', err);
      }
    }
  };

  const handleRegenerateSecret = async (id) => {
    if (window.confirm('Regenerate secret for this application?')) {
      try {
        const response = await applicationService.regenerateSecret(id);
        setSnackbar({
          open: true,
          message: `New secret: ${response.clientSecret} - Please save it securely!`,
          severity: 'warning'
        });
        fetchApplications();
      } catch (err) {
        setSnackbar({ open: true, message: 'Failed to regenerate secret', severity: 'error' });
        console.error('Regenerate error:', err);
      }
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
            Applications Management
          </Typography>
          <Button
            variant="contained"
            startIcon={<Add />}
            sx={{
              borderRadius: 2,
              textTransform: 'none',
              boxShadow: 3,
              '&:hover': { boxShadow: 6 }
            }}
          >
            Add Application
          </Button>
        </Box>

        <Card elevation={3} sx={{ borderRadius: 2, overflow: 'hidden' }}>
          <TableContainer>
            <Table sx={{ minWidth: { xs: 300, sm: 650 } }}>
              <TableHead sx={{ bgcolor: 'primary.light' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', sm: 'table-cell' } }}>
                    Client ID
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', md: 'table-cell' } }}>
                    Type
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }} align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {applications.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                      <Typography color="text.secondary">No applications found</Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  applications.map((app) => (
                    <TableRow
                      key={app.id}
                      hover
                      sx={{
                        '&:hover': { bgcolor: 'action.hover' },
                        transition: 'background-color 0.3s'
                      }}
                    >
                      <TableCell sx={{ fontWeight: 'medium' }}>{app.name}</TableCell>
                      <TableCell sx={{ display: { xs: 'none', sm: 'table-cell' }, fontFamily: 'monospace' }}>
                        {app.clientId}
                      </TableCell>
                      <TableCell sx={{ display: { xs: 'none', md: 'table-cell' } }}>
                        {app.applicationType}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={app.isActive ? 'Active' : 'Inactive'}
                          color={app.isActive ? 'success' : 'default'}
                          size="small"
                          sx={{ fontWeight: 'bold' }}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" color="primary" sx={{ mr: 0.5 }}>
                          <Edit fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          color="warning"
                          sx={{ mr: 0.5 }}
                          onClick={() => handleRegenerateSecret(app.id)}
                        >
                          <Refresh fontSize="small" />
                        </IconButton>
                        <IconButton size="small" color="error" onClick={() => handleDelete(app.id)}>
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
          <Alert severity={snackbar.severity} sx={{ width: '100%' }}>
            {snackbar.message}
          </Alert>
        </Snackbar>
      </Box>
    </Fade>
  );
};

export default ApplicationsList;
