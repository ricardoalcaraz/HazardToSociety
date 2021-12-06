using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;

namespace HazardToSociety.Server.Mediatr.Command;

/// <summary>
/// 
/// </summary>
public record SaveDataForLocationFromNoaa : IRequest
{
    public IEnumerable<Location> Locations { get; init; }
}

public record SaveDataForLocation : IRequest<IEnumerable<NoaaDataOptions>>
{
    public Location Location { get; init; }
    public IEnumerable<NoaaDataOptions> DataQueries { get; init; }
}

public class SaveDataForLocationFromNoaaHandler : IRequestHandler<SaveDataForLocationFromNoaa>,
    IRequestHandler<SaveDataForLocation, IEnumerable<NoaaDataOptions>>
{
    private readonly WeatherContext _weatherContext;
    private readonly IWeatherClient _weatherClient;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SaveDataForLocationFromNoaaHandler(WeatherContext weatherContext, IWeatherClient weatherClient, IMediator mediator,
        IMapper mapper)
    {
        _weatherContext = weatherContext;
        _weatherClient = weatherClient;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(SaveDataForLocationFromNoaa request, CancellationToken cancellationToken)
    {
        var remainingQueriesList = new List<NoaaDataOptions>();
        foreach (var location in request.Locations)
        {
            var queriesForData = location.ToDataQueries(TimeSpan.FromDays(7));
            var saveDataCommand = new SaveDataForLocation()
            {
                Location = location,
                DataQueries = queriesForData
            };
            var remainingQueries = await _mediator.Send(saveDataCommand, cancellationToken);
            remainingQueriesList.AddRange(remainingQueries);
        }

        await _weatherContext.SaveChangesAsync(cancellationToken);
        
        foreach (var query in remainingQueriesList)
        {
            var data = await _weatherClient.GetAllData(query, cancellationToken);
            var locationDataPoints = _mapper.Map<IEnumerable<Datapoint>>(data);
            _weatherContext.Datapoints.AddRange(locationDataPoints);
            await _weatherContext.SaveChangesAsync(cancellationToken);
        }
        
        return Unit.Value;
    }

    public async Task<IEnumerable<NoaaDataOptions>> Handle(SaveDataForLocation request, CancellationToken cancellationToken)
    {
        var remainingDataQueries = new List<NoaaDataOptions>();
        foreach (var queryChunk in request.DataQueries.Chunk(5))
        {
            var taskList = queryChunk.Select(dq => _weatherClient.GetData(dq, cancellationToken));
            var results = await Task.WhenAll(taskList);
            
            var datapoints = results
                .SelectMany(r => r.Results)
                .Select(r => _mapper.Map<NoaaData, Datapoint>(r));
            
            _weatherContext.Datapoints.AddRange(datapoints);
            
            var noaaDataOptions = results.SelectMany(r => r.GetRemainingQueries());
            remainingDataQueries.AddRange(noaaDataOptions);
        }

        return remainingDataQueries;
    }
}