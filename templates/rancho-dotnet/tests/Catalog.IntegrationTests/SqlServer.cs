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
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(Catalog.IntegrationTests.SqlServer))]

namespace Catalog.IntegrationTests;

/// <summary>
/// One real SQL Server for the whole run, never an in-memory stand-in, with the module's migrations applied:
/// twice, to prove a second run changes nothing.
/// </summary>
public sealed class SqlServer : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public CatalogApi Api { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = new SqlConnectionStringBuilder(_container.GetConnectionString()) { InitialCatalog = "Catalog" }.ConnectionString;
        Api = new CatalogApi(ConnectionString);

        await Migrate();
        await Migrate();
    }

    public async Task Migrate()
    {
        using var scope = Api.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    }

    /// <summary>Reads the database directly: what the tests assert is what was committed.</summary>
    public async Task<T?> Scalar<T>(FormattableString sql)
    {
        using var scope = Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await db.Database.SqlQuery<T>(sql).SingleOrDefaultAsync();
    }

    public async Task Execute(FormattableString sql)
    {
        using var scope = Api.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.ExecuteSqlAsync(sql);
    }

    public async ValueTask DisposeAsync()
    {
        await Api.DisposeAsync();
        await _container.DisposeAsync();
    }
}

/// <summary>The real host on the real database, with a test authentication scheme that reads the caller from headers.</summary>
public sealed class CatalogApi(string connectionString) : WebApplicationFactory<Program>
{
    private const string Subject = "X-Test-Subject";
    private const string Permissions = "X-Test-Permissions";

    public HttpClient ClientFor(string subject, params string[] permissions)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Subject, subject);
        client.DefaultRequestHeaders.Add(Permissions, string.Join(',', permissions));
        return client;
    }

    public static ClaimsPrincipal Principal(string subject, params string[] permissions) =>
        new(new ClaimsIdentity(
            [new Claim("sub", subject), .. permissions.Select(p => new Claim("permissions", p))],
            HeaderAuthentication.SchemeName));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Catalog", connectionString);
        builder.UseSetting("RateLimits:PermitLimit", "100000");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, HeaderAuthentication>(HeaderAuthentication.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = HeaderAuthentication.SchemeName;
                options.DefaultAuthenticateScheme = HeaderAuthentication.SchemeName;
                options.DefaultChallengeScheme = HeaderAuthentication.SchemeName;
            });
        });
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
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Principal(subject!, permissions), SchemeName)));
        }
    }
}
