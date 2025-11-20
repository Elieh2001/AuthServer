import api from './api';

const tenantService = {
  getAll: async () => {
    const response = await api.get('/Tenant/GetAll');
    return response.data;
  },

  getById: async (tenantId) => {
    const response = await api.get('/Tenant/GetById', {
      params: { tenantId },
    });
    return response.data;
  },

  getBySubdomain: async (subdomain) => {
    const response = await api.get('/Tenant/GetBySubdomain', {
      params: { subdomain },
    });
    return response.data;
  },

  create: async (tenantData) => {
    const response = await api.post('/Tenant/Add', tenantData);
    return response.data;
  },

  update: async (tenantId, tenantData) => {
    const response = await api.post('/Tenant/Update', tenantData, {
      params: { tenantId },
    });
    return response.data;
  },

  delete: async (tenantId) => {
    const response = await api.post('/Tenant/Delete', null, {
      params: { tenantId },
    });
    return response.data;
  },

  suspend: async (tenantId) => {
    const response = await api.post('/Tenant/Suspend', null, {
      params: { tenantId },
    });
    return response.data;
  },

  activate: async (tenantId) => {
    const response = await api.post('/Tenant/Activate', null, {
      params: { tenantId },
    });
    return response.data;
  },
};

export default tenantService;
