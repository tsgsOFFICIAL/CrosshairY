using System.Text.Json.Serialization;
using CrosshairY.Models.Dto;
using System.Text.Json;

namespace CrosshairY.Models
{
    public class AppSettings
    {
        public bool StartWithWindows { get; set; }
        public bool StartMinimized { get; set; }
        public bool RunInBackground { get; set; }
        public bool AutoUpdate { get; set; }

        [JsonIgnore]
        public bool UpdateAvailable { get; set; }

        public void Apply(AppSettingsDto dto)
        {
            if (dto == null) 
                return;

            StartWithWindows = dto.StartWithWindows;
            StartMinimized = dto.StartMinimized;
            RunInBackground = dto.RunInBackground;
            AutoUpdate = dto.AutoUpdate;
        }

        public static T Clone<T>(T source)
        {
            string json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}