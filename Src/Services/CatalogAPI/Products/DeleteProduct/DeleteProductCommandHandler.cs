

namespace CatalogAPI.Products.DeleteProduct;

#region Records
public record DeleteProductCommand(Guid id) : ICommand<DeleteProductResult>;
public record DeleteProductResult(bool IsSuccess);
#endregion

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("Product ID is required");
    }
}

public class DeleteProductCommandHandler(IDocumentSession session) : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        session.Delete<Product>(command.id);
        await session.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true);

    }
}
