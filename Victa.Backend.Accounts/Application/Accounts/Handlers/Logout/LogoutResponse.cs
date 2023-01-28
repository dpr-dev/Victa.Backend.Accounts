using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Logout;

public sealed class LogoutResponse : OneOfBase<Success, ExecutionError>
{
    public LogoutResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }

    public static LogoutResponse Success { get; } = new LogoutResponse(new Success());
    public static LogoutResponse Unhandled { get; } = new LogoutResponse(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });
}
