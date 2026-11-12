using Api.Endpoints;
using Api.Http;
using Api.OpenApi;
using Api.Telemetry;
using Catalog.Application;
using Catalog.Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Platform.Application.Behaviors;
using Platform.Infrastructure;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddContractDocument();
builder.AddTracing();

// Logs redact by data class: Public and Internal pass; Confidential, Restricted and any other class are erased.
builder.Logging.EnableRedaction();
builder.Services.AddRedaction(redaction => redaction
    .SetRedactor<NullRedactor>(new DataClassificationSet(DataClasses.Public), new DataClassificationSet(DataClasses.Internal))
    .SetFallbackRedactor<ErasingRedactor>());
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddHttpPlatform();
builder.Services.AddPlatformPersistence(builder.Configuration.GetConnectionString(PlatformPersistence.ConnectionStringName));
builder.Services.AddPlatformPaging();

builder.Services.AddMediator((MediatorOptions options) =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(CatalogPolicies), typeof(CatalogModule)];

    // The pipeline, in this order. Changing it needs a reason in the PR.
    options.PipelineBehaviors =
    [
        typeof(LoggingBehavior<,>),
        typeof(AuthorizationBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(PreconditionBehavior<,>),
        typeof(PagingBehavior<,>),
        typeof(UnitOfWorkBehavior<,>),
        typeof(HandlerSpanBehavior<,>),
    ];
});

builder.Services.AddCatalogModule(builder.Configuration);

var app = builder.Build();

app.UseTraceBoundary();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();

// Before authorization, so a refused request still counts against the caller's limit.
app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // A framework endpoint, mapped outside discovery and allowed explicitly.
    app.MapOpenApi().AllowAnonymous();
}

app.MapDiscoveredEndpoints();

await app.RunAsync();

/// <summary>Visible to the API tests' host.</summary>
public partial class Program;
