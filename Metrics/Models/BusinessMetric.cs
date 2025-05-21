using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Observability.Metrics.Models
{
    public class BusinessMetric : IDisposable
    {
        private readonly BusinessMetricsOption _metricOption;
        private readonly Meter _meter;
        private readonly Counter<long> _requestCounter;
        private readonly Histogram<double> _requestDuration;
        private readonly Counter<long> _errorCounter;

        public BusinessMetric(BusinessMetricsOption metricOption)
        {
            _metricOption = metricOption;

            if (_metricOption is null || string.IsNullOrWhiteSpace(_metricOption.MetricName))
            {
                throw new ArgumentException("Metric Option cannot be null or empty.", nameof(_metricOption.MetricName));
            }

            _meter = new Meter(_metricOption.MetricName);

            if (_metricOption.RecordSuccess)
            {
                _requestCounter = _meter.CreateCounter<long>($"{_metricOption.MetricName}_http_requests_total", "requests", "Total number of HTTP requests");
            }

            if (_metricOption.RecordFailure)
            {
                _errorCounter = _meter.CreateCounter<long>($"{_metricOption.MetricName}_http_errors_total", "errors", "Total number of HTTP errors");
            }

            if (metricOption.RecordDuration)
            {
                _requestDuration = _meter.CreateHistogram<double>($"{_metricOption.MetricName}_http_request_duration_seconds", "seconds", "Duration of HTTP requests");
            }
        }

        public void RecordMetrics(int statusCode, string endpoint, string method, double duration)
        {
            if (statusCode >= 200 && statusCode < 300 && _metricOption.RecordSuccess)
            {
                RecordRequest(endpoint, method, statusCode);
            }
            else if (_metricOption.RecordFailure)
            {
                RecordError(endpoint, method, statusCode);
            }
            else
            {
                //ignore
            };

            if (_metricOption.RecordDuration)
            {
                RecordRequestDuration(endpoint, method, duration);
            }
        }


        /// <summary>
        /// Records a request metric with endpoint, method, and status code.
        /// </summary>
        public void RecordRequest(string endpoint,
                                  string method,
                                  int statusCode,
                                  IDictionary<string, object> additionalTags = null)
        {
            var tags = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name" , _metricOption.MetricName),
                new KeyValuePair<string, object>("endpoint", endpoint),
                new KeyValuePair<string, object>("method", method),
                new KeyValuePair<string, object>("status_code", statusCode)
            };

            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    tags.Add(new KeyValuePair<string, object>(tag.Key, tag.Value));
                }
            }

            _requestCounter.Add(1, tags.ToArray());
        }

        /// <summary>
        /// Records the duration of a request.
        /// </summary>
        public void RecordRequestDuration(string endpoint,
                                          string method,
                                          double duration,
                                          IDictionary<string, object> additionalTags = null)
        {
            var tags = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name" , _metricOption.MetricName),
                new KeyValuePair<string, object>("endpoint", endpoint),
                new KeyValuePair<string, object>("method", method),
                new KeyValuePair<string, object>("duration", duration)
            };

            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    tags.Add(new KeyValuePair<string, object>(tag.Key, tag.Value));
                }
            }

            _requestDuration.Record(duration, tags.ToArray());
        }

        /// <summary>
        /// Records an error metric with endpoint and status code.
        /// </summary>
        public void RecordError(string endpoint,
                                string method,
                                int statusCode,
                                string errorMessage = null,
                                IDictionary<string, object> additionalTags = null)
        {
            var tags = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name" , _metricOption.MetricName),
                new KeyValuePair<string, object>("endpoint", endpoint),
                new KeyValuePair<string, object>("method", method),
                new KeyValuePair<string, object>("status_code", statusCode)
            };

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                tags.Add(new KeyValuePair<string, object>("error_message", errorMessage));
            }

            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    tags.Add(new KeyValuePair<string, object>(tag.Key, tag.Value));
                }
            }

            _errorCounter.Add(1, tags.ToArray());
        }

        /// <summary>
        /// Creates a custom counter metric dynamically.
        /// </summary>
        public Counter<long> CreateCustomCounter(string name, string unit, string description)
        {
            return _meter.CreateCounter<long>(name, unit, description);
        }

        /// <summary>
        /// Creates a custom histogram metric dynamically.
        /// </summary>
        public Histogram<double> CreateCustomHistogram(string name, string unit, string description)
        {
            return _meter.CreateHistogram<double>(name, unit, description);
        }

        /// <summary>
        /// Disposes the meter when no longer needed.
        /// </summary>
        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}
