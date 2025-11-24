import { useState, useMemo } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Navbar, Nav, Container, Dropdown, Offcanvas } from 'react-bootstrap';
import { useAuth } from '../../contexts/AuthContext';
import { isSystemAdmin, getUserRole } from '../../utils/roleUtils';

const MainLayout = () => {
  const [showSidebar, setShowSidebar] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const menuItems = useMemo(() => {
    const items = [
      { text: 'Dashboard', icon: 'bi-speedometer2', path: '/dashboard', roles: ['all'] },
      { text: 'Users', icon: 'bi-people', path: '/users', roles: ['systemAdmin', 'tenantAdmin'] },
      { text: 'Tenants', icon: 'bi-building', path: '/tenants', roles: ['systemAdmin'] },
      { text: 'Applications', icon: 'bi-app', path: '/applications', roles: ['systemAdmin', 'tenantAdmin'] },
      { text: 'Audit Logs', icon: 'bi-clock-history', path: '/audit', roles: ['systemAdmin', 'tenantAdmin'] },
    ];

    return items.filter(item => {
      if (item.roles.includes('all')) return true;
      if (item.roles.includes('systemAdmin') && isSystemAdmin(user)) return true;
      if (item.roles.includes('tenantAdmin') && !isSystemAdmin(user) && user?.tenantId) return true;
      return false;
    });
  }, [user]);

  const isActivePath = (path) => location.pathname === path;

  return (
    <div className="d-flex flex-column min-vh-100">
      {/* Top Navbar */}
      <Navbar expand="lg" className="shadow-sm" style={{
        background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)'
      }}>
        <Container fluid className="px-3 px-lg-4">
          <Navbar.Brand
            onClick={() => navigate('/dashboard')}
            className="text-white fw-bold fs-4 d-flex align-items-center"
            style={{ cursor: 'pointer' }}
          >
            <i className="bi bi-shield-lock-fill me-2"></i>
            AuthServer
          </Navbar.Brand>

          {/* Mobile Menu Toggle */}
          <button
            className="btn btn-link text-white d-lg-none"
            onClick={() => setShowSidebar(!showSidebar)}
          >
            <i className="bi bi-list fs-3"></i>
          </button>

          {/* Desktop Navigation */}
          <Navbar.Collapse id="main-navbar">
            <Nav className="me-auto d-none d-lg-flex">
              {menuItems.map((item) => (
                <Nav.Link
                  key={item.path}
                  onClick={() => navigate(item.path)}
                  className={`mx-2 px-3 py-2 rounded ${
                    isActivePath(item.path)
                      ? 'bg-white text-primary fw-bold'
                      : 'text-white'
                  }`}
                  style={{
                    cursor: 'pointer',
                    transition: 'all 0.3s ease',
                    fontWeight: isActivePath(item.path) ? '600' : '500',
                  }}
                >
                  <i className={`${item.icon} me-2`}></i>
                  {item.text}
                </Nav.Link>
              ))}
            </Nav>

            {/* User Menu */}
            <div className="d-flex align-items-center">
              <div className="text-white me-3 d-none d-md-block">
                <div className="fw-semibold">{user?.firstName} {user?.lastName}</div>
                <div className="small opacity-75">{getUserRole(user)}</div>
              </div>
              <Dropdown align="end">
                <Dropdown.Toggle
                  variant="link"
                  id="user-dropdown"
                  className="text-decoration-none p-0 border-0"
                  style={{ boxShadow: 'none' }}
                >
                  <div
                    className="bg-white text-primary rounded-circle d-flex align-items-center justify-content-center fw-bold"
                    style={{
                      width: '45px',
                      height: '45px',
                      fontSize: '1.1rem',
                      border: '3px solid white',
                      boxShadow: '0 4px 8px rgba(0,0,0,0.2)',
                    }}
                  >
                    {user?.firstName?.[0]}{user?.lastName?.[0]}
                  </div>
                </Dropdown.Toggle>

                <Dropdown.Menu className="mt-2 shadow-lg border-0" style={{ minWidth: '220px' }}>
                  <div className="px-3 py-2 border-bottom">
                    <div className="fw-semibold">{user?.firstName} {user?.lastName}</div>
                    <div className="small text-muted">{user?.email}</div>
                  </div>
                  <Dropdown.Item
                    onClick={() => navigate('/profile')}
                    className="py-2"
                  >
                    <i className="bi bi-person me-2"></i>
                    Profile
                  </Dropdown.Item>
                  <Dropdown.Divider />
                  <Dropdown.Item
                    onClick={handleLogout}
                    className="text-danger py-2"
                  >
                    <i className="bi bi-box-arrow-right me-2"></i>
                    Logout
                  </Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            </div>
          </Navbar.Collapse>
        </Container>
      </Navbar>

      {/* Mobile Sidebar */}
      <Offcanvas
        show={showSidebar}
        onHide={() => setShowSidebar(false)}
        placement="start"
        className="d-lg-none"
      >
        <Offcanvas.Header
          closeButton
          className="text-white"
          style={{ background: 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)' }}
        >
          <Offcanvas.Title className="fw-bold">
            <i className="bi bi-shield-lock-fill me-2"></i>
            AuthServer
          </Offcanvas.Title>
        </Offcanvas.Header>
        <Offcanvas.Body>
          <Nav className="flex-column">
            {menuItems.map((item) => (
              <Nav.Link
                key={item.path}
                onClick={() => {
                  navigate(item.path);
                  setShowSidebar(false);
                }}
                className={`mb-2 px-3 py-2 rounded ${
                  isActivePath(item.path)
                    ? 'bg-primary text-white'
                    : 'text-dark'
                }`}
                style={{
                  cursor: 'pointer',
                  transition: 'all 0.3s ease',
                  fontWeight: isActivePath(item.path) ? '600' : '500',
                }}
              >
                <i className={`${item.icon} me-2`}></i>
                {item.text}
              </Nav.Link>
            ))}
          </Nav>
          <div className="mt-auto pt-3 border-top">
            <div className="px-3 py-2">
              <div className="fw-semibold">{user?.firstName} {user?.lastName}</div>
              <div className="small text-muted">{user?.email}</div>
            </div>
          </div>
        </Offcanvas.Body>
      </Offcanvas>

      {/* Main Content */}
      <Container fluid className="flex-grow-1 py-4 px-3 px-lg-4" style={{ backgroundColor: '#f8f9fa' }}>
        <div className="fade-in">
          <Outlet />
        </div>
      </Container>

      {/* Footer */}
      <footer className="py-3 border-top bg-white">
        <Container fluid className="px-3 px-lg-4">
          <div className="d-flex justify-content-between align-items-center flex-wrap">
            <p className="mb-0 text-muted small">
              &copy; {new Date().getFullYear()} AuthServer. All rights reserved.
            </p>
            <p className="mb-0 text-muted small">
              Version 1.0.0
            </p>
          </div>
        </Container>
      </footer>
    </div>
  );
};

export default MainLayout;
