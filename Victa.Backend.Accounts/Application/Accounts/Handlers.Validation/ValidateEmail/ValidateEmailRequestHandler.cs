using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateEmail;

public sealed class ValidateEmailRequestHandler 
    : IRequestHandler<ValidateEmailRequest, ValidateEmailResponse>
{
    public Task<ValidateEmailResponse> Handle(ValidateEmailRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
