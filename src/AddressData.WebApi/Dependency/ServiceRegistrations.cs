namespace AddressData.WebApi.Dependency;

using System.Net;
using Core.Services;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.OpenApi;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Extensions.Http;

public static class ServiceRegistrations
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddHttpClient()
            .ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.SetHandlerLifetime(TimeSpan.FromMinutes(5));
                clientBuilder.AddPolicyHandler(GetRetryPolicy());
            });

        builder.Services.AddScoped<IOverpassTurboService, OverpassTurboService>();
        builder.Services.AddScoped<ISeedingService, SeedingService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();

        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AddressData Web Api", Version = "v1" });
        });

        return builder;
    }

    // OpenTelemetry
    public static WebApplicationBuilder AddOpenTelemetryLogs(this WebApplicationBuilder builder)
    {
        var otel = builder.Services.AddOpenTelemetry();

        otel.ConfigureResource(resource => resource
            .AddService(builder.Environment.ApplicationName));

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
        builder.Services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy =
                new RequestTimeoutPolicy { Timeout = TimeSpan.FromDays(10) };
        });
        return builder;
    }

    // Based on
    // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
    private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
}
