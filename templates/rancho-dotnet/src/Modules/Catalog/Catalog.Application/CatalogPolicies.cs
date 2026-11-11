namespace Catalog.Application;

/// <summary>The module's policy names. Which claims grant each is decided at the composition root.</summary>
public static class CatalogPolicies
{
    public const string Read = "catalog.read";
    public const string Write = "catalog.write";
}
