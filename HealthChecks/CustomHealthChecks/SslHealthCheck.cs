using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Observability.HealthChecks.CustomHealthChecks
{
    public sealed class SslHealthCheck : IHealthCheck
    {
        private readonly string _host;

        public SslHealthCheck(string host)
        {
            _host = host;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    client.Connect(_host, 443);
                    using (var sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, errors) => true))
                    {
                        sslStream.AuthenticateAsClient(_host);
                        var cert = sslStream.RemoteCertificate as X509Certificate2;

                        if (cert == null || DateTime.Now > cert.NotAfter)
                        {
                            return HealthCheckResult.Unhealthy("SSL certificate is invalid or expired.");
                        }

                        return HealthCheckResult.Healthy("SSL certificate is valid.");
                    }
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"SSL check failed: {ex.Message}");
            }
        }
    }
}
