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
        await userManagementService.BlockUsersAsync(selectedUserIds);

        TempData["Success"] = "Selected users have been blocked.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(
        IEnumerable<string> selectedUserIds)
    {
        await userManagementService.UnblockUsersAsync(selectedUserIds);

        TempData["Success"] = "Selected users have been unblocked.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        IEnumerable<string> selectedUserIds)
    {
        await userManagementService.DeleteUsersAsync(selectedUserIds);

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