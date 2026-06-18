namespace UsersAPI.Profile.UpdateMyProfile;

public record UpdateMyProfileRequest(string? FirstName, string? LastName, string? Phone);
public record UpdateMyProfileResponse(bool IsSuccess);

public class UpdateMyProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/me/profile", async (UpdateMyProfileRequest request, ClaimsPrincipal user, ISender sender) =>
        {
            var result = await sender.Send(new UpdateMyProfileCommand(
                user.GetUserId(), user.GetEmail(), user.GetUserName(),
                request.FirstName, request.LastName, request.Phone));

            return Results.Ok(new UpdateMyProfileResponse(result.IsSuccess));
        })
        .RequireAuthorization()
        .WithName("UpdateMyProfile")
        .Produces<UpdateMyProfileResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update my profile");
    }
}
