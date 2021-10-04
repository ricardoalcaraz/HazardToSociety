using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
                    var noaaLocationOptions = new NoaaLocationOptions()
                    {
                        SortField = SortField.Name,
                    };
                    var locations = await mediator.Send(new LocationsQuery(noaaLocationOptions), stoppingToken);
                    //get dataset options
                    //get data
                    //publish data to get processed
                    await Task.Delay(_updateTime, stoppingToken);
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogError(httpRequestException, 
                        "Unable to retrieve data from source");
                }
            }
        }
    }
}