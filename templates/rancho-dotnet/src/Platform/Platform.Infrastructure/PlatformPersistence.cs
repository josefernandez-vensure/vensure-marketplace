using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure;

public static class PlatformPersistence
{
    /// <summary>The unit of work and the outbox. Called once, at the composition root.</summary>
    public static IServiceCollection AddPlatformPersistence(this IServiceCollection services)
    {
        services.AddScoped<UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());
        services.AddScoped<IOutbox, Outbox>();
        return services;
    }

    /// <summary>A module's one DbContext, on its own schema, with its migrations history there too.</summary>
    public static IServiceCollection AddModuleDbContext<TContext>(this IServiceCollection services, string connectionString, string schema)
        where TContext : ModuleDbContext
    {
        services.AddDbContext<TContext>(options => options.UseSqlServer(
            connectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema)));
        services.AddScoped<ModuleDbContext>(sp => sp.GetRequiredService<TContext>());
        return services;
    }
}
