namespace UsersAPI.Address.AddAddress;

public record AddAddressCommand(
    string UserId, string Email, string UserName,
    string Line, string City, string Country, string ZipCode, bool IsDefault) : ICommand<AddAddressResult>;

public record AddAddressResult(Guid AddressId);

public class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Line).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
        RuleFor(x => x.ZipCode).NotEmpty();
    }
}

public class AddAddressCommandHandler(IDocumentSession session)
    : ICommandHandler<AddAddressCommand, AddAddressResult>
{
    public async Task<AddAddressResult> Handle(AddAddressCommand command, CancellationToken cancellationToken)
    {
        var profile = await session.LoadAsync<UserProfile>(command.UserId, cancellationToken)
            ?? new UserProfile { Id = command.UserId, Email = command.Email, UserName = command.UserName };

        var address = new Models.Address
        {
            Line = command.Line,
            City = command.City,
            Country = command.Country,
            ZipCode = command.ZipCode,
            IsDefault = command.IsDefault
        };

        // A single default address: clear the previous default when a new default is added.
        if (address.IsDefault)
            profile.Addresses.ForEach(a => a.IsDefault = false);

        profile.Addresses.Add(address);

        session.Store(profile);
        await session.SaveChangesAsync(cancellationToken);

        return new AddAddressResult(address.Id);
    }
}
