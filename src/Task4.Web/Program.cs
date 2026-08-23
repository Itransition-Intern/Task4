using Task4.Web.Services;
using Microsoft.EntityFrameworkCore;
using Task4.Web.Data;
using Task4.Web.Models;
using Task4.Web.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 1;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userManager =
            context.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.GetUserAsync(context.User);

        if (user is null || user.Status == UserStatus.Blocked)
        {
            await context.SignOutAsync(
                IdentityConstants.ApplicationScheme);

            context.Response.Redirect("/Account/Login");

            return;
        }
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapStaticAssets();

app.Run();