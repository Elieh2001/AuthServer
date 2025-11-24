import api from './api';
import { TenantDto, CreateTenantDto, UpdateTenantDto } from '../models';

const tenantService = {
  getAll: async () => {
    const response = await api.get('/Tenant/GetAll');
    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/Tenant/GetById?tenantId=${id}`);
    return response.data;
  },

  getBySubdomain: async (subdomain) => {
    const response = await api.get(`/Tenant/GetBySubdomain?subdomain=${subdomain}`);
    return response.data;
  },

  create: async (data) => {
    const response = await api.post('/Tenant/Add', data);
    return response.data;
  },

  update: async (id, data) => {
    const response = await api.post(`/Tenant/Update?tenantId=${id}`, data);
    return response.data;
  },

  delete: async (id) => {
    const response = await api.post(`/Tenant/Delete?tenantId=${id}`);
    return response.data;
  },

  activate: async (id) => {
    const response = await api.post(`/Tenant/Activate?tenantId=${id}`);
    return response.data;
  },

  suspend: async (id) => {
    const response = await api.post(`/Tenant/Suspend?tenantId=${id}`);
    return response.data;
  },
};

export default tenantService;
