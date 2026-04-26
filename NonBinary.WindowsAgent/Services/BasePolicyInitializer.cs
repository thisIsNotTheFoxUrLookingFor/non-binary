using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NonBinary.WindowsAgent.Services;

public class BasePolicyInitializer : IHostedService
{
    private readonly ILogger<BasePolicyInitializer> _logger;
    private readonly string _policyDir;

    public BasePolicyInitializer(ILogger<BasePolicyInitializer> logger, IConfiguration config)
    {
        _logger = logger;
        _policyDir = config["Policy:BasePolicyDirectory"] ?? @"C:\Program Files\NonBinary\Policy";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Checking for NonBinary base WDAC policy in {PolicyDir}...", _policyDir);

            var checkResult = await RunPowerShellCommandAsync("CiTool -lp -json");

            if (checkResult.Contains("NonBinary-BasePolicy"))
            {
                _logger.LogInformation("NonBinary base policy already exists — skipping.");
                return;
            }

            _logger.LogWarning("No NonBinary base policy found. Running BasePolicySetup.ps1...");

            var scriptPath = Path.Combine(AppContext.BaseDirectory, "Assets", "BasePolicySetup.ps1");

            if (!File.Exists(scriptPath))
            {
                _logger.LogError("BasePolicySetup.ps1 not found in Assets folder!");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
                _logger.LogInformation("Script output: {Output}", output);

            if (!string.IsNullOrWhiteSpace(error))
                _logger.LogWarning("Script error: {Error}", error);

            _logger.LogInformation("✅ Base policy setup completed via script.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BasePolicyInitializer failed");
        }
    }

    private async Task<string> RunPowerShellCommandAsync(string command)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}