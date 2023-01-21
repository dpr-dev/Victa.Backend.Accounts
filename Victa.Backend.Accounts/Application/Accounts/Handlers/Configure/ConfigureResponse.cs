using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Configure;

public sealed class ConfigureResponse : OneOfBase<Success, ExecutionError>
{
    public ConfigureResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }
}
