namespace AddressData.WebApi;

using Core;
using Dependency;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder
            .ConfigureServices()
            .AddLargeApiTimeout()
            .AddOpenTelemetryLogs()
            .Build();

        app.UseCors(cors =>
            cors
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
        );

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseExceptionHandler($"/{Constants.ErrorControllerRoute}");
        app.MapControllers();

        await app.RunAsync();
    }
}
