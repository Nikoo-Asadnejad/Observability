using Observability.Miscellaneous.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Observability.Metrics.Models
{
    public class MetricsOptions
    {
        public MetricsOptions(string applicationName)
        {
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; private set; }

        public ExporterOptions Exporter { get; private set; }

        private List<BusinessMetricsOption> _businessMetrics = new List<BusinessMetricsOption>();

        public IReadOnlyList<BusinessMetricsOption> BusinessMetrics => _businessMetrics.AsReadOnly();

        public bool EnableAspNetCoreInstrumentation { get; private set; } = true;

        public bool EnableHttpClientInstrumentation { get; private set; } = true;

        public bool EnableRuntimeInstrumentation { get; private set; } = true;

        public bool BusinessMetricsEnabled => BusinessMetrics != null && BusinessMetrics.Count > 0;

        public static MetricsOptions CreateFromMetricsSetting(MetricsSetting metricsSetting)
        {
            if (metricsSetting == null)
            {
                throw new ArgumentNullException(nameof(metricsSetting));
            }

            if (string.IsNullOrWhiteSpace(metricsSetting.ApplicationName))
            {
                throw new ArgumentException("ApplicationName cannot be null or empty.", nameof(metricsSetting.ApplicationName));
            }

            var options = new MetricsOptions(metricsSetting.ApplicationName)
            {
                EnableAspNetCoreInstrumentation = metricsSetting.EnableAspNetCoreInstrumentation ?? true,
                EnableHttpClientInstrumentation = metricsSetting.EnableHttpClientInstrumentation ?? true,
                EnableRuntimeInstrumentation = metricsSetting.EnableRuntimeInstrumentation ?? true,
                _businessMetrics = metricsSetting.BusinessMetrics?
                .Select(b => new BusinessMetricsOption(metricName: b.MetricName,
                                                       endpoint: b.Endpoint,
                                                       recordSuccess: b.RecordSuccess,
                                                       recordFailure: b.RecordFailure,
                                                       recordDuration: b.RecordDuration))
                .ToList() ?? new List<BusinessMetricsOption>(),
            };

            if (metricsSetting.Exporter != null)
            {
                options.Exporter = new ExporterOptions(
                    metricsSetting.Exporter.Type,
                    metricsSetting.Exporter.Endpoint,
                    metricsSetting.Exporter.Protocol,
                    metricsSetting.Exporter.ProcessorType,
                    metricsSetting.Exporter.TimeoutMilliseconds);
            }

            return options;
        }

        public BusinessMetricsOption FindBusinessMetricOptionOf(string endpoint)
        {
            if (!BusinessMetricsEnabled)
            {
                return null;
            }

            return _businessMetrics.Find(businessMetric => string.Equals(businessMetric.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase));
        }

        public bool ShouldRecordMetricsOfThisEndpoint(string endpoint)
        {
            if (!BusinessMetricsEnabled)
            {
                return false;
            }

            return _businessMetrics.Exists(businessMetric => string.Equals(businessMetric.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class BusinessMetricsOption
    {
        public BusinessMetricsOption(string metricName,
                                     string endpoint,
                                     bool recordSuccess = true,
                                     bool recordFailure = true,
                                     bool recordDuration = false)
        {
            if (string.IsNullOrWhiteSpace(metricName))
            {
                throw new ArgumentException("Metric name cannot be null or empty.", nameof(metricName));
            }

            MetricName = metricName.Trim().ToLower();
            Endpoint = endpoint;
            RecordSuccess = recordSuccess;
            RecordFailure = recordFailure;
            RecordDuration = recordDuration;
        }

        public string MetricName { get; private set; }
        public string Endpoint { get; private set; }
        public bool RecordSuccess { get; private set; } = true;
        public bool RecordFailure { get; private set; } = true;
        public bool RecordDuration { get; private set; }
    }

    //business metric class which with /endpoint the metric will be add trhough a middleware
}
