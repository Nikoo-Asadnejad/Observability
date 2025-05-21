using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Observability.HealthChecks.Models;
using Observability.Miscellaneous.Models;
using Serilog;
using System;
using System.Linq;

namespace Observability.HealthChecks
{
    public static class Extensions
    {
        public static IServiceCollection AddHealthChecksServices(this IServiceCollection services, HealthCheckSetting healthCheckSetting)
        {
            try
            {
                var healthCheckOptions = HealthCheckOptions.CreateFromSetting(healthCheckSetting);

                services.AddHealthChecks()
                        .AddSql(healthCheckOptions)
                        .AddRedis(healthCheckOptions)
                        .AddMongoDb(healthCheckOptions)
                        .AddRabbitMq(healthCheckOptions)
                        .AddExternalApis(healthCheckOptions)
                        .AddHangfire(healthCheckOptions)
                        .AddElasticsearch(healthCheckOptions)
                        .AddCdn(healthCheckOptions)
                        .AddNetwork(healthCheckOptions)
                        .AddSignalR(healthCheckOptions)
                        .AddSSL(healthCheckOptions)
                        .AddGrpc(healthCheckOptions);

                return services;
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding health checks services: {ex.Message}");
                return services;
            }
        }

        public static IApplicationBuilder AddHealthChecks(this IApplicationBuilder app, ObservabilitySetting observabilitySetting)
        {
            try
            {
                app.UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
                        if (!observabilitySetting.IsIpAllowed(remoteIp))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Forbidden: Your IP is not allowed to access this resource.");
                            return;
                        }

                        context.Response.ContentType = "application/json";
                        context.Response.Headers["Cache-Control"] = "no-store";

                        var response = new
                        {
                            status = report.Status.ToString(),
                            totalDuration = report.TotalDuration,
                            machineName = Environment.MachineName,
                            results = report.Entries.Select(entry => new
                            {
                                name = entry.Key,
                                status = entry.Value.Status.ToString(),
                                description = entry.Value.Description,
                                duration = entry.Value.Duration,
                                tags = entry.Value.Tags,
                                exception = entry.Value.Exception?.Message,
                                exceptionStackTrace = entry.Value.Exception?.StackTrace,
                                data = entry.Value.Data.ToDictionary(
                                    d => d.Key,
                                    d => d.Value?.ToString()
                                )
                            })
                        };

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(response, Formatting.Indented));
                    }
                });
                return app;
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding health checks middleware: {ex.Message}");
                return app;
            }
        }
    }
}
