using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.ChangePassword;

/// <summary>
/// 
/// </summary>
public class ChangePasswordRequest : IRequest<ChangePasswordResponse>
{
    public ChangePasswordRequest(string userId, string password)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException($"'{nameof(password)}' cannot be null or empty.", nameof(password));
        }

        UserId = userId;
        Password = password;
    }

    public string UserId { get; }
    public string Password { get; }
}
