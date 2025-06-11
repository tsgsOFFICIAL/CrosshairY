using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Text.Json;
using System.Windows;
using System.IO;

namespace CrosshairY
{
    public partial class MainWindow : Window
    {
        private OverlayWindow _overlayWindow;
        private Settings _settings;
        private List<string> _crosshairImages;
        private bool _isCrosshairEnabled;

        public MainWindow()
        {
            _overlayWindow = new OverlayWindow();
            _settings = LoadSettings();
            _crosshairImages = LoadCrosshairLibrary();
            _isCrosshairEnabled = false;

            InitializeComponent();

            // Register hotkey (Ctrl+Shift+C)
            RegisterHotKey(new WindowInteropHelper(this).Handle, 1, 0x0004 | 0x0002, 0x43); // 0x0004 = MOD_CONTROL, 0x0002 = MOD_SHIFT, 0x43 = 'C'

            Closed += (s, e) =>
            {
                _overlayWindow?.Close();
                Close();
            };
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Populate crosshair combobox
            CrosshairComboBox.ItemsSource = _crosshairImages;
            CrosshairComboBox.SelectedItem = _settings.CrosshairPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "crosshairs", "aim.png");
            SizeSlider.Value = _settings.Size;
            OpacitySlider.Value = _settings.Opacity;
            UpdateOverlay();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            UnregisterHotKey(new WindowInteropHelper(this).Handle, 1); // Unregister hotkey with ID 1
        }

        private void OnUploadImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _settings.CrosshairPath = openFileDialog.FileName;
                CrosshairComboBox.SelectedItem = _settings.CrosshairPath;
                UpdateOverlay();
            }
        }

        private void OnCrosshairComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CrosshairComboBox.SelectedItem != null)
            {
                _settings.CrosshairPath = CrosshairComboBox.SelectedItem.ToString() ?? "";
                UpdateOverlay();
            }
        }

        private void OnSizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SizeValueText == null || OpacityValueText == null)
                return;

            _settings.Size = SizeSlider.Value;
            SizeValueText.Text = $"{(int)SizeSlider.Value}px";
            UpdateOverlay();
        }

        private void OnOpacitySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OpacityValueText == null)
                return;

            _settings.Opacity = OpacitySlider.Value;
            OpacityValueText.Text = $"{(int)(OpacitySlider.Value * 100)}%";
            UpdateOverlay();
        }

        private void OnToggleButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleCrosshair();
        }

        private void ToggleCrosshair()
        {
            _isCrosshairEnabled = !_isCrosshairEnabled;
            if (_isCrosshairEnabled)
            {
                _overlayWindow.Show();
                ToggleButtonText.Text = "Disable Crosshair";
                StatusText.Text = "Crosshair Enabled";
            }
            else
            {
                _overlayWindow.Hide();
                ToggleButtonText.Text = "Enable Crosshair";
                StatusText.Text = "Crosshair Disabled";
            }
        }

        private void UpdateOverlay()
        {
            if (File.Exists(_settings.CrosshairPath))
            {
                _overlayWindow.UpdateCrosshair(_settings.CrosshairPath, _settings.Size, _settings.Opacity);
                StatusText.Text = "Crosshair updated";
            }
            else
            {
                StatusText.Text = "Invalid crosshair image";
            }
        }

        private List<string> LoadCrosshairLibrary()
        {
            List<string> images = new List<string>();
            string crosshairFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "crosshairs");

            if (Directory.Exists(crosshairFolder))
            {
                images.AddRange(Directory.GetFiles(crosshairFolder, "*.png"));
            }

            return [.. images.Distinct()]; // Remove duplicates, if any
        }

        private Settings LoadSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairY", "settings.json");
            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }

            return new Settings();
        }

        private void SaveSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairY", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            string json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(settingsPath, json);
        }

        // Hotkey handling
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == 1)
            {
                ToggleCrosshair();
                handled = true;
            }
            return IntPtr.Zero;
        }
    }

    public class Settings
    {
        public string CrosshairPath { get; set; } = "";
        public double Size { get; set; } = 50;
        public double Opacity { get; set; } = 1.0;
    }
}