using Microsoft.Extensions.Logging;
using NonBinary.WindowsAgent.Models;
using System.Management.Automation;
using System.Text.Json;

namespace NonBinary.WindowsAgent.Services;

public class PolicyGenerator
{
    private readonly ILogger<PolicyGenerator> _logger;

    public PolicyGenerator(ILogger<PolicyGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateSupplementalXmlAsync(RemotePolicy policy)
    {
        var xmlPath = Path.Combine(Path.GetTempPath(), $"NonBinary-Supplemental-{policy.PolicyName}-{DateTime.UtcNow.Ticks}.xml");
        var tempDir = Path.GetDirectoryName(xmlPath)!;

        _logger.LogInformation("Generating supplemental policy XML: {Path}", xmlPath);

        using var ps = PowerShell.Create();

        // Create a basic supplemental policy XML with the rules we received
        ps.AddScript($@"
            $xml = @'
<?xml version=""1.0"" encoding=""utf-8""?>
<SiPolicy xmlns=""urn:schemas-microsoft-com:si-policy"" PolicyType=""Supplemental"">
  <VersionEx>10.0.0.0</VersionEx>
  <PolicyID>{Guid.NewGuid()}</PolicyID>
  <PolicyName>{policy.PolicyName}</PolicyName>
  <Rules>
'@
");

        // Add signer rules
        foreach (var signer in policy.Signers)
        {
            ps.AddScript($@"
                $xml += @'
    <Allow ID=""Signer_{Guid.NewGuid()}"" Name=""{signer.PublisherName}"" PublisherName=""{signer.PublisherName}"" ProductName=""{signer.ProductName}"" BinaryName=""{signer.BinaryName}"" />
'@
");
        }

        // Add path rules
        foreach (var pathRule in policy.Paths)
        {
            var recursive = pathRule.Recursive ? "True" : "False";
            ps.AddScript($@"
                $xml += @'
    <Allow ID=""Path_{Guid.NewGuid()}"" Name=""PathRule"" Path=""{pathRule.Path}"" Recursive=""{recursive}"" />
'@
");
        }

        // Add hash rules
        foreach (var hashRule in policy.Hashes)
        {
            ps.AddScript($@"
                $xml += @'
    <Allow ID=""Hash_{Guid.NewGuid()}"" Name=""{hashRule.FileName}"" Hash=""{hashRule.Sha256}"" />
'@
");
        }

        ps.AddScript($@"
            $xml += @'
  </Rules>
</SiPolicy>
'@
            $xml | Out-File -FilePath '{xmlPath}' -Encoding utf8
            Write-Output '{xmlPath}'
");

        var result = await Task.Run(() => ps.Invoke());
        if (result.Count > 0)
            return result[0].ToString();

        throw new InvalidOperationException("Failed to generate policy XML");
    }
}