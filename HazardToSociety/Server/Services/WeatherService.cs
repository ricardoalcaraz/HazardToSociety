using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Utilities;
using HazardToSociety.Shared;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Services
{
    public class WeatherService : BackgroundService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly IWeatherClient _weatherClient;
        private readonly int _updateTime;
        
        public WeatherService(IConfiguration configuration, ILogger<WeatherService> logger, IWeatherClient weatherClient)
        {
            _logger = logger;
            _weatherClient = weatherClient;
            _updateTime = configuration.GetValue<int>("UpdateTime");
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_updateTime, stoppingToken);
                try
                {
                    await UpdateData(stoppingToken);
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogError(httpRequestException, 
                        "Unable to retrieve data from source");
                }
            }
        }

        private async Task UpdateData(CancellationToken cancellationToken)
        {
            //read config options from db
            var locationOptions = new NoaaLocationOptions();
            await foreach (var location in _weatherClient.GetLocations(locationOptions, cancellationToken))
            {
                _logger.LogDebug("Processing: {Location}", location);
                var noaaDatasetOptions = new NoaaDatasetOptions()
                {
                    LocationId = location.Id
                };
                
                //filter datasets to allowed list
                await foreach (var dataset in _weatherClient.GetDataSet(noaaDatasetOptions, cancellationToken))
                {
                    _logger.LogDebug("Processing Dataset: {Dataset}", dataset);
                    var noaaDataOptions = new NoaaDataOptions
                    {
                        DataSetId = dataset.Id,
                        StartDate = dataset.MaxDate - TimeSpan.FromDays(1),
                        EndDate = dataset.MaxDate
                    };
                    //get data
                    await foreach (var data in _weatherClient.GetData(noaaDataOptions, cancellationToken))
                    {
                        _logger.LogDebug("Data received: {Data}", data);
                    }
                }
            }
        }
    }
}