using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Victa.Backend.Accounts.Domain.Models.RoleAggregate;
using Victa.Backend.Accounts.Domain.Models.UserAggregate;

namespace Victa.Backend.Accounts.Infrastructure.Identity.Adapters;

public class UserStore :
       IUserClaimStore<AccountsUser>,
       IUserLoginStore<AccountsUser>,
       IUserRoleStore<AccountsUser>,
       IUserPasswordStore<AccountsUser>,
       IUserSecurityStampStore<AccountsUser>,
       IUserEmailStore<AccountsUser>,
       IUserPhoneNumberStore<AccountsUser>,
       IQueryableUserStore<AccountsUser>,
       IUserTwoFactorStore<AccountsUser>,
       IUserLockoutStore<AccountsUser>,
       IUserAuthenticatorKeyStore<AccountsUser>,
       IUserAuthenticationTokenStore<AccountsUser>,
       IUserTwoFactorRecoveryCodeStore<AccountsUser>,
       IProtectedUserStore<AccountsUser>
{
    /// <summary>
    /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
    /// </summary>
    public IdentityErrorDescriber ErrorDescriber { get; set; }

    /// <summary>
    /// A navigation property for the users the store contains.
    /// </summary>
    public IQueryable<AccountsUser> Users => _userCollection.AsQueryable();

    private readonly IMongoCollection<AccountsUser> _userCollection;

    private readonly IMongoCollection<AccountsRole> _roleCollection;

    private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();

    private static readonly FindOptions<AccountsUser> FindOptions = new FindOptions<AccountsUser>();

    private static readonly ReplaceOptions ReplaceOptions = new ReplaceOptions();

    private const string InternalLoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
    private const string RecoveryCodeTokenName = "RecoveryCodes";

    private bool _disposed;

    /// <summary>
    /// Creates a new instance of the store.
    /// </summary>
    /// <param name="userCollection">The user mongo collection.</param>
    /// <param name="roleCollection">The role mongo collection.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public UserStore(IMongoCollection<AccountsUser> userCollection, IMongoCollection<AccountsRole> roleCollection, IdentityErrorDescriber describer)
    {
        _userCollection = userCollection;
        _roleCollection = roleCollection;
        ErrorDescriber = describer ?? new IdentityErrorDescriber();
    }

    /// <summary>
    /// Sets the token value for a particular user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task SetTokenAsync(AccountsUser user, string loginProvider, string name, string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        IdentityUserToken<string> token = await FindTokenAsync(user, loginProvider, name, cancellationToken);
        if (token == null)
        {
            user.Tokens.Add(new IdentityUserToken<string>()
            {
                UserId = user.Id.ToString(),
                LoginProvider = loginProvider,
                Name = name,
                Value = value
            });
        }
        else
        {
            token.Value = value;

            int idx = user.Tokens.FindIndex(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name);
            user.Tokens[idx] = token;
        }
    }

    /// <summary>
    /// Deletes a token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task RemoveTokenAsync(AccountsUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        IdentityUserToken<string> entry = await FindTokenAsync(user, loginProvider, name, cancellationToken);
        if (entry != null)
        {
            _ = user.Tokens.RemoveAll(x => x.LoginProvider == entry.LoginProvider && x.Name == entry.Name);
        }
    }

    /// <summary>
    /// Returns the token value.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task<string> GetTokenAsync(AccountsUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        IdentityUserToken<string> token = await FindTokenAsync(user, loginProvider, name, cancellationToken);
        return token?.Value;
    }

    /// <summary>
    /// Get the authenticator key for the specified <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user whose security stamp should be set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
    public Task<string> GetAuthenticatorKeyAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        return GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);
    }

    /// <summary>
    /// Sets the authenticator key for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose authenticator key should be set.</param>
    /// <param name="key">The authenticator key to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetAuthenticatorKeyAsync(AccountsUser user, string key, CancellationToken cancellationToken = default)
    {
        return SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);
    }

    /// <summary>
    /// Creates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the creation operation.</returns>
    public async Task<IdentityResult> CreateAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await _userCollection.InsertOneAsync(user, InsertOneOptions, cancellationToken).ConfigureAwait(false);

        return IdentityResult.Success;
    }

    /// <summary>
    /// Deletes the specified <paramref name="user"/> from the user store.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public async Task<IdentityResult> DeleteAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        DeleteResult result = await _userCollection.DeleteOneAsync(x => x.Id.Equals(user.Id) && x.ConcurrencyStamp.Equals(user.ConcurrencyStamp), cancellationToken).ConfigureAwait(false);
        if (!result.IsAcknowledged || result.DeletedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userId"/> if it exists.
    /// </returns>
    public Task<AccountsUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return ByIdAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified normalized user name.
    /// </summary>
    /// <param name="normalizedUserName">The normalized user name to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="normalizedUserName"/> if it exists.
    /// </returns>
    public Task<AccountsUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return _userCollection.AsQueryable().FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public async Task<IdentityResult> UpdateAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        string currentConcurrencyStamp = user.ConcurrencyStamp;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        ReplaceOneResult result = await _userCollection.ReplaceOneAsync(x => x.Id.Equals(user.Id) && x.ConcurrencyStamp.Equals(currentConcurrencyStamp), user, ReplaceOptions, cancellationToken).ConfigureAwait(false);

        if (!result.IsAcknowledged || result.ModifiedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    /// <summary>
    /// Adds the <paramref name="claims"/> given to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the claim to.</param>
    /// <param name="claims">The claim to add to the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task AddClaimsAsync(AccountsUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (claims == null)
        {
            throw new ArgumentNullException(nameof(claims));
        }

        foreach (Claim claim in claims)
        {
            var identityClaim = new IdentityUserClaim<string>()
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            };

            user.Claims.Add(identityClaim);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Replaces the <paramref name="claim"/> on the specified <paramref name="user"/>, with the <paramref name="newClaim"/>.
    /// </summary>
    /// <param name="user">The user to replace the claim on.</param>
    /// <param name="claim">The claim replace.</param>
    /// <param name="newClaim">The new claim replacing the <paramref name="claim"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task ReplaceClaimAsync(AccountsUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (claim == null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        if (newClaim == null)
        {
            throw new ArgumentNullException(nameof(newClaim));
        }

        var matchedClaims = user.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
        foreach (IdentityUserClaim<string>? matchedClaim in matchedClaims)
        {
            matchedClaim.ClaimValue = newClaim.Value;
            matchedClaim.ClaimType = newClaim.Type;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the <paramref name="claims"/> given from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the claims from.</param>
    /// <param name="claims">The claim to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task RemoveClaimsAsync(AccountsUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (claims == null)
        {
            throw new ArgumentNullException(nameof(claims));
        }

        foreach (Claim claim in claims)
        {
            _ = user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all users with the specified claim.
    /// </summary>
    /// <param name="claim">The claim whose users should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> contains a list of users, if any, that contain the specified claim.
    /// </returns>
    public async Task<IList<AccountsUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (claim == null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        return await _userCollection.AsQueryable().Where(u => u.Claims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value)).ToListAsync();
    }

    /// <summary>
    /// Gets the normalized user name for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose normalized name should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the normalized user name for the specified <paramref name="user"/>.</returns>
    public Task<string> GetNormalizedUserNameAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.NormalizedUserName);
    }

    /// <summary>
    /// Gets the user identifier for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose identifier should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the identifier for the specified <paramref name="user"/>.</returns>
    public Task<string> GetUserIdAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.Id);
    }

    /// <summary>
    /// Gets the user name for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the name for the specified <paramref name="user"/>.</returns>
    public Task<string> GetUserNameAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.UserName);
    }

    /// <summary>
    /// Get the claims associated with the specified <paramref name="user"/> as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose claims should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a user.</returns>
    public async Task<IList<Claim>> GetClaimsAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        AccountsUser dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
        return dbUser?.Claims?.Select(x => new Claim(x.ClaimType, x.ClaimValue))?.ToList() ?? new List<Claim>();
    }

    /// <summary>
    /// Sets the given normalized name for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be set.</param>
    /// <param name="normalizedName">The normalized name to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetNormalizedUserNameAsync(AccountsUser user, string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.NormalizedUserName = normalizedName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be set.</param>
    /// <param name="userName">The user name to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetUserNameAsync(AccountsUser user, string userName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.UserName = userName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the email address for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object containing the results of the asynchronous operation, the email address for the specified <paramref name="user"/>.</returns>
    public Task<string> GetEmailAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.Email);
    }

    /// <summary>
    /// Gets a flag indicating whether the email address for the specified <paramref name="user"/> has been verified, true if the email address is verified otherwise
    /// false.
    /// </summary>
    /// <param name="user">The user whose email confirmation status should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous operation, a flag indicating whether the email address for the specified <paramref name="user"/>
    /// has been confirmed or not.
    /// </returns>
    public Task<bool> GetEmailConfirmedAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.EmailConfirmed);
    }

    /// <summary>
    /// Gets the user, if any, associated with the specified, normalized email address.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified normalized email address.
    /// </returns>
    public Task<AccountsUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return _userCollection.AsQueryable().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Returns the normalized email for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email address to retrieve.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the normalized email address if any associated with the specified user.
    /// </returns>
    public Task<string> GetNormalizedEmailAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.NormalizedEmail);
    }

    /// <summary>
    /// Sets the flag indicating whether the specified <paramref name="user"/>'s email address has been confirmed or not.
    /// </summary>
    /// <param name="user">The user whose email confirmation status should be set.</param>
    /// <param name="confirmed">A flag indicating if the email address has been confirmed, true if the address is confirmed otherwise false.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task SetEmailConfirmedAsync(AccountsUser user, bool confirmed, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the normalized email for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email address to set.</param>
    /// <param name="normalizedEmail">The normalized email to set for the specified <paramref name="user"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task SetNormalizedEmailAsync(AccountsUser user, string normalizedEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the <paramref name="email"/> address for a <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email should be set.</param>
    /// <param name="email">The email to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task SetEmailAsync(AccountsUser user, string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.Email = email;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the current failed access count for the specified <paramref name="user"/>..
    /// </summary>
    /// <param name="user">The user whose failed access count should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the failed access count.</returns>
    public Task<int> GetAccessFailedCountAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.AccessFailedCount);
    }

    /// <summary>
    /// Retrieves a flag indicating whether user lockout can enabled for the specified user.
    /// </summary>
    /// <param name="user">The user whose ability to be locked out should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, true if a user can be locked out, otherwise false.
    /// </returns>
    public Task<bool> GetLockoutEnabledAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.LockoutEnabled);
    }

    /// <summary>
    /// Records that a failed access has occurred, incrementing the failed access count.
    /// </summary>
    /// <param name="user">The user whose cancellation count should be incremented.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the incremented failed access count.</returns>
    public Task<int> IncrementAccessFailedCountAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    /// <summary>
    /// Resets a user's failed access count.
    /// </summary>
    /// <param name="user">The user whose failed access count should be reset.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>This is typically called after the account is successfully accessed.</remarks>
    public Task ResetAccessFailedCountAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the last <see cref="DateTimeOffset"/> a user's last lockout expired, if any.
    /// Any time in the past should be indicates a user is not locked out.
    /// </summary>
    /// <param name="user">The user whose lockout date should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a <see cref="DateTimeOffset"/> containing the last time
    /// a user's lockout expired, if any.
    /// </returns>
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.LockoutEnd);
    }

    /// <summary>
    /// Locks out a user until the specified end date has passed. Setting a end date in the past immediately unlocks a user.
    /// </summary>
    /// <param name="user">The user whose lockout date should be set.</param>
    /// <param name="lockoutEnd">The <see cref="DateTimeOffset"/> after which the <paramref name="user"/>'s lockout should end.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetLockoutEndDateAsync(AccountsUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Set the flag indicating if the specified <paramref name="user"/> can be locked out..
    /// </summary>
    /// <param name="user">The user whose ability to be locked out should be set.</param>
    /// <param name="enabled">A flag indicating if lock out can be enabled for the specified <paramref name="user"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetLockoutEnabledAsync(AccountsUser user, bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the <paramref name="login"/> given to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the login to.</param>
    /// <param name="login">The login to add to the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task AddLoginAsync(AccountsUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (login == null)
        {
            throw new ArgumentNullException(nameof(login));
        }

        var iul = new IdentityUserLogin<string>
        {
            UserId = user.Id,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName,
            ProviderKey = login.ProviderKey
        };

        user.Logins.Add(iul);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the <paramref name="loginProvider"/> given from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the login from.</param>
    /// <param name="loginProvider">The login to remove from the user.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task RemoveLoginAsync(AccountsUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        _ = user.Logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the user associated with the specified login provider and login provider key.
    /// </summary>
    /// <param name="loginProvider">The login provider who provided the <paramref name="providerKey"/>.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> for the asynchronous operation, containing the user, if any which matched the specified login provider and key.
    /// </returns>
    public async Task<AccountsUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return await _userCollection.AsQueryable().FirstOrDefaultAsync(u =>
            u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken).ConfigureAwait(true);
    }

    /// <summary>
    /// Retrieves the associated logins for the specified <param ref="user"/>.
    /// </summary>
    /// <param name="user">The user whose associated logins to retrieve.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="UserLoginInfo"/> for the specified <paramref name="user"/>, if any.
    /// </returns>
    public async Task<IList<UserLoginInfo>> GetLoginsAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        AccountsUser dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

        return dbUser?.Logins?.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))?.ToList() ?? new List<UserLoginInfo>();
    }

    /// <summary>
    /// Gets the password hash for a user.
    /// </summary>
    /// <param name="user">The user to retrieve the password hash for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the password hash for the user.</returns>
    public Task<string> GetPasswordHashAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.PasswordHash);
    }

    /// <summary>
    /// Returns a flag indicating if the specified user has a password.
    /// </summary>
    /// <param name="user">The user to retrieve the password hash for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user has a password. If the 
    /// user has a password the returned value with be true, otherwise it will be false.</returns>
    public Task<bool> HasPasswordAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.PasswordHash != null);
    }

    /// <summary>
    /// Sets the password hash for a user.
    /// </summary>
    /// <param name="user">The user to set the password hash for.</param>
    /// <param name="passwordHash">The password hash to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetPasswordHashAsync(AccountsUser user, string passwordHash, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.PasswordHash = passwordHash;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the telephone number, if any, for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telephone number should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telephone number, if any.</returns>
    public Task<string> GetPhoneNumberAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.PhoneNumber);
    }

    /// <summary>
    /// Gets a flag indicating whether the specified <paramref name="user"/>'s telephone number has been confirmed.
    /// </summary>
    /// <param name="user">The user to return a flag for, indicating whether their telephone number is confirmed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified <paramref name="user"/> has a confirmed
    /// telephone number otherwise false.
    /// </returns>
    public Task<bool> GetPhoneNumberConfirmedAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    /// <summary>
    /// Sets the telephone number for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telephone number should be set.</param>
    /// <param name="phoneNumber">The telephone number to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetPhoneNumberAsync(AccountsUser user, string phoneNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets a flag indicating if the specified <paramref name="user"/>'s phone number has been confirmed..
    /// </summary>
    /// <param name="user">The user whose telephone number confirmation status should be set.</param>
    /// <param name="confirmed">A flag indicating whether the user's telephone number has been confirmed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetPhoneNumberConfirmedAsync(AccountsUser user, bool confirmed, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.PhoneNumberConfirmed = confirmed;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the given <paramref name="normalizedRoleName"/> to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the role to.</param>
    /// <param name="normalizedRoleName">The role to add.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task AddToRoleAsync(AccountsUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            throw new ArgumentNullException("Value cannot be null or empty.", nameof(normalizedRoleName));
        }

        AccountsRole roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (roleEntity == null)
        {
            throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.CurrentCulture, "Role {0} does not exist.", normalizedRoleName));
        }

        user.Roles.Add(roleEntity.Id);
    }

    /// <summary>
    /// Removes the given <paramref name="normalizedRoleName"/> from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the role from.</param>
    /// <param name="normalizedRoleName">The role to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task RemoveFromRoleAsync(AccountsUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            throw new ArgumentNullException("Value cannot be null or empty.", nameof(normalizedRoleName));
        }

        AccountsRole roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (roleEntity != null)
        {
            _ = user.Roles.Remove(roleEntity.Id);
        }
    }

    /// <summary>
    /// Retrieves all users in the specified role.
    /// </summary>
    /// <param name="normalizedRoleName">The role whose users should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> contains a list of users, if any, that are in the specified role.
    /// </returns>
    public async Task<IList<AccountsUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(normalizedRoleName))
        {
            throw new ArgumentNullException(nameof(normalizedRoleName));
        }

        AccountsRole role = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (role == null)
        {
            return new List<AccountsUser>();
        }

        FilterDefinition<AccountsUser> filter = Builders<AccountsUser>.Filter.AnyEq(x => x.Roles, role.Id);
        return (await _userCollection.FindAsync(filter, FindOptions, cancellationToken).ConfigureAwait(true)).ToList();
    }

    /// <summary>
    /// Retrieves the roles the specified <paramref name="user"/> is a member of.
    /// </summary>
    /// <param name="user">The user whose roles should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
    public async Task<IList<string>> GetRolesAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        AccountsUser userDb = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
        if (userDb == null)
        {
            return new List<string>();
        }

        var roles = new List<string>();
        foreach (string item in userDb.Roles)
        {
            AccountsRole dbRole = await _roleCollection.AsQueryable().FirstOrDefaultAsync(r => r.Id.Equals(item), cancellationToken).ConfigureAwait(true);
            if (dbRole != null)
            {
                roles.Add(dbRole.Name);
            }
        }
        return roles;
    }

    /// <summary>
    /// Returns a flag indicating if the specified user is a member of the give <paramref name="normalizedRoleName"/>.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <param name="normalizedRoleName">The role to check membership of</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user is a member of the given group. If the
    /// user is a member of the group the returned value with be true, otherwise it will be false.</returns>
    public async Task<bool> IsInRoleAsync(AccountsUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        AccountsUser dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

        AccountsRole role = await FindRoleAsync(normalizedRoleName, cancellationToken).ConfigureAwait(true);

        if (role == null)
        {
            return false;
        }

        return dbUser?.Roles.Contains(role.Id) ?? false;
    }

    /// <summary>
    /// Get the security stamp for the specified <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user whose security stamp should be set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
    public Task<string> GetSecurityStampAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.SecurityStamp);
    }

    /// <summary>
    /// Sets the provided security <paramref name="stamp"/> for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose security stamp should be set.</param>
    /// <param name="stamp">The security stamp to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetSecurityStampAsync(AccountsUser user, string stamp, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (stamp == null)
        {
            throw new ArgumentNullException(nameof(stamp));
        }

        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the recovery codes for the user while invalidating any previous recovery codes.
    /// </summary>
    /// <param name="user">The user to store new recovery codes for.</param>
    /// <param name="recoveryCodes">The new recovery codes for the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The new recovery codes for the user.</returns>
    public Task ReplaceCodesAsync(AccountsUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken = default)
    {
        string mergedCodes = string.Join(";", recoveryCodes);
        return SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
    }

    /// <summary>
    /// Returns whether a recovery code is valid for a user. Note: recovery codes are only valid
    /// once, and will be invalid after use.
    /// </summary>
    /// <param name="user">The user who owns the recovery code.</param>
    /// <param name="code">The recovery code to use.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>True if the recovery code was found for the user.</returns>
    public async Task<bool> RedeemCodeAsync(AccountsUser user, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (code == null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        string mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
        string[] splitCodes = mergedCodes.Split(';');
        if (splitCodes.Contains(code))
        {
            var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
            await ReplaceCodesAsync(user, updatedCodes, cancellationToken);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns how many recovery code are still valid for a user.
    /// </summary>
    /// <param name="user">The user who owns the recovery code.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The number of valid recovery codes for the user..</returns>
    public async Task<int> CountCodesAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        string mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
        if (mergedCodes.Length > 0)
        {
            return mergedCodes.Split(';').Length;
        }
        return 0;
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled or not,
    /// as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose two factor authentication enabled status should be set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified 
    /// <paramref name="user"/> has two factor authentication enabled or not.
    /// </returns>
    public Task<bool> GetTwoFactorEnabledAsync(AccountsUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return Task.FromResult(user.TwoFactorEnabled);
    }

    /// <summary>
    /// Sets a flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled or not,
    /// as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose two factor authentication enabled status should be set.</param>
    /// <param name="enabled">A flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetTwoFactorEnabledAsync(AccountsUser user, bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        user.TwoFactorEnabled = enabled;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Throws if this class has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Dispose the store
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
    }

    #region private
    /// <summary>
    /// Return a role with the normalized name if it exists.
    /// </summary>
    /// <param name="normalizedRoleName">The normalized role name.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The role if it exists.</returns>
    private Task<AccountsRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        return _roleCollection.AsQueryable().FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken);
    }

    private async Task<IdentityUserToken<string>?> FindTokenAsync(AccountsUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
    {
        AccountsUser dbUser = await ByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
        return dbUser?.Tokens?.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);
    }

    private Task<AccountsUser> ByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return _userCollection.AsQueryable().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }
    #endregion
}

