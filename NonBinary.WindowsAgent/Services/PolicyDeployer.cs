using Microsoft.Extensions.Logging;
using System.Management.Automation;

namespace NonBinary.WindowsAgent.Services;

public class PolicyDeployer
{
    private readonly ILogger<PolicyDeployer> _logger;

    public PolicyDeployer(ILogger<PolicyDeployer> logger)
    {
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> DeployAsync(string xmlPath)
    {
        try
        {
            var cipPath = Path.ChangeExtension(xmlPath, ".cip");

            _logger.LogInformation("Converting XML to CIP and deploying: {XmlPath}", xmlPath);

            using var ps = PowerShell.Create();

            // Convert XML → CIP
            ps.AddCommand("ConvertFrom-CIPolicy")
              .AddParameter("XmlFilePath", xmlPath)
              .AddParameter("BinaryFilePath", cipPath);

            await Task.Run(() => ps.Invoke());

            // Deploy supplemental policy (rebootless)
            ps.Commands.Clear();
            ps.AddScript($"CiTool.exe --update-policy \"{cipPath}\" --verbose");

            var result = await Task.Run(() => ps.Invoke());

            _logger.LogInformation("Policy deployed successfully");
            return (true, $"Supplemental policy applied: {cipPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Policy deployment failed");
            return (false, $"Deployment failed: {ex.Message}");
        }
    }
}