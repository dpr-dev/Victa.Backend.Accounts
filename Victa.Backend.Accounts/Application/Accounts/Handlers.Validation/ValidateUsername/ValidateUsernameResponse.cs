﻿using FluentValidation.Results;

using OneOf;

using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Validation.ValidateUsername;

public sealed class ValidateUsernameResponse : OneOfBase<ValidationResult, ExecutionError>
{
    private ValidateUsernameResponse(OneOf<ValidationResult, ExecutionError> input)
        : base(input)
    {
    }


    public static ValidateUsernameResponse Failure(ExecutionError data)
    {
        return new ValidateUsernameResponse(OneOf<ValidationResult, ExecutionError>.FromT1(data));
    }

    public static ValidateUsernameResponse Success(ValidationResult data)
    {
        return new ValidateUsernameResponse(OneOf<ValidationResult, ExecutionError>.FromT0(data));
    }
}
