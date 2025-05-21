using Observability.HealthChecks.Models;
using Observability.Metrics.Models;
using Observability.Trace.Models;
using System;
using System.Linq;
using System.Reflection;

namespace Observability.Miscellaneous.Models
{
    public class ObservabilitySetting
    {
        public ObservabilitySetting()
        {
            AllowedIPs = Array.Empty<string>();
        }

        private static string[] _localHostIps = new[] { "localhost", "127.0.0.1", "::1" };

        public string ApplicationName { get; set; }

        public HealthCheckSetting HealthCheck { get; set; }

        public MetricsSetting Metrics { get; set; }

        public TraceSetting TraceSetting { get; set; }

        public string[] AllowedIPs { get; set; }

        public bool AllIPsAllowed => AllowedIPs == null || AllowedIPs.Length == 0 || (AllowedIPs.Length == 1 && AllowedIPs[0] == "*");

        public bool IsHealthCheckEnabled => HealthCheck != null && HealthCheck.Items != null && HealthCheck.Items.Count > 0;

        public bool IsMetricsEnabled => Metrics != null && Metrics.Exporter != null && !string.IsNullOrWhiteSpace(Metrics.Exporter.Endpoint);

        public bool IsTraceEnabled => TraceSetting != null && TraceSetting.Items != null && TraceSetting.Items.Count > 0;
        public void SetApplicationNames()
        {
            if (string.IsNullOrWhiteSpace(ApplicationName))
            {
                ApplicationName = Assembly.GetCallingAssembly()?.GetName()?.Name;
            }

            if (HealthCheck != null)
            {
                HealthCheck.ApplicationName = ApplicationName;
            }

            if (Metrics != null)
            {
                Metrics.ApplicationName = ApplicationName;
            }
        }

        public bool IsIpAllowed(string remoteIp)
        {
            if (AllIPsAllowed)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(remoteIp))
            {
                return false;
            }

            if (IsIpLocalhost(remoteIp))
            {
                AddAllLocalhostIps();
            }

            if (AllowedIPs.Contains(remoteIp, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool IsIpLocalhost(string remoteIp) => _localHostIps.Contains(remoteIp, StringComparer.OrdinalIgnoreCase);

        private void AddAllLocalhostIps()
        {
            if (AllowedIPs is null)
            {
                AllowedIPs = Array.Empty<string>();
            }

            AllowedIPs = AllowedIPs.Concat(_localHostIps).Distinct().ToArray();
        }

    }

}
