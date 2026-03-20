using System.Windows.Input;

namespace CrosshairY.Managers
{
    public class HotkeyService
    {
        public event Action? HotkeyChanged;

        private KeyGesture? _toggleCrosshair;

        public KeyGesture? ToggleCrosshair
        {
            get => _toggleCrosshair;
            set
            {
                if (_toggleCrosshair == value)
                    return;

                _toggleCrosshair = value;
                HotkeyChanged?.Invoke();
            }
        }
    }
}