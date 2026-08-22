using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task4.Web.Data;
using Task4.Web.Models;

namespace Task4.Web.Services;

public class UserManagementService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext)
    : IUserManagementService
{
    public async Task<IReadOnlyList<ApplicationUser>> GetUsersAsync()
    {
        return await userManager.Users
            .OrderByDescending(x => x.LastActionTime)
            .ToListAsync();
    }

    public async Task BlockUsersAsync(IEnumerable<string> userIds)
    {
        var users = await GetUsersByIdsAsync(userIds);

        foreach (var user in users)
        {
            user.Status = UserStatus.Blocked;
            user.LastActionTime = DateTime.UtcNow;

            dbContext.UserActivities.Add(new UserActivity
            {
                UserId = user.Id,
                OccurredAt = DateTime.UtcNow,
                ActivityType = UserActivityType.Block
            });
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task UnblockUsersAsync(IEnumerable<string> userIds)
    {
        var users = await GetUsersByIdsAsync(userIds);

        foreach (var user in users)
        {
            user.Status = UserStatus.Active;
            user.LastActionTime = DateTime.UtcNow;

            dbContext.UserActivities.Add(new UserActivity
            {
                UserId = user.Id,
                OccurredAt = DateTime.UtcNow,
                ActivityType = UserActivityType.Unblock
            });
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteUsersAsync(IEnumerable<string> userIds)
    {
        var users = await GetUsersByIdsAsync(userIds);

        // Activity yozuvi qo'shilmaydi: foydalanuvchi shu yerda o'chiriladi,
        // shuning uchun UserId FK'ga ega bo'lgan yozuv saqlab bo'lmaydi
        // (va u baribir cascade orqali o'chib ketardi).
        foreach (var user in users)
        {
            await userManager.DeleteAsync(user);
        }
    }

    public async Task DeleteUnverifiedUsersAsync()
    {
        var users = await userManager.Users
            .Where(x => x.Status == UserStatus.Unverified)
            .ToListAsync();

        foreach (var user in users)
        {
            await userManager.DeleteAsync(user);
        }
    }

    private async Task<List<ApplicationUser>> GetUsersByIdsAsync(
        IEnumerable<string> userIds)
    {
        var ids = userIds.ToList();

        return await userManager.Users
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }
}