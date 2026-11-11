using Catalog.Application;
using Catalog.Domain.Products;
using Catalog.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Catalog")
            ?? throw new InvalidOperationException("Connection string 'Catalog' is missing.");

        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddValidatorsFromAssembly(typeof(CatalogPolicies).Assembly, ServiceLifetime.Scoped);

        // Claims map to policies here, at the composition root. Application code names only the policy.
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(CatalogPolicies.Read, policy => policy.RequireAuthenticatedUser().RequireClaim("permissions", CatalogPolicies.Read));
            options.AddPolicy(CatalogPolicies.Write, policy => policy.RequireAuthenticatedUser().RequireClaim("permissions", CatalogPolicies.Write));
        });

        return services;
    }
}
