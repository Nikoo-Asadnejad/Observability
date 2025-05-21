using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Observability.HealthChecks.CustomHealthChecks
{
    public sealed class GrpcHealthCheck : IHealthCheck
    {
        private readonly string _endpoint;
        private readonly string _name;
        public GrpcHealthCheck(string endpoint, string name)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_endpoint))
                {
                    return HealthCheckResult.Unhealthy("gRPC endpoint is not configured.");
                }

                var channel = GrpcChannel.ForAddress(_endpoint, new GrpcChannelOptions
                {
                    HttpHandler = new HttpClientHandler()
                });

                try
                {
                    var client = new Health.HealthClient(channel);
                    var request = new HealthCheckRequest { Service = _name };

                    var response = await client.CheckAsync(request, cancellationToken: cancellationToken);

                    return response.Status == HealthCheckResponse.Types.ServingStatus.Serving
                        ? HealthCheckResult.Healthy($"gRPC service at {_endpoint} is healthy.")
                        : HealthCheckResult.Unhealthy($"gRPC service at {_endpoint} is not serving.");
                }
                finally
                {
                    channel.Dispose();
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"gRPC service at {_endpoint} failed: {ex.Message}");
            }
        }
    }
}


