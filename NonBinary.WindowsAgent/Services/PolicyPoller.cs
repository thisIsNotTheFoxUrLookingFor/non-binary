using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NonBinary.WindowsAgent.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace NonBinary.WindowsAgent.Services;

public class PolicyPoller : IHostedService, IDisposable
{
    private readonly ILogger<PolicyPoller> _logger;
    private readonly PolicyService? _policyService;
    private readonly IConfiguration _config;
    private readonly HttpClient? _httpClient;
    private Timer? _timer;

    public PolicyPoller(ILogger<PolicyPoller> logger, PolicyService policyService, IConfiguration config)
    {
        _logger = logger;
        _policyService = policyService;
        _config = config;

        var host = config["Dashboard:Host"];
        if (string.IsNullOrEmpty(host))
        {
            _logger.LogWarning("Dashboard:Host not configured in appsettings.json — polling disabled for now.");
            return; // don't start polling
        }

        var port = config.GetValue<int>("Dashboard:Port", 443);
        var useHttps = config.GetValue<bool>("Dashboard:UseHttps", true);

        var scheme = useHttps ? "https" : "http";
        var baseUrl = port == 443 && useHttps || port == 80 && !useHttps
            ? $"{scheme}://{host}"
            : $"{scheme}://{host}:{port}";

        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_httpClient == null)
            return Task.CompletedTask; // polling disabled

        var intervalSeconds = _config.GetValue<int>("Dashboard:PollIntervalSeconds", 300);
        _logger.LogInformation("PolicyPoller started — checking dashboard every {Seconds}s", intervalSeconds);

        _timer = new Timer(DoPoll, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
        return Task.CompletedTask;
    }

    private async void DoPoll(object? state)
    {
        try
        {
            _logger.LogInformation("Checking in with dashboard...");

            _httpClient!.DefaultRequestHeaders.Clear();
            var apiKey = _config["Dashboard:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            var policy = await _httpClient.GetFromJsonAsync<RemotePolicy>("api/policy",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (policy == null)
            {
                _logger.LogWarning("No policy returned from dashboard");
                return;
            }

            var result = await _policyService!.ApplyPolicyAsync(policy);
            _logger.LogInformation("Policy check-in result: {Success} — {Message}", result.Success, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to poll dashboard");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}