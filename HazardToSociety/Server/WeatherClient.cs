using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HazardToSociety.Server
{
    public interface IWeatherClient
    {
        public Task<NoaaPagedData<NoaaLocation>> GetLocations(CancellationToken cancellationToken);
        Task<NoaaPagedData<WeatherData>> GetData(NoaaLocation location, CancellationToken cancellationToken);
    }

    public record WeatherData
    {
        public DateTime Date { get; set; }
        public string DataType { get; set; }
        public string Station { get; set; }
        public string Attributes { get; set; }
        public int Value { get; set; }
    }
    

    public class WeatherClient : IWeatherClient
    {
        private readonly ILogger<WeatherClient> _logger;
        private readonly HttpClient _httpClient;
        
        public WeatherClient(IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ILogger<WeatherClient> logger)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("token", configuration["NoaaApiKey"]);
            _httpClient.BaseAddress = new Uri("https://www.ncdc.noaa.gov/cdo-web/api/v2/");
        }
        
        
        public async Task<NoaaPagedData<NoaaLocation>> GetLocations(CancellationToken cancellationToken)
        {
            var locationRequest = await _httpClient.GetAsync("locations", cancellationToken);
            locationRequest = locationRequest.EnsureSuccessStatusCode();
            return await locationRequest.Content.ReadFromJsonAsync<NoaaPagedData<NoaaLocation>>(cancellationToken: cancellationToken);
        }

        public async Task<NoaaPagedData<WeatherData>> GetData(NoaaLocation location,
            CancellationToken cancellationToken)
        {
            //https://www.ncdc.noaa.gov/cdo-web/api/v2/datasets
            var dataRequest = await _httpClient.GetAsync($"data?datasetid={location.Id}", cancellationToken);
            dataRequest = dataRequest.EnsureSuccessStatusCode();
            return await dataRequest.Content.ReadFromJsonAsync<NoaaPagedData<WeatherData>>(cancellationToken: cancellationToken);
        }
    }
}