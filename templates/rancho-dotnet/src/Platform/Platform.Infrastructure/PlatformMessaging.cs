using System.Reflection;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Messaging;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure;

public static class PlatformMessaging
{
    /// <summary>
    /// The dispatch context of every scope: the one the outbox dispatcher entered for a reaction, or else the one the composition root
    /// builds from the request. Called once, at the composition root.
    /// </summary>
    public static IServiceCollection AddDispatchContext(this IServiceCollection services, Func<IServiceProvider, DispatchContext> fromRequest)
    {
        services.AddScoped<DispatchScope>();
        services.AddScoped(sp => sp.GetRequiredService<DispatchScope>().Context ?? fromRequest(sp));
        return services;
    }

    /// <summary>
    /// The outbox dispatcher, and the hosted service that runs it on <c>Outbox:PollInterval</c>, <c>Outbox:BatchSize</c> messages a module
    /// at a time. A missing or unusable setting stops startup.
    /// </summary>
    public static IServiceCollection AddOutboxDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.Section))
            .Validate(o => o.PollInterval > TimeSpan.Zero && o.BatchSize is > 0 and <= 1000, "Outbox delivery needs a positive poll interval and a batch size of 1 to 1000.")
            .ValidateOnStart();
        services.AddSingleton<ReactionMap>();
        services.AddSingleton<OutboxDispatcher>();
        services.AddSingleton<OpenDeliveryScope>(root => context =>
        {
            var scope = root.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            if (context is not null)
            {
                scope.ServiceProvider.GetRequiredService<DispatchScope>().Enter(context);
            }

            return new DeliveryScope(scope, scope.ServiceProvider.GetServices<ModuleDbContext>(), scope.ServiceProvider.GetRequiredService<ISender>());
        });
        services.AddHostedService<OutboxDelivery>();
        return services;
    }

    /// <summary>The reactions a module's Application declares. Each module's registration entry point calls it for its own assembly.</summary>
    public static IServiceCollection AddReactions(this IServiceCollection services, Assembly application)
    {
        services.AddSingleton(new ReactionAssembly(application));
        return services;
    }

    /// <summary>
    /// A policy only <paramref name="module"/>'s service principal holds: the principal its reactions run as. No caller over HTTP can hold it.
    /// </summary>
    public static AuthorizationPolicyBuilder RequireServicePrincipal(this AuthorizationPolicyBuilder policy, string module) =>
        policy.RequireAssertion(context => ServicePrincipal.Is(context.User, module));
}
