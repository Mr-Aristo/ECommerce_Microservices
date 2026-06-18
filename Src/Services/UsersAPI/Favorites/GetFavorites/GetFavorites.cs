using UsersAPI.Integration.Catalog;

namespace UsersAPI.Favorites.GetFavorites;

// Favorite enriched with a Catalog product summary. Available=false when the product could not be fetched.
public record FavoriteProduct(Guid ProductId, DateTime AddedAt, string? Name, decimal? Price, string? ImageFile, bool Available);
public record GetFavoritesResponse(List<FavoriteProduct> Favorites);

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
        .WithSummary("List my favorite products (enriched with Catalog summaries)");
    }
}

public record GetFavoritesQuery(string UserId) : IQuery<GetFavoritesResult>;
public record GetFavoritesResult(List<FavoriteProduct> Favorites);

public class GetFavoritesQueryHandler(IDocumentSession session, ICatalogClient catalog)
    : IQueryHandler<GetFavoritesQuery, GetFavoritesResult>
{
    public async Task<GetFavoritesResult> Handle(GetFavoritesQuery query, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(query.UserId, cancellationToken);
        var favorites = profile?.Favorites ?? new List<FavoriteItem>();

        if (favorites.Count == 0)
            return new GetFavoritesResult(new List<FavoriteProduct>());

        var summaries = await catalog.GetProductsAsync(
            favorites.Select(f => f.ProductId).ToList(), cancellationToken);

        var enriched = favorites.Select(f =>
        {
            summaries.TryGetValue(f.ProductId, out var p);
            return new FavoriteProduct(f.ProductId, f.AddedAt, p?.Name, p?.Price, p?.ImageFile, p is not null);
        }).ToList();

        return new GetFavoritesResult(enriched);
    }
}
