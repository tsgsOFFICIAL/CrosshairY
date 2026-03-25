using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using CrosshairY.Models;

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
                _settings.ColorR = value.R;
                _settings.ColorG = value.G;
                _settings.ColorB = value.B;
                _settings.Alpha = value.A;

                // Notify / Update UI
                Notify();
                Notify(nameof(ColorR));
                Notify(nameof(ColorG));
                Notify(nameof(ColorB));
                Notify(nameof(Alpha));
            }
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
            get => Color.R;
            set => Color = System.Windows.Media.Color.FromArgb(Color.A, value, Color.G, Color.B);
        }

        public byte ColorG
        {
            get => Color.G;
            set => Color = System.Windows.Media.Color.FromArgb(Color.A, Color.R, value, Color.B);
        }

        public byte ColorB
        {
            get => Color.B;
            set => Color = System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, value);
        }

        public byte Alpha
        {
            get => Color.A;
            set => Color = System.Windows.Media.Color.FromArgb(value, Color.R, Color.G, Color.B);
        }
    }
}