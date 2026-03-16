using System.Windows.Input;
using System.Windows;
using GlobalHotKey;

namespace CrosshairY
{
    public partial class MainWindow : Window
    {
        private readonly SettingsManager _settingsManager = new();
        private CrosshairSettings _currentSettings = new();
        private OverlayWindow _overlay;

        private bool _isLoading = false;

        public MainWindow()
        {
            // Temporarily prevent event spam
            _isLoading = true;

            InitializeComponent();

            _overlay = new OverlayWindow();

            // Load settings first
            _currentSettings = _settingsManager.Load() ?? new CrosshairSettings();

            ApplySettingsToUI(); // ← this may trigger events, but we ignore them

            _isLoading = false;

            // Now do the first real render with loaded values
            RenderPreview();
            _overlay.UpdateCrosshair(_currentSettings);

            // Hotkey (Ctrl+F1)
            HotKeyManager hotkey = new HotKeyManager();
            hotkey.KeyPressed += (_, _) => ToggleOverlay();
            hotkey.Register(Key.F1, ModifierKeys.Control);
        }

        private void ApplySettingsToUI()
        {
            GapSlider.Value = _currentSettings.Gap;
            LengthSlider.Value = _currentSettings.Length;
            ThicknessSlider.Value = _currentSettings.Thickness;
            OutlineSlider.Value = _currentSettings.OutlineThickness;
            DotCheck.IsChecked = _currentSettings.Dot;
            TStyleCheck.IsChecked = _currentSettings.TStyle;
            OutlineCheck.IsChecked = _currentSettings.Outline;
            RedSlider.Value = _currentSettings.ColorR;
            GreenSlider.Value = _currentSettings.ColorG;
            BlueSlider.Value = _currentSettings.ColorB;
            AlphaSlider.Value = _currentSettings.Alpha;
        }

        private void Slider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoading)
                return;

            UpdateAll();
        }

        private void Checkbox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoading)
                return;

            UpdateAll();
        }

        private void UpdateAll()
        {
            if (_isLoading)
                return;

            _currentSettings = new CrosshairSettings
            {
                Gap = (float)GapSlider.Value,
                Length = (float)LengthSlider.Value,
                Thickness = (float)ThicknessSlider.Value,
                OutlineThickness = (float)OutlineSlider.Value,
                Dot = DotCheck.IsChecked ?? false,
                TStyle = TStyleCheck.IsChecked ?? false,
                Outline = OutlineCheck.IsChecked ?? true,
                ColorR = (byte)RedSlider.Value,
                ColorG = (byte)GreenSlider.Value,
                ColorB = (byte)BlueSlider.Value,
                Alpha = (byte)AlphaSlider.Value
            };

            RenderPreview();
            _overlay.UpdateCrosshair(_currentSettings);
            _settingsManager.Save(_currentSettings);
        }

        private void RenderPreview() => CrosshairRenderer.Render(PreviewCanvas, _currentSettings);

        private void CopyShareCode_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(CS2ShareCode.Encode(_currentSettings));
            System.Windows.MessageBox.Show("Share code copied!", "CrosshairY");
        }

        private void ImportShareCode_Click(object sender, RoutedEventArgs e)
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox("Paste CS2 share code:", "Import");
            if (!string.IsNullOrEmpty(code))
            {
                CrosshairSettings? imported = CS2ShareCode.Decode(code);
                if (imported != null)
                {
                    _currentSettings = imported;
                    ApplySettingsToUI();
                    UpdateAll();
                }
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e) => ToggleOverlay();
        private void ToggleOverlay() => _overlay.ToggleVisibility();
    }
}