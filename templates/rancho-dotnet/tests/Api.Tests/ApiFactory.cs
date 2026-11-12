using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Tests;

/// <summary>The real host, with one change: a test authentication scheme that reads the caller from request headers.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string Subject = "X-Test-Subject";
    public const string Permissions = "X-Test-Permissions";

    /// <summary>Every registration the application made, as the host saw it.</summary>
    public IReadOnlyList<ServiceDescriptor> Registrations { get; private set; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            Registrations = [.. services];
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, HeaderAuthentication>(HeaderAuthentication.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = HeaderAuthentication.SchemeName;
                options.DefaultAuthenticateScheme = HeaderAuthentication.SchemeName;
                options.DefaultChallengeScheme = HeaderAuthentication.SchemeName;
            });
        });
    }

    /// <summary>The tenant every test principal belongs to.</summary>
    public static readonly Guid Tenant = Guid.Parse("7a1c2d3e-0000-4000-8000-000000000001");

    public static ClaimsPrincipal Principal(string subject, params string[] permissions) =>
        new(new ClaimsIdentity(
            [new Claim("sub", subject), new Claim("tenant", Tenant.ToString()), .. permissions.Select(p => new Claim("permissions", p))],
            HeaderAuthentication.SchemeName));

    public HttpClient ClientFor(string subject, params string[] permissions)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(Subject, subject);
        client.DefaultRequestHeaders.Add(Permissions, string.Join(',', permissions));
        return client;
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
