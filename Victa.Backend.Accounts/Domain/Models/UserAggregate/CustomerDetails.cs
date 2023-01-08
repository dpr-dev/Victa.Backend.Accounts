namespace Victa.Backend.Accounts.Domain.Models.UserAggregate;

public class CustomerDetails
{
    public Gender? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Picture { get; set; }
    public string? ZoneInfo { get; set; }
    public string? Locale { get; set; }
    public string? Address { get; set; }
    public string? AvatarId { get; set; }
    public int? Age { get; set; }
    public int? Weight { get; set; }
    public double? Height { get; set; }
    public string? InvitedBy { get; set; }
    public string? PromotedBy { get; set; }
}
