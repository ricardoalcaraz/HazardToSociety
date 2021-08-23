using System.Collections.Generic;
using System.Net.Http;
using HazardToSociety.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HazardToSociety.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<NoaaLocation> Records()
        {
            return new List<NoaaLocation>()
            {
                new() { Name = "Los Angeles" },
                new() { Name = "New York" }
            };
        }
    }
}