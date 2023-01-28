using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.CreateRecoveryCode;

public class CreateRecoveryCodeRequestHandler
    : IRequestHandler<CreateRecoveryCodeRequest, CreateRecoveryCodeResponse>
{
    public Task<CreateRecoveryCodeResponse> Handle(CreateRecoveryCodeRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
