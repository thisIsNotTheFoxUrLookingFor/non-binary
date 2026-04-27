using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NonBinary.WindowsAgent.Services;

namespace NonBinary.WindowsAgent;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Force clean console logging (works in .NET 10)
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Core services
        builder.Services.AddSingleton<PolicyDeployer>();
        builder.Services.AddHostedService<EventForwarder>();
        builder.Services.AddHostedService<PolicyPoller>();

        // Windows Service hosting (still works when installed as service)
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "NonBinary.WindowsAgent";
        });

        var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("NonBinary.WindowsAgent starting...");

        await host.RunAsync();
    }
}