-- Update the existing admin user to have the correct roles
UPDATE Users
SET
    Roles = 'SuperAdmin,Admin',
    IsSystemAdmin = 1,
    TenantId = NULL
WHERE Email = 'admin@example.com';

-- Verify the update
SELECT
    Id,
    Email,
    TenantId,
    Roles,
    IsSystemAdmin,
    FirstName,
    LastName,
    IsActive
FROM Users
WHERE Email = 'admin@example.com';

-- This should show:
-- - TenantId: NULL
-- - Roles: SuperAdmin,Admin
-- - IsSystemAdmin: 1 (true)
