using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Acembly.Ftx
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .UseShutdownTimeout(TimeSpan.FromSeconds(60))
                .CaptureStartupErrors(true)
                //.UseEnvironment(EnvironmentName.Production)
                .UseStartup<Startup>();
    }
}