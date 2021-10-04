using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Models;
using HazardToSociety.Server.Services;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HazardToSociety.Server.Mediatr.Query;

public class LocationsQueryHandler : IRequestHandler<LocationsQuery, IEnumerable<Location>>
{
    private readonly WeatherContext _weatherContext;
    private readonly IWeatherClient _weatherClient;

    public LocationsQueryHandler(WeatherContext weatherContext, IWeatherClient weatherClient)
    {
        _weatherContext = weatherContext;
        _weatherClient = weatherClient;
    }

    public async Task<IEnumerable<Location>> Handle(LocationsQuery request, CancellationToken cancellationToken)
    {
        var isDataNeeded = await _weatherContext.UpdateHistories
            .AnyAsync(uh => uh.DateUpdated == null && uh.Name == nameof(NoaaLocation), cancellationToken);
        if (isDataNeeded)
        {
            var pagedData = await _weatherClient.GetLocations(request.LocationOptions, cancellationToken);
            var newLocations = pagedData.Results
                .Select(l => new Location
                {
                    Name = l.Name,
                    NoaaId = l.Id
                });
        
            //return locations of interest from database
            _weatherContext.Locations.AddRange(newLocations);
            await _weatherContext.SaveChangesAsync(cancellationToken);
        }

        var locations = await _weatherContext.Locations.ToListAsync(cancellationToken: cancellationToken);
        return locations;
    }
}