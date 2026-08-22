using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Task4.Web.Data;
using Task4.Web.Models;
using Task4.Web.Services;

namespace Task4.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    EmailQueue emailQueue,
    ApplicationDbContext dbContext)
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

        // Faqat muvaffaqiyatli signInda activity yoziladi va vaqt yangilanadi.
        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View();
        }

        var now = DateTime.UtcNow;

        user.LastLoginTime = now;
        user.LastActionTime = now;

        dbContext.UserActivities.Add(new UserActivity
        {
            UserId = user.Id,
            OccurredAt = now,
            ActivityType = UserActivityType.Login
        });

        await userManager.UpdateAsync(user);
        await dbContext.SaveChangesAsync();

        return LocalRedirect(
            returnUrl ?? Url.Action("Index", "Users")!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is not null)
        {
            user.LastActionTime = DateTime.UtcNow;

            dbContext.UserActivities.Add(new UserActivity
            {
                UserId = user.Id,
                OccurredAt = DateTime.UtcNow,
                ActivityType = UserActivityType.Logout
            });

            await dbContext.SaveChangesAsync();
        }

        await signInManager.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }
}