using Homely.Identity.Api.Database;
using Homely.Identity.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Homely.Identity.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().ConfigureApplicationPartManager(x =>
        {
            x.ApplicationParts.Add(new AssemblyPart(typeof(DependencyInjection).Assembly));
        });

        services.AddAuthorization();
        services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("Identity"));
        });

        services.AddIdentityCore<IdentityUser>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddApiEndpoints();

        return services;
    }

    public static void UseIdentityModule(this WebApplication app)
    {
        app.UseAuthorization();
        app.UseAuthentication();

        app.MapGroup("/api/identity")
            .MapIdentityApi<IdentityUser>();

        if (app.Environment.IsDevelopment())
        {
            app.ApplyMigrations();
        }
    }
}
