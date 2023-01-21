﻿using OneOf;
using OneOf.Types;

using Victa.Backend.Accounts.Core.Errors;

namespace Victa.Backend.Accounts.Application.Accounts.Handlers.Logout;

public sealed class LogoutResponse : OneOfBase<Success, ExecutionError>
{
    public LogoutResponse(OneOf<Success, ExecutionError> input) : base(input)
    {
    }
}
