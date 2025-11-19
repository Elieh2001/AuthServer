using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthServer.Infrastructure.Data;

public class AuthServerDbContextFactory : IDesignTimeDbContextFactory<AuthServerDbContext>
{
    public AuthServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthServerDbContext>();

        // Connection string for design-time (migrations)
        optionsBuilder.UseSqlServer(
            "Server=DESKTOP-D1D0JO4\\SQLEXPRESS;Database=AuthServerDb;Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=true"
        );

        // Pass null for tenantId to disable tenant filtering during migrations
        return new AuthServerDbContext(optionsBuilder.Options);
    }
}