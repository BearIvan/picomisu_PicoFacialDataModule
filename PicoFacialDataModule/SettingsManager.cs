using System.Reflection;
using System.Text.Json;

namespace PicoFacialDataModule
{
    public class ModuleSettings {
        public bool DisableEyeTracking { get; set; }
        public bool DisableFaceTracking { get; set; }
        /// <summary>
        /// When defined, it will not send a multicast but instead connect directly.
        /// </summary>
        public string? IP { get; set; }
        public TrackingSettings TrackingSettings { get; set; } = new();
    }

    public class TrackingSettings {
        public float eyePupilDilationEyeOpennessThreshold { get; set; } = 0.8f;
    }

    public class DaemonSettings
    {
        //TODO.
    }

    public class SettingsManager
    {
        private const string configFileName = "PicoFacialDataModule.json";
        
        public static ModuleSettings GetOrCreate()
        {
            string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, configFileName);

            if (!File.Exists(configPath))
            {
                ModuleSettings moduleSettings = new ModuleSettings();

                string defaultJson = JsonSerializer.Serialize(moduleSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, defaultJson);

                return moduleSettings;
            }

            var settings = JsonSerializer.Deserialize<ModuleSettings>(File.ReadAllText(configPath))!;

            // Re-save the settings, so that any new settings between versions are added to the file.
            File.WriteAllText(configPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));

            return settings;
        }
    }
}
