import api from './api';

const authService = {
  login: async (email, password, tenantId) => {
    const response = await api.post('/Authentication/Login', {
      email,
      password,
      tenantId,
    });
    return response.data;
  },

  register: async (data) => {
    const response = await api.post('/Authentication/Register', data);
    return response.data;
  },

  logout: async (refreshToken) => {
    const response = await api.post('/Authentication/Logout', { refreshToken });
    return response.data;
  },

  changePassword: async (currentPassword, newPassword) => {
    const response = await api.post('/Authentication/ChangePassword', {
      currentPassword,
      newPassword,
    });
    return response.data;
  },

  requestPasswordReset: async (email, tenantId) => {
    const response = await api.post('/Authentication/RequestPasswordReset', {
      email,
      tenantId,
    });
    return response.data;
  },

  resetPassword: async (token, newPassword) => {
    const response = await api.post('/Authentication/ResetPassword', {
      token,
      newPassword,
    });
    return response.data;
  },
};

export default authService;
