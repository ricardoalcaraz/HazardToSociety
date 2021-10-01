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

namespace HazardToSociety.Server
{
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
            services.AddTransient<IWeatherClient, WeatherClient>();
            services.AddLogging();
            services.AddHttpClient();
            services.AddSingleton<IQueryBuilderService, QueryBuilderService>();
            services.AddMediatR(typeof(Startup));
            if (_configuration.GetValue<bool>("EnableBackgroundService"))
            {
                services.AddHostedService<WeatherService>();
            }

            services.AddDbContext<WeatherContext>(
                options => options.UseSqlServer(_configuration.GetConnectionString("WeatherContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
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

    // Copyright (c) .NET Foundation. Licensed under the Apache License, Version 2.0.
}