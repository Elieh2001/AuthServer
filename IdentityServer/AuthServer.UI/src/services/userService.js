import api from './api';
import { UserDto, CreateUserDto, UpdateUserDto } from '../models';

const userService = {
  getAll: async (tenantId, page = 1, pageSize = 100) => {
    const response = await api.get(`/User/GetByTenant?tenantId=${tenantId}&page=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  getById: async (id) => {
    const response = await api.get(`/User/GetById?userId=${id}`);
    return response.data;
  },

  getByEmail: async (email, tenantId) => {
    const response = await api.get(`/User/GetByEmail?email=${email}&tenantId=${tenantId}`);
    return response.data;
  },

  create: async (data) => {
    const response = await api.post('/User/Add', data);
    return response.data;
  },

  update: async (id, data) => {
    const response = await api.post(`/User/Update?userId=${id}`, data);
    return response.data;
  },

  delete: async (id) => {
    const response = await api.post(`/User/Delete?userId=${id}`);
    return response.data;
  },

  lock: async (id, durationMinutes) => {
    const response = await api.post(`/User/Lock?userId=${id}`, { durationMinutes });
    return response.data;
  },

  unlock: async (id) => {
    const response = await api.post(`/User/Unlock?userId=${id}`);
    return response.data;
  },

  linkProvider: async (userId, data) => {
    const response = await api.post(`/User/LinkProvider?userId=${userId}`, data);
    return response.data;
  },

  unlinkProvider: async (userId, provider) => {
    const response = await api.post(`/User/UnlinkProvider?userId=${userId}&provider=${provider}`);
    return response.data;
  },
};

export default userService;
