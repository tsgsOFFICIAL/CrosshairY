using CrosshairY.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrosshairY.Pages
{
    public partial class DesignerPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly CrosshairSettings _settings = new();

        public DesignerPage()
        {
            InitializeComponent();
            DataContext = this;
        }
        // ---- Shape ----
        public float Gap
        {
            get => _settings.Gap;
            set { _settings.Gap = value; Notify(); }
        }

        public float Length
        {
            get => _settings.Length;
            set { _settings.Length = value; Notify(); }
        }

        public float Thickness
        {
            get => _settings.Thickness;
            set { _settings.Thickness = value; Notify(); }
        }

        public float OutlineThickness
        {
            get => _settings.OutlineThickness;
            set { _settings.OutlineThickness = value; Notify(); }
        }

        public bool Dot
        {
            get => _settings.Dot;
            set { _settings.Dot = value; Notify(); }
        }

        public bool TStyle
        {
            get => _settings.TStyle;
            set { _settings.TStyle = value; Notify(); }
        }

        public bool Outline
        {
            get => _settings.Outline;
            set { _settings.Outline = value; Notify(); }
        }

        // ---- Color ----
        public byte ColorR
        {
            get => _settings.ColorR;
            set { _settings.ColorR = value; Notify(); Notify(nameof(ColorBrush)); }
        }

        public byte ColorG
        {
            get => _settings.ColorG;
            set { _settings.ColorG = value; Notify(); Notify(nameof(ColorBrush)); }
        }

        public byte ColorB
        {
            get => _settings.ColorB;
            set { _settings.ColorB = value; Notify(); Notify(nameof(ColorBrush)); }
        }

        public byte Alpha
        {
            get => _settings.Alpha;
            set { _settings.Alpha = value; Notify(); Notify(nameof(ColorBrush)); }
        }

        public System.Windows.Media.Brush ColorBrush => new SolidColorBrush(System.Windows.Media.Color.FromArgb(Alpha, ColorR, ColorG, ColorB));
    }
}