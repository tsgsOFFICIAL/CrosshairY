using System.Collections.ObjectModel;
using CrosshairY.Models;
using System.Text.Json;
using System.Windows;
using System.IO;

namespace CrosshairY.Managers
{
    public sealed class CrosshairManager
    {
        private static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RecentCrosshairs.json");

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        private static readonly Lazy<CrosshairManager> _instance = new(() => new CrosshairManager());
        public static CrosshairManager Instance => _instance.Value;

        public ObservableCollection<CrosshairSettings> RecentCrosshairs { get; private set; } = new ObservableCollection<CrosshairSettings>();
        private List<string> _recentCodes = new();

        public event Action<CrosshairSettings>? CrosshairChanged;

        private CrosshairManager()
        {
            LoadRecentCrosshairs();
        }

        private void LoadRecentCrosshairs()
        {
            if (!File.Exists(Path))
                return;

            try
            {
                string json = File.ReadAllText(Path);
                _recentCodes = JsonSerializer.Deserialize<List<string>>(json) ?? new();

                RecentCrosshairs.Clear();

                foreach (string code in _recentCodes)
                {
                    if (ShareCode.Decode(code) is CrosshairSettings cs)
                        RecentCrosshairs.Add(cs);
                }
            }
            catch
            {
                _recentCodes.Clear();
                RecentCrosshairs.Clear();
            }
        }

        private async Task SaveRecentCrosshairsAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_recentCodes, Options);
                await File.WriteAllTextAsync(Path, json);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Failed to save recent crosshairs.", "CrosshairY", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Initialize()
        {
            _ = UpdateCrosshair(App.Settings.Crosshair);
        }

        public async Task UpdateCrosshair(CrosshairSettings crosshair)
        {
            if (crosshair == null)
                return;

            App.Settings.Crosshair = JsonSerializer.Deserialize<CrosshairSettings>(
                JsonSerializer.Serialize(crosshair)!
            )!;

            App.Overlay.UpdateCrosshair(crosshair);

            await SettingsService.SaveAsync(App.Settings);

            string encoded = ShareCode.Encode(crosshair);

            // Remove if already exists (so we can re-add it to the end = most recent)
            int existingIndex = _recentCodes.IndexOf(encoded);
            if (existingIndex >= 0)
            {
                _recentCodes.RemoveAt(existingIndex);
                RecentCrosshairs.RemoveAt(existingIndex);
            }

            // Add newest
            _recentCodes.Add(encoded);
            RecentCrosshairs.Add(crosshair);

            // Keep max 5
            if (_recentCodes.Count > 5)
            {
                _recentCodes.RemoveAt(0);
                RecentCrosshairs.RemoveAt(0);
            }

            CrosshairChanged?.Invoke(crosshair);

            await SaveRecentCrosshairsAsync();
        }
    }
}