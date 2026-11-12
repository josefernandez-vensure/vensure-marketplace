using Microsoft.AspNetCore.DataProtection;
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

    /// <summary>
    /// Cursors, sealed with Data Protection. Where keys persist is the composition root's choice:
    /// a deployment with more than one instance shares them, such as in blob storage protected by Key Vault.
    /// </summary>
    public static IServiceCollection AddPlatformPaging(this IServiceCollection services)
    {
        services.AddDataProtection();
        services.AddSingleton<ICursorCodec, CursorCodec>();
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
