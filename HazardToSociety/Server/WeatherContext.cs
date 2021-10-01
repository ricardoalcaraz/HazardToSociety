using HazardToSociety.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace HazardToSociety.Server;

public class WeatherContext : DbContext
{
    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
    
    public DbSet<WeatherRecord> WeatherRecords { get; set; }
}