using Microsoft.Extensions.Logging;
using NonBinary.WindowsAgent.Models;
using NonBinary.WindowsAgent.Utils;

namespace NonBinary.WindowsAgent.Services;

public class PolicyService
{
    private readonly CryptoHelper _cryptoHelper;
    private readonly PolicyGenerator _generator;
    private readonly PolicyDeployer _deployer;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(
        CryptoHelper cryptoHelper,
        PolicyGenerator generator,
        PolicyDeployer deployer,
        ILogger<PolicyService> logger)
    {
        _cryptoHelper = cryptoHelper;
        _generator = generator;
        _deployer = deployer;
        _logger = logger;
    }

    public bool ValidateSignature(string body, string signature)
        => _cryptoHelper.ValidateSignature(body, signature);

    public async Task<(bool Success, string Message)> ApplyPolicyAsync(RemotePolicy policy)
    {
        try
        {
            _logger.LogInformation("Applying policy: {PolicyName}", policy.PolicyName);

            var xmlPath = await _generator.GenerateSupplementalXmlAsync(policy);
            var result = await _deployer.DeployAsync(xmlPath);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply policy");
            return (false, $"Policy application failed: {ex.Message}");
        }
    }
}