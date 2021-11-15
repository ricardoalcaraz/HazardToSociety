using System;
using System.Linq;
using System.Threading.Tasks;
using HazardToSociety.Server.Mediatr.Command;
using HazardToSociety.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webHostBuilder = CreateHostBuilder(args).Build();

            var logger = webHostBuilder.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                using var scope = webHostBuilder.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();
                var mediatr = scope.ServiceProvider.GetRequiredService<IMediator>();
                logger.LogInformation("Migrating database");
                await dbContext.Database.MigrateAsync();
                var updatesNeeded = await dbContext.UpdateHistories
                    .Where(uh => uh.RequiresUpdates)
                    .ToListAsync();
                logger.LogInformation("The following updates are needed: {@Updates}", updatesNeeded);

                foreach (var updateNeeded in updatesNeeded)
                {
                    switch (updateNeeded.UpdateType)
                    {
                        case UpdateType.InitialSeeding: 
                            await mediatr.Send(new AddLocationsToTrack());
                            updateNeeded.DateUpdated = DateTime.Now;
                            updateNeeded.DataUpdated = nameof(AddLocationsToTrack);
                            updateNeeded.RequiresUpdates = false;
                            break;
                        case UpdateType.Invalid:
                        default:
                            logger.LogWarning("Received invalid Update Type:{UpdateType}", updateNeeded.UpdateType);
                            throw new ArgumentException(nameof(updateNeeded.UpdateType));
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to migrate database");
                throw;
            }
            
            await webHostBuilder.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}