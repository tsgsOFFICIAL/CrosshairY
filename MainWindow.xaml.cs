using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
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
        private readonly string _crosshairFolder;

        public MainWindow()
        {
            _overlayWindow = new OverlayWindow();
            _crosshairFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "crosshairs");
            _crosshairImages = LoadCrosshairLibrary();
            _settings = LoadSettings();
            _isCrosshairEnabled = false;

            InitializeComponent();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            CrosshairComboBox.ItemsSource = _crosshairImages;
            CrosshairComboBox.SelectedItem = _settings.CrosshairPath;

            if (CrosshairComboBox.SelectedItem == null && _crosshairImages.Count > 0)
            {
                CrosshairComboBox.SelectedItem = _crosshairImages[0];
                _settings.CrosshairPath = _crosshairImages[0];
            }

            SizeSlider.Value = Math.Clamp(_settings.Size, 10, 200);
            OpacitySlider.Value = Math.Clamp(_settings.Opacity, 0.1, 1.0);
            UpdateOverlay();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _overlayWindow?.Close();

            SaveSettings();
            UnregisterHotKey(new WindowInteropHelper(this).Handle, 1);
        }

        private void OnUploadImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Image (*.png)|*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
               
                _settings.CrosshairPath = fileName;
                CrosshairComboBox.SelectedItem = fileName;
                
                if (!_crosshairImages.Contains(fileName))
                    _crosshairImages.Add(fileName);
               
                CrosshairComboBox.ItemsSource = null; // Reset to refresh
                CrosshairComboBox.ItemsSource = _crosshairImages;
                CrosshairComboBox.SelectedItem = fileName;
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
            if (SizeValueText == null)
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
            try
            {
                string crosshairPath = Path.Combine(_crosshairFolder, _settings.CrosshairPath + ".png");
                
                if (!string.IsNullOrEmpty(_settings.CrosshairPath) && File.Exists(crosshairPath))
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(crosshairPath));

                    _overlayWindow.UpdateCrosshair(crosshairPath, _settings.Size, _settings.Opacity);
                    PreviewImage.Source = bitmap;
                    StatusText.Text = "Crosshair updated";
                }
                else
                {
                    PreviewImage.Source = null;
                    StatusText.Text = _crosshairImages.Count > 0 ? "Invalid crosshair image" : "No crosshairs found";
                }
            }
            catch (Exception ex)
            {
                PreviewImage.Source = null;
                StatusText.Text = $"Failed to load crosshair: {ex.Message}";
            }
        }

        private List<string> LoadCrosshairLibrary()
        {
            List<string> images = new List<string>();
            
            if (Directory.Exists(_crosshairFolder))
            {
                images.AddRange(Directory.GetFiles(_crosshairFolder, "*.png").Select(Path.GetFileNameWithoutExtension)!);
            }

            return [.. images.Distinct()];
        }

        private Settings LoadSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairY", "settings.json");
            
            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                Settings settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                settings.Size = Math.Clamp(settings.Size, 10, 200);
                settings.Opacity = Math.Clamp(settings.Opacity, 0.1, 1.0);
               
                if (string.IsNullOrEmpty(settings.CrosshairPath) && _crosshairImages.Count > 0)
                    settings.CrosshairPath = _crosshairImages[0];

                return settings;
            }

            Settings defaultSettings = new Settings();

            if (_crosshairImages.Count > 0)
                defaultSettings.CrosshairPath = _crosshairImages[0];
            return defaultSettings;
        }

        private void SaveSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairY", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            string json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(settingsPath, json);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
            RegisterHotKey(new WindowInteropHelper(this).Handle, 1, 0x0004 | 0x0002, 0x43); // 0x0004 = MOD_CONTROL, 0x0002 = MOD_SHIFT, 0x43 = 'C'
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