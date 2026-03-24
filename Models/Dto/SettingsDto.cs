namespace CrosshairY.Models.Dto
{
    public class SettingsDto
    {
        public CrosshairSettings Crosshair { get; set; } = new();

        public Dictionary<string, HotkeyDto> Hotkeys { get; set; } = new();
    }

    public class HotkeyDto
    {
        public int Key { get; set; }
        public int Modifiers { get; set; }
    }
}