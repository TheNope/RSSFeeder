using System;
using System.IO;
using System.Reflection;
using Application;
using Application.Common.Models;
using CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NLog.Extensions.Logging;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rest;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Api;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Register the IMemoryCache service
        services.AddMemoryCache();

        var settings = Configuration.GetSection("Application").Get<ApplicationSettings>();

        var origins = string.IsNullOrEmpty(settings.Origins)
            ? new[] { "http://localhost:3000/", "http://localhost:80" }
            : JArray.Parse(settings.Origins).ToObject<string[]>();

        Console.WriteLine(
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] Allowed origins: [{string.Join(", ", origins)}]");

        services.Configure<ApplicationSettings>(Configuration.GetSection("Application"));

        // configure logging
        services.AddLogging(builder =>
        {
            // configure Logging with NLog
            builder.ClearProviders();

            var minimumLogLevel = int.TryParse(settings.MinimumLogLevel, out var minimumLogLevelResult);
            var httpConnectionsLogLevel =
                int.TryParse(settings.HttpConnectionsLogLevel, out var httpConnectionsLogLevelResult);

            builder.SetMinimumLevel(minimumLogLevel ? (LogLevel)minimumLogLevelResult : LogLevel.Debug);
            builder.AddFilter("Microsoft.AspNetCore.Http.Connections",
                httpConnectionsLogLevel ? (LogLevel)httpConnectionsLogLevelResult : LogLevel.Debug);
            builder.AddNLog(Configuration);
        });

        IConfiguration tempConfiguration;

        if (string.IsNullOrEmpty(Configuration.GetSection("Database")["Server"]))
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.efcore.json", true, true)
                .AddEnvironmentVariables();

            tempConfiguration = builder.Build();
        }
        else
            tempConfiguration = Configuration;

        services.Configure<ApplicationSettings>(Configuration.GetSection("Application"));

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.SetIsOriginAllowed(origin => origins.Contains(origin))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddHealthChecks();

        services.AddControllers()
            .AddXmlSerializerFormatters()
            .AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                x.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        services.AddApplication();
            
        services.AddRestInfrastructure(tempConfiguration);

        services.AddHttpContextAccessor();

        // Register the Swagger services
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "RSSFeeder",
                    Description = "RSSFeeder",
                });
            c.ExampleFilters();
            c.EnableAnnotations();
        });

        services.AddSwaggerExamplesFromAssemblyOf<Startup>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseSwagger();

        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
        // specifying the Swagger JSON endpoint.
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RSSFeeder V1");
            c.DocumentTitle = "RSSFeeder";
            c.RoutePrefix = "RSSFeeder";

            c.DocExpansion(DocExpansion.None);

            c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete);
        });

        // The order must remain the same
        app.UseCors();

        // Temporary diagnostics to confirm whether readiness middleware is short-circuiting requests.
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/RSSFeeder", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogDebug("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            await next();

            if (context.Request.Path.StartsWithSegments("/RSSFeeder", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogDebug("Completed request {Method} {Path} with status code {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode);
            }
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}