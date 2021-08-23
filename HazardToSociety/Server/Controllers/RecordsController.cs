using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HazardToSociety.Shared.Models;
using HazardToSociety.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HazardToSociety.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        private readonly IWeatherClient _weatherClient;

        public RecordsController(IWeatherClient weatherClient)
        {
            _weatherClient = weatherClient;
        }

        [HttpGet]
        public IEnumerable<NoaaLocation> Records()
        {
            return new List<NoaaLocation>()
            {
                new() { Name = "Los Angeles" },
                new() { Name = "New York" }
            };
        }

        [HttpGet("data-types")]
        public async Task<NoaaPagedData<NoaaDataType>> GetDataTypes()
        {
            return await _weatherClient.GetDataTypes(new NoaaDataTypeOptions());
        }
    }
}