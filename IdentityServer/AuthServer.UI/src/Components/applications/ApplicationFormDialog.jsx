import { useState, useEffect } from 'react';
import { Modal, Button, Form, Row, Col, Nav, Tab, Card } from 'react-bootstrap';
import { useAuth } from '../../contexts/AuthContext';
import './ApplicationFormDialog.css';

const ApplicationFormDialog = ({ open, onClose, onSubmit, application, tenants = [] }) => {
  const { user } = useAuth();
  const isEditing = !!application;

  const [formData, setFormData] = useState({
    tenantId: user?.tenantId || '',
    name: '',
    description: '',
    applicationType: 'Native',
    allowedGrantTypes: 'authorization_code,refresh_token',
    allowedScopes: 'openid,profile,email',
    redirectUris: '',
    postLogoutRedirectUris: '',
    accessTokenLifetimeSeconds: 3600,
    refreshTokenLifetimeSeconds: 2592000,
    googleEnabled: false,
    googleClientId: '',
    googleClientSecret: '',
    appleEnabled: false,
    appleClientId: '',
    appleTeamId: '',
    appleKeyId: '',
    applePrivateKey: '',
    linkedInEnabled: false,
    linkedInClientId: '',
    linkedInClientSecret: '',
    hasLegacyDatabase: false,
    legacyDatabaseConnectionString: '',
    legacyDatabaseType: 'SqlServer',
    legacyUserTableName: '',
    legacyUserIdColumn: 'Id',
    legacyEmailColumn: 'Email',
    legacyPasswordColumn: 'PasswordHash',
    legacyPasswordHashAlgorithm: 'BCrypt',
    isActive: true,
  });

  useEffect(() => {
    if (application) {
      setFormData({
        tenantId: application.tenantId || user?.tenantId || '',
        name: application.name || '',
        description: application.description || '',
        applicationType: application.applicationType || 'Native',
        allowedGrantTypes: application.allowedGrantTypes || 'authorization_code,refresh_token',
        allowedScopes: application.allowedScopes || 'openid,profile,email',
        redirectUris: application.redirectUris || '',
        postLogoutRedirectUris: application.postLogoutRedirectUris || '',
        accessTokenLifetimeSeconds: application.accessTokenLifetimeSeconds || 3600,
        refreshTokenLifetimeSeconds: application.refreshTokenLifetimeSeconds || 2592000,
        googleEnabled: application.googleEnabled || false,
        googleClientId: application.googleClientId || '',
        googleClientSecret: '',
        appleEnabled: application.appleEnabled || false,
        appleClientId: application.appleClientId || '',
        appleTeamId: application.appleTeamId || '',
        appleKeyId: application.appleKeyId || '',
        applePrivateKey: '',
        linkedInEnabled: application.linkedInEnabled || false,
        linkedInClientId: application.linkedInClientId || '',
        linkedInClientSecret: '',
        hasLegacyDatabase: application.hasLegacyDatabase || false,
        legacyDatabaseConnectionString: '',
        legacyDatabaseType: application.legacyDatabaseType || 'SqlServer',
        legacyUserTableName: application.legacyUserTableName || '',
        legacyUserIdColumn: application.legacyUserIdColumn || 'Id',
        legacyEmailColumn: application.legacyEmailColumn || 'Email',
        legacyPasswordColumn: application.legacyPasswordColumn || 'PasswordHash',
        legacyPasswordHashAlgorithm: application.legacyPasswordHashAlgorithm || 'BCrypt',
        isActive: application.isActive !== undefined ? application.isActive : true,
      });
    } else {
      setFormData({
        tenantId: user?.tenantId || '',
        name: '',
        description: '',
        applicationType: 'Native',
        allowedGrantTypes: 'authorization_code,refresh_token',
        allowedScopes: 'openid,profile,email',
        redirectUris: '',
        postLogoutRedirectUris: '',
        accessTokenLifetimeSeconds: 3600,
        refreshTokenLifetimeSeconds: 2592000,
        googleEnabled: false,
        googleClientId: '',
        googleClientSecret: '',
        appleEnabled: false,
        appleClientId: '',
        appleTeamId: '',
        appleKeyId: '',
        applePrivateKey: '',
        linkedInEnabled: false,
        linkedInClientId: '',
        linkedInClientSecret: '',
        hasLegacyDatabase: false,
        legacyDatabaseConnectionString: '',
        legacyDatabaseType: 'SqlServer',
        legacyUserTableName: '',
        legacyUserIdColumn: 'Id',
        legacyEmailColumn: 'Email',
        legacyPasswordColumn: 'PasswordHash',
        legacyPasswordHashAlgorithm: 'BCrypt',
        isActive: true,
      });
    }
  }, [application, user?.tenantId]);

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
    <Modal show={open} onHide={onClose} size="xl" centered className="application-form-modal">
      <Modal.Header closeButton className="bg-primary text-white">
        <Modal.Title>
          <i className="bi bi-app me-2"></i>
          {isEditing ? 'Edit Application' : 'Create New Application'}
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
                    <Nav.Link eventKey="security" className="mb-2">
                      <i className="bi bi-shield-lock me-2"></i>
                      Security
                    </Nav.Link>
                  </Nav.Item>
                  <Nav.Item>
                    <Nav.Link eventKey="providers" className="mb-2">
                      <i className="bi bi-key me-2"></i>
                      External Providers
                    </Nav.Link>
                  </Nav.Item>
                  {formData.applicationType === 'LegacyDatabase' && (
                    <Nav.Item>
                      <Nav.Link eventKey="legacy" className="mb-2">
                        <i className="bi bi-database me-2"></i>
                        Legacy Database
                      </Nav.Link>
                    </Nav.Item>
                  )}
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
                          {!isEditing && user?.isSystemAdmin && (
                            <Col md={12} className="mb-3">
                              <Form.Group>
                                <Form.Label className="fw-bold">
                                  <i className="bi bi-building me-2"></i>
                                  Tenant <span className="text-danger">*</span>
                                </Form.Label>
                                <Form.Select
                                  name="tenantId"
                                  value={formData.tenantId}
                                  onChange={handleChange}
                                  required
                                  className="form-select-lg"
                                >
                                  <option value="">Select Tenant</option>
                                  {tenants.map((tenant) => (
                                    <option key={tenant.id} value={tenant.id}>
                                      {tenant.name}
                                    </option>
                                  ))}
                                </Form.Select>
                              </Form.Group>
                            </Col>
                          )}

                          {!isEditing && (
                            <Col md={12} className="mb-3">
                              <Form.Group>
                                <Form.Label className="fw-bold">
                                  <i className="bi bi-gear me-2"></i>
                                  Application Type <span className="text-danger">*</span>
                                </Form.Label>
                                <Form.Select
                                  name="applicationType"
                                  value={formData.applicationType}
                                  onChange={handleChange}
                                  required
                                  className="form-select-lg"
                                >
                                  <option value="Native">Native - Standard OAuth/OIDC</option>
                                  <option value="LegacyDatabase">Legacy Database - Migrate existing users</option>
                                  <option value="Federated">Federated - External Identity Provider</option>
                                </Form.Select>
                                <Form.Text className="text-muted">
                                  Choose the type based on your authentication requirements
                                </Form.Text>
                              </Form.Group>
                            </Col>
                          )}

                          {isEditing && (
                            <Col md={12} className="mb-3">
                              <Form.Group>
                                <Form.Label className="fw-bold">
                                  <i className="bi bi-gear me-2"></i>
                                  Application Type
                                </Form.Label>
                                <Form.Control
                                  type="text"
                                  value={formData.applicationType}
                                  disabled
                                  className="form-control-lg"
                                />
                                <Form.Text className="text-muted">
                                  Application type cannot be changed after creation
                                </Form.Text>
                              </Form.Group>
                            </Col>
                          )}

                          <Col md={6}>
                            <Form.Group>
                              <Form.Label className="fw-bold">
                                <i className="bi bi-pencil me-2"></i>
                                Application Name <span className="text-danger">*</span>
                              </Form.Label>
                              <Form.Control
                                type="text"
                                name="name"
                                value={formData.name}
                                onChange={handleChange}
                                required
                                placeholder="e.g., My Web Application"
                                className="form-control-lg"
                              />
                            </Form.Group>
                          </Col>

                          <Col md={6}>
                            <Form.Group>
                              <Form.Label className="fw-bold">
                                <i className="bi bi-file-text me-2"></i>
                                Description
                              </Form.Label>
                              <Form.Control
                                type="text"
                                name="description"
                                value={formData.description}
                                onChange={handleChange}
                                placeholder="Brief description of your application"
                                className="form-control-lg"
                              />
                            </Form.Group>
                          </Col>
                        </Row>

                        <Row className="mb-3">
                          <Col md={6}>
                            <Form.Group>
                              <Form.Label className="fw-bold">
                                <i className="bi bi-arrow-return-left me-2"></i>
                                Redirect URIs
                              </Form.Label>
                              <Form.Control
                                type="text"
                                name="redirectUris"
                                value={formData.redirectUris}
                                onChange={handleChange}
                                placeholder="https://example.com/callback"
                                className="form-control-lg"
                              />
                              <Form.Text className="text-muted">
                                Comma-separated list of allowed redirect URIs
                              </Form.Text>
                            </Form.Group>
                          </Col>

                          <Col md={6}>
                            <Form.Group>
                              <Form.Label className="fw-bold">
                                <i className="bi bi-box-arrow-right me-2"></i>
                                Post Logout Redirect URIs
                              </Form.Label>
                              <Form.Control
                                type="text"
                                name="postLogoutRedirectUris"
                                value={formData.postLogoutRedirectUris}
                                onChange={handleChange}
                                placeholder="https://example.com/logout"
                                className="form-control-lg"
                              />
                              <Form.Text className="text-muted">
                                Where to redirect after logout
                              </Form.Text>
                            </Form.Group>
                          </Col>
                        </Row>

                        {!isEditing && (
                          <Row className="mb-3">
                            <Col md={6}>
                              <Form.Group>
                                <Form.Label className="fw-bold">
                                  <i className="bi bi-check2-circle me-2"></i>
                                  Allowed Grant Types
                                </Form.Label>
                                <Form.Control
                                  type="text"
                                  name="allowedGrantTypes"
                                  value={formData.allowedGrantTypes}
                                  onChange={handleChange}
                                  className="form-control-lg"
                                />
                                <Form.Text className="text-muted">
                                  Comma-separated (e.g., authorization_code,refresh_token)
                                </Form.Text>
                              </Form.Group>
                            </Col>

                            <Col md={6}>
                              <Form.Group>
                                <Form.Label className="fw-bold">
                                  <i className="bi bi-shield-check me-2"></i>
                                  Allowed Scopes
                                </Form.Label>
                                <Form.Control
                                  type="text"
                                  name="allowedScopes"
                                  value={formData.allowedScopes}
                                  onChange={handleChange}
                                  className="form-control-lg"
                                />
                                <Form.Text className="text-muted">
                                  Comma-separated (e.g., openid,profile,email)
                                </Form.Text>
                              </Form.Group>
                            </Col>
                          </Row>
                        )}

                        {isEditing && (
                          <Row className="mb-3">
                            <Col md={12}>
                              <Form.Group>
                                <Form.Check
                                  type="switch"
                                  id="isActive"
                                  name="isActive"
                                  label="Application is Active"
                                  checked={formData.isActive}
                                  onChange={handleChange}
                                  className="form-check-lg"
                                />
                              </Form.Group>
                            </Col>
                          </Row>
                        )}
                      </Card.Body>
                    </Card>
                  </Tab.Pane>

                  {/* Security Tab */}
                  <Tab.Pane eventKey="security">
                    <Card className="border-0 shadow-sm">
                      <Card.Body>
                        <h5 className="mb-4 text-primary">
                          <i className="bi bi-shield-lock me-2"></i>
                          Token Settings
                        </h5>

                        <Row>
                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-clock me-2"></i>
                                Access Token Lifetime
                              </Form.Label>
                              <div className="input-group input-group-lg">
                                <Form.Control
                                  type="number"
                                  name="accessTokenLifetimeSeconds"
                                  value={formData.accessTokenLifetimeSeconds}
                                  onChange={handleChange}
                                  min="60"
                                />
                                <span className="input-group-text">seconds</span>
                              </div>
                              <Form.Text className="text-muted">
                                Recommended: 3600 (1 hour)
                              </Form.Text>
                            </Form.Group>
                          </Col>

                          <Col md={6}>
                            <Form.Group className="mb-3">
                              <Form.Label className="fw-bold">
                                <i className="bi bi-arrow-clockwise me-2"></i>
                                Refresh Token Lifetime
                              </Form.Label>
                              <div className="input-group input-group-lg">
                                <Form.Control
                                  type="number"
                                  name="refreshTokenLifetimeSeconds"
                                  value={formData.refreshTokenLifetimeSeconds}
                                  onChange={handleChange}
                                  min="3600"
                                />
                                <span className="input-group-text">seconds</span>
                              </div>
                              <Form.Text className="text-muted">
                                Recommended: 2592000 (30 days)
                              </Form.Text>
                            </Form.Group>
                          </Col>
                        </Row>
                      </Card.Body>
                    </Card>
                  </Tab.Pane>

                  {/* External Providers Tab */}
                  <Tab.Pane eventKey="providers">
                    <Card className="border-0 shadow-sm mb-3">
                      <Card.Header className="bg-light">
                        <div className="d-flex align-items-center">
                          <i className="bi bi-google text-danger me-2 fs-5"></i>
                          <h6 className="mb-0">Google Authentication</h6>
                          <Form.Check
                            type="switch"
                            name="googleEnabled"
                            checked={formData.googleEnabled}
                            onChange={handleChange}
                            className="ms-auto"
                          />
                        </div>
                      </Card.Header>
                      {formData.googleEnabled && (
                        <Card.Body>
                          <Row>
                            <Col md={6}>
                              <Form.Group className="mb-3">
                                <Form.Label className="fw-bold">Client ID</Form.Label>
                                <Form.Control
                                  type="text"
                                  name="googleClientId"
                                  value={formData.googleClientId}
                                  onChange={handleChange}
                                  placeholder="Google OAuth Client ID"
                                  className="form-control-lg"
                                />
                              </Form.Group>
                            </Col>
                            <Col md={6}>
                              <Form.Group className="mb-3">
                                <Form.Label className="fw-bold">Client Secret</Form.Label>
                                <Form.Control
                                  type="password"
                                  name="googleClientSecret"
                                  value={formData.googleClientSecret}
                                  onChange={handleChange}
                                  placeholder="Google OAuth Client Secret"
                                  className="form-control-lg"
                                />
                              </Form.Group>
                            </Col>
                          </Row>
                        </Card.Body>
                      )}
                    </Card>

                    <Card className="border-0 shadow-sm mb-3">
                      <Card.Header className="bg-light">
                        <div className="d-flex align-items-center">
                          <i className="bi bi-apple text-dark me-2 fs-5"></i>
                          <h6 className="mb-0">Apple Authentication</h6>
                          <Form.Check
                            type="switch"
                            name="appleEnabled"
                            checked={formData.appleEnabled}
                            onChange={handleChange}
                            className="ms-auto"
                          />
                        </div>
                      </Card.Header>
                      {formData.appleEnabled && (
                        <Card.Body>
                          <Row>
                            <Col md={6}>
                              <Form.Group className="mb-3">
                                <Form.Label className="fw-bold">Client ID</Form.Label>
                                <Form.Control
                                  type="text"
                                  name="appleClientId"
                                  value={formData.appleClientId}
                                  onChange={handleChange}
                                  placeholder="Apple Service ID"
                                  className="form-control-lg"
                                />
                              </Form.Group>
                            </Col>
                            {!isEditing && (
                              <>
                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Team ID</Form.Label>
                                    <Form.Control
                                      type="text"
                                      name="appleTeamId"
                                      value={formData.appleTeamId}
                                      onChange={handleChange}
                                      placeholder="Apple Team ID"
                                      className="form-control-lg"
                                    />
                                  </Form.Group>
                                </Col>
                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Key ID</Form.Label>
                                    <Form.Control
                                      type="text"
                                      name="appleKeyId"
                                      value={formData.appleKeyId}
                                      onChange={handleChange}
                                      placeholder="Apple Key ID"
                                      className="form-control-lg"
                                    />
                                  </Form.Group>
                                </Col>
                                <Col md={12}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Private Key</Form.Label>
                                    <Form.Control
                                      as="textarea"
                                      rows={4}
                                      name="applePrivateKey"
                                      value={formData.applePrivateKey}
                                      onChange={handleChange}
                                      placeholder="-----BEGIN PRIVATE KEY-----&#10;...&#10;-----END PRIVATE KEY-----"
                                      className="font-monospace"
                                    />
                                  </Form.Group>
                                </Col>
                              </>
                            )}
                          </Row>
                        </Card.Body>
                      )}
                    </Card>

                    <Card className="border-0 shadow-sm">
                      <Card.Header className="bg-light">
                        <div className="d-flex align-items-center">
                          <i className="bi bi-linkedin text-primary me-2 fs-5"></i>
                          <h6 className="mb-0">LinkedIn Authentication</h6>
                          <Form.Check
                            type="switch"
                            name="linkedInEnabled"
                            checked={formData.linkedInEnabled}
                            onChange={handleChange}
                            className="ms-auto"
                          />
                        </div>
                      </Card.Header>
                      {formData.linkedInEnabled && (
                        <Card.Body>
                          <Row>
                            <Col md={6}>
                              <Form.Group className="mb-3">
                                <Form.Label className="fw-bold">Client ID</Form.Label>
                                <Form.Control
                                  type="text"
                                  name="linkedInClientId"
                                  value={formData.linkedInClientId}
                                  onChange={handleChange}
                                  placeholder="LinkedIn Client ID"
                                  className="form-control-lg"
                                />
                              </Form.Group>
                            </Col>
                            <Col md={6}>
                              <Form.Group className="mb-3">
                                <Form.Label className="fw-bold">Client Secret</Form.Label>
                                <Form.Control
                                  type="password"
                                  name="linkedInClientSecret"
                                  value={formData.linkedInClientSecret}
                                  onChange={handleChange}
                                  placeholder="LinkedIn Client Secret"
                                  className="form-control-lg"
                                />
                              </Form.Group>
                            </Col>
                          </Row>
                        </Card.Body>
                      )}
                    </Card>
                  </Tab.Pane>

                  {/* Legacy Database Tab */}
                  {formData.applicationType === 'LegacyDatabase' && (
                    <Tab.Pane eventKey="legacy">
                      <Card className="border-0 shadow-sm">
                        <Card.Body>
                          <h5 className="mb-4 text-primary">
                            <i className="bi bi-database me-2"></i>
                            Legacy Database Configuration
                          </h5>

                          <Form.Group className="mb-4">
                            <Form.Check
                              type="switch"
                              id="hasLegacyDatabase"
                              name="hasLegacyDatabase"
                              label="Enable Legacy Database Integration"
                              checked={formData.hasLegacyDatabase}
                              onChange={handleChange}
                              className="form-check-lg"
                            />
                          </Form.Group>

                          {formData.hasLegacyDatabase && (
                            <>
                              <Row className="mb-3">
                                <Col md={12}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">
                                      <i className="bi bi-link-45deg me-2"></i>
                                      Connection String {isEditing && <span className="text-muted">(Leave empty to keep current)</span>}
                                    </Form.Label>
                                    <Form.Control
                                      type="password"
                                      name="legacyDatabaseConnectionString"
                                      value={formData.legacyDatabaseConnectionString}
                                      onChange={handleChange}
                                      placeholder={isEditing ? "Enter new connection string to update" : "Server=localhost;Database=mydb;User Id=user;Password=pass;"}
                                      className="form-control-lg font-monospace"
                                    />
                                    {isEditing && (
                                      <Form.Text className="text-muted">
                                        For security reasons, the connection string is not displayed. Leave empty to keep the existing connection string.
                                      </Form.Text>
                                    )}
                                  </Form.Group>
                                </Col>
                              </Row>

                              <Row className="mb-3">
                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Database Type</Form.Label>
                                    <Form.Select
                                      name="legacyDatabaseType"
                                      value={formData.legacyDatabaseType}
                                      onChange={handleChange}
                                      className="form-select-lg"
                                    >
                                      <option value="SqlServer">SQL Server</option>
                                      <option value="MySql">MySQL</option>
                                      <option value="PostgreSql">PostgreSQL</option>
                                    </Form.Select>
                                  </Form.Group>
                                </Col>

                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">User Table Name</Form.Label>
                                    <Form.Control
                                      type="text"
                                      name="legacyUserTableName"
                                      value={formData.legacyUserTableName}
                                      onChange={handleChange}
                                      placeholder="Users"
                                      className="form-control-lg"
                                    />
                                  </Form.Group>
                                </Col>
                              </Row>

                              <Row className="mb-3">
                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">User ID Column</Form.Label>
                                    <Form.Control
                                      type="text"
                                      name="legacyUserIdColumn"
                                      value={formData.legacyUserIdColumn}
                                      onChange={handleChange}
                                      placeholder="Id"
                                      className="form-control-lg"
                                    />
                                  </Form.Group>
                                </Col>

                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Email Column</Form.Label>
                                    <Form.Control
                                      type="text"
                                      name="legacyEmailColumn"
                                      value={formData.legacyEmailColumn}
                                      onChange={handleChange}
                                      placeholder="Email"
                                      className="form-control-lg"
                                    />
                                  </Form.Group>
                                </Col>
                              </Row>

                              <Row>
                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Password Column</Form.Label>
                                    <Form.Control
                                      type="text"
                                      name="legacyPasswordColumn"
                                      value={formData.legacyPasswordColumn}
                                      onChange={handleChange}
                                      placeholder="PasswordHash"
                                      className="form-control-lg"
                                    />
                                  </Form.Group>
                                </Col>

                                <Col md={6}>
                                  <Form.Group className="mb-3">
                                    <Form.Label className="fw-bold">Password Hash Algorithm</Form.Label>
                                    <Form.Select
                                      name="legacyPasswordHashAlgorithm"
                                      value={formData.legacyPasswordHashAlgorithm}
                                      onChange={handleChange}
                                      className="form-select-lg"
                                    >
                                      <option value="BCrypt">BCrypt</option>
                                      <option value="SHA256">SHA256</option>
                                      <option value="SHA512">SHA512</option>
                                      <option value="MD5">MD5 (Not Recommended)</option>
                                    </Form.Select>
                                  </Form.Group>
                                </Col>
                              </Row>
                            </>
                          )}
                        </Card.Body>
                      </Card>
                    </Tab.Pane>
                  )}
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
            {isEditing ? 'Update Application' : 'Create Application'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

export default ApplicationFormDialog;