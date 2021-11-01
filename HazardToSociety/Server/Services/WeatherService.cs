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
                    var dataQueriesForLocation = await dbContext.Locations
                        .Select(l => new NoaaDataOptions()
                        {
                            DataSetId = "GHCND",
                            StartDate = l.MaxDate - TimeSpan.FromDays(7),
                            EndDate = l.MaxDate,
                            LocationId = l.NoaaId,
                            Units = TempUnit.Standard
                        }).ToListAsync(stoppingToken);
                    
                    foreach (var dataQuery in dataQueriesForLocation)
                    {
                        var asyncDataSet = weatherClient.GetAllData(dataQuery, stoppingToken);
                        
                        var noaaDataset = new List<NoaaData>();
                        await foreach (var dataset in asyncDataSet.WithCancellation(stoppingToken))
                        {
                            noaaDataset.Add(dataset);
                        }

                        var groupedData = noaaDataset
                            .GroupBy(d => d.DataType)
                            .Select(g => new
                            {
                                DataType = g.Key,
                                Average = g.Select(a => a.Value).Average(),
                                Count = g.Count(),
                                Data = g.Select(a => a.Value)
                            });
                        foreach (var entry in groupedData)
                        {
                            //publish mediator event for each group of data
                            _logger.LogInformation("{DataType}, {Average}, {Count}", entry.DataType, entry.Average, entry.Count);
                        }
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
    }
}