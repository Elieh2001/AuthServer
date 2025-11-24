import { useState, useEffect, useCallback } from 'react';
import { Table, Button, Badge, Spinner, Card, Toast, ToastContainer, Form, Row, Col } from 'react-bootstrap';
import { format } from 'date-fns';
import auditService from '../../services/auditService';

const AuditLogs = () => {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState({ show: false, message: '', variant: 'success' });
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
      setToast({ show: true, message: 'Failed to load audit logs', variant: 'danger' });
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

  const handleSearch = (e) => {
    e.preventDefault();
    fetchLogs();
  };

  const handleClearFilters = () => {
    setFilters({
      tenantId: '',
      userId: '',
      from: '',
      to: '',
    });
  };

  if (loading && logs.length === 0) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" className="spinner-border-custom" />
        <p className="mt-3 text-muted">Loading audit logs...</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="page-header mb-4">
        <h1 className="page-title text-gradient mb-2">
          <i className="bi bi-clock-history me-2"></i>
          Audit Logs
        </h1>
        <p className="text-muted mb-0">Track and monitor system activities and user actions</p>
      </div>

      <Card className="border-0 shadow-sm mb-4">
        <Card.Header className="bg-white border-0 py-3">
          <h5 className="mb-0 fw-bold">
            <i className="bi bi-funnel me-2 text-primary"></i>
            Search Filters
          </h5>
        </Card.Header>
        <Card.Body>
          <Form onSubmit={handleSearch}>
            <Row className="g-3">
              <Col md={3}>
                <Form.Group>
                  <Form.Label className="fw-semibold">
                    <i className="bi bi-calendar-range me-2"></i>
                    From Date
                  </Form.Label>
                  <Form.Control
                    type="date"
                    name="from"
                    value={filters.from}
                    onChange={handleFilterChange}
                  />
                </Form.Group>
              </Col>
              <Col md={3}>
                <Form.Group>
                  <Form.Label className="fw-semibold">
                    <i className="bi bi-calendar-check me-2"></i>
                    To Date
                  </Form.Label>
                  <Form.Control
                    type="date"
                    name="to"
                    value={filters.to}
                    onChange={handleFilterChange}
                  />
                </Form.Group>
              </Col>
              <Col md={3}>
                <Form.Group>
                  <Form.Label className="fw-semibold">
                    <i className="bi bi-person me-2"></i>
                    User ID
                  </Form.Label>
                  <Form.Control
                    type="text"
                    name="userId"
                    value={filters.userId}
                    onChange={handleFilterChange}
                    placeholder="Enter user ID"
                  />
                </Form.Group>
              </Col>
              <Col md={3} className="d-flex align-items-end">
                <div className="d-flex gap-2 w-100">
                  <Button
                    variant="primary"
                    type="submit"
                    className="flex-grow-1"
                    disabled={loading}
                  >
                    <i className="bi bi-search me-2"></i>
                    Search
                  </Button>
                  <Button
                    variant="outline-secondary"
                    type="button"
                    onClick={handleClearFilters}
                  >
                    <i className="bi bi-x-circle"></i>
                  </Button>
                </div>
              </Col>
            </Row>
          </Form>
        </Card.Body>
      </Card>

      <Card className="border-0 shadow-sm">
        <Card.Body className="p-0">
          <div className="table-responsive">
            <Table hover className="mb-0">
              <thead>
                <tr>
                  <th><i className="bi bi-clock me-2"></i>Timestamp</th>
                  <th><i className="bi bi-tag me-2"></i>Event Type</th>
                  <th className="d-none d-md-table-cell"><i className="bi bi-person me-2"></i>User</th>
                  <th className="d-none d-lg-table-cell"><i className="bi bi-geo-alt me-2"></i>IP Address</th>
                  <th className="d-none d-xl-table-cell"><i className="bi bi-info-circle me-2"></i>Details</th>
                </tr>
              </thead>
              <tbody>
                {logs.length === 0 ? (
                  <tr>
                    <td colSpan={5} className="text-center py-5">
                      <i className="bi bi-inbox fs-1 text-muted d-block mb-3"></i>
                      <p className="text-muted mb-0">No audit logs found</p>
                      <p className="small text-muted">Try adjusting your search filters</p>
                    </td>
                  </tr>
                ) : (
                  logs.map((log) => (
                    <tr key={log.id}>
                      <td className="text-muted small font-monospace">
                        {log.timestamp ? format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss') : '-'}
                      </td>
                      <td>
                        <Badge bg="primary" className="px-3 py-2">
                          {log.eventType}
                        </Badge>
                      </td>
                      <td className="d-none d-md-table-cell">
                        <div className="d-flex align-items-center">
                          <i className="bi bi-person-circle text-primary me-2"></i>
                          {log.userEmail || log.userId || '-'}
                        </div>
                      </td>
                      <td className="d-none d-lg-table-cell font-monospace small text-muted">
                        {log.ipAddress || '-'}
                      </td>
                      <td className="d-none d-xl-table-cell text-muted small" style={{ maxWidth: '300px' }}>
                        <div
                          className="text-truncate"
                          title={log.details}
                          style={{ maxWidth: '300px' }}
                        >
                          {log.details || '-'}
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </Table>
          </div>
        </Card.Body>
        {logs.length > 0 && (
          <Card.Footer className="bg-white border-0 py-3">
            <div className="d-flex justify-content-between align-items-center text-muted small">
              <span>Showing {logs.length} log{logs.length !== 1 ? 's' : ''}</span>
              <Button variant="outline-primary" size="sm" onClick={fetchLogs} disabled={loading}>
                <i className="bi bi-arrow-clockwise me-2"></i>
                Refresh
              </Button>
            </div>
          </Card.Footer>
        )}
      </Card>

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

export default AuditLogs;
