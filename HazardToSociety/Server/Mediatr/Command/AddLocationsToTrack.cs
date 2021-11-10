using System;
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

        var locations = new List<Location>();
        await foreach (var noaaLocation in _weatherClient.GetAllLocations(locationsQuery, cancellationToken))
        {
            try
            {
                var splitName = noaaLocation.Name.Split(',');
                var city = splitName[0];
                var area = splitName[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var location = new Location
                {
                    MaxDate = noaaLocation.MaxDate,
                    MinDate = noaaLocation.MinDate,
                    Name = noaaLocation.Name,
                    NoaaId = noaaLocation.Id,
                    City = city
                };
                
                if (area.Length == 2)
                {
                    location.State = area[0];
                    location.Country = area[1];
                }
                else
                {
                    location.Country = area[0];
                }

                locations.Add(location);
                _logger.LogDebug("Saving [{Name}] into db. Id: {Id}", location.Name, location.NoaaId);
            }
            catch (IndexOutOfRangeException outOfRangeException)
            {
                _logger.LogError(outOfRangeException, "Item: [{Item}]", noaaLocation);
            }
        }
        
        _logger.LogInformation("Retrieve {Count} locations from API", locations.Count);
        
        var existingLocations = (await _db.Locations
            .Select(l => l.NoaaId)
            .ToListAsync(cancellationToken)).ToHashSet();

        var locationsToAdd = locations.Where(l => !existingLocations.Contains(l.NoaaId)).ToList();
        var uniqueLocations = locationsToAdd
            .GroupBy(lt => lt.Name)
            .Select(g =>
            {
                var count = g.Count();
                if (count != 1)
                {
                    _logger.LogWarning("{Key} has {Count} elements", g.Key, count);
                }

                return g.First();
            }).ToList();
        _logger.LogInformation("Saving {Count} locations into database", uniqueLocations.Count);
        _db.Locations.AddRange(uniqueLocations);
        
        await _db.SaveChangesAsync(cancellationToken);
        
        return await _db.Locations.ToListAsync(cancellationToken);
    }
} 