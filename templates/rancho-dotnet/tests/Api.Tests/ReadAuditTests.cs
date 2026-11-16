using Api.Endpoints;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Behaviors;
using Shouldly;

namespace Api.Tests;

/// <summary>
/// Core §4: a read of Restricted data, and by default of Confidential, is audited. The behavior decides by the class of what a query returns
/// (Platform.Tests proves that); this proves every query in the host passes through it.
/// </summary>
public sealed class ReadAuditTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public void Every_query_passes_the_read_audit()
    {
        _ = factory.Server;
        var queries = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .Select(e => e.Metadata.GetMetadata<DiscoveredEndpoint>()?.MessageType)
            .OfType<Type>()
            .Select(m => (Message: m, Response: m.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Mediator.IQuery<>))?.GetGenericArguments()[0]))
            .Where(q => q.Response is not null)
            .Distinct()
            .ToList();

        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext { User = ApiFactory.Principal("auditor") };

        queries.ShouldNotBeEmpty();
        queries
            .Where(q => !scope.ServiceProvider.GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(q.Message, q.Response!))
                .Any(b => b!.GetType().IsGenericType && b.GetType().GetGenericTypeDefinition() == typeof(ReadAuditBehavior<,>)))
            .Select(q => q.Message.Name)
            .ShouldBeEmpty("every query's pipeline includes the read audit");
    }
}
