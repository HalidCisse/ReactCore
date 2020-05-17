using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using R.Domain;

namespace R
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private AppSettings _settings;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()
                    .Configure<AppSettings>(Configuration)
                    .PostConfigure<AppSettings>(s =>
                        {
                            s.IsDev = _env.IsDevelopment();
                            s.JsonSettings = new JsonSerializerSettings
                                             {
                                                 Converters = new List<JsonConverter> { new StringEnumConverter() },
                                                 NullValueHandling = NullValueHandling.Ignore,
                                                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                 DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                 ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
                                             };

                            JsonConvert.DefaultSettings = () => _settings.JsonSettings;
                            _settings                   = s;
                        });

            services
                .AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);

            services
                .AddSignalR();

            services
                .AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()))));

            services.AddControllersWithViews();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "UI/build";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IFileProvider fileProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseGlobalExceptionHandler(x => {
                        x.ResponseBody(e => JsonConvert.SerializeObject(new { e.Message, StackTrace = _env.IsProduction() ? null : e }, _settings.JsonSettings));
                        x.DebugMode = _env.IsDevelopment();
                        x.Map<ArgumentNullException>().ToStatusCode(HttpStatusCode.BadRequest);
                        x.Map<ArgumentException>().ToStatusCode(HttpStatusCode.BadRequest);
                        x.Map<UnauthorizedAccessException>().ToStatusCode(HttpStatusCode.Forbidden);
                        x.Map<SecurityException>().ToStatusCode(HttpStatusCode.Forbidden);
                        x.Map<NotImplementedException>().ToStatusCode(HttpStatusCode.NotImplemented);
                    });

                // app.UseHsts();
                // app.UseHttpsRedirection();
            }

            app
                .UseCors(builder =>
                    {
                        builder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(host => true)
                            .AllowCredentials();
                    });

            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
                                    {
                                        FileProvider = fileProvider,
                                        RequestPath  = new PathString("/fs")
                                    });
            app.UseRouting();

            app
                .UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                            name: "default",
                            pattern: "{controller}/{action=Index}/{id?}");

                        endpoints.MapHub<NotifyHub>("/ws");
                    });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "UI";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
