namespace UsersAPI.Favorites.GetFavorites;

public record GetFavoritesResponse(List<FavoriteItem> Favorites);

public class GetFavoritesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/favorites", async (ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new GetFavoritesQuery(user.GetUserId()));
            return Results.Ok(new GetFavoritesResponse(result.Favorites));
        })
        .RequireAuthorization()
        .WithName("GetFavorites")
        .Produces<GetFavoritesResponse>(StatusCodes.Status200OK)
        .WithSummary("List my favorite products");
        // Note: enriching with Catalog product summaries is a follow-up (needs a Catalog batch-by-ids read).
    }
}

public record GetFavoritesQuery(string UserId) : IQuery<GetFavoritesResult>;
public record GetFavoritesResult(List<FavoriteItem> Favorites);

public class GetFavoritesQueryHandler(IDocumentSession session)
    : IQueryHandler<GetFavoritesQuery, GetFavoritesResult>
{
    public async Task<GetFavoritesResult> Handle(GetFavoritesQuery query, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(query.UserId, cancellationToken);
        return new GetFavoritesResult(profile?.Favorites ?? new List<FavoriteItem>());
    }
}
