using Api.Endpoints;
using Api.Http;
using Catalog.Application;
using Catalog.Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Platform.Application.Behaviors;
using Platform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddHttpPlatform();
builder.Services.AddPlatformPersistence();

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
        typeof(UnitOfWorkBehavior<,>),
    ];
});

builder.Services.AddCatalogModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    // A framework endpoint, mapped outside discovery and allowed explicitly.
    app.MapOpenApi().AllowAnonymous();
}

app.MapDiscoveredEndpoints();

await app.RunAsync();

/// <summary>Visible to the API tests' host.</summary>
public partial class Program;
