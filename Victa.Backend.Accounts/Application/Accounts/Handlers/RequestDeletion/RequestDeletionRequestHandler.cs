using MediatR;

using OneOf.Types;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.RequestDeletion;

public sealed class RequestDeletionRequestHandler
    : IRequestHandler<RequestDeletionRequest, RequestDeletionResponse>
{
    public Task<RequestDeletionResponse> Handle(RequestDeletionRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new RequestDeletionResponse(new Success()));
    }
}

