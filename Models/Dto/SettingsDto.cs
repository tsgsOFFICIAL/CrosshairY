namespace CrosshairY.Models.Dto
{
    public class SettingsDto
    {
        public int Version { get; set; } = 1; // Default for old files

        public const int CurrentVersion = 2;

        public CrosshairSettings Crosshair { get; set; } = new();
        public Dictionary<string, HotkeyDto> Hotkeys { get; set; } = new();
        public AppSettingsDto? App { get; set; } // Nullable for v1 support
    }
}