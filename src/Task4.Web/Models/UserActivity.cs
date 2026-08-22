namespace Task4.Web.Models;

public class UserActivity
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public DateTime OccurredAt { get; set; }

    public UserActivityType ActivityType { get; set; }
}