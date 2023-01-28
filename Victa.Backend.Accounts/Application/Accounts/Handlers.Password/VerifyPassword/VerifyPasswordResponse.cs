using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Password.VerifyPassword;

public class VerifyPasswordResponse : OneOfBase<Success, ExecutionError>
{
    protected VerifyPasswordResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }
}
