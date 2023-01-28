using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.RequestDeletion;

public sealed class RequestDeletionResponse : OneOfBase<Success, ExecutionError>
{
    public RequestDeletionResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }

    public static RequestDeletionResponse Success { get; } = new(new Success());
    public static RequestDeletionResponse Unhandled { get; } = new(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });
}

