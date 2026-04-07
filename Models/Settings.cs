using System.Text.Json.Serialization;
using CrosshairY.Models.Dto;
using System.Windows.Input;
using CrosshairY.Managers;

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
        private readonly HotkeyManager _hotkeyManager = new();
        private HotkeyManager.Hotkey? _toggleRegisteredHotkey;
        private readonly Dictionary<string, HotkeyManager.Hotkey> _libraryRegisteredHotkeys = new();

        private KeyGesture? _toggleCrosshair;

        public event Action? ToggleCrosshairHotkeyPressed;
        public event Action? ToggleCrosshairHotkeyChanged;

        public KeyGesture? ToggleCrosshair
        {
            get => _toggleCrosshair;
            set
            {
                if (_toggleCrosshair == value)
                    return;

                UnregisterToggle();

                _toggleCrosshair = value;

                RegisterToggle();

                ToggleCrosshairHotkeyChanged?.Invoke();
            }
        }

        public void Initialize()
        {  
            ReloadLibraryHotkeys();  
        }

        private void RegisterToggle()
        {
            if (_toggleCrosshair == null)
                return;

            HotkeyManager.Hotkey hotkey = new HotkeyManager.Hotkey(_toggleCrosshair.Key, _toggleCrosshair.Modifiers)
            {
                Action = () => ToggleCrosshairHotkeyPressed?.Invoke()
            };

            _hotkeyManager.RegisterHotkey(hotkey);
            _toggleRegisteredHotkey = hotkey;
        }

        private void UnregisterToggle()
        {
            if (_toggleRegisteredHotkey != null)
            {
                _hotkeyManager.UnregisterHotkey(_toggleRegisteredHotkey);
                _toggleRegisteredHotkey = null;
            }
        }

        public void ReloadLibraryHotkeys()
        {
            // clear old library registrations
            foreach (HotkeyManager.Hotkey hotkey in _libraryRegisteredHotkeys.Values)
                _hotkeyManager.UnregisterHotkey(hotkey);
            _libraryRegisteredHotkeys.Clear();

            if (App.Settings?.LibraryHotkeys == null) return;

            foreach (KeyValuePair<string, KeyGesture?> kvp in App.Settings.LibraryHotkeys)
            {
                KeyGesture? gesture = kvp.Value;
                if (gesture == null) continue;

                string shareCode = kvp.Key;

                HotkeyManager.Hotkey hotkey = new HotkeyManager.Hotkey(gesture.Key, gesture.Modifiers)
                {
                    Action = async () =>
                    {
                        if (ShareCode.Decode(shareCode) is CrosshairSettings cs)
                           await CrosshairManager.Instance.UpdateCrosshair(cs);
                    }
                };

                _hotkeyManager.RegisterHotkey(hotkey);
                _libraryRegisteredHotkeys[shareCode] = hotkey;
            }
        }

        public void Shutdown()
        {
            UnregisterToggle();

            foreach (HotkeyManager.Hotkey hotkey in _libraryRegisteredHotkeys.Values)
                _hotkeyManager.UnregisterHotkey(hotkey);
            _libraryRegisteredHotkeys.Clear();

            _hotkeyManager.Dispose();
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