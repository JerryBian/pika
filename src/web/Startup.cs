using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pika.Lib;
using Pika.Lib.Command;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.HostedServices;
using System;

namespace Pika.Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddLogging(config =>
        {
            _ = config.SetMinimumLevel(LogLevel.Debug);
            _ = config.AddConsole();
            _ = config.AddDebug();
        });

        _ = services.Configure<PikaOptions>(Configuration);
        _ = services.AddSingleton<PikaSetting>();
        _ = services.AddSingleton<ICommandManager, CommandManager>();
        _ = services.AddSingleton<IDbRepository, SqliteDbRepository>();

        _ = services.AddHostedService<StartupHostedService>();
        _ = services.AddHostedService<TaskHostedService>();

        _ = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = ".APP.AUTH";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.Cookie.HttpOnly = true;
            options.ReturnUrlParameter = "returnUrl";
            options.LoginPath = new PathString("/login");
            options.LogoutPath = new PathString("/logout");
        });

        _ = services.AddControllersWithViews(config =>
        {
            AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            config.Filters.Add(new AuthorizeFilter(policy));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Home/Error");

        _ = app.UseStaticFiles();

        _ = app.UseRouting();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });
    }
}