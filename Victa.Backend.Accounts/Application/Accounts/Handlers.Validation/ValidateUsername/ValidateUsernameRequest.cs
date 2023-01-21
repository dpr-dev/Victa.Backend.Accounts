using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateUsername;

public sealed class ValidateUsernameRequest : IRequest<ValidateUsernameResponse>
{
    public ValidateUsernameRequest(string? value)
    {
        Username = value;
    }

    public string? Username { get; }
}
