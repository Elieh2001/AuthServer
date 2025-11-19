import api from './api';

const applicationService = {
  getByTenant: async (tenantId) => {
    const response = await api.get('/Application/GetByTenant', {
      params: { tenantId },
    });
    return response.data;
  },

  getById: async (applicationId) => {
    const response = await api.get('/Application/GetById', {
      params: { applicationId },
    });
    return response.data;
  },

  getByClientId: async (clientId) => {
    const response = await api.get('/Application/GetByClientId', {
      params: { clientId },
    });
    return response.data;
  },

  create: async (applicationData) => {
    const response = await api.post('/Application/Add', applicationData);
    return response.data;
  },

  update: async (applicationId, applicationData) => {
    const response = await api.post('/Application/Update', applicationData, {
      params: { applicationId },
    });
    return response.data;
  },

  delete: async (applicationId) => {
    const response = await api.post('/Application/Delete', null, {
      params: { applicationId },
    });
    return response.data;
  },

  regenerateSecret: async (applicationId) => {
    const response = await api.post('/Application/RegenerateSecret', null, {
      params: { applicationId },
    });
    return response.data;
  },

  validateCredentials: async (clientId, clientSecret) => {
    const response = await api.post('/Application/ValidateCredentials', {
      clientId,
      clientSecret,
    });
    return response.data;
  },
};

export default applicationService;
