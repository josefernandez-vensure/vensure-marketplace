namespace Platform.Application.Abstractions;

/// <summary>
/// Declared once, on the composition root (<c>[assembly: NoTenantScope(...)]</c> in <c>Api</c>), by a service whose policies grant every
/// record to every principal they admit. Its reads then have no tenant to apply, and its aggregates no tenant to store, so the
/// architecture tests that require one are skipped. Name the decision that made the service global, and why.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class NoTenantScopeAttribute(string decision, string reason) : Attribute
{
    /// <summary>The decision that made access global, such as an entry in the project's <c>DECISIONS.md</c>.</summary>
    public string Decision { get; } = decision;

    public string Reason { get; } = reason;
}
