namespace Platform.Application.Abstractions;

/// <summary>A message that declares its operation. A message without one doesn't compile, so it can't dispatch.</summary>
public interface IDeclaresOperation
{
    static abstract Operation Operation { get; }
}

public interface ICommand : Mediator.ICommand<Result>, IDeclaresOperation;

/// <summary>A command returns at most an id, an outcome, and the new version.</summary>
public interface ICommand<T> : Mediator.ICommand<Result<T>>, IDeclaresOperation;

public interface IQuery<T> : Mediator.IQuery<Result<T>>, IDeclaresOperation;

public interface ICommandHandler<in TCommand> : Mediator.ICommandHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, T> : Mediator.ICommandHandler<TCommand, Result<T>>
    where TCommand : ICommand<T>;

public interface IQueryHandler<in TQuery, T> : Mediator.IQueryHandler<TQuery, Result<T>>
    where TQuery : IQuery<T>;
