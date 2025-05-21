using Observability.Miscellaneous.Constants;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using System;

namespace Observability.Miscellaneous.Models
{
    public class ExporterOptions
    {
        public ExporterOptions(ExporterType type,
           string endpoint,
           ExporterProtocol protocol = ExporterProtocol.HttpProtobuf,
           ExportProcessorType processorType = ExportProcessorType.Batch,
           int timeoutMilliseconds = 5000)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty.", nameof(endpoint));
            }

            Type = type;
            Endpoint = endpoint;
            Protocol = protocol;
            ProcessorType = processorType;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public ExporterType Type { get; private set; }
        public string Endpoint { get; private set; }
        public ExporterProtocol Protocol { get; private set; }
        public ExportProcessorType ProcessorType { get; private set; }
        public int TimeoutMilliseconds { get; private set; }
        public Uri EndpointUri => new Uri(EnsureEndpointStartsWithHttp(Endpoint));

        public OtlpExportProtocol GetOtlpExportProtocol()
        {
            switch (Protocol)
            {
                case ExporterProtocol.HttpProtobuf:
                    return OtlpExportProtocol.HttpProtobuf;
                case ExporterProtocol.Grpc:
                    return OtlpExportProtocol.Grpc;
                default:
                    return OtlpExportProtocol.HttpProtobuf;
            }
        }

        private static string EnsureEndpointStartsWithHttp(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return endpoint;
            }

            if (!endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = "http://" + endpoint;
            }

            return endpoint;
        }
    }

    //business metric class which with /endpoint the metric will be add trhough a middleware
}
