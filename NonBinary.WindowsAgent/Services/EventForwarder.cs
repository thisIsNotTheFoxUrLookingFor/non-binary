using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Eventing.Reader;

namespace NonBinary.WindowsAgent.Services;

public class EventForwarder : IHostedService, IDisposable
{
    private readonly ILogger<EventForwarder> _logger;
    private EventLogWatcher? _watcher;
    private bool _disposed;

    public EventForwarder(ILogger<EventForwarder> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting CodeIntegrity event forwarder");

        // Live watcher only (no historical events)
        var query = new EventLogQuery("Microsoft-Windows-CodeIntegrity/Operational", PathType.LogName);
        _watcher = new EventLogWatcher(query);

        _watcher.EventRecordWritten += (sender, args) =>
        {
            if (args.EventRecord != null)
            {
                _logger.LogInformation("CodeIntegrity Event {Id} - {Message}",
                    args.EventRecord.Id, args.EventRecord.FormatDescription()?.Trim() ?? "(no description)");
            }
        };

        _watcher.Enabled = true;
        _logger.LogInformation("✅ CodeIntegrity event forwarder is now watching live events");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _watcher?.Enabled = false;
        _watcher?.Dispose();
        _watcher = null;
    }
}