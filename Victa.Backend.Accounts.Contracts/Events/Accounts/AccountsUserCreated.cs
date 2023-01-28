using Victa.Backend.Accounts.Contracts.Output.Accounts;

namespace Victa.Backend.Accounts.Contracts.Events.Accounts;

public sealed class AccountsUserCreated
{
    public OAccountsUser AccountsUser { get; set; }
}
