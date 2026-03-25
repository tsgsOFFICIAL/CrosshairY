using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using CrosshairY.Managers;
using CrosshairY.Windows;
using CrosshairY.Models;
using System.Windows;

namespace CrosshairY.Pages
{
    public partial class HomePage : Page, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>This event is typically raised by the implementation of the INotifyPropertyChanged
        /// interface to notify subscribers that a property value has changed. Handlers receive the name of the property
        /// that changed in the event data. This event is commonly used in data binding scenarios to update UI elements
        /// when underlying data changes.</remarks>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Raises the PropertyChanged event to notify listeners that a property value has changed.
        /// </summary>
        /// <remarks>Use this method to implement the INotifyPropertyChanged interface in classes that
        /// support data binding. Calling this method with the correct property name ensures that UI elements or other
        /// listeners are updated when the property value changes.</remarks>
        /// <param name="name">The name of the property that changed. This value is optional and is automatically provided when called from
        /// a property setter.</param>
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _toggleCrosshairString = "";

        public string ToggleCrosshairString
        {
            get => string.IsNullOrEmpty(_toggleCrosshairString) ? "None" : _toggleCrosshairString;
            set
            {
                _toggleCrosshairString = value;
                OnPropertyChanged();
            }
        }

        private CrosshairSettings _crosshair = new();

        public CrosshairSettings Crosshair
        {
            get => _crosshair;
            set
            {
                _crosshair = value;
                OnPropertyChanged();
            }
        }

        public HomePage()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnPageLoaded;

            App.Settings.Hotkey.ToggleCrosshairHotkeyChanged += OnToggleCrosshairHotkeyChanged;
            CrosshairManager.Instance.CrosshairChanged += OnCrosshairChanged;
            ActiveCrosshairCanvas.SizeChanged += OnActiveCrosshairCanvasSizeChanged;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            OnToggleCrosshairHotkeyChanged();
            Crosshair = App.Settings.Crosshair;
        }

        private void OnActiveCrosshairCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 1 && e.NewSize.Height > 1)  // avoid tiny initial values
            {
                CrosshairRenderer.Render(ActiveCrosshairCanvas, App.Settings.Crosshair);
            }
        }

        private void OnCrosshairChanged(CrosshairSettings settings)
        {
            CrosshairRenderer.Render(ActiveCrosshairCanvas, settings);
        }

        private void OnToggleCrosshairHotkeyChanged()
        {
            KeyGesture? toggleCrosshair = App.Settings.Hotkey.ToggleCrosshair;
            ToggleCrosshairString = toggleCrosshair == null ? "None" : $"{(toggleCrosshair.Modifiers == ModifierKeys.None ? "" : $"{toggleCrosshair.Modifiers} + ")}{toggleCrosshair.Key}"; ;
        }

        private async void OnUnbindHotkeyButtonClicked(object sender, RoutedEventArgs e)
        {
            // Unbind hotkey
            App.Settings.Hotkey.ToggleCrosshair = null;
            // Save settings file
            await SettingsService.SaveAsync(App.Settings);
        }

        private async void OnChangeHotkeyButtonClicked(object sender, RoutedEventArgs e)
        {
            HotkeyDialog hotkeyDialog = new HotkeyDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (hotkeyDialog.ShowDialog() == true)
            {
                if (hotkeyDialog.SelectedHotkey != null)
                {
                    // Bind new hotkey
                    App.Settings.Hotkey.ToggleCrosshair = hotkeyDialog.SelectedHotkey;

                    // Save settings file
                    await SettingsService.SaveAsync(App.Settings);
                }
            }
        }

        private void OnOpenDesignerButtonClicked(object sender, RoutedEventArgs e)
        {
            AppNavigationService.Navigate<DesignerPage>();
        }

        private void OnBrowseCrosshairsButtonClicked(object sender, RoutedEventArgs e)
        {
            AppNavigationService.Navigate<LibraryPage>();
        }
    }
}