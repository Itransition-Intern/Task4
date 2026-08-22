using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task4.Web.Services;

namespace Task4.Web.Controllers;

[Authorize]
public class UsersController(
    IUserManagementService userManagementService)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await userManagementService.GetUsersAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(
        IEnumerable<string> selectedUserIds)
    {
        var ids = selectedUserIds?.ToList() ?? [];

        if (ids.Count == 0)
        {
            TempData["Error"] = "No users were selected.";
            return RedirectToAction(nameof(Index));
        }

        await userManagementService.BlockUsersAsync(ids);

        TempData["Success"] = "Selected users have been blocked.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(
        IEnumerable<string> selectedUserIds)
    {
        var ids = selectedUserIds?.ToList() ?? [];

        if (ids.Count == 0)
        {
            TempData["Error"] = "No users were selected.";
            return RedirectToAction(nameof(Index));
        }

        await userManagementService.UnblockUsersAsync(ids);

        TempData["Success"] = "Selected users have been unblocked.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        IEnumerable<string> selectedUserIds)
    {
        var ids = selectedUserIds?.ToList() ?? [];

        if (ids.Count == 0)
        {
            TempData["Error"] = "No users were selected.";
            return RedirectToAction(nameof(Index));
        }

        await userManagementService.DeleteUsersAsync(ids);

        TempData["Success"] = "Selected users have been deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnverified()
    {
        await userManagementService.DeleteUnverifiedUsersAsync();

        TempData["Success"] = "Unverified users have been deleted.";

        return RedirectToAction(nameof(Index));
    }
}