using Task4.Web.Models;

namespace Task4.Web.Services;

public interface IUserManagementService
{
    Task<IReadOnlyList<ApplicationUser>> GetUsersAsync();

    Task BlockUsersAsync(IEnumerable<string> userIds);

    Task UnblockUsersAsync(IEnumerable<string> userIds);

    Task DeleteUsersAsync(IEnumerable<string> userIds);

    Task DeleteUnverifiedUsersAsync();

    Task<IReadOnlyDictionary<string, int[]>> GetActivitySummaryAsync(
        IEnumerable<string> userIds);
}