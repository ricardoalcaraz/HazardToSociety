using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server.Mediatr.Command;
using HazardToSociety.Server.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server.Services;

public class DatabaseSeeder
{
    private readonly WeatherContext _db;
    private readonly IMediator _mediator;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(WeatherContext db, IMediator mediator, ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task MigrateDb(CancellationToken token = default)
    {
        var databaseType = _db.Database.IsSqlite() ? "Sql lite" : "Sql Server";
        _logger.LogInformation("Migrating {Name} database", databaseType);
        await _db.Database.MigrateAsync(cancellationToken: token);
        _logger.LogInformation("Successfully migrated database");
    }

    public async Task<UpdateHistory> SeedLocations(CancellationToken token = default)
    {
        await _mediator.Send(new AddLocationsToTrack(), token);
        var locationSeed = new UpdateHistory(UpdateType.LocationSeeding)
        {
            DateUpdated = DateTime.Now,
            RequiresUpdates = false
        };

        _db.Add(locationSeed);
        await _db.SaveChangesAsync(token);
        
        return locationSeed;
    }

    public async Task AddLocationsOfInterest(CancellationToken token = default)
    {
        var citiesOfInterest = new List<string>
        {
            "Los Angeles",
            "New York"
        };

        var locationIds = await _db.Locations
            .Where(l => citiesOfInterest.Contains(l.City))
            .Select(l => new LocationOfInterest{ LocationId = l.Id })
            .ToListAsync(token);
        
        _db.LocationOfInterests.AddRange(locationIds);

        await _db.SaveChangesAsync(token);
    }
}