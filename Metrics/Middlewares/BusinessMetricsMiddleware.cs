using Microsoft.AspNetCore.Http;
using Observability.Metrics.Models;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Observability.Metrics.Middlewares
{
    public sealed class BusinessMetricsMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly MetricsOptions _options;

        public BusinessMetricsMiddleware(RequestDelegate next, MetricsOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (_options is null || !_options.BusinessMetricsEnabled)
                {
                    await _next(context);
                }

                var endpoint = context.Request.Path.ToString();

                if (!_options.ShouldRecordMetricsOfThisEndpoint(endpoint))
                {
                    await _next(context);
                }

                Stopwatch stopwatch = null;

                var metricOption = _options.FindBusinessMetricOptionOf(endpoint);

                if (metricOption.RecordDuration)
                {
                    stopwatch = Stopwatch.StartNew();
                }

                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();

                    using (var metric = new BusinessMetric(metricOption))
                    {
                        var method = context.Request.Method;
                        var statusCode = context.Response.StatusCode;
                        var duration = stopwatch.Elapsed.TotalSeconds;

                        metric.RecordMetrics(statusCode, endpoint, method, duration);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in BusinessMetricsMiddleware: {ex.Message}");
                await _next(context);
            }
        }
    }
}
