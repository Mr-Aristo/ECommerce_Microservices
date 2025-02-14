namespace BuildingBlock.CQRS.Handlers;

public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand<Unit>
{ }
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>// This expression forces TCommand to be the type of ICommand<TResponse>
    where TResponse : notnull
{ }
