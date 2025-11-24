import { useState, useEffect } from 'react';
import { Modal, Button, Form, Row, Col } from 'react-bootstrap';

const UserFormDialog = ({ open, onClose, onSubmit, user, tenants = [], currentUser }) => {
  const isEditing = !!user;
  const [formData, setFormData] = useState({
    email: '',
    firstName: '',
    lastName: '',
    password: '',
    tenantId: currentUser?.tenantId || '',
  });

  useEffect(() => {
    if (user) {
      setFormData({
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        password: '',
        tenantId: user.tenantId,
      });
    } else {
      setFormData({
        email: '',
        firstName: '',
        lastName: '',
        password: '',
        tenantId: currentUser?.tenantId || '',
      });
    }
  }, [user, currentUser]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <Modal show={open} onHide={onClose} size="lg" centered>
      <Modal.Header closeButton>
        <Modal.Title>
          <i className="bi bi-person me-2"></i>
          {isEditing ? 'Edit User' : 'Create New User'}
        </Modal.Title>
      </Modal.Header>
      <Form onSubmit={handleSubmit}>
        <Modal.Body className="p-4">
          <Row className="mb-3">
            {!isEditing && tenants.length > 0 && (
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

            <Col md={6}>
              <Form.Group>
                <Form.Label className="fw-bold">
                  <i className="bi bi-person-badge me-2"></i>
                  First Name <span className="text-danger">*</span>
                </Form.Label>
                <Form.Control
                  type="text"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                  placeholder="John"
                  className="form-control-lg"
                />
              </Form.Group>
            </Col>

            <Col md={6}>
              <Form.Group>
                <Form.Label className="fw-bold">
                  <i className="bi bi-person-badge me-2"></i>
                  Last Name <span className="text-danger">*</span>
                </Form.Label>
                <Form.Control
                  type="text"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                  placeholder="Doe"
                  className="form-control-lg"
                />
              </Form.Group>
            </Col>
          </Row>

          <Row className="mb-3">
            <Col md={12}>
              <Form.Group>
                <Form.Label className="fw-bold">
                  <i className="bi bi-envelope me-2"></i>
                  Email Address <span className="text-danger">*</span>
                </Form.Label>
                <Form.Control
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  placeholder="john.doe@example.com"
                  className="form-control-lg"
                />
              </Form.Group>
            </Col>
          </Row>

          {!isEditing && (
            <Row className="mb-3">
              <Col md={12}>
                <Form.Group>
                  <Form.Label className="fw-bold">
                    <i className="bi bi-key me-2"></i>
                    Password <span className="text-danger">*</span>
                  </Form.Label>
                  <Form.Control
                    type="password"
                    name="password"
                    value={formData.password}
                    onChange={handleChange}
                    required
                    placeholder="Enter a strong password"
                    className="form-control-lg"
                  />
                  <Form.Text className="text-muted">
                    Password must meet the tenant's security requirements
                  </Form.Text>
                </Form.Group>
              </Col>
            </Row>
          )}
        </Modal.Body>
        <Modal.Footer className="bg-light">
          <Button variant="outline-secondary" onClick={onClose} size="lg">
            <i className="bi bi-x-circle me-2"></i>
            Cancel
          </Button>
          <Button variant="primary" type="submit" size="lg">
            <i className={`bi bi-${isEditing ? 'check' : 'plus'}-circle me-2`}></i>
            {isEditing ? 'Update User' : 'Create User'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  );
};

export default UserFormDialog;
