using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using CrosshairY.Managers;
using CrosshairY.Models;
using System.Text.Json;
using System.Windows;

namespace CrosshairY.Pages
{
    public partial class DesignerPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private CrosshairSettings _crosshair = new();

        private System.Windows.Media.Color _color;

        public System.Windows.Media.Color Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;

                _color = value;

                // Sync into settings
                _crosshair.ColorR = value.R;
                _crosshair.ColorG = value.G;
                _crosshair.ColorB = value.B;
                _crosshair.Alpha = value.A;

                // Notify / Update UI
                Notify();
                Notify(nameof(ColorR));
                Notify(nameof(ColorG));
                Notify(nameof(ColorB));
                Notify(nameof(Alpha));

                CrosshairRenderer.Render(CrosshairCanvas, _crosshair);
            }
        }

        // ---- Metadata ----
        public string CrosshairName
        {
            get => _crosshair.CrosshairName;
            set
            {
                if (_crosshair.CrosshairName == value)
                    return;

                _crosshair.CrosshairName = value;
                Notify(nameof(CrosshairName));
            }
        }

        public string Description
        {
            get => _crosshair.Description;
            set
            {
                if (_crosshair.Description == value)
                    return;

                _crosshair.Description = value;
                Notify(nameof(Description));
            }
        }

        // ---- Shape ----
        public float Gap
        {
            get => _crosshair.Gap;
            set
            {
                if (_crosshair.Gap == value)
                    return;

                _crosshair.Gap = value;
                SetAndRender(nameof(Gap));
            }
        }

        public float Length
        {
            get => _crosshair.Length;
            set
            {
                if (_crosshair.Length == value)
                    return;

                _crosshair.Length = value;
                SetAndRender(nameof(Length));
            }
        }

        public float Thickness
        {
            get => _crosshair.Thickness;
            set
            {
                if (_crosshair.Thickness == value)
                    return;

                _crosshair.Thickness = value;
                SetAndRender(nameof(Thickness));
            }
        }

        public float OutlineThickness
        {
            get => _crosshair.OutlineThickness;
            set
            {
                if (_crosshair.OutlineThickness == value)
                    return;

                _crosshair.OutlineThickness = value;
                SetAndRender(nameof(OutlineThickness));
            }
        }

        public bool Dot
        {
            get => _crosshair.Dot;
            set
            {
                if (_crosshair.Dot == value)
                    return;

                _crosshair.Dot = value;
                SetAndRender(nameof(Dot));
            }
        }

        public bool TStyle
        {
            get => _crosshair.TStyle;
            set
            {
                if (_crosshair.TStyle == value)
                    return;

                _crosshair.TStyle = value;
                SetAndRender(nameof(TStyle));
            }
        }

        public bool Outline
        {
            get => _crosshair.Outline;
            set
            {
                if (_crosshair.Outline == value)
                    return;

                _crosshair.Outline = value;
                SetAndRender(nameof(Outline));
            }
        }

        // ---- Color ----
        public byte ColorR
        {
            get => Color.R;
            set
            {
                if (Color.R == value)
                    return;

                Color = System.Windows.Media.Color.FromArgb(Color.A, value, Color.G, Color.B);
                SetAndRender(nameof(ColorR));
            }
        }

        public byte ColorG
        {
            get => Color.G;
            set
            {
                if (Color.G == value)
                    return;

                Color = System.Windows.Media.Color.FromArgb(Color.A, Color.R, value, Color.B);
                SetAndRender(nameof(ColorG));
            }
        }

        public byte ColorB
        {
            get => Color.B;
            set
            {
                if (Color.B == value)
                    return;

                Color = System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, value);
                SetAndRender(nameof(ColorB));
            }
        }

        public byte Alpha
        {
            get => Color.A;
            set
            {
                if (Color.A == value)
                    return;

                Color = System.Windows.Media.Color.FromArgb(value, Color.R, Color.G, Color.B);
                SetAndRender(nameof(Alpha));
            }
        }

        public DesignerPage()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnPageLoaded;
        }

        private void SetAndRender(string propertyName)
        {
            Notify(propertyName);
            CrosshairRenderer.Render(CrosshairCanvas, _crosshair);
        }

        #region Event Handlers
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _crosshair = App.Settings.Crosshair == null ? new() : JsonSerializer.Deserialize<CrosshairSettings>(
                JsonSerializer.Serialize(App.Settings.Crosshair)!
            )!;

            CrosshairRenderer.Render(CrosshairCanvas, _crosshair);

            Color = System.Windows.Media.Color.FromArgb(_crosshair.Alpha, _crosshair.ColorR, _crosshair.ColorG, _crosshair.ColorB);

            Notify(nameof(Gap));
            Notify(nameof(Length));
            Notify(nameof(Thickness));
            Notify(nameof(OutlineThickness));
            Notify(nameof(Dot));
            Notify(nameof(TStyle));
            Notify(nameof(Outline));
            Notify(nameof(CrosshairName));
            Notify(nameof(Description));
        }
        private void OnCopyShareCodeButtonClicked(object sender, RoutedEventArgs e)
        {
            string shareCode = ShareCode.Encode(_crosshair);
            System.Windows.Clipboard.SetText(shareCode);
            System.Windows.MessageBox.Show($"Share code copied!\n{shareCode}", "CrosshairY");
        }
        private void OnImportShareCodeButtonClicked(object sender, RoutedEventArgs e)
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox("Paste share code:", "Import");
            if (!string.IsNullOrEmpty(code))
            {
                CrosshairSettings? imported = ShareCode.Decode(code);
                if (imported != null)
                {
                    _crosshair = imported;

                    CrosshairRenderer.Render(CrosshairCanvas, _crosshair);

                    Color = System.Windows.Media.Color.FromArgb(_crosshair.Alpha, _crosshair.ColorR, _crosshair.ColorG, _crosshair.ColorB);

                    Notify(nameof(Gap));
                    Notify(nameof(Length));
                    Notify(nameof(Thickness));
                    Notify(nameof(OutlineThickness));
                    Notify(nameof(Dot));
                    Notify(nameof(TStyle));
                    Notify(nameof(Outline));
                    Notify(nameof(CrosshairName));
                    Notify(nameof(Description));
                }
            }
        }
        private void OnSaveCrosshairButtonClicked(object sender, RoutedEventArgs e)
        {
            CrosshairManager.Instance.UpdateCrosshair(_crosshair);

            System.Windows.MessageBox.Show("Crosshair saved!", "CrosshairY");
        }
        private void OnResetCrosshairButtonClicked(object sender, RoutedEventArgs e)
        {
            string defaultCrosshairCode = "TSGS-FRpV8-zVGEc-CUKFU-uhf2x-wFYym-ng8Hq-gi"; // ← pre-encoded default crosshair settings

            CrosshairSettings? imported = ShareCode.Decode(defaultCrosshairCode);
            if (imported != null)
            {
                _crosshair = imported;

                CrosshairRenderer.Render(CrosshairCanvas, _crosshair);

                Color = System.Windows.Media.Color.FromArgb(_crosshair.Alpha, _crosshair.ColorR, _crosshair.ColorG, _crosshair.ColorB);

                Notify(nameof(Gap));
                Notify(nameof(Length));
                Notify(nameof(Thickness));
                Notify(nameof(OutlineThickness));
                Notify(nameof(Dot));
                Notify(nameof(TStyle));
                Notify(nameof(Outline));
                Notify(nameof(CrosshairName));
                Notify(nameof(Description));
            }
        }
        #endregion
    }
}