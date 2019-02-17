using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Acembly.Ftx.Controllers
{
    [Route("api/[controller]/[action]")]
    public class FilesController : Controller
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet, ActionName("")]
        public IActionResult Drives() => Ok(
            DriveInfo.GetDrives().Where(d=> d.IsReady).Select(d=> new
            {
                d.Name,
                d.DriveType,
                d.VolumeLabel,
                d.DriveFormat,
                d.TotalFreeSpace,
                d.AvailableFreeSpace,
                d.TotalSize,
            }));

        [HttpGet]
        public IActionResult Files([FromServices] IFileProvider fileProvider)
        {
            var contents = fileProvider
                .GetDirectoryContents("");

            var files =
                contents.ToList()
                    .OrderByDescending(f => f.LastModified);

            return Ok(files);
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get { return 32 + (int) (TemperatureC / 0.5556); }
            }
        }
    }
}