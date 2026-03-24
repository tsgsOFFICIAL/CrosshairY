using CrosshairY.Models.Dto;
using System.Windows.Input;
using CrosshairY.Models;
using System.Text.Json;
using System.IO;

namespace CrosshairY.Managers
{
    public static class SettingsService
    {
        private static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public static async Task<SettingsDto> LoadAsync()
        {
            if (!File.Exists(Path))
                return new SettingsDto();

            string json = await File.ReadAllTextAsync(Path);
            return JsonSerializer.Deserialize<SettingsDto>(json) ?? new SettingsDto();
        }

        public static Task SaveAsync(Settings settings)
        {
            SettingsDto dto = new SettingsDto
            {
                Crosshair = settings.Crosshair,
                Hotkeys = new Dictionary<string, HotkeyDto>
                {
                    ["ToggleCrosshair"] = new HotkeyDto
                    {
                        Key = (int)(settings.Hotkey.ToggleCrosshair?.Key ?? Key.None),
                        Modifiers = (int)(settings.Hotkey.ToggleCrosshair?.Modifiers ?? ModifierKeys.None)
                    }
                }
            };

            string json = JsonSerializer.Serialize(dto, Options);
            return File.WriteAllTextAsync(Path, json);
        }
    }
}