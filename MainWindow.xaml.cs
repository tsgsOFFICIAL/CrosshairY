using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;

namespace CrosshairY
{
    public partial class MainWindow : Window
    {
        private CrosshairWindow? _crosshairWindow;
        private CrosshairSettings _settings;

        public MainWindow()
        {
            // Load settings from file if it exists
            _settings = LoadSettings() ?? new CrosshairSettings
            {
                Type = "Cross",
                Color = Colors.Red,
                Size = 20,
                Thickness = 2,
                Opacity = 1.0
            };
            _crosshairWindow = new CrosshairWindow(_settings);

            InitializeComponent();
            DataContext = _settings; // Set DataContext to enable bindings

            Closed += (s, e) =>
            {
                _crosshairWindow?.Close();
                Close();
            };

            // Update toggle button text based on initial crosshair window state
            if (ToggleButtonText != null)
                ToggleButtonText.Text = _crosshairWindow?.IsVisible == true ? "Hide Crosshair" : "Show Crosshair";
        }

        private CrosshairSettings? LoadSettings()
        {
            try
            {
                if (File.Exists("crosshair_settings.json"))
                {
                    string json = File.ReadAllText("crosshair_settings.json");
                    CrosshairSettings? settings = JsonSerializer.Deserialize<CrosshairSettings>(json);
                    if (settings != null)
                    {
                        // Validate Type to ensure it's one of the allowed values
                        if (settings.Type != "Cross" && settings.Type != "Dot" && settings.Type != "Circle")
                        {
                            settings.Type = "Cross";
                        }
                        // Validate other properties
                        settings.Size = Math.Clamp(settings.Size, 5, 50);
                        settings.Thickness = Math.Clamp(settings.Thickness, 1, 10);
                        settings.Opacity = Math.Clamp(settings.Opacity, 0.1, 1.0);
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                    StatusText.Text = $"Error loading settings: {ex.Message}";
            }
            return null;
        }

        private void OnPickColorClick(object sender, RoutedEventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.Color = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                if (StatusText != null)
                    StatusText.Text = "Color updated";
            }
        }

        private void OnToggleCrosshairClick(object sender, RoutedEventArgs e)
        {
            if (_crosshairWindow != null)
            {
                if (_crosshairWindow.IsVisible)
                {
                    _crosshairWindow.Hide();
                    if (ToggleButtonText != null)
                        ToggleButtonText.Text = "Show Crosshair";
                    if (StatusText != null)
                        StatusText.Text = "Crosshair hidden";
                }
                else
                {
                    _crosshairWindow.Show();
                    if (ToggleButtonText != null)
                        ToggleButtonText.Text = "Hide Crosshair";
                    if (StatusText != null)
                        StatusText.Text = "Crosshair visible";
                }
            }
        }

        private void OnSaveProfileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("crosshair_settings.json", json);
                if (StatusText != null)
                    StatusText.Text = "Profile saved successfully";
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                    StatusText.Text = $"Error saving profile: {ex.Message}";
            }
        }
    }

    public class CrosshairSettings : INotifyPropertyChanged
    {
        private string? _type;
        private Color _color;
        private int _size;
        private int _thickness;
        private double _opacity;
        private ComboBoxItem? _selectedCrosshairTypeItem;

        public string? Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                    // Update SelectedCrosshairTypeItem when Type changes
                    UpdateSelectedCrosshairTypeItem();
                }
            }
        }

        public ComboBoxItem? SelectedCrosshairTypeItem
        {
            get => _selectedCrosshairTypeItem;
            set
            {
                if (_selectedCrosshairTypeItem != value)
                {
                    _selectedCrosshairTypeItem = value;
                    // Update Type when SelectedCrosshairTypeItem changes
                    Type = value?.Content?.ToString() ?? "Cross";
                    OnPropertyChanged(nameof(SelectedCrosshairTypeItem));
                }
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public int Thickness
        {
            get => _thickness;
            set
            {
                if (_thickness != value)
                {
                    _thickness = value;
                    OnPropertyChanged(nameof(Thickness));
                }
            }
        }

        public double Opacity
        {
            get => _opacity;
            set
            {
                if (_opacity != value)
                {
                    _opacity = value;
                    OnPropertyChanged(nameof(Opacity));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateSelectedCrosshairTypeItem()
        {
            // Find the ComboBoxItem in the MainWindow's ComboBox that matches Type
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow && mainWindow.CrosshairTypeComboBox != null)
            {
                ComboBoxItem? item = mainWindow.CrosshairTypeComboBox.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content?.ToString() == _type);
                if (item != null)
                {
                    _selectedCrosshairTypeItem = item;
                    OnPropertyChanged(nameof(SelectedCrosshairTypeItem));
                }
            }
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color color)
                return new SolidColorBrush(color);
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush.Color;
            return Colors.Transparent;
        }
    }

    public class ComboBoxItemToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert string Type to ComboBoxItem for display
            if (value is string type)
            {
                return new ComboBoxItem { Content = type };
            }
            return new ComboBoxItem { Content = "Cross" };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert ComboBoxItem back to string Type
            if (value is ComboBoxItem item && item.Content is string content)
            {
                return content;
            }
            return "Cross";
        }
    }
}