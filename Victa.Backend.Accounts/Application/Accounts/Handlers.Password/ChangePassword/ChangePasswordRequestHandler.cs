using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.ChangePassword;

public class ChangePasswordRequestHandler : IRequestHandler<ChangePasswordRequest, ChangePasswordResponse>
{
    public Task<ChangePasswordResponse> Handle(ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
