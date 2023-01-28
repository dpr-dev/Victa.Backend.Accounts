using IdentityServer4.Models;
using IdentityServer4.Validation;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

using static IdentityModel.OidcConstants;

namespace Victa.Backend.Accounts.Infrastructure.IdentityServer.Adapters.Validation;

public class ResourceOwnerValidator : IResourceOwnerPasswordValidator
{
    private readonly UserManager<AccountsUser> _userManager;

    public ResourceOwnerValidator(UserManager<AccountsUser> userManager)
    {
        if (userManager is null)
        {
            throw new ArgumentNullException(nameof(userManager));
        }

        _userManager = userManager;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        AccountsUser? user = await _userManager.FindByEmailAsync(context.UserName);
        if (user is null)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                errorDescription: $"Unable to find user ({context.UserName})");
            return;
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, context.Password);
        if (!isValid)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant,
                errorDescription: $"Password does not match for user ({context.UserName})");
            return;
        }

        context.Result = new GrantValidationResult(user.Id, AuthenticationMethods.Password);
    }
}
