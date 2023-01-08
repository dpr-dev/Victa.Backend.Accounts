using System.Security.Claims;

using IdentityModel;

using IdentityServer4.Models;
using IdentityServer4.Services;

using Microsoft.AspNetCore.Identity;

using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Configuration.IdentityServer.Adapters.Services;

internal class ProfileService : IProfileService
{
    private static readonly Dictionary<string, Func<AccountsUser, string?>> _fieldsMapping = new()
    {
        [JwtClaimTypes.Name] = x => x.Name,
        [JwtClaimTypes.Subject] = x => x.Id,
        [JwtClaimTypes.GivenName] = x => x.GivenName,
        [JwtClaimTypes.Email] = x => x.Email,
        [JwtClaimTypes.PhoneNumber] = x => x.PhoneNumber
    };

    private readonly ILogger _logger;
    private readonly UserManager<AccountsUser> _userStore;

    public ProfileService(ILogger<ProfileService> logger,
        UserManager<AccountsUser> userStore)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (userStore is null)
        {
            throw new ArgumentNullException(nameof(userStore));
        }

        _logger = logger;
        _userStore = userStore;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        if (!context.RequestedClaimTypes.Any())
        {
            return;
        }

        context.AddRequestedClaims(context.Subject.Claims);

        string userId = GetUserId(context);
        AccountsUser? user = await _userStore.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User was not found");
        }

        // TODO: NRE if user not found in the database
        foreach (string claim in context.RequestedClaimTypes)
        {
            if (_fieldsMapping.TryGetValue(claim,
                out Func<AccountsUser, string?>? getter)
                    && getter is { } && getter(user) is { } value)
            {
                context.IssuedClaims.Add(new Claim(claim, value));
            }
        }

        if (context.RequestedClaimTypes.Contains(JwtClaimTypes.Role))
        {
            context.IssuedClaims.AddRange(user.Roles.Select(x => new Claim(JwtClaimTypes.Role, x)));
        }
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        string userId = GetUserId(context);
        AccountsUser? user = await _userStore.FindByIdAsync(userId);
        if (user is null
            || user.LockoutEnd >= DateTimeOffset.UtcNow)
        {
            context.IsActive = false;
        }
        else
        {
            context.IsActive = true;
        }
    }

    private static string GetUserId(IsActiveContext context)
    {
        string? userId = context.Subject.FindFirstValue(JwtClaimTypes.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Can't find user id claim");
        }

        return userId;
    }

    private static string GetUserId(ProfileDataRequestContext context)
    {
        string? userId = context.Subject.FindFirstValue(JwtClaimTypes.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("Can't find user id claim");
        }

        return userId;
    }
}
