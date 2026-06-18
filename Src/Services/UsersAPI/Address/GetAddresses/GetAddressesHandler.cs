namespace UsersAPI.Address.GetAddresses;

public record GetAddressesQuery(string UserId) : IQuery<GetAddressesResult>;
public record GetAddressesResult(List<Models.Address> Addresses);

public class GetAddressesQueryHandler(IDocumentSession session)
    : IQueryHandler<GetAddressesQuery, GetAddressesResult>
{
    public async Task<GetAddressesResult> Handle(GetAddressesQuery query, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(query.UserId, cancellationToken);
        return new GetAddressesResult(profile?.Addresses ?? new List<Models.Address>());
    }
}
