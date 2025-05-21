using Observability.Miscellaneous.Models;
using System;
using System.Collections.Generic;

namespace Observability.Trace.Models
{
    public class TraceOptions
    {
        public TraceOptions(string applicationName,
                            bool enableAspNetCoreInstrumentation = true,
                            bool enableHttpClientInstrumentation = true)
        {
            ApplicationName = applicationName;
            EnableAspNetCoreInstrumentation = enableAspNetCoreInstrumentation;
            EnableHttpClientInstrumentation = enableHttpClientInstrumentation;
        }

        private HashSet<string> _traceNames = new HashSet<string>();

        public string ApplicationName { get; private set; }

        public bool EnableAspNetCoreInstrumentation { get; private set; } = true;

        public bool EnableHttpClientInstrumentation { get; private set; } = true;

        public bool EnableSqlInstrumentation { get; private set; } 

        public bool EnableHangfireInstrumentation { get; private set; }

        public bool EnableMassTransitInstrumentation { get; private set; }

        public Sql SqlOption { get; private set; }

        public Job jobOption { get; private set; }



        public ExporterOptions Exporter { get; private set; }

        public static TraceOptions CreateFromSetting(TraceSetting setting)
        {
            var options = new TraceOptions(setting?.ApplicationName , setting.EnableAspNetCoreInstrumentation , setting.EnableHttpClientInstrumentation);

            if (setting is null || setting.Items.Count <= 0)
            {
                return options;
            }

            if (setting.Exporter != null)
            {
                options.Exporter = new ExporterOptions(
                   setting.Exporter.Type,
                   setting.Exporter.Endpoint,
                   setting.Exporter.Protocol,
                   setting.Exporter.ProcessorType,
                   setting.Exporter.TimeoutMilliseconds);
            }


         

            return options;
        }

        private bool TraceNameIsDuplicate(string name)
        {
            if (_traceNames == null)
            {
                _traceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return !string.IsNullOrWhiteSpace(name) && _traceNames.Contains(name);
        }

        private bool IsNotValid(string name, string url)
        {
            return string.IsNullOrWhiteSpace(name)
                            || string.IsNullOrWhiteSpace(url)
                            || TraceNameIsDuplicate(name);
        }

        private void AddTraceName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (_traceNames is null)
            {
                _traceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            _traceNames.Add(name);
        }
    }


    public class TraceOptionItem
    {
        public TraceOptionItem(bool recordException)
        {
            RecordException = recordException;
        }

        public bool RecordException { get; private set; }
    }

    public class Job : TraceOptionItem
    {
        public Job(string[] jobNames, bool recordException)
            : base(recordException)
        {
            JobNames = jobNames;
        }

        public string[] JobNames { get;private set; } = Array.Empty<string>();

        internal static Job CreateHangfire(string url, string[] jobNames, bool recordException , string name = "Hangfire")
        {
            return new Job(jobNames, recordException);
        }
    }

    public class Sql : TraceOptionItem
    {
        public Sql(bool setDbStatementForText,
                   bool recordException,
                   bool enableConnectionLevelAttributes,
                   bool setDbStatementForStoredProcedure,
                   string[] commandNames)
           : base(recordException)
        {
            SetDbStatementForText = setDbStatementForText;
            EnableConnectionLevelAttributes = enableConnectionLevelAttributes;
            SetDbStatementForStoredProcedure = setDbStatementForStoredProcedure;
            CommandNames = commandNames ?? Array.Empty<string>();
        }

        public bool SetDbStatementForText { get; private set; }
        public bool EnableConnectionLevelAttributes { get; private set; }
        public bool SetDbStatementForStoredProcedure { get; private set; }
        public string[] CommandNames { get; private set; } = Array.Empty<string>();
    }
}
