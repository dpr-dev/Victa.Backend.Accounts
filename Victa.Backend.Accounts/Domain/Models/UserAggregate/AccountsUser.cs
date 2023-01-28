using Microsoft.AspNetCore.Identity;

namespace Victa.Backend.Accounts.Domain.Models.UserAggregate;

public class AccountsUser : IdentityUser<string>
{
    public AccountsUser()
    {
    }

    public AccountsUser(
        string email,
        string userName,
        List<string>? roles = null)
    {
        Id = Guid
            .NewGuid()
            .ToShortString();

        Email = email;
        UserName = userName;
        FirebaseTokens = new List<string>();
        Roles = roles
            ?? new List<string>();

        Claims = new List<IdentityUserClaim<string>>();
        Logins = new List<IdentityUserLogin<string>>();
        Tokens = new List<IdentityUserToken<string>>();
        PasswordRecoveryTokens = new List<PasswordRecoveryToken>();
        CompletionSteps = new CompletionSteps
        {
        };

    }

    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? Picture { get; set; }
    public string? ZoneInfo { get; set; }
    public string? Locale { get; set; }
    public List<string> Roles { get; set; }
    public List<string> FirebaseTokens { get; set; }
    public List<IdentityUserClaim<string>> Claims { get; set; }
    public List<IdentityUserLogin<string>> Logins { get; set; }
    public List<IdentityUserToken<string>> Tokens { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public double? Height { get; set; }
    public string? InvitedBy { get; set; }
    public string? PromotedBy { get; set; }
    public int? Age { get; set; }
    public int? Weight { get; set; }

    /// <summary>
    /// For backward capability
    /// </summary>
    [Obsolete]
    public string? AvatarId { get; set; }
    public CompletionSteps CompletionSteps { get; set; }
    public List<PasswordRecoveryToken> PasswordRecoveryTokens { get; set; }

    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public async Task<PasswordRecoveryToken> GeneratePasswordRecoveryToken(Func<AccountsUser, Task<string>> recoveryTokenGenerator)
    {
        if (PasswordRecoveryTokens is null)
        {
            PasswordRecoveryTokens = new List<PasswordRecoveryToken>();
        }

        PasswordRecoveryTokens.ForEach(token =>
        {
            token.TokenState = PasswordRecoveryTokenState.Expired;
        });

        var recoveryToken = new PasswordRecoveryToken
        {
            CreatedDate = DateTime.UtcNow,
            TokenState = PasswordRecoveryTokenState.Pending,
            RecoveryCode = await recoveryTokenGenerator(this),
        };

        PasswordRecoveryTokens.Add(recoveryToken);

        return recoveryToken;
    }

    public async Task<IdentityResult> ChangePassword(
        Func<string, string> hashPassword,
        Func<string, Task<IdentityResult>> validatePassword,
        string password)
    {
        if (hashPassword is null)
        {
            throw new ArgumentNullException(nameof(hashPassword));
        }

        if (validatePassword is null)
        {
            throw new ArgumentNullException(nameof(validatePassword));
        }

        IdentityResult validationResult = await validatePassword(password);
        if (!validationResult.Succeeded)
        {
            return validationResult;
        }

        PasswordHash = hashPassword(password);

        return IdentityResult.Success;
    }
}
