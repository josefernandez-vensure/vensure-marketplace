using System.Reflection;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Messaging;

/// <summary>An assembly whose reactions a module registered.</summary>
internal sealed record ReactionAssembly(Assembly Assembly);

/// <summary>One reaction: the message it answers, the command it maps it to, and the module whose service principal dispatches it.</summary>
internal sealed record Reaction(Type ReactionType, Type MessageType, Type CommandType, string Module, Func<object, object> Map)
{
    public string Name => ReactionType.Name;
}

/// <summary>Every registered reaction, by the full name of the message it answers, as an outbox row records it.</summary>
internal sealed class ReactionMap
{
    private readonly ILookup<string, Reaction> _byMessage;

    public ReactionMap(IEnumerable<ReactionAssembly> assemblies)
    {
        All = assemblies.Select(a => a.Assembly).Distinct().SelectMany(Discover).ToList();
        _byMessage = All.ToLookup(r => r.MessageType.FullName!);
    }

    public IReadOnlyList<Reaction> All { get; }

    public IReadOnlyList<Reaction> For(string messageType) => [.. _byMessage[messageType]];

    private static IEnumerable<Reaction> Discover(Assembly assembly)
    {
        var module = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "Module")?.Value;
        var bind = typeof(ReactionMap).GetMethod(nameof(Bind), BindingFlags.NonPublic | BindingFlags.Static)!;
        foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var reaction in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReaction<,>)))
            {
                if (string.IsNullOrEmpty(module))
                {
                    throw new InvalidOperationException($"{type.FullName} is a reaction outside a module.");
                }

                var arguments = reaction.GetGenericArguments();
                var map = (Func<object, object>)bind.MakeGenericMethod(type, arguments[0], arguments[1]).Invoke(null, null)!;
                yield return new Reaction(type, arguments[0], arguments[1], module, map);
            }
        }
    }

    private static Func<object, object> Bind<TReaction, TMessage, TCommand>()
        where TReaction : IReaction<TMessage, TCommand>
        where TCommand : Mediator.IBaseCommand, IDeclaresOperation =>
        message => TReaction.Map((TMessage)message);
}
