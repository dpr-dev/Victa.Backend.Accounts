using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.RequestDeletion;

/// <summary>
/// Request that performs account deletion
/// </summary>
public sealed class RequestDeletionRequest : IRequest<RequestDeletionResponse>
{
    public RequestDeletionRequest(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or whitespace.", nameof(userId));
        }

        UserId = userId;
    }

    public string UserId { get; }
}

