namespace Victa.Backend.Accounts.Contracts.Input.Accounts;

public class PasswordRegistrationBody
{
    public int Age { get; set; }
    public string UserName { get; set; }
    public string Name
    {
        get => UserName;
        set => UserName = value;
    }

    public string Email { get; set; }
    public string Password { get; set; }
    public SGender Gender { get; set; }
    public string? InvitedBy { get; set; }
    public string? PromotedBy { get; set; }
}
