namespace BuildingBlock.CQRS;

/*
    ICommand<Unit> : Unit is void type in Mediator
 */
public interface ICommand : ICommand<Unit>
{ }

public interface ICommand<out TResponse> : IRequest<TResponse>
{ }

