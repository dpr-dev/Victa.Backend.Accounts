using Microsoft.AspNetCore.Identity;

namespace Victa.Backend.Accounts.Domain.Models.RoleAggregate;

public class AccountsRole : IdentityRole<string>
{
    public List<IdentityRoleClaim<string>> Claims { get; set; }
}
