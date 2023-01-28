using MediatR;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.VerifyPassword;


/// <summary>
/// 
/// </summary>
public class VerifyPasswordRequest : IRequest<VerifyPasswordResponse>
{
    public VerifyPasswordRequest(string userId, string password)
    {
        UserId = userId;
        Password = password;
    }

    public string UserId { get; }
    public string Password { get; }
}
