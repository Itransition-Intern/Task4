namespace Task4.Web.Models;

public class UserActivity
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public ActivityType ActivityType { get; set; }

    public ApplicationUser User { get; set; } = null!;
}