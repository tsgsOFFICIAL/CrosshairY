using System.Windows.Input;

namespace CrosshairY.Models
{
    public class Settings
    {
        public CrosshairSettings Crosshair { get; set; } = new CrosshairSettings();
        public Hotkey Hotkey { get; set; } = new Hotkey();
    }

    public class Hotkey
    {
        public KeyGesture? ToggleCrosshair { get; set; }
    }

    public class CrosshairSettings
    {
        public float Gap { get; set; } = 0;
        public float Length { get; set; } = 10;
        public float Thickness { get; set; } = 1;
        public float OutlineThickness { get; set; } = 1;
        public bool Dot { get; set; }
        public bool TStyle { get; set; }
        public bool Outline { get; set; } = true;
        public byte ColorR { get; set; } = 255;
        public byte ColorG { get; set; } = 255;
        public byte ColorB { get; set; } = 255;
        public byte Alpha { get; set; } = 255;
    }
}