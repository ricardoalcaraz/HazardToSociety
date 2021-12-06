using System.Reflection;
using HazardToSociety.Server.Profiles;
using HazardToSociety.Server.Services;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server;

public class Startup
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }


    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddLogging();
        services.AddWeatherClient();
        services.AddMediatR(typeof(Startup));
        services.AddWeatherContext();
        if (_config.GetValue<bool>("EnableBackgroundService"))
        {
            services.AddHostedService<WeatherService>();
        }

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<NoaaProfile>();
        }, Assembly.GetExecutingAssembly());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
    {
        logger.LogInformation("Starting in {Environment} environment", _env.EnvironmentName);
        logger.LogInformation("ApiKey:{ApiKey}", _config["NoaaApiKey"]);
        logger.LogInformation("UpdateTime:{Time}", _config["UpdateTime"]);
            
        if (_env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("index.html");
        });
    }
}

public enum Database
{
    Invalid,
    SqlLiteFile,
    SqlLiteMemory,
    SqlServer,
    Postgres
}