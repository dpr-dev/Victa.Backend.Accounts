using OneOf;

using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Registration.RegisterViaPassword;

public sealed class RegisterViaPasswordResponse : OneOfBase<OAccountsUser, ExecutionError>
{
    private RegisterViaPasswordResponse(OneOf<OAccountsUser, ExecutionError> input)
        : base(input)
    {
    }

    public static RegisterViaPasswordResponse Unhandled { get; } = Failure(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });


    public static RegisterViaPasswordResponse Failure(ExecutionError data)
    {
        return new RegisterViaPasswordResponse(OneOf<OAccountsUser, ExecutionError>.FromT1(data));
    }

    public static RegisterViaPasswordResponse Success(OAccountsUser data)
    {
        return new RegisterViaPasswordResponse(OneOf<OAccountsUser, ExecutionError>.FromT0(data));
    }
}
