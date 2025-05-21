using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Observability.HealthChecks.CustomHealthChecks
{
    public sealed class ExternalApiHealthCheck : IHealthCheck
    {
        private readonly string _mainUrl;

        private readonly static List<string> _fallbackPaths = new List<string>
                                                              {
                                                                  "swagger/index.html",
                                                                  "healthz"
                                                              };

        public ExternalApiHealthCheck(string mainUrl)
        {
            _mainUrl = mainUrl;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        
        {
            var mainUrlSuccess = await TryUrlHealthCheck(new Uri(_mainUrl), cancellationToken);
            if (mainUrlSuccess)
            {
                return HealthCheckResult.Healthy($"Main URL {_mainUrl} is healthy.");
            }

            foreach (var path in _fallbackPaths)
            {
                var fallbackUrl = new Uri(new Uri(_mainUrl), path);
                var fallbackSuccess = await TryUrlHealthCheck(fallbackUrl, cancellationToken);
                if (fallbackSuccess)
                {
                    return HealthCheckResult.Healthy($"Successfully reached fallback path {fallbackUrl}");
                }
            }

            return HealthCheckResult.Unhealthy($"All checks for {_mainUrl} and fallback paths failed.");
        }

        private async Task<bool> TryUrlHealthCheck(Uri url, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                {
                    var response = await client.GetAsync(url, cancellationToken);
                    return response.IsSuccessStatusCode;
                };

            }
            catch (Exception ex)
            {
                Log.Error($"Error checking URL {url}: {ex.Message}");
                return false;
            }
        }

    }
}
