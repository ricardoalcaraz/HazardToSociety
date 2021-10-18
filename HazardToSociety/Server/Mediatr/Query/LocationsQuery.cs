using System.Collections.Generic;
using HazardToSociety.Shared.Models;
using MediatR;

namespace HazardToSociety.Server.Mediatr.Query;

public record LocationsQuery : NoaaLocationOptions, IRequest<IEnumerable<Location>> { }
public record DataQuery : NoaaDataOptions, IRequest<IEnumerable<NoaaData>> { }