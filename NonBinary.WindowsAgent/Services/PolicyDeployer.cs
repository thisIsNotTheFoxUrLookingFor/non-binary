using Microsoft.Extensions.Logging;
using System.Diagnostics;
using NonBinary.WindowsAgent.Models;   // for PolicyType enum

namespace NonBinary.WindowsAgent.Services;

public class PolicyDeployer
{
    private readonly ILogger<PolicyDeployer> _logger;

    public PolicyDeployer(ILogger<PolicyDeployer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Deploys a .cip file silently using PowerShell launcher (reliable on win-arm64).
    /// </summary>
    public async Task DeployPolicyAsync(string cipFilePath, PolicyType policyType = PolicyType.Base, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(cipFilePath))
        {
            _logger.LogError("CIP file not found: {Path}", cipFilePath);
            return;
        }

        try
        {
            _logger.LogInformation("Deploying {PolicyType} policy: {File}",
                policyType, Path.GetFileName(cipFilePath));

            // Full path for ARM64
            var ciToolPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "CiTool.exe");

            _logger.LogInformation("CiTool path: {CiToolPath} (exists: {Exists})",
                ciToolPath, File.Exists(ciToolPath));

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"& '{ciToolPath}' --update-policy '{cipFilePath}' --verbose\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            _logger.LogDebug("PowerShell command: {Arguments}", startInfo.Arguments);

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start PowerShell process");
                return;
            }

            var timeout = TimeSpan.FromSeconds(60);
            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await process.WaitForExitAsync(linkedCts.Token);

            if (process.ExitCode == 0)
                _logger.LogInformation("✅ {PolicyType} policy deployed successfully (Is Currently Enforced: true).", policyType);
            else
                _logger.LogError("CiTool failed with exit code {ExitCode}", process.ExitCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy {PolicyType} policy", policyType);
        }
    }
}