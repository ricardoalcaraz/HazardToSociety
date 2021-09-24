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

        [HttpGet("data-types")]
        public async Task<NoaaPagedData<NoaaDataType>> GetDataTypes([FromQuery]NoaaDataTypeOptions options)
        {
            options ??= new NoaaDataTypeOptions();
            return await _weatherClient.GetDataTypes(options);
        }
    }
}