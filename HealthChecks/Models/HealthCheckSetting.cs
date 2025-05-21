using Observability.HealthChecks.Constants;
using System;
using System.Collections.Generic;

namespace Observability.HealthChecks.Models
{
    public class HealthCheckSetting
    {
        public string ApplicationName { get; set; }
        public List<HealthCheckSettingItem> Items { get; set; } = new List<HealthCheckSettingItem>();
    }

    public class HealthCheckSettingItem
    {
        public HealthCheckSettingItem()
        {

        }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }
        public int? AllowedFailureThreshold { get; set; }
        public string CdnBucketName { get; set; }
        public string CdnAccessKey { get; set; }
        public string CdnSecretKey { get; set; }
        public int? PingTimeoutMilliSecond { get; set; } = 5000;
        public string UserName { get; set; }
        public string Password { get; set; }

        public HealthCheckSettingType TypeEnum {

            get
            {
                if (Enum.TryParse(Type, true, out HealthCheckSettingType result))
                {
                    return result;
                }

                return HealthCheckSettingType.Non;
            }
        }
    }
}
