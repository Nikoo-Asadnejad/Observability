using Newtonsoft.Json;
using Observability.Miscellaneous.Models;
using Serilog;
using System;
using System.IO;

namespace Observability.Miscellaneous.Services
{
    public static class ObservabilityService
    {
        private static string _settingFileAddress = "observability.json";

        public static ObservabilitySetting LoadSetting()
        {
            try
            {
                if (!ObservabilityIsEnabled())
                {
                    return null;
                }

                var jsonContent = File.ReadAllText(_settingFileAddress);

                return JsonConvert.DeserializeObject<ObservabilitySetting>(jsonContent);
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading observability settings from {_settingFileAddress}: {ex.Message}");
                return null;
            }

        }

        public static bool ObservabilityIsEnabled()
        {
            return !string.IsNullOrWhiteSpace(_settingFileAddress) && File.Exists(_settingFileAddress);
        }
    }
}
