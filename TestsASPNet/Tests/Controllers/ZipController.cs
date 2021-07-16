using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZipSlip;

namespace Tests.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ZipController : ControllerBase
    {
        public ZipController(ILogger<WeatherForecastController> logger)
        {
        }

        [HttpGet]
        public String Get(String path, Stream zip)
        {
            var z = new ZipSlipNoncompliant();
            var entries = new ZipArchive(zip).Entries.GetEnumerator();
            while (entries.MoveNext()) z.ExtractEntry(entries, path);
            return "Done";
        }
    }
}
