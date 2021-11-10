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
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();
                    var weatherClient = scope.ServiceProvider.GetRequiredService<IWeatherClient>();
                    
                    //todo parallelize requests for faster processing
                    var allLocations = await dbContext.Locations
                        .Where(l => l.Country == "US")
                        .ToDictionaryAsync(options => options.NoaaId, l => l, stoppingToken);


                    var dataQueries = GetDataQueries(allLocations.Values).ToList();
                    foreach (var dataQuery in dataQueries)
                    {
                        var asyncDataSet = weatherClient.GetAllData(dataQuery, stoppingToken);
                        
                        var noaaDataset = new List<NoaaData>();
                        await foreach (var dataset in asyncDataSet.WithCancellation(stoppingToken))
                        {
                            noaaDataset.Add(dataset);
                        }

                        var dataByKey = noaaDataset
                            .GroupBy(d => new {d.DataType, d.Date})
                            .Select(g => new
                            {
                                DataType = g.Key,
                                Average = g.Select(a => a.Value).Average(),
                                Max = g.Select(a => a.Value).Max(),
                                Min = g.Select(a => a.Value).Min(),
                            });

                        foreach (var entry in dataByKey)
                        {
                            var date = entry.DataType.Date;
                            var locationDataPoint = new LocationDataPoint()
                            {
                                Average = entry.Average,
                                Date = date.Date,
                                LocationId = allLocations[dataQuery.LocationId].Id,
                                Max = entry.Max,
                                Min = entry.Min
                            };
                            dbContext.LocationDataPoints.Add(locationDataPoint);
                        }

                        
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
        
        public IEnumerable<NoaaDataOptions> GetDataQueries(IEnumerable<Location> locations)
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