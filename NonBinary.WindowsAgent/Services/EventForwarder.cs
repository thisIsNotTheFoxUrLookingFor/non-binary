using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Eventing.Reader;

namespace NonBinary.WindowsAgent.Services;

public class EventForwarder : IHostedService, IDisposable
{
    private readonly ILogger<EventForwarder> _logger;
    private EventLogWatcher? _watcher;

    public EventForwarder(ILogger<EventForwarder> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting CodeIntegrity event forwarder");

        var query = new EventLogQuery("Microsoft-Windows-CodeIntegrity/Operational", PathType.LogName);
        _watcher = new EventLogWatcher(query);

        _watcher.EventRecordWritten += (sender, args) =>
        {
            if (args.EventRecord != null)
                _logger.LogInformation("CodeIntegrity event: {EventId} - {Message}",
                    args.EventRecord.Id, args.EventRecord.FormatDescription());
            // TODO: later forward to dashboard via HTTP
        };

        _watcher.Enabled = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose() => _watcher?.Dispose();
}