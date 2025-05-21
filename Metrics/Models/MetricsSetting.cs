using Observability.Miscellaneous.Models;
using System;

namespace Observability.Metrics.Models
{
    public class MetricsSetting
    {
        public string ApplicationName { get; set; }

        public ExporterSetting Exporter { get; set; }

        public BusinessMetricsSetting[] BusinessMetrics { get; set; } = Array.Empty<BusinessMetricsSetting>();

        public bool? EnableAspNetCoreInstrumentation { get; set; } = true;

        public bool? EnableHttpClientInstrumentation { get; set; } = true;

        public bool? EnableRuntimeInstrumentation { get; set; } = true;

    }

    public class BusinessMetricsSetting
    {
        public string MetricName { get; set; }
        public string Endpoint { get; set; }
        public bool RecordSuccess { get; set; } = true;
        public bool RecordFailure { get; set; } = true;
        public bool RecordDuration { get; set; }
    }
}
