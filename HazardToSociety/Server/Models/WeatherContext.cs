using HazardToSociety.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HazardToSociety.Server.Models;

public class WeatherContext : DbContext
{
    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
    
    public DbSet<WeatherRecord> WeatherRecords { get; set; }
    public DbSet<UpdateHistory> UpdateHistories { get; set; }
    public DbSet<Location> Locations { get; set; }
}