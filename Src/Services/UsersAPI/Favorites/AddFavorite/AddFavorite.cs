namespace UsersAPI.Favorites.AddFavorite;

public record AddFavoriteRequest(Guid ProductId);
public record AddFavoriteResponse(bool IsSuccess);

public class AddFavoriteEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/me/favorites", async (AddFavoriteRequest request, ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new AddFavoriteCommand(
                user.GetUserId(), user.GetEmail(), user.GetUserName(), request.ProductId));

            return Results.Ok(new AddFavoriteResponse(result.IsSuccess));
        })
        .RequireAuthorization()
        .WithName("AddFavorite")
        .Produces<AddFavoriteResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Add a product to my favorites");
    }
}

public record AddFavoriteCommand(string UserId, string Email, string UserName, Guid ProductId) : ICommand<AddFavoriteResult>;
public record AddFavoriteResult(bool IsSuccess);

public class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator() => RuleFor(x => x.ProductId).NotEmpty();
}

public class AddFavoriteCommandHandler(IDocumentSession session)
    : ICommandHandler<AddFavoriteCommand, AddFavoriteResult>
{
    public async Task<AddFavoriteResult> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(command.UserId, cancellationToken)
            ?? new UserProfile { Id = command.UserId, Email = command.Email, UserName = command.UserName };

        // Idempotent: adding an existing favorite is a no-op.
        if (!profile.Favorites.Any(f => f.ProductId == command.ProductId))
            profile.Favorites.Add(new FavoriteItem { ProductId = command.ProductId });

        session.Store(profile);
        await session.SaveChangesAsync(cancellationToken);
        return new AddFavoriteResult(true);
    }
}
