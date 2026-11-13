using System.Reflection;
using System.Security.Claims;
using Api.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Messaging;
using Shouldly;

namespace Api.Tests;

/// <summary>
/// A reaction runs as its module's service principal, under a policy only that principal holds, so no caller over HTTP reaches it.
/// Proven over every reaction the host registers and every policy an operation names.
/// </summary>
public sealed class ReactionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public void Every_reaction_in_the_solution_is_registered_with_the_outbox_dispatcher()
    {
        var declared = Directory.GetFiles(AppContext.BaseDirectory, "*.dll")
            .Select(TryLoad)
            .OfType<Assembly>()
            .Where(a => a.GetCustomAttributes<AssemblyMetadataAttribute>().Any(m => m.Key == "ProjectKind" && m.Value == "Application"))
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReaction<,>)))
            .ToList();

        declared.ShouldNotBeEmpty();
        declared.Except(Reactions.Select(r => r.ReactionType)).Select(t => t.FullName)
            .ShouldBeEmpty("Each module's registration entry point registers its reactions, or the outbox never delivers to them.");
    }

    [Fact]
    public async Task A_reaction_commands_policy_admits_its_modules_service_principal_alone()
    {
        var authorization = factory.Services.GetRequiredService<IAuthorizationService>();
        var modules = Reactions.Select(r => r.Module).Distinct().ToList();
        var violations = new List<string>();

        foreach (var reaction in Reactions)
        {
            var policy = OperationOf(reaction.CommandType).Policy;
            if (!(await authorization.AuthorizeAsync(ServicePrincipal.For(reaction.Module), policy)).Succeeded)
            {
                violations.Add($"{policy} refuses {ServicePrincipal.IdOf(reaction.Module)}, which runs {reaction.Name}");
            }

            foreach (var other in modules.Where(m => m != reaction.Module))
            {
                if ((await authorization.AuthorizeAsync(ServicePrincipal.For(other), policy)).Succeeded)
                {
                    violations.Add($"{policy} admits {ServicePrincipal.IdOf(other)}");
                }
            }

            if ((await authorization.AuthorizeAsync(EveryCaller(), policy)).Succeeded)
            {
                violations.Add($"{policy} admits a caller holding every permission");
            }
        }

        Reactions.ShouldNotBeEmpty();
        violations.ShouldBeEmpty("A reaction's command is under a policy only its module's service principal holds.");
    }

    [Fact]
    public async Task A_modules_service_principal_holds_no_policy_but_its_reactions()
    {
        var authorization = factory.Services.GetRequiredService<IAuthorizationService>();
        var violations = new List<string>();

        foreach (var module in Reactions.Select(r => r.Module).Distinct())
        {
            var own = Reactions.Where(r => r.Module == module).Select(r => OperationOf(r.CommandType).Policy).ToHashSet();
            foreach (var policy in AllPolicies().Where(p => !own.Contains(p)))
            {
                if ((await authorization.AuthorizeAsync(ServicePrincipal.For(module), policy)).Succeeded)
                {
                    violations.Add($"{ServicePrincipal.IdOf(module)} holds {policy}");
                }
            }
        }

        violations.ShouldBeEmpty("A module's service principal may run its reactions and nothing else.");
    }

    [Fact]
    public void No_route_dispatches_a_reaction_command()
    {
        var commands = Reactions.Select(r => r.CommandType).ToHashSet();

        EndpointRegistrar.Discover()
            .Where(e => commands.Contains(e.Message))
            .Select(e => $"{e.Endpoint.Name} dispatches {e.Message.Name}")
            .ShouldBeEmpty("A reaction's command is dispatched by the outbox only.");
    }

    private IReadOnlyList<Reaction> Reactions => factory.Services.GetRequiredService<ReactionMap>().All;

    /// <summary>Every policy an operation names: each route's, and each reaction command's.</summary>
    private IEnumerable<string> AllPolicies() =>
        EndpointRegistrar.Discover().Select(e => OperationOf(e.Message).Policy)
            .Concat(Reactions.Select(r => OperationOf(r.CommandType).Policy))
            .Distinct();

    /// <summary>A caller as HTTP authentication could present it: in a tenant, holding a permission named for every policy there is.</summary>
    private ClaimsPrincipal EveryCaller() =>
        new(new ClaimsIdentity(
            [
                new Claim("sub", "everyone"),
                new Claim("tenant", ApiFactory.Tenant.ToString()),
                .. AllPolicies().Select(p => new Claim("permissions", p)),
            ],
            "Bearer"));

    private static Operation OperationOf(Type message) =>
        (Operation)message.GetProperty(nameof(IDeclaresOperation.Operation), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

    private static Assembly? TryLoad(string path)
    {
        try
        {
            return Assembly.LoadFrom(path);
        }
        catch (BadImageFormatException)
        {
            return null;
        }
    }
}
