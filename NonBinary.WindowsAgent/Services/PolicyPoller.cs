using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NonBinary.WindowsAgent.Services;
using NonBinary.WindowsAgent.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NonBinary.WindowsAgent.Services;

public class PolicyPoller : IHostedService, IDisposable
{
    private readonly ILogger<PolicyPoller> _logger;
    private readonly IConfiguration _config;
    private readonly PolicyDeployer _policyDeployer;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public PolicyPoller(
        ILogger<PolicyPoller> logger,
        IConfiguration config,
        PolicyDeployer policyDeployer)
    {
        _logger = logger;
        _config = config;
        _policyDeployer = policyDeployer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var baseDir = AppContext.BaseDirectory;
        var policyDir = Path.Combine(baseDir, "Policy");
        var assetsDir = Path.Combine(baseDir, "Assets");

        // 1. Create Policy folder if missing
        if (!Directory.Exists(policyDir))
        {
            Directory.CreateDirectory(policyDir);
            _logger.LogInformation("Created Policy folder at {Path}", policyDir);
        }

        // 2. Bootstrap from Assets/BasePolicy.cip if present
        var assetPath = Path.Combine(assetsDir, "BasePolicy.cip");
        var localPolicyPath = Path.Combine(policyDir, "BasePolicy.cip");

        if (File.Exists(assetPath))
        {
            try
            {
                File.Copy(assetPath, localPolicyPath, overwrite: true);
                _logger.LogInformation("Copied BasePolicy.cip from Assets to Policy folder");

                await _policyDeployer.DeployPolicyAsync(localPolicyPath, PolicyType.Base);
                _logger.LogInformation("✅ BasePolicy.cip successfully deployed as WDAC allow-list");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy or deploy BasePolicy.cip from Assets");
            }
        }
        else
        {
            _logger.LogWarning("No BasePolicy.cip found in Assets folder – local bootstrap skipped");
        }

        // Start cancellable polling loop (Ctrl+C now works cleanly)
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = RunPollingLoopAsync(_cts.Token);   // background loop

        var host = _config["Dashboard:Host"];
        var interval = _config.GetValue<int>("Dashboard:PollIntervalSeconds", 3);

        if (string.IsNullOrEmpty(host))
        {
            _logger.LogInformation("Dashboard:Host not configured – running in local-only mode");
        }
        else
        {
            _logger.LogInformation("PolicyPoller started – will check dashboard every {Seconds} seconds", interval);
        }
    }

    private async Task RunPollingLoopAsync(CancellationToken token)
    {
        var interval = _config.GetValue<int>("Dashboard:PollIntervalSeconds", 3);

        while (!token.IsCancellationRequested)
        {
            await DoPollAsync(token);
            if (!token.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromSeconds(interval), token);
        }
    }

    private async Task DoPollAsync(CancellationToken token)
    {
        // TODO: Future V1+ polling logic (when dashboard REST API is ready)
        // 1. Download .cip into ./Policy
        // 2. Call _policyDeployer.DeployPolicyAsync(...)
        _logger.LogDebug("PolicyPoller tick – dashboard polling not yet implemented in V1");
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

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}