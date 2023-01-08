﻿using Microsoft.AspNetCore.Authorization;

namespace Victa.Backend.Accounts.Core.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class AuthorizeAdminAttribute : AuthorizeAttribute
{
    public AuthorizeAdminAttribute() : base(AuthorizationPolicies.AdminPolicyName)
    {
    }
}
