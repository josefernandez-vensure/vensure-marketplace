using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure;

public static class PlatformPersistence
{
    /// <summary>The name of the one connection string, <c>ConnectionStrings:Database</c>: the service's one database, which every module shares.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>
    /// The service's one database, the unit of work and the outbox. Called once, at the composition root, with the one connection string.
    /// A missing connection string stops startup.
    /// </summary>
    public static IServiceCollection AddPlatformPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddOptions<DatabaseOptions>()
            .Configure(options => options.ConnectionString = connectionString ?? string.Empty)
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), $"ConnectionStrings:{ConnectionStringName} is missing; every module's schema lives in that one database.")
            .ValidateOnStart();
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

    /// <summary>
    /// A module's one DbContext, on its own schema in the service's one database, with its migrations history in that schema too.
    /// It takes no connection string: every module connects where the composition root said.
    /// </summary>
    public static IServiceCollection AddModuleDbContext<TContext>(this IServiceCollection services, string schema)
        where TContext : ModuleDbContext
    {
        services.AddDbContext<TContext>((provider, options) => options.UseSqlServer(
            provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema)));
        services.AddScoped<ModuleDbContext>(sp => sp.GetRequiredService<TContext>());
        return services;
    }
}
