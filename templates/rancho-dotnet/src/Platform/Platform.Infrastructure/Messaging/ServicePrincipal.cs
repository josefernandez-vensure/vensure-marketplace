using System.Security.Claims;

namespace Platform.Infrastructure.Messaging;

/// <summary>
/// A module's own identity, under which its reactions run. Only the outbox dispatcher builds one, and no authentication handler can:
/// the type is this assembly's, so a policy that requires it is held by that principal alone.
/// </summary>
internal sealed class ServiceIdentity(string module) : ClaimsIdentity([new Claim("sub", ServicePrincipal.IdOf(module))], ServicePrincipal.AuthenticationType)
{
    public string Module { get; } = module;
}

internal static class ServicePrincipal
{
    public const string AuthenticationType = "Service";

    /// <summary>The principal id a module's reactions are audited and keyed under, such as <c>service:catalog</c>.</summary>
    public static string IdOf(string module) => $"service:{module.ToLowerInvariant()}";

    public static ClaimsPrincipal For(string module) => new(new ServiceIdentity(module));

    public static bool Is(ClaimsPrincipal user, string module) =>
        user.Identities.Any(identity => identity is ServiceIdentity service && service.Module == module);
}
