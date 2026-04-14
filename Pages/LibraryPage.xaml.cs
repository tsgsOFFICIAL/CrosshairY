using System.Windows.Controls;
using System.Windows.Input;
using CrosshairY.Managers;
using CrosshairY.Windows;
using CrosshairY.Models;
using System.Text.Json;
using System.Windows;
using System.IO;
using System.Net.Http;

namespace CrosshairY.Pages
{
    public partial class LibraryPage : Page
    {
        private static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyCrosshairs.json");

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public enum LibraryTab
        {
            MyCrosshairs,
            Community
        }

        private LibraryTab _selectedTab = LibraryTab.MyCrosshairs;

        public LibraryTab SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                LoadCrosshairs();
            }
        }

        private List<string> _myCrosshairCodes = [];
        private List<string> _communityCrosshairCodes = [];

        public LibraryPage()
        {
            InitializeComponent();

            MyCrosshairsTab.Checked += (_, __) => SelectedTab = LibraryTab.MyCrosshairs;
            CommunityTab.Checked += (_, __) => SelectedTab = LibraryTab.Community;

            LoadCrosshairs();
        }

        private async void LoadCrosshairs()
        {
            if (SelectedTab == LibraryTab.MyCrosshairs)
                LoadMyCrosshairsFromFile();
            else
               await LoadCommunityCrosshairs();

            List<CrosshairSettings> crosshairsToDisplay = new List<CrosshairSettings>();

            if (SelectedTab == LibraryTab.MyCrosshairs)
            {
                foreach (string code in _myCrosshairCodes)
                {
                    if (ShareCode.Decode(code) is CrosshairSettings cs)
                        crosshairsToDisplay.Add(cs);
                }
            }
            else
            {
                foreach (string code in _communityCrosshairCodes)
                {
                    if (ShareCode.Decode(code) is CrosshairSettings cs)
                        crosshairsToDisplay.Add(cs);
                }
            }

            CrosshairsGrid.ItemsSource = crosshairsToDisplay;
        }

        private void LoadMyCrosshairsFromFile()
        {
            if (!File.Exists(Path))
            {
                _myCrosshairCodes.Clear();
                return;
            }

            try
            {
                string json = File.ReadAllText(Path);
                _myCrosshairCodes = JsonSerializer.Deserialize<List<string>>(json) ?? [];
            }
            catch
            {
                _myCrosshairCodes.Clear();   // Fallback on error
            }
        }

        private async Task LoadCommunityCrosshairs()
        {
            try
            {
                using HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = true
                };

                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                string response = await client.GetStringAsync("https://raw.githubusercontent.com/tsgsOFFICIAL/CrosshairY/master/CommunityCrosshairs.json");

                _communityCrosshairCodes = JsonSerializer.Deserialize<List<string>>(response) ?? [];
            }
            catch (Exception)
            {
                _communityCrosshairCodes.Clear();
            }
        }

        public async Task SaveCrosshairsAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_myCrosshairCodes, Options);
                await File.WriteAllTextAsync(Path, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save library: {ex.Message}", "CrosshairY", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string? FindConflictingHotkeyOwner(KeyGesture gesture, string? currentShareCode = null)
        {
            if (gesture == null)
                return null;

            KeyGesture? toggle = App.Settings.Hotkey.ToggleCrosshair;
            if (toggle != null &&
                toggle.Key == gesture.Key &&
                toggle.Modifiers == gesture.Modifiers)
            {
                return "Toggle Crosshair";
            }

            foreach (KeyValuePair<string, KeyGesture?> kvp in App.Settings.LibraryHotkeys)
            {
                if (kvp.Key == currentShareCode)
                    continue; // allow re-assigning the SAME crosshair

                KeyGesture? existing = kvp.Value;
                if (existing != null &&
                    existing.Key == gesture.Key &&
                    existing.Modifiers == gesture.Modifiers)
                {
                    return "another Library crosshair";
                }
            }

            return null; // no conflict
        }

        private void OnPreviewCanvasLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Canvas canvas && canvas.DataContext is CrosshairSettings settings)
            {
                canvas.Children.Clear();
                CrosshairRenderer.Render(canvas, settings);
            }
        }

        private async void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            string newCode = ShareCode.Encode(App.Settings.Crosshair);

            if (_myCrosshairCodes.Contains(newCode))
            {
                System.Windows.MessageBox.Show("This crosshair is already in your library.", "CrosshairY", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _myCrosshairCodes.Add(newCode);
            await SaveCrosshairsAsync();
            LoadCrosshairs();
        }

        private async void OnApplyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CrosshairSettings crosshair)
            {
                await CrosshairManager.Instance.UpdateCrosshair(crosshair);
            }
        }

        private async void OnHotkeyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CrosshairSettings crosshair)
            {
                string shareCode = ShareCode.Encode(crosshair); // this is the unique key

                HotkeyDialog dialog = new HotkeyDialog();
                if (dialog.ShowDialog() == true && dialog.SelectedHotkey != null)
                {
                    string? conflict = FindConflictingHotkeyOwner(dialog.SelectedHotkey, shareCode);
                    if (conflict != null)
                    {
                        System.Windows.MessageBox.Show(
                            $"This hotkey is already assigned to {conflict}.\n\nPlease choose a different combination.",
                            "CrosshairY",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return; // ← do NOT save
                    }

                    App.Settings.LibraryHotkeys[shareCode] = dialog.SelectedHotkey;
                }
                else
                {
                    App.Settings.LibraryHotkeys.Remove(shareCode);
                }

                await SettingsService.SaveAsync(App.Settings);
                App.Settings.Hotkey.ReloadLibraryHotkeys();
                LoadCrosshairs();
            }
        }

        private async void OnDeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CrosshairSettings itemToDelete)
            {
                // Find and remove the matching encoded string
                string? codeToRemove = _myCrosshairCodes.FirstOrDefault(code =>
                {
                    CrosshairSettings? decoded = ShareCode.Decode(code);
                    return decoded != null &&
                           decoded.CrosshairName == itemToDelete.CrosshairName &&
                           decoded.Description == itemToDelete.Description;
                });

                if (codeToRemove != null)
                {
                    _myCrosshairCodes.Remove(codeToRemove);
                    await SaveCrosshairsAsync();
                    LoadCrosshairs();
                }
            }
        }
    }
}