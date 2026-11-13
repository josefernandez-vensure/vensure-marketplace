using System.Security.Claims;
using System.Text.Encodings.Web;
using Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Infrastructure.Messaging;
using Platform.Infrastructure.Persistence;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(Catalog.IntegrationTests.SqlServer))]

namespace Catalog.IntegrationTests;

/// <summary>
/// One real SQL Server and the service's one database for the whole run, never an in-memory stand-in, with every module's migrations applied:
/// twice, to prove a second run changes nothing. The server is a container of this run's own, or, when <see cref="SharedServerVariable"/>
/// names one, that server, with a database of this run's own that is dropped at the end.
/// </summary>
public sealed class SqlServer : IAsyncLifetime
{
    /// <summary>A connection string to a server that several runs share, such as the seed proof's; its database name is ignored.</summary>
    public const string SharedServerVariable = "RANCHO_TEST_SQL";

    private MsSqlContainer? _container;
    private string? _sharedServer;

    public string ConnectionString { get; private set; } = string.Empty;

    public CatalogApi Api { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _sharedServer = Environment.GetEnvironmentVariable(SharedServerVariable) is { Length: > 0 } shared ? shared : null;
        if (_sharedServer is null)
        {
            _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await _container.StartAsync();
        }

        ConnectionString = new SqlConnectionStringBuilder(_sharedServer ?? _container!.GetConnectionString())
        {
            InitialCatalog = _sharedServer is null ? "Starter" : $"Starter_{Guid.NewGuid():N}",
        }.ConnectionString;
        Api = new CatalogApi(ConnectionString);

        await Migrate();
        await Migrate();
    }

    /// <summary>Every module's migrations, each into its own schema of the one database.</summary>
    public async Task Migrate()
    {
        using var scope = Api.Services.CreateScope();
        foreach (var module in scope.ServiceProvider.GetServices<Platform.Infrastructure.Persistence.ModuleDbContext>())
        {
            await module.Database.MigrateAsync();
        }
    }

    /// <summary>Reads the database directly: what the tests assert is what was committed.</summary>
    public async Task<T?> Scalar<T>(FormattableString sql)
    {
        using var scope = Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await db.Database.SqlQuery<T>(sql).SingleOrDefaultAsync();
    }

    /// <summary>
    /// Delivers the outbox until a pass delivers nothing more, as the hosted service would on its ticks. The shared host's own delivery
    /// never ticks during the run, so a test that dispatches decides when its messages are delivered.
    /// </summary>
    public async Task DeliverOutbox()
    {
        var dispatcher = Api.Services.GetRequiredService<OutboxDispatcher>();
        while (await dispatcher.DeliverPending(100, TestContext.Current.CancellationToken) > 0)
        {
        }
    }

    /// <summary>Denials recorded in every module's audit trail: each module records the refusals of its own operations.</summary>
    public async Task<int> CountDenied(string subject)
    {
        using var scope = Api.Services.CreateScope();
        var count = 0;
        foreach (var module in scope.ServiceProvider.GetServices<ModuleDbContext>())
        {
            count += await module.Set<AuditRecord>().CountAsync(a => a.PrincipalId == subject && a.Outcome == "Denied", TestContext.Current.CancellationToken);
        }

        return count;
    }

    public async Task Execute(FormattableString sql)
    {
        using var scope = Api.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.ExecuteSqlAsync(sql);
    }

    public async ValueTask DisposeAsync()
    {
        await Api.DisposeAsync();
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }

        if (_sharedServer is not null)
        {
            await DropDatabase();
        }
    }

    /// <summary>A shared server keeps only what its runs are using: this run's database goes with it.</summary>
    private async Task DropDatabase()
    {
        SqlConnection.ClearAllPools();
        var database = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
        await using var master = new SqlConnection(new SqlConnectionStringBuilder(_sharedServer!) { InitialCatalog = "master" }.ConnectionString);
        await master.OpenAsync();
        await using var drop = master.CreateCommand();
        drop.CommandText = $"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}];";
        await drop.ExecuteNonQueryAsync();
    }
}

/// <summary>The real host on the real database, with a test authentication scheme that reads the caller from headers.</summary>
public sealed class CatalogApi(string connectionString) : WebApplicationFactory<Program>
{
    /// <summary>The tenant a test principal belongs to unless the test names another.</summary>
    public static readonly Guid DefaultTenant = Guid.Parse("7a1c2d3e-0000-4000-8000-000000000001");

    /// <summary>Every line the host logged, with the trace it was logged in.</summary>
    public CapturedLogs Logs { get; } = new();

    private const string Subject = "X-Test-Subject";
    private const string Permissions = "X-Test-Permissions";
    private const string Tenant = "X-Test-Tenant";

    public HttpClient ClientFor(string subject, params string[] permissions) => ClientFor(subject, DefaultTenant, permissions);

    /// <summary>A caller in the given tenant; <see cref="Guid.Empty"/> is a caller with no tenant at all.</summary>
    public HttpClient ClientFor(string subject, Guid tenant, params string[] permissions)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Subject, subject);
        client.DefaultRequestHeaders.Add(Permissions, string.Join(',', permissions));
        client.DefaultRequestHeaders.Add(Tenant, tenant.ToString());
        return client;
    }

    public static ClaimsPrincipal Principal(string subject, params string[] permissions) => Principal(subject, DefaultTenant, permissions);

    public static ClaimsPrincipal Principal(string subject, Guid tenant, params string[] permissions) =>
        new(new ClaimsIdentity(
            [
                new Claim("sub", subject),
                .. permissions.Select(p => new Claim("permissions", p)),
                .. tenant == Guid.Empty ? Array.Empty<Claim>() : [new Claim("tenant", tenant.ToString())],
            ],
            HeaderAuthentication.SchemeName));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", connectionString);
        builder.UseSetting("RateLimits:PermitLimit", "100000");

        // The shared host never delivers on its own; tests deliver through SqlServer.DeliverOutbox.
        builder.UseSetting("Outbox:PollInterval", "1.00:00:00");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ILoggerProvider>(Logs);
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, HeaderAuthentication>(HeaderAuthentication.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = HeaderAuthentication.SchemeName;
                options.DefaultAuthenticateScheme = HeaderAuthentication.SchemeName;
                options.DefaultChallengeScheme = HeaderAuthentication.SchemeName;
            });
        });
    }

    public sealed class CapturedLogs : ILoggerProvider
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<(string Category, string Message, System.Diagnostics.ActivityTraceId? TraceId)> _lines = new();

        public IEnumerable<(string Category, string Message, System.Diagnostics.ActivityTraceId? TraceId)> Lines => _lines;

        public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _lines);

        public void Dispose()
        {
        }

        private sealed class Logger(string category, System.Collections.Concurrent.ConcurrentQueue<(string, string, System.Diagnostics.ActivityTraceId?)> lines) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                lines.Enqueue((category, formatter(state, exception), System.Diagnostics.Activity.Current?.TraceId));
        }
    }

    private sealed class HeaderAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(Subject, out var subject))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var permissions = Request.Headers[Permissions].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            var tenant = Guid.TryParse(Request.Headers[Tenant], out var parsed) ? parsed : DefaultTenant;
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Principal(subject!, tenant, permissions), SchemeName)));
        }
    }
}
