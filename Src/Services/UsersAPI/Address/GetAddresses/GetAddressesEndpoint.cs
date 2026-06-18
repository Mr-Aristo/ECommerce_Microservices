namespace UsersAPI.Address.GetAddresses;

public record GetAddressesResponse(List<Models.Address> Addresses);

public class GetAddressesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/addresses", async (ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new GetAddressesQuery(user.GetUserId()));
            return Results.Ok(new GetAddressesResponse(result.Addresses));
        })
        .RequireAuthorization()
        .WithName("GetAddresses")
        .Produces<GetAddressesResponse>(StatusCodes.Status200OK)
        .WithSummary("List my addresses");
    }
}
