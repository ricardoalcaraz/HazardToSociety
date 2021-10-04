using System.Collections.Generic;
using HazardToSociety.Shared.Models;
using MediatR;

namespace HazardToSociety.Server.Mediatr.Query;

public class LocationsQuery : IRequest<IEnumerable<Location>>
{
    public LocationsQuery(NoaaLocationOptions locationOptions)
    {
        LocationOptions = locationOptions;
    }

    public NoaaLocationOptions LocationOptions { get; }
}