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

        // Allow all cors requests
        app.UseCors(cors =>
            cors
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
        );

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio#add-and-configure-swagger-middleware
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseHttpsRedirection()
            .UseRouting()
            .UseExceptionHandler($"/{Constants.ErrorControllerRoute}") // When an error happens, use ErrorController.cs
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        await app.RunAsync();
    }
}
