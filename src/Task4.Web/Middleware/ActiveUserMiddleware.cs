using Microsoft.AspNetCore.Identity;
using Task4.Web.Models;

namespace Task4.Web.Middleware;

public class ActiveUserMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedAnonymousPathPrefixes =
    [
        "/account/login",
        "/account/register",
        "/account/registersuccess",
        "/account/confirmemail",
        "/account/logout",
        "/lib/",
        "/css/",
        "/js/",
        "/favicon.ico"
    ];

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        var isAllowedAnonymousPath = AllowedAnonymousPathPrefixes
            .Any(prefix => path.StartsWith(prefix, StringComparison.Ordinal));

        if (!isAllowedAnonymousPath &&
            context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user is null || user.Status == UserStatus.Blocked)
            {
                await signInManager.SignOutAsync();
                context.Response.Redirect("/Account/Login");
                return;
            }
        }

        await next(context);
    }
}