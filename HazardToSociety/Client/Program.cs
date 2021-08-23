using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HazardToSociety.Shared.Utilities;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HazardToSociety.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("local", (provider, client) =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });
            
            builder.Services.AddScoped(sp =>
            {
                var factory = sp.GetService<IHttpClientFactory>();
                return factory?.CreateClient("local");
            });

            builder.Configuration.AddInMemoryCollection(new []{new KeyValuePair<string, string>("NoaaApiKey", "test")});
            builder.Services.AddScoped<IWeatherClient, WeatherClient>();
            builder.Services.AddSingleton<IQueryBuilderService, QueryBuilderService>();
            await builder.Build().RunAsync();
        }
    }
}