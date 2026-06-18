namespace UsersAPI.Favorites.RemoveFavorite;

public record RemoveFavoriteResponse(bool IsSuccess);

public class RemoveFavoriteEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/me/favorites/{productId:guid}", async (Guid productId, ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new RemoveFavoriteCommand(user.GetUserId(), productId));
            return Results.Ok(new RemoveFavoriteResponse(result.IsSuccess));
        })
        .RequireAuthorization()
        .WithName("RemoveFavorite")
        .Produces<RemoveFavoriteResponse>(StatusCodes.Status200OK)
        .WithSummary("Remove a product from my favorites");
    }
}

public record RemoveFavoriteCommand(string UserId, Guid ProductId) : ICommand<RemoveFavoriteResult>;
public record RemoveFavoriteResult(bool IsSuccess);

public class RemoveFavoriteCommandHandler(IDocumentSession session)
    : ICommandHandler<RemoveFavoriteCommand, RemoveFavoriteResult>
{
    public async Task<RemoveFavoriteResult> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(command.UserId, cancellationToken);
        if (profile is not null)
        {
            profile.Favorites.RemoveAll(f => f.ProductId == command.ProductId);
            session.Store(profile);
            await session.SaveChangesAsync(cancellationToken);
        }

        return new RemoveFavoriteResult(true);
    }
}
