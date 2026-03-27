using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.ComponentModel;
using CrosshairY.Managers;
using CrosshairY.Utility;
using System.Diagnostics;
using CrosshairY.Models;
using System.Windows;
using System.IO;

namespace CrosshairY.Pages
{
    public partial class SettingsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Working copy of AppSettings
        public AppSettings Settings { get; private set; }

        // Backing fields for UI-bound properties
        private bool _startWithWindows;
        private bool _startMinimized;
        private bool _runInBackground;
        private bool _autoUpdate;
        private bool _updateAvailable;

        // Exposed properties bound to the UI
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                if (SetField(ref _startWithWindows, value, nameof(StartWithWindows)))
                {
                    if (!value && StartMinimized)
                        StartMinimized = false;
                }
            }
        }

        public bool StartMinimized
        {
            get => _startMinimized;
            set => SetField(ref _startMinimized, value, nameof(StartMinimized));
        }

        public bool RunInBackground
        {
            get => _runInBackground;
            set => SetField(ref _runInBackground, value, nameof(RunInBackground));
        }

        public bool AutoUpdate
        {
            get => _autoUpdate;
            set => SetField(ref _autoUpdate, value, nameof(AutoUpdate));
        }

        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set => SetField(ref _updateAvailable, value, nameof(UpdateAvailable));
        }

        public string AppVersion
        {
            get
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(Helper.GetExePath());
                return info.FileVersion ?? "N/A";
            }
        }

        public SettingsPage()
        {
            InitializeComponent();

            // Clone the live settings
            Settings = AppSettings.Clone(App.Settings.App);
            Settings.UpdateAvailable = App.Settings.App.UpdateAvailable; // Json ignore forces us to manually copy this over

            // Initialize backing fields from the cloned settings
            StartWithWindows = Settings.StartWithWindows;
            StartMinimized = Settings.StartMinimized;
            RunInBackground = Settings.RunInBackground;
            AutoUpdate = Settings.AutoUpdate;
            UpdateAvailable = Settings.UpdateAvailable;

            DataContext = this;

            UpdateManager.Instance.UpdateAvailable += OnUpdateAvailable;
        }

        private void OnUpdateAvailable(object? sender, EventArgs e)
        {
            Settings.UpdateAvailable = App.Settings.App.UpdateAvailable; // Json ignore forces us to manually copy this over
            UpdateAvailable = Settings.UpdateAvailable;
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);

            // Special handling
            if (propertyName is nameof(StartWithWindows) or nameof(StartMinimized))
                UpdateStartupRegistry();

            // Auto-save on change
            Task.Run(SaveSettings);

            return true;
        }

        private Task SaveSettings()
        {
            // Push backing fields to working Settings clone
            Settings.StartWithWindows = _startWithWindows;
            Settings.StartMinimized = _startMinimized;
            Settings.RunInBackground = _runInBackground;
            Settings.AutoUpdate = _autoUpdate;

            // Copy to live settings and persist
            App.Settings.App = AppSettings.Clone(Settings);
            return SettingsService.SaveAsync(App.Settings);
        }

        private void UpdateStartupRegistry()
        {
            string keyName = "CrosshairY";
            string exePath = Helper.GetExePath();

            try
            {
                if (!StartWithWindows)
                {
                    // Just remove it - clean and simple
                    Helper.RemoveFromRegistry(keyName);
                    return;
                }

                // StartWithWindows = true -> we MUST have a registry entry
                if (StartMinimized)
                {
                    // Launch minimized
                    Helper.WriteToRegistry(keyName, exePath, ["--minimize"]);
                }
                else
                {
                    // Launch normally
                    Helper.WriteToRegistry(keyName, exePath);
                }
            }
            catch (Exception)
            { }
        }

        private async void OnCheckForUpdatesButtonClicked(object sender, RoutedEventArgs e)
        {
            UpdateAvailable = await UpdateManager.Instance.IsUpdateAvailableAsync();
        }

        private async void OnUpdateButtonClicked(object sender, RoutedEventArgs e)
        {
            await UpdateManager.Instance.DownloadUpdate();
        }

        private void OnExportButtonClicked(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyCrosshairs.json");

            if (!File.Exists(path))
            {
                NotificationManager.ShowNotification("Export Failed", "No MyCrosshairs.json file found to export.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            });
        }

        private async void OnImportButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Select MyCrosshairs.json"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyCrosshairs.json");
            string backupPath = targetPath + ".bak";

            try
            {
                // Backup existing file
                if (File.Exists(targetPath))
                {
                    File.Copy(targetPath, backupPath, overwrite: true);
                }

                // Copy new file
                File.Copy(dialog.FileName, targetPath, overwrite: true);

                await App.LoadSettingsAsync();

                NotificationManager.ShowNotification("Imported Successfully", "Crosshairs imported successfully");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Import failed:\n{ex.Message}", "CrosshairY", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnResetButtonClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
                "This will reset all settings and crosshairs. Continue?",
                "CrosshairY",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] files =
            [
                Path.Combine(baseDir, "MyCrosshairs.json"),
                Path.Combine(baseDir, "RecentCrosshairs.json"),
                Path.Combine(baseDir, "Settings.json")
            ];

            try
            {
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        // Create backup (*.bak)
                        File.Copy(file, $"{file}.bak");

                        // Delete
                        File.Delete(file);
                    }
                }

                await App.LoadSettingsAsync();
            }
            catch (Exception ex)
            {
                NotificationManager.ShowNotification("Reset Failed", $"An error occurred while resetting settings. See details.\n{ex.Message}");
            }
        }
    }
}