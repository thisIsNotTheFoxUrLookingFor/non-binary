using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NonBinary.WindowsAgent.Services;
using NonBinary.WindowsAgent.Utils;

var builder = Host.CreateDefaultBuilder(args)
    .UseConsoleLifetime()                    // This makes Ctrl+C work properly
    .ConfigureServices(services =>
    {
        services.AddWindowsService();

        services.AddSingleton<CryptoHelper>();
        services.AddSingleton<PolicyGenerator>();
        services.AddSingleton<PolicyDeployer>();
        services.AddSingleton<PolicyService>();
        services.AddHostedService<EventForwarder>();
        services.AddHostedService<PolicyPoller>();
        services.AddHostedService<BasePolicyInitializer>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.AddEventLog();
    })
    .Build();

await builder.RunAsync();