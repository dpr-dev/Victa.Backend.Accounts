namespace Victa.Backend.Accounts.Contracts.Input.Accounts;

public sealed class RestorePasswordWithRecoveryCodeBody
{ 
    public string Email { get; set; }
    public string RecoveryCode { get; set; }
    public string Password { get; set; }
}
