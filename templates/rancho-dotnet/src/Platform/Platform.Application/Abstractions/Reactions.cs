namespace Platform.Application.Abstractions;

/// <summary>
/// A command message: it asks the module whose Contracts declares it to do something, and that module owns it.
/// Another module sends it by writing it to its own outbox, never by calling the module. Its type name ends in its version, such as V1.
/// </summary>
public interface ICommandMessage;

/// <summary>
/// A reaction: it maps one message to one command of its own module, and nothing else. The outbox dispatcher dispatches that command
/// through the pipeline, as the module's service principal, with the causing message's id as its idempotency key, in its own transaction,
/// at least once. The message is a domain event of the same module, an integration event, or a command message the module owns.
/// </summary>
public interface IReaction<in TMessage, out TCommand>
    where TCommand : Mediator.IBaseCommand, IDeclaresOperation
{
    static abstract TCommand Map(TMessage message);
}
