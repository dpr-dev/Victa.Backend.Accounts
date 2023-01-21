using FluentValidation;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateEmail;

public class ValidateEmailRequestValidator : AbstractValidator<ValidateEmailRequest>
{
    public ValidateEmailRequestValidator(
        IdentityErrorDescriber errorDescriber,
        UserManager<AccountsUser> accountsUserManager)
    {
        _ = RuleFor(x => x.Email).EmailAddress()
            .WithMessage(x => errorDescriber.InvalidEmail(x.Email).Code);

        _ = RuleFor(x => x.Email)
            .CustomAsync(async (email, ctx, cts) =>
            {
                AccountsUser? existing = await accountsUserManager.FindByEmailAsync(email);
                if (existing != null)
                {
                    ctx.AddFailure(errorDescriber.DuplicateEmail(email).Code);
                }
            });
    }
}
