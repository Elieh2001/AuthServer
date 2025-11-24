import api from './api';
import { LoginRequestDto, LoginResponseDto, CreateUserDto, RefreshTokenRequest } from '../models';

const authService = {
  login: async (email, password,tenantName, clientId = '', clientSecret = '') => {
    const loginRequest = new LoginRequestDto({ email, password,tenantName, clientId, clientSecret });
    const response = await api.post('/Authentication/Login', loginRequest);
    
    if (response.status==200) {
      try{
      const loginResponse = new LoginResponseDto(response.data);
      localStorage.setItem('accessToken', loginResponse.accessToken);
      localStorage.setItem('refreshToken', loginResponse.refreshToken);
      localStorage.setItem('user', JSON.stringify(loginResponse.user));
      return loginResponse;
      }catch(ex){
        throw ex;
      }
    }
    throw new Error(response.data.message || 'Login failed');
  },

  register: async (data) => {
    const createUserDto = new CreateUserDto(data);
    const response = await api.post('/Authentication/Register', createUserDto);
    return response.data;
  },

  logout: async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await api.post('/Authentication/Logout', { refreshToken });
      } catch (error) {
        console.error('Logout error:', error);
      }
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  refreshToken: async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const request = new RefreshTokenRequest({ refreshToken });
    const response = await api.post('/Authentication/RefreshToken', request);

    if (response.data.success) {
      const loginResponse = new LoginResponseDto(response.data.data);
      localStorage.setItem('accessToken', loginResponse.accessToken);
      localStorage.setItem('refreshToken', loginResponse.refreshToken);
      return loginResponse;
    }
    throw new Error(response.data.message || 'Token refresh failed');
  },

  changePassword: async (currentPassword, newPassword) => {
    const response = await api.post('/Authentication/ChangePassword', {
      currentPassword,
      newPassword,
    });
    return response.data;
  },

  requestPasswordReset: async (email, tenantId = null) => {
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

  verifyEmail: async (token) => {
    const response = await api.post('/Authentication/VerifyEmail', { token });
    return response.data;
  },

  getCurrentUser: () => {
    const userJson = localStorage.getItem('user');
    return userJson ? JSON.parse(userJson) : null;
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('accessToken');
  },
};

export default authService;
