namespace BuildingBlock.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse> where TResponse : notnull
{
}
