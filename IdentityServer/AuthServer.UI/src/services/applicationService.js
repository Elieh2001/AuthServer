import api from './api';
import { CreateApplicationDto, UpdateApplicationDto, ApplicationDto } from '../models';

const applicationService = {
  getById: async (id) => {
    const response = await api.get(`/Application/GetById?applicationId=${id}`);
    return response.data;
  },

  getByClientId: async (clientId) => {
    const response = await api.get(`/Application/GetByClientId?clientId=${clientId}`);
    return response.data;
  },

  getByTenant: async (tenantId) => {
    const response = await api.get(`/Application/GetByTenant?tenantId=${tenantId}`);
    return response.data;
  },

  create: async (data) => {
    const response = await api.post('/Application/Add', data);
    return response.data;
  },

  update: async (id, data) => {
    const response = await api.post(`/Application/Update?applicationId=${id}`, data);
    return response.data;
  },

  delete: async (id) => {
    const response = await api.post(`/Application/Delete?applicationId=${id}`);
    return response.data;
  },

  regenerateSecret: async (id) => {
    const response = await api.post(`/Application/RegenerateSecret?applicationId=${id}`);
    return response.data;
  },

  validateCredentials: async (clientId, clientSecret) => {
    const response = await api.post('/Application/ValidateCredentials', { clientId, clientSecret });
    return response.data;
  },
};

export default applicationService;
