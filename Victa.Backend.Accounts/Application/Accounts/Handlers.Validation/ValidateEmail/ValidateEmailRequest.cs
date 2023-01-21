using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateEmail;

public sealed class ValidateEmailRequest : IRequest<ValidateEmailResponse>
{
    public ValidateEmailRequest(string? email)
    {
        Email = email;
    }

    public string? Email { get; }
}
