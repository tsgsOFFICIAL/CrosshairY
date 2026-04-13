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
                return new SettingsDto { Version = SettingsDto.CurrentVersion };

            string json = await File.ReadAllTextAsync(Path);

            SettingsDto dto;

            try
            {
                dto = JsonSerializer.Deserialize<SettingsDto>(json) ?? new SettingsDto();
            }
            catch
            {
                return new SettingsDto { Version = SettingsDto.CurrentVersion };
            }

            return Upgrade(dto);
        }

        public static Task SaveAsync(Settings settings)
        {
            Dictionary<string, HotkeyDto> Hotkeys = new Dictionary<string, HotkeyDto>
            {
                ["ToggleCrosshair"] = new HotkeyDto
                {
                    Key = (int)(settings.Hotkey.ToggleCrosshair?.Key ?? Key.None),
                    Modifiers = (int)(settings.Hotkey.ToggleCrosshair?.Modifiers ?? ModifierKeys.None)
                }
            };

            foreach (KeyValuePair<string, KeyGesture?> kv in settings.LibraryHotkeys)
            {
                if (kv.Value == null) continue;
                Hotkeys[kv.Key] = new HotkeyDto
                {
                    Key = (int)kv.Value.Key,
                    Modifiers = (int)kv.Value.Modifiers
                };
            }

            SettingsDto dto = new SettingsDto
            {
                Version = SettingsDto.CurrentVersion, // Always latest

                Crosshair = settings.Crosshair,

                Hotkeys = Hotkeys,

                App = new AppSettingsDto
                {
                    StartWithWindows = settings.App.StartWithWindows,
                    StartMinimized = settings.App.StartMinimized,
                    RunInBackground = settings.App.RunInBackground,
                    AutoUpdate = settings.App.AutoUpdate,
                }
            };

            string json = JsonSerializer.Serialize(dto, Options);
            return File.WriteAllTextAsync(Path, json);
        }

        private static SettingsDto UpgradeFrom1To2(SettingsDto dto)
        {
            dto.App ??= new AppSettingsDto
            {
                StartWithWindows = true,
                StartMinimized = false,
                RunInBackground = true,
                AutoUpdate = true,
            };

            dto.Version = 2;
            return dto;
        }

        private static SettingsDto Upgrade(SettingsDto dto)
        {
            while (dto.Version < SettingsDto.CurrentVersion)
            {
                dto = dto.Version switch
                {
                    1 => UpgradeFrom1To2(dto),
                    _ => dto
                };
            }

            return dto;
        }
    }
}