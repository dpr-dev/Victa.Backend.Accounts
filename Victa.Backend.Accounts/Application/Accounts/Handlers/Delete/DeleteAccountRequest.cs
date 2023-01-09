using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Delete;

/// <summary>
/// Request that performs account deletion
/// </summary>
public sealed class DeleteRequest : IRequest<DeleteResponse>
{
}

