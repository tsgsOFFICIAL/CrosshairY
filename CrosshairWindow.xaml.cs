using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CrosshairY
{
    public partial class CrosshairWindow : Window
    {
        private CrosshairSettings _settings;
        private Line _horizontalLine = null!;
        private Line _verticalLine = null!;
        private Ellipse _dot = null!;
        private Ellipse _circle = null!;

        public CrosshairWindow(CrosshairSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            InitializeCrosshair();
            _settings.PropertyChanged += Settings_PropertyChanged;
            UpdateCrosshair();
        }

        private void InitializeCrosshair()
        {
            _horizontalLine = new Line();
            _verticalLine = new Line();
            _dot = new Ellipse();
            _circle = new Ellipse { Fill = System.Windows.Media.Brushes.Transparent };
            CrosshairCanvas.Children.Add(_horizontalLine);
            CrosshairCanvas.Children.Add(_verticalLine);
            CrosshairCanvas.Children.Add(_dot);
            CrosshairCanvas.Children.Add(_circle);
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateCrosshair();
        }

        public void UpdateCrosshair()
        {
            double centerX = Width / 2;
            double centerY = Height / 2;

            // Ensure Type is valid
            string type = _settings.Type ?? "Cross";

            // Clear previous settings
            _horizontalLine.Visibility = Visibility.Hidden;
            _verticalLine.Visibility = Visibility.Hidden;
            _dot.Visibility = Visibility.Hidden;
            _circle.Visibility = Visibility.Hidden;

            switch (type)
            {
                case "Cross":
                    _horizontalLine.Visibility = Visibility.Visible;
                    _verticalLine.Visibility = Visibility.Visible;

                    _horizontalLine.X1 = centerX - _settings.Size / 2;
                    _horizontalLine.X2 = centerX + _settings.Size / 2;
                    _horizontalLine.Y1 = centerY;
                    _horizontalLine.Y2 = centerY;
                    _horizontalLine.Stroke = new SolidColorBrush(_settings.Color);
                    _horizontalLine.StrokeThickness = _settings.Thickness;
                    _horizontalLine.Opacity = _settings.Opacity;

                    _verticalLine.X1 = centerX;
                    _verticalLine.X2 = centerX;
                    _verticalLine.Y1 = centerY - _settings.Size / 2;
                    _verticalLine.Y2 = centerY + _settings.Size / 2;
                    _verticalLine.Stroke = new SolidColorBrush(_settings.Color);
                    _verticalLine.StrokeThickness = _settings.Thickness;
                    _verticalLine.Opacity = _settings.Opacity;
                    break;

                case "Dot":
                    _dot.Visibility = Visibility.Visible;

                    _dot.Width = _settings.Thickness;
                    _dot.Height = _settings.Thickness;
                    Canvas.SetLeft(_dot, centerX - _settings.Thickness / 2);
                    Canvas.SetTop(_dot, centerY - _settings.Thickness / 2);
                    _dot.Fill = new SolidColorBrush(_settings.Color);
                    _dot.Opacity = _settings.Opacity;
                    break;

                case "Circle":
                    _circle.Visibility = Visibility.Visible;

                    _circle.Width = _settings.Size;
                    _circle.Height = _settings.Size;
                    Canvas.SetLeft(_circle, centerX - _settings.Size / 2);
                    Canvas.SetTop(_circle, centerY - _settings.Size / 2);
                    _circle.Stroke = new SolidColorBrush(_settings.Color);
                    _circle.StrokeThickness = _settings.Thickness;
                    _circle.Opacity = _settings.Opacity;
                    break;
            }
        }
    }
}