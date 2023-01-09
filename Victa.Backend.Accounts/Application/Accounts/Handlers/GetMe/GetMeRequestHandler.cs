using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.GetMe;

public sealed class GetMeRequestHandler
    : IRequestHandler<GetMeRequest, GetMeResponse>
{
    public Task<GetMeResponse> Handle(GetMeRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
