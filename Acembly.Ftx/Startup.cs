using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Acembly.Ftx.Domain;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Polly;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Acembly.Ftx
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger<Startup> _logger;
        private AppSettings _settings;

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            _env = env;
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()
                .Configure<AppSettings>(Configuration)
                .PostConfigure<AppSettings>(s =>
                {
                    s.IsDev = _env.IsDevelopment();
                    s.JsonSettings = new JsonSerializerSettings
                    {
                        Converters            = new List<JsonConverter> {new StringEnumConverter() },
                        NullValueHandling     = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        DateFormatHandling    = DateFormatHandling.IsoDateFormat,
                        ContractResolver      = new DefaultContractResolver {NamingStrategy = new SnakeCaseNamingStrategy()}
                    };
                    s.Resilience.RetryPolicy = Policy
                        .Handle<Exception>(e => ! (e is TaskCanceledException))
                        .WaitAndRetryAsync(s.Resilience.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retries, context) => _logger.LogWarning(exception, exception.Message, retries.ToString()));

                    JsonConvert.DefaultSettings = () => _settings.JsonSettings;
                    _settings = s;
                });

            services
                .AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);
            
            services
                .AddCors()
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddSignalR();
            
            services
                .AddSingleton<IFileProvider>(new PhysicalFileProvider(
                Path.Combine(Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()))));

            
            services
                .AddSpaStaticFiles(configuration => configuration.RootPath = "ui/build");
            
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info
                {
                    Title          = "Acembly API",
                    Version        = "v1",
                    Description    = "Acembly API",
                    TermsOfService = "None",

                    Contact = new Contact
                    {
                        Name  = "Acembly Technology",
                        Email = "support@example.com",
                        Url   = "https://example.com"
                    }
                });

                c.AddSecurityDefinition("Bearer",
                    new ApiKeyScheme {
                        In          = "header",
                        Description = "Please insert JWT with Bearer into field",
                        Name        = "Authorization",
                        Type        = "apiKey"
                    });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.IgnoreObsoleteActions();
                c.IgnoreObsoleteProperties();
                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase(); 
                var xmlDoc = Path.Combine(AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                if (File.Exists(xmlDoc))
                    c.IncludeXmlComments(xmlDoc);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IFileProvider fileProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseGlobalExceptionHandler(x => {
                    x.ResponseBody(e => JsonConvert.SerializeObject(new { e.Message, StackTrace = _env.IsProduction() ? null : e }, _settings.JsonSettings));
                    x.OnError((exception, httpContext) => {
                        _logger.LogError(new EventId(0, nameof(Startup)), exception, exception.Message);
                        return Task.CompletedTask;
                    });
                    x.DebugMode = _env.IsDevelopment();
                    x.Map<ArgumentNullException>().ToStatusCode(HttpStatusCode.BadRequest);
                    x.Map<ArgumentException>().ToStatusCode(HttpStatusCode.BadRequest);
                    x.Map<UnauthorizedAccessException>().ToStatusCode(HttpStatusCode.Forbidden);
                    x.Map<SecurityException>().ToStatusCode(HttpStatusCode.Forbidden);
                    x.Map<NotImplementedException>().ToStatusCode(HttpStatusCode.NotImplemented);
                });
                //app.UseHsts();
                //app.UseHttpsRedirection();
            }

            app
                .UseCors(builder =>{
                    builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(host => true)
                        .AllowCredentials();
            })
                .UseStaticFiles()
                .UseSpaStaticFiles();
            
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = fileProvider,
                RequestPath = new PathString("/fs")
            });

            app.UseMvc(routes => routes.MapRoute(
                "default",
                "{controller}/{action=Index}/{id?}"));
            
            app
                .UseSwagger()
                .UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Acembly API");
                
                c.RoutePrefix = "api";
                c.EnableDeepLinking();
                c.ShowExtensions();
                c.DisplayRequestDuration();
                c.DefaultModelExpandDepth(0);
                c.DocExpansion(DocExpansion.None);
            });

            app.UseMvc();
            app.UseSignalR(routes => routes.MapHub<NotifyHub>("/api/rm"));

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ui";
               
                if (env.IsDevelopment())
                {
                    //spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                    spa.UseReactDevelopmentServer("start");
                }
            });
        }
    }
}