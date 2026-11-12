namespace Platform.Application.Abstractions;

/// <summary>
/// A command that writes an existing aggregate. It carries the version the client last read, from <c>If-Match</c>;
/// the pipeline refuses it without one (428) and when the aggregate has moved on (412), before anything commits.
/// </summary>
public interface IVersionedCommand
{
    ExpectedVersion ExpectedVersion { get; }
}

/// <summary>What an <c>If-Match</c> header asks for: a version, nothing usable (missing or <c>*</c>), or a tag that can never match.</summary>
public readonly record struct ExpectedVersion(long? Version, bool Unmatchable = false)
{
    /// <summary>Missing, or <c>*</c>: the write is refused with 428.</summary>
    public static ExpectedVersion None { get; } = new(null);

    public bool IsMissing => Version is null && !Unmatchable;

    public bool Matches(long version) => Version == version;

    /// <summary>A strong entity tag of the aggregate's version, such as <c>"7"</c>. A weak or foreign tag can never match.</summary>
    public static ExpectedVersion FromIfMatch(string? header)
    {
        var value = header?.Trim();
        if (string.IsNullOrEmpty(value) || value == "*")
        {
            return None;
        }

        return value.Length > 2 && value[0] == '"' && value[^1] == '"' && long.TryParse(value.AsSpan(1, value.Length - 2), out var version)
            ? new(version)
            : new(null, Unmatchable: true);
    }
}
