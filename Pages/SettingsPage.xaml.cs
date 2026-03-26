using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.ComponentModel;
using CrosshairY.Managers;
using CrosshairY.Utility;
using System.Diagnostics;
using CrosshairY.Models;

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

            // Initialize backing fields from the cloned settings
            _startWithWindows = Settings.StartWithWindows;
            _startMinimized = Settings.StartMinimized;
            _runInBackground = Settings.RunInBackground;
            _autoUpdate = Settings.AutoUpdate;

            DataContext = this;
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
    }
}