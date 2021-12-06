using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Shared.Models;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Shared.Utilities;

public class WeatherClient : IWeatherClient
{
    private readonly ILogger<WeatherClient> _logger;
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly HttpClient _httpClient;
        
    public WeatherClient(HttpClient httpClient, ILogger<WeatherClient> logger)
    {
        if (httpClient.BaseAddress == null)
            throw new ArgumentException("BaseAddress is empty", nameof(httpClient));
            
        _httpClient = httpClient;
        _logger = logger;
        _queryBuilderService = new QueryBuilderService(httpClient.BaseAddress.ToString());
    }
        
        
    public IAsyncEnumerable<NoaaLocation> GetAllLocations(NoaaLocationOptions locationOptions,
        CancellationToken cancellationToken)
    {
        return GetAllPagedData<NoaaLocation, NoaaLocationOptions>("locations", locationOptions, cancellationToken);
    }

    public async Task<NoaaPagedData<NoaaLocation, NoaaLocationOptions>> GetLocations(NoaaLocationOptions locationOptions, CancellationToken cancellationToken)
    {
        const string locationUrl = "locations";
        return await GetNextResultSet<NoaaLocation, NoaaLocationOptions>(locationUrl, locationOptions, cancellationToken);
    }

    public Task<IEnumerable<NoaaData>> GetAllData(NoaaDataOptions options, CancellationToken cancellationToken = default)
        => GetAsList<NoaaData, NoaaDataOptions>("data", options, cancellationToken);

    public async Task<NoaaPagedData<NoaaData, NoaaDataOptions>> GetData(NoaaDataOptions options,
        CancellationToken cancellationToken = default) =>
        await GetNextResultSet<NoaaData, NoaaDataOptions>("data", options, cancellationToken);

    public IAsyncEnumerable<NoaaDataSet> GetDataSet(NoaaDatasetOptions options,
        CancellationToken cancellationToken)
        => GetAllPagedData<NoaaDataSet, NoaaDatasetOptions>("datasets", options, cancellationToken);

    public IAsyncEnumerable<NoaaDataType> GetAllDataTypes(NoaaDataTypeOptions options, CancellationToken cancellationToken)
    {
        return GetAllPagedData<NoaaDataType, NoaaDataTypeOptions>("datatypes", options, cancellationToken);
    }

    public async Task<NoaaPagedData<NoaaDataType, NoaaDataTypeOptions>> GetDataTypes(NoaaDataTypeOptions options,
        CancellationToken cancellationToken = default) 
        => await GetNextResultSet<NoaaDataType, NoaaDataTypeOptions>("datatypes", options, cancellationToken);

        
    private async Task<NoaaPagedData<T, TOption>> GetNextResultSet<T, TOption>(string path, TOption options,
        CancellationToken cancellationToken) where TOption : NoaaOptions
    {
        var urlWithParams = _queryBuilderService.GetUrl(path, options);
        var request = await _httpClient.GetAsync(urlWithParams, cancellationToken);
        try
        {
            request.EnsureSuccessStatusCode();
            var readFromJsonAsync = await request.Content.ReadFromJsonAsync<NoaaPagedData<T, TOption>>(cancellationToken: cancellationToken);
            if (readFromJsonAsync != null)
            {
                return readFromJsonAsync with {Options = options};
            }
            
            return null;
        }
        catch (HttpRequestException requestException)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(requestException, "Unable to make request: {Content}", content);
            throw;
        }
    }

    private async IAsyncEnumerable<T> GetAllPagedData<T, TOptions>(string baseUrl, TOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TOptions : NoaaOptions
    {
        var nextOffset = 0;
        var isNextPageAvailable = true;
        while (isNextPageAvailable && !cancellationToken.IsCancellationRequested)
        {
            options = options with { Offset = nextOffset };
            nextOffset = options.Offset + options.Limit;
            var pagedData = await GetNextResultSet<T, TOptions>(baseUrl, options, cancellationToken);
            foreach (var item in pagedData.Results)
            {
                yield return item;
            }
            isNextPageAvailable = options.Offset + options.Limit <= pagedData.Metadata?.ResultSet.Count;
        }
    }

    private async Task<IEnumerable<T>> GetAsList<T, TOptions>(string baseUrl, TOptions options, 
        CancellationToken cancellationToken = default) where TOptions : NoaaOptions
    {
        var pagedData = await GetNextResultSet<T, TOptions>(baseUrl, options, cancellationToken);
        if (pagedData.Metadata == null)
        {
            _logger.LogDebug("Received nothing for {Options}", options);
            return Enumerable.Empty<T>();
        }
            
        var count = pagedData.Metadata.ResultSet.Count;
        var returnList = new List<T>(count);
        returnList.AddRange(pagedData.Results);

        var optionRequests = pagedData.GetRemainingDataOptions().ToList();
        foreach (var optionChunk in optionRequests.Chunk(5))
        {
            var tasks = optionChunk
                .Select(option => GetNextResultSet<T, TOptions>(baseUrl, option, cancellationToken));

            var data = (await Task.WhenAll(tasks)).SelectMany(t => t.Results);
            returnList.AddRange(data);
        }
        return returnList;
    }
}