import api from './api';
import { AuditLogDto } from '../models';

const auditService = {
  getAll: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get(`/Audit?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    if (response.data.success) {
      return {
        logs: response.data.data.items.map(log => new AuditLogDto(log)),
        totalCount: response.data.data.totalCount,
        pageNumber: response.data.data.pageNumber,
        pageSize: response.data.data.pageSize,
        totalPages: response.data.data.totalPages,
      };
    }
    throw new Error(response.data.message || 'Failed to fetch audit logs');
  },

  getByTenant: async (tenantId, pageNumber = 1, pageSize = 10) => {
    const response = await api.get(`/Audit/tenant/${tenantId}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    if (response.data.success) {
      return {
        logs: response.data.data.items.map(log => new AuditLogDto(log)),
        totalCount: response.data.data.totalCount,
        pageNumber: response.data.data.pageNumber,
        pageSize: response.data.data.pageSize,
        totalPages: response.data.data.totalPages,
      };
    }
    throw new Error(response.data.message || 'Failed to fetch audit logs');
  },

  getByUser: async (userId, pageNumber = 1, pageSize = 10) => {
    const response = await api.get(`/Audit/user/${userId}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    if (response.data.success) {
      return {
        logs: response.data.data.items.map(log => new AuditLogDto(log)),
        totalCount: response.data.data.totalCount,
        pageNumber: response.data.data.pageNumber,
        pageSize: response.data.data.pageSize,
        totalPages: response.data.data.totalPages,
      };
    }
    throw new Error(response.data.message || 'Failed to fetch audit logs');
  },

  getByAction: async (action, pageNumber = 1, pageSize = 10) => {
    const response = await api.get(`/Audit/action/${action}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    if (response.data.success) {
      return {
        logs: response.data.data.items.map(log => new AuditLogDto(log)),
        totalCount: response.data.data.totalCount,
        pageNumber: response.data.data.pageNumber,
        pageSize: response.data.data.pageSize,
        totalPages: response.data.data.totalPages,
      };
    }
    throw new Error(response.data.message || 'Failed to fetch audit logs');
  },

  getByDateRange: async (startDate, endDate, tenantId = null, pageNumber = 1, pageSize = 10) => {
    let url = `/Audit/daterange?startDate=${startDate}&endDate=${endDate}&pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (tenantId) {
      url += `&tenantId=${tenantId}`;
    }
    const response = await api.get(url);
    if (response.data.success) {
      return {
        logs: response.data.data.items.map(log => new AuditLogDto(log)),
        totalCount: response.data.data.totalCount,
        pageNumber: response.data.data.pageNumber,
        pageSize: response.data.data.pageSize,
        totalPages: response.data.data.totalPages,
      };
    }
    throw new Error(response.data.message || 'Failed to fetch audit logs');
  },
};

export default auditService;
