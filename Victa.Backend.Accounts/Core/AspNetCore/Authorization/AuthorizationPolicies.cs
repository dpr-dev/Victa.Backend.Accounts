using IdentityModel;

using Microsoft.AspNetCore.Authorization;

namespace Victa.Backend.Accounts.Core.AspNetCore.Authorization;

public static class AuthorizationPolicies
{
    private const string IdentityServiceAuthenticationScheme = "IdentityServerAccessToken";

    public static AuthorizationPolicy Customer { get; }
        = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(IdentityServiceAuthenticationScheme)
            .RequireClaim(JwtClaimTypes.Subject)
            .RequireClaim(JwtClaimTypes.Role, "customer")
            .Build();

    public static AuthorizationPolicy Admin { get; }
        = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(IdentityServiceAuthenticationScheme)
            .RequireClaim(JwtClaimTypes.Subject)
            .RequireClaim(JwtClaimTypes.Role, "admin")
            .Build();

    public static AuthorizationPolicy Simple { get; }
        = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(IdentityServiceAuthenticationScheme)
            .RequireClaim(JwtClaimTypes.Role)
            .RequireClaim(JwtClaimTypes.Subject)
            .Build();

    public const string AdminPolicyName = nameof(Admin);
    public const string CustomerPolicyName = nameof(Customer);
    public const string SimplePolicyName = nameof(Simple);
}
