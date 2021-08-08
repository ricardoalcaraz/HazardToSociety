using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HazardToSociety.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Records()
        {
            return new List<string>()
            {
                "record1",
                "record2"
            };
        }
    }
}