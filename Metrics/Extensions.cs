using Microsoft.AspNetCore.Builder;
using Observability.Metrics.Middlewares;
using Observability.Metrics.Models;
using Observability.Miscellaneous.Constants;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System;
namespace Observability.Metrics
{
    public static class Extensions
    {
        public static OpenTelemetryBuilder AddMetrics(this OpenTelemetryBuilder builder, MetricsSetting metricsSetting)
        {
            try
            {
                if (metricsSetting == null)
                {
                    return builder;
                }

                var metricsOptions = MetricsOptions.CreateFromMetricsSetting(metricsSetting);

                if (metricsOptions == null)
                {
                    return builder;
                }


                builder.WithMetrics(metrics =>
                        {
                            metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: metricsOptions.ApplicationName));

                            if (metricsOptions.EnableAspNetCoreInstrumentation)
                            {
                                metrics.AddAspNetCoreInstrumentation();
                            }

                            if (metricsOptions.EnableHttpClientInstrumentation)
                            {
                                metrics.AddHttpClientInstrumentation();
                            }

                            if (metricsOptions.EnableRuntimeInstrumentation)
                            {
                                metrics.AddRuntimeInstrumentation();
                            }

                            switch (metricsOptions.Exporter.Type)
                            {
                                case ExporterType.OPTL:
                                    metrics.AddOtlpExporter(options =>
                                    {
                                        options.Protocol = metricsOptions.Exporter.GetOtlpExportProtocol();
                                        options.Endpoint = metricsOptions.Exporter.EndpointUri;
                                        options.ExportProcessorType = metricsOptions.Exporter.ProcessorType;
                                        options.TimeoutMilliseconds = metricsOptions.Exporter.TimeoutMilliseconds;
                                    });
                                    break;

                                default:
                                    metrics.AddOtlpExporter(options =>
                                    {
                                        options.Protocol = metricsOptions.Exporter.GetOtlpExportProtocol();
                                        options.Endpoint = metricsOptions.Exporter.EndpointUri;
                                        options.ExportProcessorType = metricsOptions.Exporter.ProcessorType;
                                        options.TimeoutMilliseconds = metricsOptions.Exporter.TimeoutMilliseconds;
                                    });
                                    break;
                            }

                        });
                        

                return builder;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error adding OpenTelemetry metrics: {ex.Message}");
                return builder;
            }

        }

        public static IApplicationBuilder AddMetrics(this IApplicationBuilder app, MetricsSetting metricsSetting)
        {
            try
            {
                if (metricsSetting == null)
                {
                    return app;
                }

                var metricsOptions = MetricsOptions.CreateFromMetricsSetting(metricsSetting);

                if(metricsOptions == null)
                {
                    return app;
                }

                if (metricsOptions.BusinessMetricsEnabled)
                {
                    app.UseMiddleware<BusinessMetricsMiddleware>(metricsOptions);
                }

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
