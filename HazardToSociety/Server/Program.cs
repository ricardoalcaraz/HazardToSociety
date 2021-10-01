using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHostBuilder = CreateHostBuilder(args).Build();

            var logger = webHostBuilder.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                using var scope = webHostBuilder.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();
                if (!dbContext.Database.EnsureCreated())
                {
                    logger.LogInformation("Migrating database");
                    dbContext.Database.Migrate();
                }
                else
                {
                    logger.LogInformation("Created new database");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to migrate database");
            }
            
            webHostBuilder.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}