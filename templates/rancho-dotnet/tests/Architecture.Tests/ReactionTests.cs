using Platform.Application.Abstractions;
using SharedKernel;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

/// <summary>
/// A reaction is a mapping, by construction (<see cref="IReaction{TMessage,TCommand}"/>); the platform dispatches its command.
/// What the type can't hold is checked here: where a reaction lives, what it may hear, and what its command declares.
/// </summary>
public sealed class ReactionTests
{
    private static List<(Type Reaction, Type Message, Type Command)> Reactions =>
        TypesOf(Kinds.All)
            .Where(t => !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReaction<,>))
                .Select(i => (t, i.GetGenericArguments()[0], i.GetGenericArguments()[1])))
            .ToList();

    [Fact]
    public void Reactions_live_in_their_modules_application()
    {
        Reactions
            .Where(r => Owner(r.Reaction)!.Kind != Kinds.Application)
            .Select(r => $"{r.Reaction.FullName} is a reaction outside a module's Application")
            .ShouldHaveNoViolations("A reaction lives in its module's Application, beside the command it maps to.");
    }

    [Fact]
    public void A_reaction_maps_to_a_command_of_its_own_module()
    {
        Reactions
            .Where(r => Owner(r.Command)?.Module != Owner(r.Reaction)?.Module)
            .Select(r => $"{r.Reaction.FullName} maps to {r.Command.FullName}, a command of module {Owner(r.Command)?.Module ?? "none"}")
            .ShouldHaveNoViolations("A reaction maps a message to one command of its own module; another module is asked with a command message.");
    }

    [Fact]
    public void A_reaction_answers_only_a_message_its_module_may_hear()
    {
        Reactions
            .Select(r => (r.Reaction, r.Message, Module: Owner(r.Reaction)?.Module, From: Owner(r.Message)?.Module))
            .Select(r => r switch
            {
                _ when typeof(IIntegrationEvent).IsAssignableFrom(r.Message) => null,
                _ when typeof(IDomainEvent).IsAssignableFrom(r.Message) && r.From == r.Module => null,
                _ when typeof(ICommandMessage).IsAssignableFrom(r.Message) && r.From == r.Module => null,
                _ when typeof(IDomainEvent).IsAssignableFrom(r.Message) => $"{r.Reaction.FullName} hears module {r.From}'s domain event {r.Message.Name}",
                _ when typeof(ICommandMessage).IsAssignableFrom(r.Message) => $"{r.Reaction.FullName} handles {r.Message.Name}, a command message module {r.From} owns",
                _ => $"{r.Reaction.FullName} answers {r.Message.Name}, which is no event or command message",
            })
            .OfType<string>()
            .ShouldHaveNoViolations("Domain events reach reactions in their own module only; any module may react to an integration event; a command message is handled by the module that owns it.");
    }

    [Fact]
    public void A_reaction_command_is_keyed_transactional_and_unversioned()
    {
        Reactions
            .Select(r => (r.Reaction, r.Command, Operation: (Operation)r.Command.GetProperty(nameof(IDeclaresOperation.Operation))!.GetValue(null)!))
            .SelectMany(r => new[]
            {
                r.Operation.Idempotency != IdempotencyMechanism.Key ? $"{r.Command.Name} declares {r.Operation.Idempotency} idempotency; a reaction is keyed by its message's id" : null,
                r.Operation.Transaction != TransactionRequirement.Required ? $"{r.Command.Name} runs without a transaction; a reaction commits in its own" : null,
                typeof(IVersionedCommand).IsAssignableFrom(r.Command) ? $"{r.Command.Name} is versioned; a reaction applies the state it loads and carries no If-Match" : null,
            })
            .OfType<string>()
            .ShouldHaveNoViolations("A reaction's command declares an idempotency key, which the dispatcher fills with the causing message's id, and its own transaction.");
    }
}
