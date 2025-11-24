export const isSystemAdmin = (user) => {
  return user?.isSystemAdmin === true || user?.tenantId === null;
};

export const isTenantAdmin = (user) => {
  return user?.tenantId !== null && !user?.isSystemAdmin;
};

export const canManageTenants = (user) => {
  return isSystemAdmin(user);
};

export const canManageApplications = (user) => {
  return isSystemAdmin(user) || isTenantAdmin(user);
};

export const canManageUsers = (user) => {
  return isSystemAdmin(user) || isTenantAdmin(user);
};

export const canViewAllUsers = (user) => {
  return isSystemAdmin(user);
};

export const getUserRole = (user) => {
  if (isSystemAdmin(user)) {
    return 'System Admin';
  }
  if (isTenantAdmin(user)) {
    return 'Tenant Admin';
  }
  return 'User';
};
