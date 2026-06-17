namespace UsersAPI.Profile.GetMyProfile;

public record GetMyProfileResponse(string Id, string Email, string UserName, string? FirstName, string? LastName, string? Phone, List<Models.Address> Addresses);

public class GetMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/profile", async (ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new GetMyProfileQuery(user.GetUserId(), user.GetEmail(), user.GetUserName()));
            var p = result.Profile;
            return Results.Ok(new GetMyProfileResponse(p.Id, p.Email, p.UserName, p.FirstName, p.LastName, p.Phone, p.Addresses));
        })
        .RequireAuthorization()
        .WithName("GetMyProfile")
        .Produces<GetMyProfileResponse>(StatusCodes.Status200OK)
        .WithSummary("Get my profile")
        .WithDescription("Returns the caller's profile, creating it on first access (JIT provisioning).");
    }
}
