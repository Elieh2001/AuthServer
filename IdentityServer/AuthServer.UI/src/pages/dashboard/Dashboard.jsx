import { useState, useEffect } from 'react';
import { Row, Col, Card, Spinner } from 'react-bootstrap';

const Dashboard = () => {
  const [stats, setStats] = useState({
    users: 0,
    tenants: 0,
    applications: 0,
    activeUsers: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setTimeout(() => {
      setStats({
        users: 156,
        tenants: 12,
        applications: 24,
        activeUsers: 89,
      });
      setLoading(false);
    }, 500);
  }, []);

  const StatCard = ({ title, value, icon, color, bgColor }) => (
    <Col xs={12} sm={6} lg={3} className="mb-4">
      <Card className="h-100 border-0 shadow-sm hover-lift" style={{ transition: 'transform 0.3s ease, box-shadow 0.3s ease' }}>
        <Card.Body className="d-flex align-items-center">
          <div className="flex-grow-1">
            <p className="text-muted text-uppercase mb-2 small fw-semibold" style={{ letterSpacing: '0.5px' }}>
              {title}
            </p>
            <h2 className="mb-0 fw-bold" style={{ color }}>
              {value.toLocaleString()}
            </h2>
          </div>
          <div
            className="rounded-circle d-flex align-items-center justify-content-center"
            style={{
              width: '60px',
              height: '60px',
              backgroundColor: bgColor,
            }}
          >
            <i className={`${icon} fs-2`} style={{ color }}></i>
          </div>
        </Card.Body>
      </Card>
    </Col>
  );

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" className="spinner-border-custom" />
        <p className="mt-3 text-muted">Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="page-header mb-4">
        <h1 className="page-title text-gradient">
          <i className="bi bi-speedometer2 me-2"></i>
          Dashboard Overview
        </h1>
        <p className="text-muted">Welcome to your AuthServer management dashboard</p>
      </div>

      <Row>
        <StatCard
          title="Total Users"
          value={stats.users}
          icon="bi-people-fill"
          color="#6366f1"
          bgColor="rgba(99, 102, 241, 0.1)"
        />
        <StatCard
          title="Tenants"
          value={stats.tenants}
          icon="bi-building"
          color="#10b981"
          bgColor="rgba(16, 185, 129, 0.1)"
        />
        <StatCard
          title="Applications"
          value={stats.applications}
          icon="bi-app"
          color="#f59e0b"
          bgColor="rgba(245, 158, 11, 0.1)"
        />
        <StatCard
          title="Active Users"
          value={stats.activeUsers}
          icon="bi-graph-up-arrow"
          color="#8b5cf6"
          bgColor="rgba(139, 92, 246, 0.1)"
        />
      </Row>

      <Row>
        <Col lg={8} className="mb-4">
          <Card className="border-0 shadow-sm h-100">
            <Card.Header className="bg-white border-0 py-3">
              <h5 className="mb-0 fw-bold">
                <i className="bi bi-activity me-2 text-primary"></i>
                Recent Activity
              </h5>
            </Card.Header>
            <Card.Body>
              <div className="d-flex align-items-center justify-content-center py-5">
                <div className="text-center">
                  <i className="bi bi-inbox fs-1 text-muted mb-3 d-block"></i>
                  <p className="text-muted">No recent activity to display</p>
                </div>
              </div>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={4} className="mb-4">
          <Card className="border-0 shadow-sm h-100">
            <Card.Header className="bg-white border-0 py-3">
              <h5 className="mb-0 fw-bold">
                <i className="bi bi-bell me-2 text-primary"></i>
                Quick Actions
              </h5>
            </Card.Header>
            <Card.Body>
              <div className="d-grid gap-2">
                <button className="btn btn-outline-primary text-start" onClick={() => window.location.href = '/users'}>
                  <i className="bi bi-person-plus me-2"></i>
                  Add New User
                </button>
                <button className="btn btn-outline-primary text-start" onClick={() => window.location.href = '/tenants'}>
                  <i className="bi bi-building-add me-2"></i>
                  Create Tenant
                </button>
                <button className="btn btn-outline-primary text-start" onClick={() => window.location.href = '/applications'}>
                  <i className="bi bi-plus-circle me-2"></i>
                  New Application
                </button>
                <button className="btn btn-outline-primary text-start" onClick={() => window.location.href = '/audit'}>
                  <i className="bi bi-search me-2"></i>
                  View Audit Logs
                </button>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default Dashboard;
