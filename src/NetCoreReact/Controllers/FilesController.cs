using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace R.Controllers
{
    [Route("api/[controller]/[action]")]
    public class FilesController : Controller
    {       
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
        public IActionResult Content([FromServices] IFileProvider fileProvider, string path)
        {
            var contents = fileProvider
                .GetDirectoryContents(path??"");

            var files =
                contents.ToList()
                    .OrderByDescending(f => f.LastModified);

            return Ok(files.Take(100).ToList());
        }
    }
}