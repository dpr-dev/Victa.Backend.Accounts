using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.RequestDeletion;

public sealed class RequestDeletionResponse : OneOfBase<Success, ExecutionError>
{
    public RequestDeletionResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }
}

