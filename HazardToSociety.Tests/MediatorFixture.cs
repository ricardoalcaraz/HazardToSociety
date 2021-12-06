using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server;
using HazardToSociety.Server.Models;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Xunit;

namespace HazardToSociety.Tests;

[CollectionDefinition(nameof(SliceFixture))]
    public class SliceFixtureCollection : ICollectionFixture<SliceFixture> { }

    public class SliceFixture : IAsyncLifetime
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly WebApplicationFactory<Startup> _factory;

        public SliceFixture()
        {
            _factory = new WebApiFactory();

            _configuration = _factory.Services.GetRequiredService<IConfiguration>();
            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        }

        public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            try
            {
                await dbContext.BeginTransactionAsync();

                await action(scope.ServiceProvider);

                await dbContext.CommitTransactionAsync();
            }
            catch (Exception)
            {
                dbContext.RollbackTransaction(); 
                throw;
            }
        }

        public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            try
            {
                await dbContext.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider);

                await dbContext.CommitTransactionAsync();

                return result;
            }
            catch (Exception)
            {
                dbContext.RollbackTransaction();
                throw;
            }
        }

        public Task ExecuteDbContextAsync(Func<WeatherContext, Task> action) 
            => ExecuteScopeAsync(sp => action(sp.GetRequiredService<WeatherContext>()));

        public Task ExecuteDbContextAsync(Func<WeatherContext, ValueTask> action) 
            => ExecuteScopeAsync(sp => action(sp.GetRequiredService<WeatherContext>()).AsTask());

        public Task ExecuteDbContextAsync(Func<WeatherContext, IMediator, Task> action) 
            => ExecuteScopeAsync(sp => action(sp.GetRequiredService<WeatherContext>(), sp.GetRequiredService<IMediator>()));

        public Task<T> ExecuteDbContextAsync<T>(Func<WeatherContext, Task<T>> action) 
            => ExecuteScopeAsync(sp => action(sp.GetRequiredService<WeatherContext>()));

        public Task<T> ExecuteDbContextAsync<T>(Func<WeatherContext, ValueTask<T>> action) 
            => ExecuteScopeAsync(sp => action(sp.GetRequiredService<WeatherContext>()).AsTask());

        public Task<T> ExecuteDbContextAsync<T>(Func<WeatherContext, IMediator, Task<T>> action) 
            => ExecuteScopeAsync(sp => action(sp.GetRequiredService<WeatherContext>(), sp.GetRequiredService<IMediator>()));

        public Task InsertAsync<T>(params T[] entities) where T : class
        {
            return ExecuteDbContextAsync(db =>
            {
                foreach (var entity in entities)
                {
                    db.Set<T>().Add(entity);
                }
                return db.SaveChangesAsync();
            });
        }

        public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);

                return db.SaveChangesAsync();
            });
        }

        public Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2) 
            where TEntity : class
            where TEntity2 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);

                return db.SaveChangesAsync();
            });
        }

        public Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3) 
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);

                return db.SaveChangesAsync();
            });
        }

        public Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4) 
            where TEntity : class
            where TEntity2 : class
            where TEntity3 : class
            where TEntity4 : class
        {
            return ExecuteDbContextAsync(db =>
            {
                db.Set<TEntity>().Add(entity);
                db.Set<TEntity2>().Add(entity2);
                db.Set<TEntity3>().Add(entity3);
                db.Set<TEntity4>().Add(entity4);

                return db.SaveChangesAsync();
            });
        }

        public Task<T> FindAsync<T>(int id)
            where T : class, IEntity
        {
            return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetRequiredService<IMediator>();

                return mediator.Send(request);
            });
        }

        public Task SendAsync(IRequest request)
        {
            return ExecuteScopeAsync(sp =>
            {
                var mediator = sp.GetRequiredService<IMediator>();

                return mediator.Send(request);
            });
        }

        private int _courseNumber = 1;

        public int NextCourseNumber() => Interlocked.Increment(ref _courseNumber);

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _factory?.Dispose();
            return Task.CompletedTask;
        }
    }

public interface IEntity
{
    public int Id { get; set; }
}

public class FakeWeatherClient : IWeatherClient
{
    public IAsyncEnumerable<NoaaLocation> GetAllLocations(NoaaLocationOptions locationOptions,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NoaaPagedData<NoaaLocation, NoaaLocationOptions>> GetLocations(NoaaLocationOptions locationOptions,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NoaaPagedData<NoaaData, NoaaDataOptions>> GetData(NoaaDataOptions options,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NoaaPagedData<NoaaData, NoaaDataOptions>()
        {
            Results = new List<NoaaData>()
            {
                new NoaaData()
            },
            Metadata = new NoaaMetadata(),
            Options = new NoaaDataOptions()
        });
    }

    public IAsyncEnumerable<NoaaDataSet> GetDataSet(NoaaDatasetOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NoaaPagedData<NoaaDataType, NoaaDataTypeOptions>> GetDataTypes(NoaaDataTypeOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<NoaaData>> GetAllData(NoaaDataOptions dataQuery, CancellationToken stoppingToken)
    {
        var noaaDatas = new List<NoaaData>
        {
            new NoaaData()
        };

        return Task.FromResult(noaaDatas.AsEnumerable());
    }
}