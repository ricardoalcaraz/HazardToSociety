using System;
using System.Collections.Generic;
using System.Linq;
using HazardToSociety.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HazardToSociety.Server.Models;

public class WeatherContext : DbContext
{
    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
    
    public DbSet<WeatherRecord> WeatherRecords { get; set; }
    public DbSet<UpdateHistory> UpdateHistories { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationDataPoint> LocationDataPoints { get; set; }
    public DbSet<LocationOfInterest> LocationOfInterests { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UpdateHistory>()
            .HasData(new List<UpdateHistory> { new (UpdateType.InitialSeeding) });
    }
}

public class LocationOfInterest
{
    public LocationOfInterest(int locationId)
    {
        LocationId = locationId;
    }
    
    public int Id { get; set; }
    public int LocationId { get; }
    public virtual Location Location { get; set; }
    public DateTime ProcessedTime { get; set; }
}