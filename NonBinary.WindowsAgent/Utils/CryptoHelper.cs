using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace NonBinary.WindowsAgent.Utils;

public class CryptoHelper
{
    private readonly ILogger<CryptoHelper> _logger;
    private readonly string? _secretKey;

    public CryptoHelper(IConfiguration configuration, ILogger<CryptoHelper> logger)
    {
        _logger = logger;
        _secretKey = configuration["PolicySecret"];   // now optional
        if (string.IsNullOrEmpty(_secretKey))
        {
            _logger.LogWarning("PolicySecret not configured — signature validation disabled for now");
        }
    }

    public bool ValidateSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_secretKey) || string.IsNullOrEmpty(signature))
            return true;   // skip validation until we need it

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToBase64String(computedHash);

        return computedSignature.Equals(signature, StringComparison.Ordinal);
    }
}