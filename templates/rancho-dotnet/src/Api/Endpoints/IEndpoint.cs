using Platform.Application.Abstractions;

namespace Api.Endpoints;

/// <summary>
/// An endpoint: it maps its request to <typeparamref name="TMessage"/>, dispatches it, and translates the result to HTTP.
/// <see cref="EndpointRegistrar"/> discovers it and applies the message's policy, rate limit, and operation id;
/// an endpoint declares none of them itself.
/// </summary>
public interface IEndpoint<TMessage>
    where TMessage : IDeclaresOperation
{
    static abstract string Method { get; }

    static abstract string Pattern { get; }

    static abstract Delegate Handler { get; }
}

/// <summary>Marks a route the registrar mapped, and the endpoint type and operation behind it.</summary>
public sealed record DiscoveredEndpoint(Type EndpointType, Operation Operation);
