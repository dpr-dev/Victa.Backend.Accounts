namespace Victa.Backend.Accounts.Domain.Models.UserAggregate;

public class PasswordRecoveryToken
{
    public string RecoveryCode { get; set; }
    public PasswordRecoveryTokenState TokenState { get; set; }
    public DateTime CreatedDate { get; set; }
}

public enum PasswordRecoveryTokenState
{
    Used,
    Pending,
    Expired,
}
