namespace Victa.Backend.Accounts.Contracts.Events.Accounts;

public sealed class RecoveryCodeCreated
{
    public string UserId { get; set; }
    public string RecoveryCode { get; set; }
}
