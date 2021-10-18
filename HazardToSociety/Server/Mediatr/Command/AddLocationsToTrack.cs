using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Mediatr.Query;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Mediatr.Command;

public class AddLocationsToTrack : IRequest<IEnumerable<Location>>
{
    
}

public class AddLocationsToTrackHandler : IRequestHandler<AddLocationsToTrack, IEnumerable<Location>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<AddLocationsToTrackHandler> _logger;
    private readonly WeatherContext _db;

    public AddLocationsToTrackHandler(IMediator mediator, ILogger<AddLocationsToTrackHandler> logger, WeatherContext db)
    {
        _mediator = mediator;
        _logger = logger;
        _db = db;
    }
    
    public async Task<IEnumerable<Location>> Handle(AddLocationsToTrack request, CancellationToken cancellationToken)
    {
        var locationsQuery = new LocationsQuery()
        {
            DataCategoryId = "PRCP",
            LocationCategoryId = "CITY",
        };
        var locations = await _mediator.Send(locationsQuery, cancellationToken);
        var usLocations = locations.Where(l => l.Name.EndsWith("US")).ToList();
        _logger.LogDebug("Adding {Count} new locations", usLocations.Count);
        _db.Locations.AddRange(usLocations);
        //await _db.SaveChangesAsync(cancellationToken);
        return usLocations;
    }
} 