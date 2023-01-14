namespace Victa.Backend.Accounts.Contracts.Output.Accounts;

public class OAccountsUser
{
    public string Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? Picture { get; set; }
    public string? ZoneInfo { get; set; }
    public string? Locale { get; set; }
    public List<string> Roles { get; set; }
    public List<string> FirebaseTokens { get; set; }
    public SGender? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public double? Height { get; set; }
    public string? InvitedBy { get; set; }
    public string? PromotedBy { get; set; }
    public int? Age { get; set; }
    public int? Weight { get; set; }
    public OCompletionSteps CompletionSteps { get; set; }

    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
