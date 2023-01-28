using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.CreateRecoveryCode;

public sealed class CreateRecoveryCodeResponse : OneOfBase<Success, ExecutionError>
{
    private CreateRecoveryCodeResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }


    public static CreateRecoveryCodeResponse Unhandled { get; } = Failure(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });

    public static CreateRecoveryCodeResponse UserNotFoundByEmail(string email)
    {
        return Failure(new NotFoundError
        {
            Code = ErrorCodes.UserWasNotFoundByEmail,

            // for backward capability. replace to description "User with email (email) was not found"
            Details = "email_doesnt_exists"
        });
    }


    public static CreateRecoveryCodeResponse Failure(ExecutionError data)
    {
        return new CreateRecoveryCodeResponse(OneOf<Success, ExecutionError>.FromT1(data));
    }

    public static CreateRecoveryCodeResponse Success()
    {
        return new CreateRecoveryCodeResponse(OneOf<Success, ExecutionError>.FromT0(new()));
    }
}
