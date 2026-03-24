using CrosshairY.Models.Dto;
using System.Windows.Input;
using GlobalHotKey;

namespace CrosshairY.Models
{
    public class Settings
    {
        public CrosshairSettings Crosshair { get; set; } = new CrosshairSettings();
        public Hotkey Hotkey { get; set; } = new Hotkey();

        public void Apply(SettingsDto dto)
        {
            Crosshair = dto.Crosshair;

            if (dto.Hotkeys.TryGetValue("ToggleCrosshair", out HotkeyDto? hk))
            {
                Hotkey.ToggleCrosshair = new KeyGesture((Key)hk.Key, (ModifierKeys)hk.Modifiers);
            }
        }
    }

    public class Hotkey
    {
        private readonly HotKeyManager _hotKeyManager = new HotKeyManager();

        private KeyGesture? _toggleCrosshair;
        private HotKey? _registeredToggleCrosshair;

        public event Action? ToggleCrosshairHotkeyPressed;
        public event Action? ToggleCrosshairHotkeyChanged;

        public KeyGesture? ToggleCrosshair
        {
            get => _toggleCrosshair;
            set
            {
                if (_toggleCrosshair == value)
                    return;

                Unregister();

                _toggleCrosshair = value;

                Register();

                ToggleCrosshairHotkeyChanged?.Invoke();
            }
        }

        public void Initialize()
        {
            _hotKeyManager.KeyPressed += OnHotKeyPressed;
        }

        private void Register()
        {
            if (_toggleCrosshair == null)
                return;

            _registeredToggleCrosshair =
                _hotKeyManager.Register(_toggleCrosshair.Key, _toggleCrosshair.Modifiers);
        }

        private void Unregister()
        {
            if (_registeredToggleCrosshair == null)
                return;

            _hotKeyManager.Unregister(_registeredToggleCrosshair);
            _registeredToggleCrosshair = null;
        }

        private void OnHotKeyPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == _toggleCrosshair?.Key && e.HotKey.Modifiers == _toggleCrosshair?.Modifiers)
                ToggleCrosshairHotkeyPressed?.Invoke();
        }

        public void Shutdown()
        {
            _hotKeyManager.KeyPressed -= OnHotKeyPressed;
            _hotKeyManager.Dispose();
        }
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