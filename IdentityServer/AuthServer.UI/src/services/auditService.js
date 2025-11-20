import api from './api';

const auditService = {
  getLogs: async (filters = {}) => {
    const { tenantId, userId, from, to, page = 1, pageSize = 50 } = filters;
    const response = await api.get('/Audit/GetLogs', {
      params: { tenantId, userId, from, to, page, pageSize },
    });
    return response.data;
  },

  getByTenant: async (tenantId, filters = {}) => {
    const { from, to, page = 1, pageSize = 50 } = filters;
    const response = await api.get('/Audit/GetByTenant', {
      params: { tenantId, from, to, page, pageSize },
    });
    return response.data;
  },

  getByUser: async (userId, filters = {}) => {
    const { from, to, page = 1, pageSize = 50 } = filters;
    const response = await api.get('/Audit/GetByUser', {
      params: { userId, from, to, page, pageSize },
    });
    return response.data;
  },
};

export default auditService;
