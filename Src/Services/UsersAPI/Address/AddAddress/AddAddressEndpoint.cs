namespace UsersAPI.Address.AddAddress;

public record AddAddressRequest(string Line, string City, string Country, string ZipCode, bool IsDefault);
public record AddAddressResponse(Guid AddressId);

public class AddAddressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/me/addresses", async (AddAddressRequest request, ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new AddAddressCommand(
                user.GetUserId(), user.GetEmail(), user.GetUserName(),
                request.Line, request.City, request.Country, request.ZipCode, request.IsDefault));

            return Results.Ok(new AddAddressResponse(result.AddressId));
        })
        .RequireAuthorization()
        .WithName("AddAddress")
        .Produces<AddAddressResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Add an address to my profile");
    }
}
