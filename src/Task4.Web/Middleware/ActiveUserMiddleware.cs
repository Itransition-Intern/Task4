using Microsoft.AspNetCore.Identity;
using Task4.Web.Models;

namespace Task4.Web.Middleware;

// Login/registratsiyadan tashqari har bir so'rovda tekshiradi: signed-in
// foydalanuvchi hali ham mavjudmi va bloklanmaganmi. Aks holda uning
// autentifikatsiya cookie'si cookie muddati tugagunicha amal qilib
// qoladi — bu esa bloklangan/o'chirilgan foydalanuvchiga ilovadan
// foydalanishni davom ettirish imkonini beradi.
public class ActiveUserMiddleware(RequestDelegate next)
{
    // Foydalanuvchi signOut qilinayotganda ham yeta olishi kerak bo'lgan
    // yo'llar — aks holda pastdagi redirect cheksiz aylanaga aylanadi.
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

            // user null bo'lsa — hisob cookie berilgandan keyin o'chirilgan;
            // Blocked bo'lsa — boshqa taqiqlangan holat.
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