namespace Platform.Infrastructure.Persistence;

/// <summary>The service's one database. Every module's context connects to it, each on its own schema.</summary>
internal sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}
