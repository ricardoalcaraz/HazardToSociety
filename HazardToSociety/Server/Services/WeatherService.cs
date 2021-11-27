using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Services
{
    public class WeatherService : BackgroundService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _updateTime;
        
        public WeatherService(IConfiguration configuration, 
            ILogger<WeatherService> logger, 
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _updateTime = configuration.GetValue<int>("UpdateTime");
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();
                    var weatherClient = scope.ServiceProvider.GetRequiredService<IWeatherClient>();
                    
                    var locations = (await dbContext.LocationOfInterests
                        .Include(l => l.Location)
                        .ToListAsync(cancellationToken: stoppingToken))
                        .Select(loi => loi.Location);
                    var dataPointTracker = new DataPointTracker();

                    foreach (var dataQuery in GetDataQueries(locations))
                    {
                        var asyncDataSet = weatherClient.GetAllData(dataQuery, stoppingToken);
                        // var weatherData = 
                        await foreach (var dataset in asyncDataSet.WithCancellation(stoppingToken))
                        {
                            dataPointTracker.AddRange(dataset);
                        }


                        var locationDataPoints = dataPointTracker.GetValues();
                        dbContext.LocationDataPoints.AddRange(locationDataPoints);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        //process data month by month for faster queries
                        //when a new location enters we process the entire history
                        //keep track of execution times?
                        //save appropriate temps
                    }
                    //get dataset options
                    //get data
                    //publish data to get processed
                    await Task.Delay(_updateTime, stoppingToken);
                }
                catch (Exception httpRequestException)
                {
                    _logger.LogError(httpRequestException, 
                        "Unable to retrieve data from source");
                }
            }
        }

        private static IEnumerable<NoaaDataOptions> GetDataQueries(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                var endDate = location.MinDate;
                while (endDate < location.MaxDate)
                {
                    var startDate = endDate;
                    endDate = startDate + TimeSpan.FromDays(7);

                    yield return new NoaaDataOptions
                    {
                        DataSetId = "GHCND",
                        StartDate = startDate,
                        EndDate = endDate,
                        LocationId = location.NoaaId,
                        Units = TempUnit.Standard
                    };
                }
            }
        }
    }
}