using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Observability.HealthChecks;
using Observability.Metrics;
using Observability.Miscellaneous.Services;
using Observability.Trace;
using Serilog;

namespace Observability.Miscellaneous
{
    public static class Extensions
    {
        public static IServiceCollection AddObservability(this IServiceCollection services)
        {
            try
            {
                var observabilitySetting = ObservabilityService.LoadSetting();

                if (observabilitySetting is null)
                {
                    return services;
                }

                observabilitySetting.SetApplicationNames();

                if (observabilitySetting.IsHealthCheckEnabled)
                {
                    services.AddHealthChecksServices(observabilitySetting.HealthCheck);
                }

                var openTelemetryBuilder = services.AddOpenTelemetry();

                if (observabilitySetting.IsMetricsEnabled)
                {
                    openTelemetryBuilder.AddMetrics(observabilitySetting.Metrics);
                }

                if(observabilitySetting.IsTraceEnabled)
                {
                    openTelemetryBuilder.AddTraces(observabilitySetting.TraceSetting);
                }

                return services;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error adding observability services: {ex.Message}");
                return services;
            }
        }

        public static IApplicationBuilder AddObservability(this IApplicationBuilder app)
        {
            try
            {
                var observabilitySetting = ObservabilityService.LoadSetting();

                if (observabilitySetting is null)
                {
                    return app;
                }

                if (observabilitySetting.IsHealthCheckEnabled)
                {
                    app.AddHealthChecks(observabilitySetting);
                }

                if (observabilitySetting.IsMetricsEnabled)
                {
                    app.AddMetrics(observabilitySetting.Metrics);
                }

                return app;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error adding observability services: {ex.Message}");
                return app;
            }
        }
    }
}
