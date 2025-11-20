import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  TextField,
  Button,
  Grid,
  Card,
  Skeleton,
  CardContent,
  Fade,
  Alert,
  Snackbar,
  Chip,
} from '@mui/material';
import { Search } from '@mui/icons-material';
import { format } from 'date-fns';
import auditService from '../../services/auditService';

const AuditLogs = () => {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [filters, setFilters] = useState({
    tenantId: '',
    userId: '',
    from: '',
    to: '',
  });

  const fetchLogs = useCallback(async () => {
    try {
      setLoading(true);
      const response = await auditService.getLogs(filters);
      setLogs(response.data || []);
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to load audit logs', severity: 'error' });
      console.error('Failed to load audit logs', error);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchLogs();
  }, [fetchLogs]);

  const handleFilterChange = (e) => {
    setFilters({ ...filters, [e.target.name]: e.target.value });
  };

  if (loading) {
    return (
      <Box>
        <Skeleton variant="text" width={300} height={60} sx={{ mb: 3 }} />
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Skeleton variant="rectangular" height={80} />
          </CardContent>
        </Card>
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
        <Typography variant="h4" fontWeight="bold" color="primary" gutterBottom sx={{ mb: 3 }}>
          Audit Logs
        </Typography>

        <Card elevation={2} sx={{ p: 2, mb: 3, borderRadius: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="From Date"
                type="date"
                name="from"
                value={filters.from}
                onChange={handleFilterChange}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="To Date"
                type="date"
                name="to"
                value={filters.to}
                onChange={handleFilterChange}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                fullWidth
                label="User ID"
                name="userId"
                value={filters.userId}
                onChange={handleFilterChange}
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <Button
                fullWidth
                variant="contained"
                startIcon={<Search />}
                onClick={fetchLogs}
                sx={{
                  borderRadius: 2,
                  textTransform: 'none',
                  boxShadow: 3,
                  '&:hover': { boxShadow: 6 }
                }}
              >
                Search
              </Button>
            </Grid>
          </Grid>
        </Card>

        <Card elevation={3} sx={{ borderRadius: 2, overflow: 'hidden' }}>
          <TableContainer>
            <Table sx={{ minWidth: { xs: 300, sm: 650 } }}>
              <TableHead sx={{ bgcolor: 'primary.light' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Timestamp</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white' }}>Event Type</TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', sm: 'table-cell' } }}>
                    User
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', md: 'table-cell' } }}>
                    IP Address
                  </TableCell>
                  <TableCell sx={{ fontWeight: 'bold', color: 'white', display: { xs: 'none', lg: 'table-cell' } }}>
                    Details
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {logs.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                      <Typography color="text.secondary">No audit logs found</Typography>
                    </TableCell>
                  </TableRow>
                ) : (
                  logs.map((log) => (
                    <TableRow
                      key={log.id}
                      hover
                      sx={{
                        '&:hover': { bgcolor: 'action.hover' },
                        transition: 'background-color 0.3s'
                      }}
                    >
                      <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.875rem' }}>
                        {log.timestamp ? format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss') : '-'}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={log.eventType}
                          size="small"
                          color="primary"
                          variant="outlined"
                          sx={{ fontWeight: 'medium' }}
                        />
                      </TableCell>
                      <TableCell sx={{ display: { xs: 'none', sm: 'table-cell' } }}>
                        {log.userEmail || log.userId || '-'}
                      </TableCell>
                      <TableCell sx={{ display: { xs: 'none', md: 'table-cell' }, fontFamily: 'monospace' }}>
                        {log.ipAddress || '-'}
                      </TableCell>
                      <TableCell sx={{ display: { xs: 'none', lg: 'table-cell' }, maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        {log.details || '-'}
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

export default AuditLogs;
