using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;

namespace HazardToSociety.Server.Mediatr.Query;

public class LocationsQueryHandler : IRequestHandler<LocationsQuery, IEnumerable<Location>>,
    IRequestHandler<DataQuery, IEnumerable<NoaaData>>
{
    private readonly IWeatherClient _weatherClient;

    public LocationsQueryHandler(IWeatherClient weatherClient)
    {
        _weatherClient = weatherClient;
    }

    public async Task<IEnumerable<Location>> Handle(LocationsQuery request, CancellationToken cancellationToken)
    {
        var allLocations = _weatherClient.GetAllLocations(request, cancellationToken);
        var locations = new List<Location>();
        await foreach (var noaaLocation in allLocations.WithCancellation(cancellationToken))
        {
            var location = new Location
            {
                MaxDate = noaaLocation.MaxDate,
                MinDate = noaaLocation.MinDate,
                NoaaId = noaaLocation.Id,
                Name = noaaLocation.Name
            };
            locations.Add(location);
        }

        return locations;
    }

    public async Task<IEnumerable<NoaaData>> Handle(DataQuery request, CancellationToken cancellationToken)
    {
        var asyncDataSet = _weatherClient.GetAllData(request, cancellationToken);
        var noaaData = new List<NoaaData>();
        await foreach (var data in asyncDataSet.WithCancellation(cancellationToken))
        {
            noaaData.Add(data);
        }

        return noaaData;
    }
}