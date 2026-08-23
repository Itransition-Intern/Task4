namespace Task4.Web.Models;

public class UserActivitySummary
{
    public string UserId { get; set; } = string.Empty;

    public int[] DailyActivity { get; set; } = new int[7];
}