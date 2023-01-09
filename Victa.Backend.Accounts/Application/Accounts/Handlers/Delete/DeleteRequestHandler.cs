using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Delete;

public sealed class DeleteRequestHandler
    : IRequestHandler<DeleteRequest, DeleteResponse>
{
    public Task<DeleteResponse> Handle(DeleteRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

