using System;
using HazardToSociety.Server.Models;
using HazardToSociety.Server.Services;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
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
        services.AddSingleton<IQueryBuilderService, QueryBuilderService>();
        services.AddMediatR(typeof(Startup));
        services.AddWeatherContext(_configuration);
        if (_configuration.GetValue<bool>("EnableBackgroundService"))
        {
            services.AddHostedService<WeatherService>();
        }

        
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        logger.LogInformation("Starting in {Environment} environment", env.EnvironmentName);
        logger.LogInformation("ApiKey:{ApiKey}, UpdateTime:{Time}", 
            _configuration["NoaaApiKey"], 
            _configuration["UpdateTime"]);
            
        if (env.IsDevelopment())
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
        //app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("index.html");
        });
    }
}

public static class ServiceExtensions
{
    public static IHttpClientBuilder AddWeatherClient(this IServiceCollection services) =>
        services.AddHttpClient<IWeatherClient, WeatherClient>()
            .ConfigureHttpClient((context, client) =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                var token = config["NoaaApiKey"];
                client.DefaultRequestHeaders.Add("token", token);
                client.BaseAddress = new Uri("https://www.ncdc.noaa.gov/cdo-web/api/v2/");
            });

    public static IServiceCollection AddWeatherContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WeatherContext>(options => 
        {
            var database = configuration.GetValue<Database>("DatabaseProvider");
            switch (database)
            {
                case Database.SqlLite:
                    const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
                    var path = Environment.GetFolderPath(folder);
                    var dbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}blogging.db";
                    var connectionString = $"Data Source={dbPath}";
                    options.UseSqlite(connectionString);
                    break;
                case Database.SqlServer:
                    options.UseSqlServer(configuration.GetConnectionString("WeatherContext"));
                    break;
                case Database.Postgres:
                    throw new NotImplementedException();
                case Database.Invalid:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        return services;
    }
}

public enum Database
{
    Invalid,
    SqlLite,
    SqlServer,
    Postgres
}