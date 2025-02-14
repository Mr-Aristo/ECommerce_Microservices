namespace BuildingBlock.Exceptions;

public class NotFoundExceptions : Exception
{
    public NotFoundExceptions(string message) : base(message)
    { }

    public NotFoundExceptions(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
    { }
}
