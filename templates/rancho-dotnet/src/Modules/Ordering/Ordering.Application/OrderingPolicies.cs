namespace Ordering.Application;

/// <summary>The module's policy names. Which claims grant each is decided at the composition root.</summary>
public static class OrderingPolicies
{
    public const string Read = "ordering.read";
    public const string Write = "ordering.write";

    /// <summary>Held only by the Ordering module's service principal: its reactions' commands, which no caller over HTTP reaches.</summary>
    public const string React = "ordering.react";
}
