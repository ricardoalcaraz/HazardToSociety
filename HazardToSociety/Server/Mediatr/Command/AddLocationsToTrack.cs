using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Mediatr.Command;

public class AddLocationsToTrack : IRequest<IEnumerable<Location>>
{
    
}

public class AddLocationsToTrackHandler : IRequestHandler<AddLocationsToTrack, IEnumerable<Location>>
{
    private readonly ILogger<AddLocationsToTrackHandler> _logger;
    private readonly WeatherContext _db;
    private readonly IWeatherClient _weatherClient;

    public AddLocationsToTrackHandler(ILogger<AddLocationsToTrackHandler> logger, WeatherContext db, IWeatherClient weatherClient)
    {
        _logger = logger;
        _db = db;
        _weatherClient = weatherClient;
    }
    
    public async Task<IEnumerable<Location>> Handle(AddLocationsToTrack request, CancellationToken cancellationToken)
    {
        var locationsQuery = new NoaaLocationOptions()
        {
            DataCategoryId = "PRCP",
            LocationCategoryId = "CITY",
        };
        
        await foreach (var noaaLocation in _weatherClient.GetAllLocations(locationsQuery, cancellationToken))
        {
            var nameTokens = noaaLocation.Name.Split(',');
            var location = new Location
            {
                MaxDate = noaaLocation.MaxDate,
                MinDate = noaaLocation.MinDate,
                Name = noaaLocation.Name,
                NoaaId = noaaLocation.Id
            };
            
            if (nameTokens.Length == 3)
            {
                location.City = nameTokens[0];
                location.State = nameTokens[1];
                location.Country = nameTokens[2];
            }
            else
            {
                location.Country = nameTokens[1];
            }
            
                
            _db.Locations.Add(location);
            _logger.LogDebug("Saving {Name} into db. Id: {Id}", location.Name, location.Id);
        }

        _logger.LogInformation("Saving {Count} locations into database", _db.Locations.Count());
        await _db.SaveChangesAsync(cancellationToken);
        return await _db.Locations.ToListAsync(cancellationToken);
    }
} 