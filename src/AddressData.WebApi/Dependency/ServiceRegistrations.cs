namespace AddressData.WebApi.Dependency;

using System.Net;
using Core.Services;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;

public static class ServiceRegistrations
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // Add polly retry
        builder.Services
            .AddHttpClient()
            .ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.SetHandlerLifetime(TimeSpan.FromMinutes(5)); //Set lifetime to five minutes
                clientBuilder.AddPolicyHandler(GetRetryPolicy());
            });

        // Configure DI
        builder.Services.AddScoped<IOverpassTurboService, OverpassTurboService>();
        builder.Services.AddScoped<ISeedingService, SeedingService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();

        // Configure lowercase URLs
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

        // Add controllers and swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AddressData Web Api", Version = "v1" });
        });

        return builder;
    }

    // Enhance logs by adding OpenTelemetry traces.
    // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel#5-configure-opentelemetry-with-the-correct-providers
    public static WebApplicationBuilder AddOpenTelemetryLogs(this WebApplicationBuilder builder)
    {
        var otel = builder.Services.AddOpenTelemetry();

        // Configure OpenTelemetry Resources with the application name
        otel.ConfigureResource(resource => resource
            .AddService(builder.Environment.ApplicationName));

        // Add Tracing for ASP.NET Core and export to Console
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddConsoleExporter();
        });

        return builder;
    }

    public static WebApplicationBuilder AddLargeApiTimeout(this WebApplicationBuilder builder)
    {
        var largeTimeout = TimeSpan.FromDays(10);

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.KeepAliveTimeout = largeTimeout;
            options.Limits.RequestHeadersTimeout = largeTimeout;
        });

        return builder;
    }

    // Based on
    // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
    private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
}
