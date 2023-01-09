using MediatR;

using Victa.Backend.Accounts.Contracts.Input.Accounts;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;


public class RegisterViaPasswordRequest : IRequest<RegisterViaPasswordResponse>
{
    public RegisterViaPasswordRequest(PasswordRegistrationBody source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Source = source;
    }

    public PasswordRegistrationBody Source { get; }
}
