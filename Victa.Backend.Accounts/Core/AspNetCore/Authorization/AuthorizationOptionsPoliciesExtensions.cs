using Microsoft.AspNetCore.Authorization;

namespace Victa.Backend.Accounts.Core.AspNetCore.Authorization;

public static class AuthorizationOptionsPoliciesExtensions
{
    public static void AddDefaultPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AuthorizationPolicies.AdminPolicyName, AuthorizationPolicies.Admin);
        options.AddPolicy(AuthorizationPolicies.CustomerPolicyName, AuthorizationPolicies.Customer);
        options.AddPolicy(AuthorizationPolicies.SimplePolicyName, AuthorizationPolicies.Simple);
    }
}
