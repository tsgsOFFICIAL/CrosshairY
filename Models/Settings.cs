using System.Text.Json.Serialization;
using CrosshairY.Models.Dto;
using System.Windows.Input;
using GlobalHotKey;

namespace CrosshairY.Models
{
    public class Settings
    {
        public CrosshairSettings Crosshair { get; set; } = new CrosshairSettings();
        public Hotkey Hotkey { get; set; } = new Hotkey();
        public AppSettings App { get; set; } = new();

        public Dictionary<string, KeyGesture?> LibraryHotkeys { get; set; } = new();

        public void Apply(SettingsDto dto)
        {
            Crosshair = dto.Crosshair;

            if (dto.Hotkeys.TryGetValue("ToggleCrosshair", out HotkeyDto? hk))
            {
                Hotkey.ToggleCrosshair = new KeyGesture((Key)hk.Key, (ModifierKeys)hk.Modifiers);
            }

            LibraryHotkeys.Clear();
            foreach (KeyValuePair<string, HotkeyDto> kvp in dto.Hotkeys)
            {
                if (kvp.Key == "ToggleCrosshair")
                    continue;

                HotkeyDto h = kvp.Value;
                LibraryHotkeys[kvp.Key] = new KeyGesture((Key)h.Key, (ModifierKeys)h.Modifiers);
            }

            App.Apply(dto.App!);
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
        [JsonIgnore]
        public string HotkeyDisplay
        {
            get
            {
                if (App.Settings?.LibraryHotkeys == null)
                    return "Assign Hotkey";

                string shareCode = ShareCode.Encode(this);

                if (App.Settings.LibraryHotkeys.TryGetValue(shareCode, out KeyGesture? gesture) && gesture != null)
                {
                    return FormatHotkey(gesture);
                }

                return "Assign Hotkey";
            }
        }

        public string CrosshairName { get; set; } = "My Crosshair";
        public string Description { get; set; } = "A very cool crosshair";

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

        private static string FormatHotkey(KeyGesture gesture)
        {
            if (gesture == null)
                return "Assign Hotkey";

            List<string> parts = new List<string>();
            if (gesture.Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (gesture.Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (gesture.Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (gesture.Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");

            parts.Add(gesture.Key.ToString());

            return string.Join(" + ", parts);
        }
    }
}