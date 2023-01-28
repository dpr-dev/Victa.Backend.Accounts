using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.HandleRecoveryCode;

/// <summary>
/// 
/// </summary>
public class HandleRecoveryCodeRequest : IRequest<HandleRecoveryCodeResponse>
{
    public HandleRecoveryCodeRequest(string email, string recoveryCode, string password)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException($"'{nameof(email)}' cannot be null or empty.", nameof(email));
        }

        if (string.IsNullOrEmpty(recoveryCode))
        {
            throw new ArgumentException($"'{nameof(recoveryCode)}' cannot be null or empty.", nameof(recoveryCode));
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException($"'{nameof(password)}' cannot be null or empty.", nameof(password));
        }

        Email = email;
        RecoveryCode = recoveryCode;
        Password = password;
    }

    public string Email { get; }
    public string RecoveryCode { get; }
    public string Password { get; }
}
