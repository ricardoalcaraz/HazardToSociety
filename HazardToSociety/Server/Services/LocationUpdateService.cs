using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Services;

public class LocationUpdateService
{
    private readonly IWeatherClient _weatherClient;
    private readonly IDbContextFactory<WeatherContext> _dbContextFactory;
    private readonly ILogger<LocationUpdateService> _logger;

    public LocationUpdateService(IWeatherClient weatherClient, 
        IDbContextFactory<WeatherContext> dbContextFactory,
        ILogger<LocationUpdateService> logger)
    {
        _weatherClient = weatherClient;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
}