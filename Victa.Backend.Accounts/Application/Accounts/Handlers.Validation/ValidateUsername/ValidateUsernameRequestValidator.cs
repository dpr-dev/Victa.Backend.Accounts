using FluentValidation;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateUsername;

public class ValidateUsernameRequestValidator : AbstractValidator<ValidateUsernameRequest>
{
    public ValidateUsernameRequestValidator(
        IdentityErrorDescriber errorDescriber,
        UserManager<AccountsUser> accountsUserManager)
    {
        _ = RuleFor(x => x.Username).EmailAddress()
            .WithMessage(x => errorDescriber.InvalidEmail(x.Username).Code);

        _ = RuleFor(x => x.Username)
            .CustomAsync(async (username, ctx, cts) =>
            {
                AccountsUser? existing = await accountsUserManager.FindByNameAsync(username);
                if (existing != null)
                {
                    ctx.AddFailure(errorDescriber.DuplicateUserName(username).Code);
                }
            });
    }
}
