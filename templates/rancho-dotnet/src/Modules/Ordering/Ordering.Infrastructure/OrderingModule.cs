using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application;
using Ordering.Domain.Orders;
using Ordering.Infrastructure.Persistence;
using Platform.Infrastructure;

namespace Ordering.Infrastructure;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<OrderingDbContext>(OrderingDbContext.SchemaName);
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddValidatorsFromAssembly(typeof(OrderingPolicies).Assembly, ServiceLifetime.Scoped);
        services.AddReactions(typeof(OrderingPolicies).Assembly);

        // Claims map to policies here, at the composition root. Application code names only the policy.
        services.AddAuthorizationCore(options =>
        {
            // Orders are a tenant's: a principal with no tenant holds no user policy.
            options.AddPolicy(OrderingPolicies.Read, policy => policy.RequireAuthenticatedUser().RequireClaim("tenant").RequireClaim("permissions", OrderingPolicies.Read));
            options.AddPolicy(OrderingPolicies.Write, policy => policy.RequireAuthenticatedUser().RequireClaim("tenant").RequireClaim("permissions", OrderingPolicies.Write));
            options.AddPolicy(OrderingPolicies.React, policy => policy.RequireServicePrincipal(OrderingDbContext.ModuleName));
        });

        return services;
    }
}
