using Microsoft.AspNetCore.Identity;

namespace Victa.Backend.Accounts.Domain.Models.UserAggregate;

public class AccountsUser : IdentityUser<string>
{
    public AccountsUser()
    {
    }

    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public List<string> Roles { get; set; }
    public List<IdentityUserClaim<string>> Claims { get; set; }
    public List<IdentityUserLogin<string>> Logins { get; set; }
    public List<IdentityUserToken<string>> Tokens { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string[] FirebaseTokens { get; set; }
    public CustomerDetails CustomerDetails { get; set; }
    public UserState UserState { get; set; }

    public static AccountsUser Customer()
    {
        throw new InvalidOperationException();
    }
}
