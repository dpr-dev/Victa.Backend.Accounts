using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.VerifyPassword;

public class VerifyPasswordRequestHandler 
    : IRequestHandler<VerifyPasswordRequest, VerifyPasswordResponse>
{
    public Task<VerifyPasswordResponse> Handle(VerifyPasswordRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
