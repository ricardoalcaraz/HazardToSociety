using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Server;
using HazardToSociety.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Shared.Utilities
{
    public interface IWeatherClient
    {
        public IAsyncEnumerable<NoaaLocation> GetAllLocations(NoaaLocationOptions locationOptions,
            CancellationToken cancellationToken = default);

        public Task<NoaaPagedData<NoaaLocation>> GetLocations(NoaaLocationOptions locationOptions, CancellationToken cancellationToken = default);
        public IAsyncEnumerable<NoaaData> GetAllData(NoaaDataOptions options, 
            CancellationToken cancellationToken = default);
        public Task<NoaaPagedData<NoaaData>> GetData(NoaaDataOptions options, 
            CancellationToken cancellationToken = default);
        public IAsyncEnumerable<NoaaDataSet> GetDataSet(NoaaDatasetOptions options, 
            CancellationToken cancellationToken = default);

        public IAsyncEnumerable<NoaaDataType> GetAllDataTypes(NoaaDataTypeOptions options,
            CancellationToken cancellationToken = default);

        public Task<NoaaPagedData<NoaaDataType>> GetDataTypes(NoaaDataTypeOptions options,
            CancellationToken cancellationToken = default);
    }

    public class WeatherClient : IWeatherClient
    {
        private readonly ILogger<WeatherClient> _logger;
        private readonly IQueryBuilderService _queryBuilderService;
        private readonly HttpClient _httpClient;
        
        public WeatherClient(IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ILogger<WeatherClient> logger,
            IQueryBuilderService queryBuilderService)
        {
            _logger = logger;
            _queryBuilderService = queryBuilderService;
            _httpClient = httpClientFactory.CreateClient();
            var token = configuration["NoaaApiKey"];
            _httpClient.DefaultRequestHeaders.Add("token", token);
            _httpClient.BaseAddress = new Uri("https://www.ncdc.noaa.gov/cdo-web/api/v2/");
        }
        
        
        public IAsyncEnumerable<NoaaLocation> GetAllLocations(NoaaLocationOptions locationOptions,
            CancellationToken cancellationToken)
        {
            return GetAllPagedData<NoaaLocation, NoaaLocationOptions>("locations", locationOptions, cancellationToken);
        }

        public async Task<NoaaPagedData<NoaaLocation>> GetLocations(NoaaLocationOptions locationOptions, CancellationToken cancellationToken)
        {
            var url = "locations" + _queryBuilderService.GetQuery(locationOptions);
            return await GetNextResultSet<NoaaLocation>(url, cancellationToken);
        }

        public IAsyncEnumerable<NoaaData> GetAllData(NoaaDataOptions options, CancellationToken cancellationToken)
            => GetAllPagedData<NoaaData, NoaaDataOptions>("data", options, cancellationToken);

        public async Task<NoaaPagedData<NoaaData>> GetData(NoaaDataOptions options,
            CancellationToken cancellationToken = default) =>
                await GetNextResultSet<NoaaData>("data", options, cancellationToken);

        public IAsyncEnumerable<NoaaDataSet> GetDataSet(NoaaDatasetOptions options,
            CancellationToken cancellationToken)
            => GetAllPagedData<NoaaDataSet, NoaaDatasetOptions>("datasets", options, cancellationToken);

        public IAsyncEnumerable<NoaaDataType> GetAllDataTypes(NoaaDataTypeOptions options, CancellationToken cancellationToken)
        {
            return GetAllPagedData<NoaaDataType, NoaaDataTypeOptions>("datatypes", options, cancellationToken);
        }

        public async Task<NoaaPagedData<NoaaDataType>> GetDataTypes(NoaaDataTypeOptions options,
            CancellationToken cancellationToken = default)
        {
            var url = "datatypes" + _queryBuilderService.GetQuery(options);
            return await GetNextResultSet<NoaaDataType>(url, cancellationToken);
        }

        private async Task<NoaaPagedData<T>> GetNextResultSet<T>(string url, NoaaOptions options,
            CancellationToken cancellationToken)
        {
            var urlWithParams = url + _queryBuilderService.GetQuery(options);
            return await GetNextResultSet<T>(urlWithParams, cancellationToken);
        }
        
        private async Task<NoaaPagedData<T>> GetNextResultSet<T>(string url, CancellationToken cancellationToken)
        {
            var request = await _httpClient.GetAsync(url, cancellationToken);
            request.EnsureSuccessStatusCode();
            // var body = await request.Content.ReadAsStringAsync(cancellationToken);
            // _logger.LogDebug(body);
            // return JsonSerializer.Deserialize<NoaaPagedData<T>>(body);
            return await request.Content.ReadFromJsonAsync<NoaaPagedData<T>>(cancellationToken: cancellationToken);
        }

        private async IAsyncEnumerable<T> GetAllPagedData<T, TOptions>(string baseUrl, TOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default) where TOptions : NoaaOptions
        {
            var nextOffset = 0;
            var isNextPageAvailable = true;
            while (isNextPageAvailable && !cancellationToken.IsCancellationRequested)
            {
                options = options with { Offset = nextOffset };
                nextOffset = options.Offset + options.Limit;
                var url = baseUrl + _queryBuilderService.GetQuery(options);
                _logger.LogDebug("Retrieving from url:{Url}", url);
                var pagedData = await GetNextResultSet<T>(url, cancellationToken);
                foreach (var item in pagedData.Results)
                {
                    yield return item;
                }
                isNextPageAvailable = pagedData.Metadata.ResultSet.Count > (options.Offset + options.Limit);
            }
        }

    }
}