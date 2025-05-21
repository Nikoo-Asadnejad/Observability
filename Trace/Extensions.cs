using Observability.Miscellaneous.Constants;
using Observability.Trace.Models;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Observability.Trace
{
    internal static class Extensions
    {
        public static OpenTelemetryBuilder AddTraces(this OpenTelemetryBuilder builder, TraceSetting traceSetting)
        {
            try
            {
                if (traceSetting == null)
                {
                    return builder;
                }

                var traceOptions = TraceOptions.CreateFromSetting(traceSetting);

                if (traceOptions == null)
                {
                    return builder;
                }

                builder.WithTracing(trace =>
                {
                    if (traceOptions.EnableHttpClientInstrumentation)
                    {
                        trace.AddHttpClientInstrumentation(options =>
                        {
                            options.FilterHttpRequestMessage = (httpRequestMessage) =>
                            {
                                // Example: Filter requests based on URI or headers  
                                return httpRequestMessage.RequestUri != null &&
                                       !httpRequestMessage.RequestUri.AbsoluteUri.Contains("exclude-this-uri");
                            };
                            options.RecordException = true;
                            options.EnrichWithHttpWebResponse = (activity, httpResponseMessage) =>
                            {
                                if (httpResponseMessage != null)
                                {
                                    activity.SetTag("http.status_code", (int)httpResponseMessage.StatusCode);
                                }
                            };
                            options.EnrichWithHttpWebRequest = (activity, httpRequestMessage) =>
                            {
                                if (httpRequestMessage != null)
                                {
                                    activity.SetTag("http.method", httpRequestMessage.Method.ToString());
                                    activity.SetTag("http.url", httpRequestMessage.RequestUri.ToString());
                                }
                            };
                            options.EnrichWithHttpRequestMessage = (activity , httpReq) =>
                            {
                                activity.SetTag("http.method", httpReq.Method);
                                activity.SetTag("http.url", httpReq.RequestUri);
                            };
                            options.EnrichWithHttpResponseMessage = (activity , httpReq) =>
                            {
                                activity.SetTag("http.status_code", 200);
                            };
                        });
                    }

                    if (traceOptions.EnableAspNetCoreInstrumentation)
                    {
                        trace.AddAspNetCoreInstrumentation(options=>
                        {
                            options.EnrichWithHttpRequest = (activity, httpRequest) =>
                            {
                                activity.SetTag("http.method", httpRequest.Method);
                                activity.SetTag("http.url", httpRequest.Path);
                            };
                            options.EnrichWithHttpResponse = (activity, httpResponse) =>
                            {
                                activity.SetTag("http.status_code", httpResponse.StatusCode);
                            };
                            options.RecordException = true;
                            options.Filter = (httpContext) =>
                            {
                                return true;
                               // return traceOptions.AspNetCoreOption.Endpoints.Any(ep => httpContext.Request.Path.ToString().Contains(ep));
                            };
                        });
                    }
                    
                    switch (traceOptions.Exporter.Type)
                    {
                        case ExporterType.OPTL:
                            trace.AddOtlpExporter(options =>
                            {
                                options.Protocol = traceOptions.Exporter.GetOtlpExportProtocol();
                                options.Endpoint = traceOptions.Exporter.EndpointUri;
                                options.ExportProcessorType = traceOptions.Exporter.ProcessorType;
                                options.TimeoutMilliseconds = traceOptions.Exporter.TimeoutMilliseconds;
                            });
                            break;

                        default:
                            trace.AddOtlpExporter(options =>
                            {
                                options.Protocol = traceOptions.Exporter.GetOtlpExportProtocol();
                                options.Endpoint = traceOptions.Exporter.EndpointUri;
                                options.ExportProcessorType = traceOptions.Exporter.ProcessorType;
                                options.TimeoutMilliseconds = traceOptions.Exporter.TimeoutMilliseconds;
                            });
                            break;
                    }

                });

                return builder;
            }
            catch (Exception ex)
            {
                return builder;
            }

        }
    }
}
