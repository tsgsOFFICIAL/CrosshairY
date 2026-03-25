using System.Windows.Controls;
using CrosshairY.Managers;
using CrosshairY.Models;
using System.Text.Json;
using System.Windows;
using System.IO;

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
        private List<string> _communityCrosshairCodes = [
                "TSGS-QLNeZ-gFBjD-5uvqP-GnZvY-R5Dhi-TQkqO-dchd9-ZbJz8-AKZzk-Mbu6V-LjHTf-sXL3A-qncE2-2JgLs-2uzDu-ARhcy-9yDXH-JxqmO-LK",
                "TSGS-FRpNW-iXbjq-QMchK-svKn8-jY94H-U4DzA-Xa",
                "TSGS-CvZNa-mRmbN-rYYJH-BBE8s-qoiqQ-oaWVN-nkuzN-3aLKQ-YgZdQ-5MFvs-LpyL2-mDvXY-RoXRB-XmGoM-8MfsZ-f5rOK-9kecL-Bs",
                "TSGS-GVAmh-KhzBp-PeRgD-XGf95-spPR7-9u6Mh-XW",
                "TSGS-FRpMH-TZLPA-GJ6xA-jgPqD-CeMnB-B9qv3-bH"
            ];

        public LibraryPage()
        {
            InitializeComponent();

            MyCrosshairsTab.Checked += (_, __) => SelectedTab = LibraryTab.MyCrosshairs;
            CommunityTab.Checked += (_, __) => SelectedTab = LibraryTab.Community;

            LoadCrosshairs();
        }

        private void LoadCrosshairs()
        {
            if (SelectedTab == LibraryTab.MyCrosshairs)
                LoadMyCrosshairsFromFile();

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

        private void OnApplyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CrosshairSettings crosshair)
            {
                CrosshairManager.Instance.UpdateCrosshair(crosshair);
            }
        }

        private void OnHotkeyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is CrosshairSettings crosshair)
            {
                System.Windows.MessageBox.Show("Hotkey assignment is not implemented yet.", "CrosshairY", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnDeleteButtonClicked(object sender, RoutedEventArgs e)
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
                    _ = SaveCrosshairsAsync();
                    LoadCrosshairs();        // Refresh the list
                }
            }
        }
    }
}