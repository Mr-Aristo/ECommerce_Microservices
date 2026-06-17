namespace UsersAPI.Profile.UpdateMyProfile;

public record UpdateMyProfileCommand(
    string UserId, string Email, string UserName,
    string? FirstName, string? LastName, string? Phone) : ICommand<UpdateMyProfileResult>;

public record UpdateMyProfileResult(bool IsSuccess);

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Phone).MaximumLength(32).When(x => x.Phone is not null);
    }
}

public class UpdateMyProfileCommandHandler(IDocumentSession session)
    : ICommandHandler<UpdateMyProfileCommand, UpdateMyProfileResult>
{
    public async Task<UpdateMyProfileResult> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        // JIT: ensure the profile exists even if the caller updates before a first GET.
        var profile = await session.LoadAsync<UserProfile>(command.UserId, cancellationToken)
            ?? new UserProfile { Id = command.UserId, Email = command.Email, UserName = command.UserName };

        profile.FirstName = command.FirstName;
        profile.LastName = command.LastName;
        profile.Phone = command.Phone;

        session.Store(profile);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateMyProfileResult(true);
    }
}
