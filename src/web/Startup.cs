using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pika.Lib;
using Pika.Lib.Command;
using Pika.Lib.Store;
using Pika.Web.HostedServices;

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
        services.AddLogging(config =>
        {
            config.SetMinimumLevel(LogLevel.Debug);
            config.AddConsole();
            config.AddDebug();
        });

        services.Configure<PikaOptions>(Configuration);
        services.AddSingleton<ICommandClient, ProcessCommandClient>();
        services.AddSingleton<IDbRepository, SqliteDbRepository>();

        services.AddHostedService<StartupHostedService>();
        services.AddHostedService<TaskHostedService>();

        services.AddControllersWithViews();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });
    }
}