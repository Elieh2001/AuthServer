import { useState, useEffect } from 'react';
import { Modal, Button, Form, Row, Col, Nav, Tab, Card } from 'react-bootstrap';
import './TenantFormDialog.css';

const TenantFormDialog = ({ open, onClose, onSubmit, tenant }) => {
  const isEditing = !!tenant;

  const [formData, setFormData] = useState({
    name: '',
    subdomain: '',
    subscriptionPlan: 'Free',
    logoUrl: '',
    primaryColor: '#1976d2',
    passwordMinLength: 8,
    passwordRequireUppercase: true,
    passwordRequireLowercase: true,
    passwordRequireDigit: true,
    passwordRequireSpecialChar: true,
    sessionTimeoutMinutes: 30,
    maxFailedLoginAttempts: 5,
    accountLockoutDurationMinutes: 15,
  });

  useEffect(() => {
    if (tenant) {
      setFormData({
        name: tenant.name || '',
        subscriptionPlan: tenant.subscriptionPlan || 'Free',
        logoUrl: tenant.logoUrl || '',
        primaryColor: tenant.primaryColor || '#1976d2',
        passwordMinLength: tenant.passwordMinLength || 8,
        passwordRequireUppercase: tenant.passwordRequireUppercase !== undefined ? tenant.passwordRequireUppercase : true,
        passwordRequireLowercase: tenant.passwordRequireLowercase !== undefined ? tenant.passwordRequireLowercase : true,
        passwordRequireDigit: tenant.passwordRequireDigit !== undefined ? tenant.passwordRequireDigit : true,
        passwordRequireSpecialChar: tenant.passwordRequireSpecialChar !== undefined ? tenant.passwordRequireSpecialChar : true,
        sessionTimeoutMinutes: tenant.sessionTimeoutMinutes || 30,
        maxFailedLoginAttempts: tenant.maxFailedLoginAttempts || 5,
        accountLockoutDurationMinutes: tenant.accountLockoutDurationMinutes || 15,
      });
    } else {
      setFormData({
        name: '',
        subdomain: '',
        subscriptionPlan: 'Free',
        logoUrl: '',
        primaryColor: '#1976d2',
        passwordMinLength: 8,
        passwordRequireUppercase: true,
        passwordRequireLowercase: true,
        passwordRequireDigit: true,
        passwordRequireSpecialChar: true,
        sessionTimeoutMinutes: 30,
        maxFailedLoginAttempts: 5,
        accountLockoutDurationMinutes: 15,
      });
    }
  }, [tenant]);

  const handleChange = (e) => {
    const { name, value, checked, type } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value,
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <Modal show={open} onHide={onClose} size="xl" centered className="tenant-form-modal">
      <Modal.Header closeButton className="bg-primary text-white">
        <Modal.Title>
          <i className="bi bi-building me-2"></i>
          {isEditing ? 'Edit Tenant' : 'Create New Tenant'}
        </Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body className="p-4">
          <Tab.Container defaultActiveKey="basic">
            <Row>
              <Col md={3}>
                <Nav variant="pills" className="flex-column">
                  <Nav.Item>
                    <Nav.Link eventKey="basic" className="mb-2">
                      <i className="bi bi-info-circle me-2"></i>
                      Basic Info
                    </Nav.Link>
                  </Nav.Item>
                  <Nav.Item>
                    <Nav.Link eventKey="branding" className="mb-2">
                      <i className="bi bi-palette me-2"></i>
                      Branding
                    </Nav.Link>
                  </Nav.Item>
                  <Nav.Item>
                    <Nav.Link eventKey="password" className="mb-2">
                      <i className="bi bi-key me-2"></i>
                      Password Policy
                    </Nav.Link>
                  </Nav.Item>
                  <Nav.Item>
                    <Nav.Link eventKey="security" className="mb-2">
                      <i className="bi bi-shield-lock me-2"></i>
                      Security Settings
                    </Nav.Link>
                  </Nav.Item>
                </Nav>
              </Col>
              <Col md={9}>
                <Tab.Content>
                  {/* Basic Information Tab */}
                  <Tab.Pane eventKey="basic">
                    <Card className="border-0 shadow-sm">
                      <Card.Body>
                        <h5 className="mb-4 text-primary">
                          <i className="bi bi-info-circle me-2"></i>
                          Basic Information
                        </h5>

                        <Row className="mb-3">
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-building me-2"></i>
                                Tenant Name <span className="text-danger">*</span>
                              </Form.Label>
                              <Form.Control
                                type="text"
                                name="name"
                                value={formData.name}
                                onChange={handleChange}
                                required
                                placeholder="e.g., Acme Corporation"
                                className="form-control-lg"
                              />
                              <Form.Text className="text-muted">
                                The display name of your organization
                              </Form.Text>
                            </Form.Group>
                          </Col>

                          {!isEditing && (
                            <Col md={6}>
                              <Form.Group className="mb-3">
                                <Form.Label className="fw-bold">
                                  <i className="bi bi-link-45deg me-2"></i>
                                  Subdomain <span className="text-danger">*</span>
                                </Form.Label>
                                <Form.Control
                                  type="text"
                                  name="subdomain"
                                  value={formData.subdomain}
                                  onChange={handleChange}
                                  required
                                  placeholder="acme"
                                  pattern="^[a-z0-9-]+$"
                                  className="form-control-lg"
                                />
                                <Form.Text className="text-muted">
                                  Lowercase letters, numbers, and hyphens only. Will be: <strong>{formData.subdomain || 'your-subdomain'}.authserver.com</strong>
                                </Form.Text>
                              </Form.Group>
                            </Col>
                          )}

                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-star me-2"></i>
                                Subscription Plan
                              </Form.Label>
                              <Form.Select
                                name="subscriptionPlan"
                                value={formData.subscriptionPlan}
                                onChange={handleChange}
                                className="form-select-lg"
                              >
                                <option value="Free">Free - Basic features</option>
                                <option value="Starter">Starter - $29/month</option>
                                <option value="Professional">Professional - $99/month</option>
                                <option value="Enterprise">Enterprise - Custom pricing</option>
                              </Form.Select>
                            </Form.Group>
                          </Col>
                        </Row>
                      </Card.Body>
                    </Card>
                  </Tab.Pane>

                  {/* Branding Tab */}
                  <Tab.Pane eventKey="branding">
                    <Card className="border-0 shadow-sm">
                      <Card.Body>
                        <h5 className="mb-4 text-primary">
                          <i className="bi bi-palette me-2"></i>
                          Branding & Appearance
                        </h5>

                        <Row>
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-image me-2"></i>
                                Logo URL
                              </Form.Label>
                              <Form.Control
                                type="url"
                                name="logoUrl"
                                value={formData.logoUrl}
                                onChange={handleChange}
                                placeholder="https://example.com/logo.png"
                                className="form-control-lg"
                              />
                              <Form.Text className="text-muted">
                                Direct URL to your organization's logo image
                              </Form.Text>
                            </Form.Group>

                            {formData.logoUrl && (
                              <div className="mb-3">
                                <Form.Label className="fw-bold">Logo Preview</Form.Label>
                                <div className="p-3 border rounded bg-light text-center">
                                  <img
                                    src={formData.logoUrl}
                                    alt="Logo preview"
                                    style={{ maxWidth: '200px', maxHeight: '100px' }}
                                    onError={(e) => { e.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="200" height="100"%3E%3Ctext x="50%25" y="50%25" text-anchor="middle" fill="%23999"%3EInvalid URL%3C/text%3E%3C/svg%3E'; }}
                                  />
                                </div>
                              </div>
                            )}
                          </Col>

                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-palette-fill me-2"></i>
                                Primary Brand Color
                              </Form.Label>
                              <div className="d-flex align-items-center gap-3">
                                <Form.Control
                                  type="color"
                                  name="primaryColor"
                                  value={formData.primaryColor}
                                  onChange={handleChange}
                                  className="form-control-color form-control-lg"
                                  style={{ width: '80px', height: '56px' }}
                                />
                                <Form.Control
                                  type="text"
                                  value={formData.primaryColor}
                                  readOnly
                                  className="form-control-lg font-monospace"
                                />
                              </div>
                              <Form.Text className="text-muted">
                                This color will be used throughout the authentication interface
                              </Form.Text>
                            </Form.Group>

                            <div className="mt-4 p-3 rounded" style={{ backgroundColor: formData.primaryColor, color: 'white' }}>
                              <h6 className="mb-2">Color Preview</h6>
                              <p className="mb-0 small">This is how your brand color will look in buttons and headers</p>
                            </div>
                          </Col>
                        </Row>
                      </Card.Body>
                    </Card>
                  </Tab.Pane>

                  {/* Password Policy Tab */}
                  <Tab.Pane eventKey="password">
                    <Card className="border-0 shadow-sm">
                      <Card.Body>
                        <h5 className="mb-4 text-primary">
                          <i className="bi bi-key me-2"></i>
                          Password Requirements
                        </h5>

                        <Row>
                          <Col md={6}>
                            <Form.Group className="mb-4">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-rulers me-2"></i>
                                Minimum Password Length
                              </Form.Label>
                              <div className="input-group input-group-lg">
                                <Form.Control
                                  type="number"
                                  name="passwordMinLength"
                                  value={formData.passwordMinLength}
                                  onChange={handleChange}
                                  min="6"
                                  max="128"
                                />
                                <span className="input-group-text">characters</span>
                              </div>
                              <Form.Text className="text-muted">
                                Minimum: 6 | Recommended: 8-12
                              </Form.Text>
                            </Form.Group>
                          </Col>

                          <Col md={6}>
                            <Form.Label className="fw-bold mb-3">
                              <i className="bi bi-check2-square me-2"></i>
                              Character Requirements
                            </Form.Label>
                            <div className="d-flex flex-column gap-3">
                              <Form.Check
                                type="switch"
                                id="passwordRequireUppercase"
                                name="passwordRequireUppercase"
                                label="Require Uppercase Letters (A-Z)"
                                checked={formData.passwordRequireUppercase}
                                onChange={handleChange}
                                className="form-check-lg"
                              />
                              <Form.Check
                                type="switch"
                                id="passwordRequireLowercase"
                                name="passwordRequireLowercase"
                                label="Require Lowercase Letters (a-z)"
                                checked={formData.passwordRequireLowercase}
                                onChange={handleChange}
                                className="form-check-lg"
                              />
                              <Form.Check
                                type="switch"
                                id="passwordRequireDigit"
                                name="passwordRequireDigit"
                                label="Require Numbers (0-9)"
                                checked={formData.passwordRequireDigit}
                                onChange={handleChange}
                                className="form-check-lg"
                              />
                              <Form.Check
                                type="switch"
                                id="passwordRequireSpecialChar"
                                name="passwordRequireSpecialChar"
                                label="Require Special Characters (!@#$%)"
                                checked={formData.passwordRequireSpecialChar}
                                onChange={handleChange}
                                className="form-check-lg"
                              />
                            </div>
                          </Col>
                        </Row>

                        <div className="mt-4 p-3 bg-light rounded">
                          <h6 className="mb-2">
                            <i className="bi bi-lightbulb me-2"></i>
                            Password Policy Summary
                          </h6>
                          <p className="mb-0 small">
                            Passwords must be at least <strong>{formData.passwordMinLength} characters</strong> and include:
                            {formData.passwordRequireUppercase && ' uppercase letters,'}
                            {formData.passwordRequireLowercase && ' lowercase letters,'}
                            {formData.passwordRequireDigit && ' numbers,'}
                            {formData.passwordRequireSpecialChar && ' special characters'}
                          </p>
                        </div>
                      </Card.Body>
                    </Card>
                  </Tab.Pane>

                  {/* Security Settings Tab */}
                  <Tab.Pane eventKey="security">
                    <Card className="border-0 shadow-sm">
                      <Card.Body>
                        <h5 className="mb-4 text-primary">
                          <i className="bi bi-shield-lock me-2"></i>
                          Security Configuration
                        </h5>

                        <Row>
                          <Col md={4}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-clock-history me-2"></i>
                                Session Timeout
                              </Form.Label>
                              <div className="input-group input-group-lg">
                                <Form.Control
                                  type="number"
                                  name="sessionTimeoutMinutes"
                                  value={formData.sessionTimeoutMinutes}
                                  onChange={handleChange}
                                  min="5"
                                  max="1440"
                                />
                                <span className="input-group-text">minutes</span>
                              </div>
                              <Form.Text className="text-muted">
                                Range: 5-1440 minutes (1 day)
                              </Form.Text>
                            </Form.Group>
                          </Col>

                          <Col md={4}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-shield-x me-2"></i>
                                Max Failed Login Attempts
                              </Form.Label>
                              <div className="input-group input-group-lg">
                                <Form.Control
                                  type="number"
                                  name="maxFailedLoginAttempts"
                                  value={formData.maxFailedLoginAttempts}
                                  onChange={handleChange}
                                  min="3"
                                  max="10"
                                />
                                <span className="input-group-text">attempts</span>
                              </div>
                              <Form.Text className="text-muted">
                                Before account lockout
                              </Form.Text>
                            </Form.Group>
                          </Col>

                          <Col md={4}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-lock me-2"></i>
                                Account Lockout Duration
                              </Form.Label>
                              <div className="input-group input-group-lg">
                                <Form.Control
                                  type="number"
                                  name="accountLockoutDurationMinutes"
                                  value={formData.accountLockoutDurationMinutes}
                                  onChange={handleChange}
                                  min="5"
                                  max="1440"
                                />
                                <span className="input-group-text">minutes</span>
                              </div>
                              <Form.Text className="text-muted">
                                How long accounts stay locked
                              </Form.Text>
                            </Form.Group>
                          </Col>
                        </Row>

                        <div className="mt-4 p-4 bg-warning bg-opacity-10 rounded border border-warning">
                          <h6 className="mb-2 text-warning">
                            <i className="bi bi-exclamation-triangle-fill me-2"></i>
                            Security Recommendations
                          </h6>
                          <ul className="mb-0 small">
                            <li>Set session timeout to 30-60 minutes for optimal security</li>
                            <li>Keep max failed attempts at 5 or lower to prevent brute force attacks</li>
                            <li>Set lockout duration to at least 15 minutes</li>
                            <li>Enable strong password requirements for better account security</li>
                          </ul>
                        </div>
                      </Card.Body>
                    </Card>
                  </Tab.Pane>
                </Tab.Content>
              </Col>
            </Row>
          </Tab.Container>
        </Modal.Body>
        <Modal.Footer className="bg-light">
          <Button variant="outline-secondary" onClick={onClose} size="lg">
            <i className="bi bi-x-circle me-2"></i>
            Cancel
          </Button>
          <Button variant="primary" type="submit" size="lg">
            <i className={`bi bi-${isEditing ? 'check' : 'plus'}-circle me-2`}></i>
            {isEditing ? 'Update Tenant' : 'Create Tenant'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

export default TenantFormDialog;
