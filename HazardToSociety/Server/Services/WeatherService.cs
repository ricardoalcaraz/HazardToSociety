using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Services
{
    public class WeatherService : BackgroundService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly IWeatherClient _weatherClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _updateTime;
        
        public WeatherService(IConfiguration configuration, 
            ILogger<WeatherService> logger, 
            IWeatherClient weatherClient,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _weatherClient = weatherClient;
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
                    await mediator.Send(new GetLocationsQuery(new NoaaLocationOptions()), stoppingToken);
                    await Task.Delay(_updateTime, stoppingToken);
                }
                catch (HttpRequestException httpRequestException)
                {
                    _logger.LogError(httpRequestException, 
                        "Unable to retrieve data from source");
                }
            }
        }

        // private async Task UpdateData(CancellationToken cancellationToken)
        // {
        //     //read config options from db
        //     var locationOptions = new NoaaLocationOptions();
        //         
        //         //filter datasets to allowed list
        //         await foreach (var dataset in _weatherClient.GetDataSet(noaaDatasetOptions, cancellationToken))
        //         {
        //             _logger.LogDebug("Processing Dataset: {Dataset}", dataset);
        //             var noaaDataOptions = new NoaaDataOptions
        //             {
        //                 DataSetId = dataset.Id,
        //                 StartDate = dataset.MaxDate - TimeSpan.FromDays(1),
        //                 EndDate = dataset.MaxDate
        //             };
        //             
        //             //get data
        //             var allData = _weatherClient.GetData(noaaDataOptions, cancellationToken);
        //             await foreach (var data in allData.WithCancellation(cancellationToken))
        //             {
        //                 _logger.LogDebug("Data received: {Data}", data);
        //             }
        //         }
        //     }
        //}
    }

    public class GetLocationsQuery : IRequest<IEnumerable<NoaaLocation>>
    {
        public GetLocationsQuery(NoaaLocationOptions locationOptions)
        {
            LocationOptions = locationOptions;
        }

        public NoaaLocationOptions LocationOptions { get; }
    }
    
    public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, IEnumerable<NoaaLocation>>
    {
        private readonly WeatherContext _weatherContext;
        private readonly IWeatherClient _weatherClient;

        public GetLocationsQueryHandler(WeatherContext weatherContext, IWeatherClient weatherClient)
        {
            _weatherContext = weatherContext;
            _weatherClient = weatherClient;
        }
        
        public async Task<IEnumerable<NoaaLocation>> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            var locations = await _weatherClient.GetLocations(request.LocationOptions, cancellationToken);
            var newLocations = locations.Results.Select(l => new Location
            {
                Name = l.Name, 
                NoaaId = l.Id
            });
            //add locations of interest from db
            _weatherContext.Locations.AddRange(newLocations);
            await _weatherContext.SaveChangesAsync(cancellationToken);
            return locations.Results;
        }
    }
}