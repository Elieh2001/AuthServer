-- Update existing admin user to have proper roles
UPDATE Users
SET Roles = 'SuperAdmin,Admin',
    IsSystemAdmin = 1,
    TenantId = NULL
WHERE Email = 'admin@example.com';

-- Or create a new system admin if the above doesn't work
-- First check if system admin exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'sysadmin@authserver.com')
BEGIN
    INSERT INTO Users (Id, TenantId, Email, EmailVerified, PasswordHash, FirstName, LastName, IsSystemAdmin, Roles, IsActive, LockoutEnabled, TwoFactorEnabled, PhoneNumber, LastLoginIp, SecurityStamp, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
    VALUES (
        NEWID(),
        NULL,
        'sysadmin@authserver.com',
        1,
        '$2a$11$FvQJ3QGxHqKxZz8ZvqJZeO7vK1xHWxGxGxHxGxHxGxHxGxHxGxHxG', -- You need to hash 'SysAdmin@123456'
        'System',
        'Admin',
        1,
        'SuperAdmin,Admin',
        1,
        1,
        0,
        '',
        '',
        NEWID(),
        GETUTCDATE(),
        GETUTCDATE(),
        NULL,
        NULL
    );
END

-- Verify the changes
SELECT Id, Email, TenantId, Roles, IsSystemAdmin, FirstName, LastName
FROM Users
WHERE Email IN ('admin@example.com', 'sysadmin@authserver.com');
