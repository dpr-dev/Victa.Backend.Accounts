using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Configure;

public sealed class ConfigureResponse : OneOfBase<Success, ExecutionError>
{
    public ConfigureResponse(OneOf<Success, ExecutionError> input)
        : base(input)
    {
    }


    public static ConfigureResponse Success { get; } = new ConfigureResponse(new Success());
    public static ConfigureResponse Unhandled { get; } = new ConfigureResponse(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });
}
