using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.ChangePassword;

public class ChangePasswordResponse : OneOfBase<None, ExecutionError>
{
    protected ChangePasswordResponse(OneOf<None, ExecutionError> input) : base(input)
    {
    }

    public static ChangePasswordResponse Unhandled { get; } = Failure(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });

    public static ChangePasswordResponse UserNotFoundById(string id)
    {
        return Failure(new UnhandledError
        {
            Code = ErrorCodes.UserWasNotFoundById,
            Details = $"Authenticated user was not found by id ({id})"
        });
    }


    public static ChangePasswordResponse Failure(ExecutionError data)
    {
        return new ChangePasswordResponse(OneOf<None, ExecutionError>.FromT1(data));
    }

    public static ChangePasswordResponse Success()
    {
        return new ChangePasswordResponse(OneOf<None, ExecutionError>.FromT0(new()));
    }
}
