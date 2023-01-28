using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.CreateRecoveryCode;

/// <summary>
/// 
/// </summary>
public class CreateRecoveryCodeRequest : IRequest<CreateRecoveryCodeResponse>
{
    public CreateRecoveryCodeRequest(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException($"'{nameof(email)}' cannot be null or whitespace.", nameof(email));
        }

        Email = email;
    }

    public string Email { get; }
}
