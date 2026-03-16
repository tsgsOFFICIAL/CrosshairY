using System.Text.Json;
using System.IO;

namespace CrosshairY
{
    public class SettingsManager
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public CrosshairSettings? Load()
        {
            if (!File.Exists(_path))
                return null;
            
            string json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<CrosshairSettings>(json);
        }

        public void Save(CrosshairSettings s) => File.WriteAllText(_path, JsonSerializer.Serialize(s));
    }
}