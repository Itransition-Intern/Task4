using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Task4.Web.Models;
using Task4.Web.Services;

namespace Task4.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    EmailQueue emailQueue)
    : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string name,
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(string.Empty, "All fields are required.");
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = name,
            Status = UserStatus.Unverified,
            RegistrationTime = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationUrl = Url.Action(
            nameof(ConfirmEmail),
            "Account",
            new
            {
                userId = user.Id,
                token
            },
            Request.Scheme);
        
        await emailQueue.EnqueueAsync(
            new ConfirmationEmail(
                user.Email!,
                confirmationUrl!));

        // Console.WriteLine($"CONFIRMATION URL: {confirmationUrl}");

        return RedirectToAction(nameof(RegisterSuccess));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(
        string userId,
        string token)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return NotFound();

        var result =
            await userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            return BadRequest("Invalid confirmation link.");

        if (user.Status == UserStatus.Unverified)
        {
            user.Status = UserStatus.Active;
            await userManager.UpdateAsync(user);
        }

        return View();
    }

    [HttpGet]
    public IActionResult RegisterSuccess()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password,
        string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(
                string.Empty,
                "Email and password are required.");

            return View();
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View();
        }

        if (user.Status == UserStatus.Blocked)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is blocked.");

            return View();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View();
        }

        user.LastLoginTime = DateTime.UtcNow;
        user.LastActionTime = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        return LocalRedirect(
            returnUrl ?? Url.Action("Index", "Users")!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }
}