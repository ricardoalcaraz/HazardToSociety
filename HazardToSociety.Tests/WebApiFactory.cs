using System;
using System.Collections.Generic;
using HazardToSociety.Server;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Tests;

public class WebApiFactory : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(cfg =>
        {
            var configOptions = new Dictionary<string, string>
            {
                ["DatabaseProvider"] = "SqlLiteMemory"
            };
            cfg.AddInMemoryCollection(configOptions);
        });
        
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WeatherContext>();
            var logger = scopedServices.GetRequiredService<ILogger<WebApiFactory>>();

            try
            {
                if (db.Database.EnsureCreated())
                    SeedData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                                    "database with test messages. Error: {Message}", ex.Message);
            }
        });
    }

    private static void SeedData(WeatherContext weatherContext)
    {
        weatherContext.LocationOfInterests.Add(new LocationOfInterest
        {
            Location = new Location
            {
                NoaaId = "CITY:US060013",
                MinDate = DateTime.Today - TimeSpan.FromDays(365),
                MaxDate = DateTime.Today
            }
        });
        weatherContext.SaveChanges();
    }
}