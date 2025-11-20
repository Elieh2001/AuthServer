import api from './api';

const userService = {
  getAll: async (tenantId, page = 1, pageSize = 20) => {
    const response = await api.get('/User/GetByTenant', {
      params: { tenantId, page, pageSize },
    });
    return response.data;
  },

  getById: async (userId) => {
    const response = await api.get('/User/GetById', {
      params: { userId },
    });
    return response.data;
  },

  getByEmail: async (email, tenantId) => {
    const response = await api.get('/User/GetByEmail', {
      params: { email, tenantId },
    });
    return response.data;
  },

  create: async (userData) => {
    const response = await api.post('/User/Add', userData);
    return response.data;
  },

  update: async (userId, userData) => {
    const response = await api.post('/User/Update', userData, {
      params: { userId },
    });
    return response.data;
  },

  delete: async (userId) => {
    const response = await api.post('/User/Delete', null, {
      params: { userId },
    });
    return response.data;
  },

  lock: async (userId, durationMinutes) => {
    const response = await api.post('/User/Lock', { durationMinutes }, {
      params: { userId },
    });
    return response.data;
  },

  unlock: async (userId) => {
    const response = await api.post('/User/Unlock', null, {
      params: { userId },
    });
    return response.data;
  },

  linkProvider: async (userId, providerData) => {
    const response = await api.post('/User/LinkProvider', providerData, {
      params: { userId },
    });
    return response.data;
  },

  unlinkProvider: async (userId, provider) => {
    const response = await api.post('/User/UnlinkProvider', null, {
      params: { userId, provider },
    });
    return response.data;
  },
};

export default userService;
