using Observability.Miscellaneous.Models;
using Observability.Trace.Constants;
using System;
using System.Collections.Generic;

namespace Observability.Trace.Models
{
    public class TraceSetting
    {
        public string ApplicationName { get; set; }
        public bool EnableAspNetCoreInstrumentation { get; private set; } = true;
        public bool EnableHttpClientInstrumentation { get; private set; } = true;
        public ExporterSetting Exporter { get; set; }
        public List<TraceSettingItem> Items { get; set; } = new List<TraceSettingItem>();
    }

    public class TraceSettingItem
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }
        public string JobName { get; set; }
        public bool RecordException { get; set; }
        public TraceSettingType TypeEnum {

            get
            {
                if (Enum.TryParse(Type, true, out TraceSettingType result))
                {
                    return result;
                }

                return TraceSettingType.Non;
            }
        }
    }
}
