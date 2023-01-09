using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Logout;

public sealed class LogoutRequestHandler
    : IRequestHandler<LogoutRequest, LogoutResponse>
{
    public Task<LogoutResponse> Handle(LogoutRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
