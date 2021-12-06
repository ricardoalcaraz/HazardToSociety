using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Shared.Models;

namespace HazardToSociety.Shared.Utilities;

public interface IWeatherClient
{
    public IAsyncEnumerable<NoaaLocation> GetAllLocations(NoaaLocationOptions locationOptions,
        CancellationToken cancellationToken = default);
    public Task<NoaaPagedData<NoaaLocation, NoaaLocationOptions>> GetLocations(NoaaLocationOptions locationOptions, CancellationToken cancellationToken = default);
    public Task<NoaaPagedData<NoaaData, NoaaDataOptions>> GetData(NoaaDataOptions options, 
        CancellationToken cancellationToken = default);
    public IAsyncEnumerable<NoaaDataSet> GetDataSet(NoaaDatasetOptions options, 
        CancellationToken cancellationToken = default);

    public Task<NoaaPagedData<NoaaDataType, NoaaDataTypeOptions>> GetDataTypes(NoaaDataTypeOptions options,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NoaaData>> GetAllData(NoaaDataOptions dataQuery, CancellationToken stoppingToken);
}