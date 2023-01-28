using FluentValidation.Results;

using OneOf;

using Victa.Backend.Accounts.Core.Errors;
using Victa.Backend.Accounts.Domain;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateEmail;

public sealed class ValidateEmailResponse : OneOfBase<ValidationResult, ExecutionError>
{
    private ValidateEmailResponse(OneOf<ValidationResult, ExecutionError> input)
        : base(input)
    {
    }

    public static ValidateEmailResponse Unhandled { get; } = Failure(new UnhandledError
    {
        Code = ErrorCodes.Unhandled
    });

    public static ValidateEmailResponse Failure(ExecutionError data)
    {
        return new ValidateEmailResponse(OneOf<ValidationResult, ExecutionError>.FromT1(data));
    }

    public static ValidateEmailResponse Success(ValidationResult data)
    {
        return new ValidateEmailResponse(OneOf<ValidationResult, ExecutionError>.FromT0(data));
    }
}
