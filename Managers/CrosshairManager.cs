using CrosshairY.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace CrosshairY.Managers
{
    public sealed class CrosshairManager
    {
        private static readonly Lazy<CrosshairManager> _instance = new(() => new CrosshairManager());
        public static CrosshairManager Instance => _instance.Value;

        public ObservableCollection<CrosshairSettings> RecentCrosshairs { get; } = new ObservableCollection<CrosshairSettings>();
        public event Action<CrosshairSettings>? CrosshairChanged;

        private CrosshairManager()
        { }

        public void Initialize()
        {
            UpdateCrosshair(App.Settings.Crosshair);

            // Load recent crosshairs from settings
            RecentCrosshairs.Clear();
        }

        public void UpdateCrosshair(CrosshairSettings crosshair)
        {
            if (crosshair == null)
                return;

            App.Settings.Crosshair = JsonSerializer.Deserialize<CrosshairSettings>(
                JsonSerializer.Serialize(crosshair)!
            )!;

            App.Overlay.UpdateCrosshair(crosshair);

            SettingsService.SaveAsync(App.Settings);

            CrosshairChanged?.Invoke(crosshair);
        }
    }
}