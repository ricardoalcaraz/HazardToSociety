using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared;
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
                    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WeatherContext>>();
                    var weatherClient = scope.ServiceProvider.GetRequiredService<IWeatherClient>();
                    
                    var locations = (await (await dbContextFactory.CreateDbContextAsync(stoppingToken)).LocationOfInterests
                        .Include(l => l.Location)
                        .ToListAsync(cancellationToken: stoppingToken))
                        .Select(loi => loi.Location);

                    var additionalOptions = new List<NoaaOptions>();
                    var pagedDatas = Array.Empty<NoaaPagedData<NoaaData, NoaaDataOptions>>();
                    var dataPoints = Enumerable.Empty<NoaaData>();
                    var saveChangesTasks = new List<Task<int>>();
                    var dataQueries = locations
                        .ToDataQueries(TimeSpan.FromDays(7))
                        .Chunk(5);
                    foreach (var dataQuery in dataQueries)
                    {
                        var remainingOptions = await GetRemainingOptions(stoppingToken, dataQuery);
                        additionalOptions.AddRange(remainingOptions);
                        
                        //process data month by month for faster queries
                        //when a new location enters we process the entire history
                        //keep track of execution times?
                        //save appropriate temps
                    }

                    await Task.WhenAll(saveChangesTasks);
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

        private static async Task<IEnumerable<NoaaOptions>> GetRemainingOptions(CancellationToken stoppingToken, IEnumerable<NoaaDataOptions> dataQuery)
        {
            IDbContextFactory<WeatherContext> dbContextFactory = null;
            IWeatherClient weatherClient = null;
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken);
            var dataPointsTask = dataQuery.Select(dq => weatherClient.GetData(dq, stoppingToken));
            var pagedDatas = await Task.WhenAll(dataPointsTask);
            
            var dataPoints = pagedDatas.SelectMany(pd => pd.Results);
            
            var dataPointTracker = new DataPointTracker(dataPoints);
            var locationDataPoints = dataPointTracker.GetValues();
            dbContext.Datapoints.AddRange(locationDataPoints);
            await dbContext.SaveChangesAsync(stoppingToken);
            var remainingOptions = pagedDatas.SelectMany(p => p.GetRemainingDataOptions());
            return remainingOptions;
        }

        
    }
}