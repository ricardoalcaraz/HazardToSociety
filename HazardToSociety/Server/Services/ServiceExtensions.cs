using System;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HazardToSociety.Server.Services;

public static class ServiceExtensions
{
    public static IHttpClientBuilder AddWeatherClient(this IServiceCollection services) =>
        services.AddHttpClient<IWeatherClient, WeatherClient>()
            .ConfigureHttpClient((context, client) =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                var token = config["NoaaApiKey"];
                if (string.IsNullOrWhiteSpace(token)) 
                    throw new ArgumentNullException(nameof(token));
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