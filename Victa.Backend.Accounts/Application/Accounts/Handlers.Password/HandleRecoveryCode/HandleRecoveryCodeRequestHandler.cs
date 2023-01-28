using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.HandleRecoveryCode;

public class HandleRecoveryCodeRequestHandler
    : IRequestHandler<HandleRecoveryCodeRequest, HandleRecoveryCodeResponse>
{
    public Task<HandleRecoveryCodeResponse> Handle(HandleRecoveryCodeRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
