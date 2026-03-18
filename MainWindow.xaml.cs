using GlobalHotKey;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CrosshairY
{
    public partial class MainWindow : Window
    {
        private readonly SettingsManager _settingsManager = new();
        private CrosshairSettings _currentSettings = new();
        private readonly OverlayWindow _overlay;

        private bool _isLoading = true;

        public MainWindow()
        {
            InitializeComponent();
            _overlay = new OverlayWindow();

            Loaded += OnMainWindowLoaded;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Load settings first
            _currentSettings = _settingsManager.Load() ?? new CrosshairSettings();

            ApplySettingsToUI(); // ← this may trigger events, but we ignore them

            _isLoading = false;

            // Now do the first real render with loaded values
            UpdateAll();

            // Hotkey (Ctrl+F1)
            HotKeyManager hotkey = new HotKeyManager();
            hotkey.KeyPressed += (_, _) => ToggleOverlay();
            hotkey.Register(Key.F1, ModifierKeys.Control);

        }

        private void ApplySettingsToUI(CrosshairSettings? settings = null)
        {
            settings ??= _currentSettings;

            GapSlider.Value = settings.Gap;
            LengthSlider.Value = settings.Length;
            ThicknessSlider.Value = settings.Thickness;
            OutlineSlider.Value = settings.OutlineThickness;
            DotCheck.IsChecked = settings.Dot;
            TStyleCheck.IsChecked = settings.TStyle;
            OutlineCheck.IsChecked = settings.Outline;
            RedSlider.Value = settings.ColorR;
            GreenSlider.Value = settings.ColorG;
            BlueSlider.Value = settings.ColorB;
            AlphaSlider.Value = settings.Alpha;
        }

        private void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoading)
                return;

            UpdateAll();
        }

        private void OnCheckboxChanged(object sender, RoutedEventArgs e)
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
            string shareCode = ShareCode.Encode(_currentSettings);
            System.Windows.Clipboard.SetText(shareCode);
            System.Windows.MessageBox.Show($"Share code copied!\n{shareCode}", "Crosshair Y");
        }

        private async void ImportShareCode_Click(object sender, RoutedEventArgs e)
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox("Paste share code:", "Import");
            if (!string.IsNullOrEmpty(code))
            {
                CrosshairSettings? imported = ShareCode.Decode(code);
                if (imported != null)
                {
                    ApplySettingsToUI(imported);
                    UpdateAll();
                }
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e) => ToggleOverlay();
        private void ToggleOverlay() => _overlay.ToggleVisibility();

        protected override void OnClosing(CancelEventArgs e) => Environment.Exit(0);
    }
}