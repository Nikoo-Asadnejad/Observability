using Observability.Miscellaneous.Constants;
using OpenTelemetry;

namespace Observability.Miscellaneous.Models
{
    public class ExporterSetting
    {
        public ExporterType Type { get; set; }
        public string Endpoint { get; set; }
        public ExporterProtocol Protocol { get; set; }
        public ExportProcessorType ProcessorType { get; set; }
        public int TimeoutMilliseconds { get; set; }
    }
}
