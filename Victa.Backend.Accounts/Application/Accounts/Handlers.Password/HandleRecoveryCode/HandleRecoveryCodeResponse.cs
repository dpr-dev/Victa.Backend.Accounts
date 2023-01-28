using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.HandleRecoveryCode;

public sealed class HandleRecoveryCodeResponse : OneOfBase<Success, ExecutionError>
{
    private HandleRecoveryCodeResponse(OneOf<Success, ExecutionError> input)
        : base(input)
    {
    }

    public static HandleRecoveryCodeResponse Unhandled { get; } = Failure(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });

    public static HandleRecoveryCodeResponse UserNotFoundByEmail(string email)
    {
        return Failure(new NotFoundError { Code = ErrorCodes.UserWasNotFoundByEmail, Details = "email_doesnt_exists" });
    }

    public static HandleRecoveryCodeResponse Failure(ExecutionError data)
    {
        return new HandleRecoveryCodeResponse(OneOf<Success, ExecutionError>.FromT1(data));
    }

    public static HandleRecoveryCodeResponse Success()
    {
        return new HandleRecoveryCodeResponse(OneOf<Success, ExecutionError>.FromT0(new()));
    }
}
