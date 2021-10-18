using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Mediatr.Command;
using HazardToSociety.Server.Mediatr.Query;
using HazardToSociety.Shared.Models;
using MediatR;
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
                    var locations = await mediator.Send(new AddLocationsToTrack(), stoppingToken);
                    foreach (var location in locations)
                    {
                        //dataset of interests goes here
                        var dataQuery = new DataQuery()
                        {
                            DataSetId = "GHCND",
                            StartDate = location.MaxDate - TimeSpan.FromDays(2),
                            EndDate = location.MaxDate,
                            LocationId = location.NoaaId,
                            Units = TempUnit.Standard
                        };
                        var data = await mediator.Send(dataQuery, stoppingToken);
                        var groupedData = data
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
                            _logger.LogInformation("{DataType}, {Average}, {Count}", entry.DataType, entry.Average, entry.Count);
                        }
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