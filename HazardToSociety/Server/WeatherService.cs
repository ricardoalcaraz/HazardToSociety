using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server
{
    public class WeatherService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IWeatherClient _weatherClient;
        private readonly ILogger<WeatherService> _logger;
        private readonly int _updateTime;
        
        public WeatherService(IConfiguration configuration, IWeatherClient weatherClient, ILogger<WeatherService> logger)
        {
            _configuration = configuration;
            _weatherClient = weatherClient;
            _logger = logger;
            _updateTime = _configuration.GetValue<int>("UpdateTime");
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_updateTime, stoppingToken);
                await UpdateData(stoppingToken);
            }
        }

        private async Task UpdateData(CancellationToken cancellationToken)
        {
            var locations = await _weatherClient.GetLocations(cancellationToken);
            foreach (var location in locations.Results)
            {
                var data = await _weatherClient.GetData(location, cancellationToken);
                _logger.LogInformation("Data received: {Data}", data);
            }
            _logger.LogDebug("Received: {Locations}", locations);
        }
    }
}