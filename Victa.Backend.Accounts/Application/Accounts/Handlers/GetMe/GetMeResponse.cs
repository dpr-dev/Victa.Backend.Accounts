using OneOf;

using Victa.Backend.Accounts.Contracts.Output.Accounts;
using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.GetMe;

public sealed class GetMeResponse : OneOfBase<OAccountsUser, ExecutionError>
{
    private GetMeResponse(OneOf<OAccountsUser, ExecutionError> input)
        : base(input)
    {
    }

    public static GetMeResponse Unhandled { get; } = Failure(new UnhandledError { });


    public static GetMeResponse Failure(ExecutionError data)
    {
        return new GetMeResponse(OneOf<OAccountsUser, ExecutionError>.FromT1(data));
    }

    public static GetMeResponse Success(OAccountsUser data)
    {
        return new GetMeResponse(OneOf<OAccountsUser, ExecutionError>.FromT0(data));
    }

}
