using CrosshairY.Models;
using System.Text.Json;
using System.IO;

namespace CrosshairY.Managers
{
    public static class SettingsService
    {
        private static readonly string Path =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public static async Task<Settings> LoadAsync()
        {
            if (!File.Exists(Path))
                return new Settings();

            try
            {
                string json = await File.ReadAllTextAsync(Path);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public static Task SaveAsync(Settings settings)
        {
            string json = JsonSerializer.Serialize(settings, Options);
            return File.WriteAllTextAsync(Path, json);
        }
    }
}