using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Configure;

public sealed class ConfigureRequest : IRequest<ConfigureResponse>
{
    public ConfigureRequest(string userId, string? firebaseToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or whitespace.", nameof(userId));
        }

        UserId = userId;
        FirebaseToken = firebaseToken;
    }

    public string UserId { get; }
    public string? FirebaseToken { get; }
}
