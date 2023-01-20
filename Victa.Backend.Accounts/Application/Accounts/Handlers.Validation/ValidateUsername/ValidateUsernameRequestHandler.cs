using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateUsername;

public sealed class ValidateUsernameRequestHandler 
    : IRequestHandler<ValidateUsernameRequest, ValidateUsernameResponse>
{
    public Task<ValidateUsernameResponse> Handle(ValidateUsernameRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
