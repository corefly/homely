using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Homely.Identity.Api.Database;

public class IdentityContext(DbContextOptions<IdentityContext> options) : IdentityDbContext(options);

public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
        optionsBuilder.UseSqlite("Data Source=identity.db");

        return new IdentityContext(optionsBuilder.Options);
    }
}
