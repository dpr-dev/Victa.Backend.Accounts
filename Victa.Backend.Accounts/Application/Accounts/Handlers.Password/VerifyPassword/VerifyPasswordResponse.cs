using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.VerifyPassword;

public sealed class VerifyPasswordResponse : OneOfBase<None, ExecutionError>
{
    private VerifyPasswordResponse(OneOf<None, ExecutionError> input) : base(input)
    {
    }

    public static VerifyPasswordResponse Unhandled { get; } = Failure(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });

    public static VerifyPasswordResponse Unmatched { get; } = Failure(new ValidationError
    {
        Code = ErrorCodes.PasswordVerificationFailed
    });

    public static VerifyPasswordResponse UserNotFoundById(string id)
    {
        return Failure(new UnhandledError
        {
            Code = ErrorCodes.UserWasNotFoundById,
            Details = $"Authenticated user was not found by id ({id})"
        });
    }


    public static VerifyPasswordResponse Failure(ExecutionError data)
    {
        return new VerifyPasswordResponse(OneOf<None, ExecutionError>.FromT1(data));
    }

    public static VerifyPasswordResponse Success()
    {
        return new VerifyPasswordResponse(OneOf<None, ExecutionError>.FromT0(new()));
    }
}
