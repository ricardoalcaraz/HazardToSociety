using HazardToSociety.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace HazardToSociety.Server;

public class WeatherContext : DbContext
{
    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
    
    public DbSet<WeatherRecord> WeatherRecords { get; set; }
    public DbSet<Location> Locations { get; set; }
}

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NoaaId { get; set; }
}