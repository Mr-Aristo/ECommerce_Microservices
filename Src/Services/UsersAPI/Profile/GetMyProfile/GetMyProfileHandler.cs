namespace UsersAPI.Profile.GetMyProfile;

public record GetMyProfileQuery(string UserId, string Email, string UserName) : IQuery<GetMyProfileResult>;
public record GetMyProfileResult(UserProfile Profile);

public class GetMyProfileQueryHandler(IDocumentSession session)
    : IQueryHandler<GetMyProfileQuery, GetMyProfileResult>
{
    public async Task<GetMyProfileResult> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(query.UserId, cancellationToken);
        if (profile is null)
        {
            // JIT provisioning: create the profile from the token on first authenticated access.
            profile = new UserProfile
            {
                Id = query.UserId,
                Email = query.Email,
                UserName = query.UserName
            };
            session.Store(profile);
            await session.SaveChangesAsync(cancellationToken);
        }

        return new GetMyProfileResult(profile);
    }
}
