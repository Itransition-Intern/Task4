using Microsoft.AspNetCore.Identity;

namespace Task4.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Unverified;

    public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginTime { get; set; }

    public DateTime? LastActionTime { get; set; }

    public ICollection<UserActivity> Activities { get; set; }
        = [];
}