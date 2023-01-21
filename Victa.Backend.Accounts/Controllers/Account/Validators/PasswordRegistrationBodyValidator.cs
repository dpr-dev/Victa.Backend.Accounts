using FluentValidation;

using Victa.Backend.Accounts.Contracts.Input.Accounts;

namespace Victa.Backend.Accounts.Controllers.Account.Validators;

public class PasswordRegistrationBodyValidator : AbstractValidator<PasswordRegistrationBody>
{
    public PasswordRegistrationBodyValidator()
    {
        _ = RuleFor(x => x.Email).NotEmpty();
        _ = RuleFor(x => x.UserName).NotEmpty();
        _ = RuleFor(x => x.Password).NotEmpty();
    }
}
