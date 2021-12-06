using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HazardToSociety.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HazardToSociety.Server.Models;

public class WeatherContext : DbContext
{
    private IDbContextTransaction _currentTransaction;
    
    public WeatherContext(DbContextOptions<WeatherContext> options) : base(options) { }
    
    public DbSet<WeatherRecord> WeatherRecords { get; set; }
    public DbSet<UpdateHistory> UpdateHistories { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Datapoint> Datapoints { get; set; }
    public DbSet<LocationOfInterest> LocationOfInterests { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UpdateHistory>()
            .HasData(new List<UpdateHistory> { new (UpdateType.InitialSeeding) });
    }
    
    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();

            await (_currentTransaction?.CommitAsync() ?? Task.CompletedTask);
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
}

public class LocationOfInterest
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public virtual Location Location { get; set; }
    public DateTime ProcessedTime { get; set; }
}