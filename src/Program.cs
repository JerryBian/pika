using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Pika.Common;
using Pika.Common.Command;
using Pika.Common.Drive;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.HostedServices;
using Pika.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PikaOptions>(builder.Configuration);
builder.Services.AddSingleton<PikaSetting>();
builder.Services.AddSingleton<IPikaDriveOps, PikaDriveOps>();
builder.Services.AddSingleton<ICommandManager, CommandManager>();
builder.Services.AddSingleton<IPikaStore, PikaStore>();

builder.Services.AddHostedService<StartupHostedService>();
builder.Services.AddHostedService<TaskHostedService>();
builder.Services.AddHostedService<PikaDriveHostedService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".APP.AUTH";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.ReturnUrlParameter = "returnUrl";
    options.LoginPath = new PathString("/login");
    options.LogoutPath = new PathString("/logout");
});

builder.Services.AddControllersWithViews(config =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<ScriptHub>("/hub/script");
app.MapHub<DriveHub>("/hub/drive");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();