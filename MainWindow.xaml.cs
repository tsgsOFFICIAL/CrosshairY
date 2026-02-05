using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Color = System.Windows.Media.Color;
using System.Windows.Media.Imaging;
using SharpVectors.Renderers.Wpf;
using SharpVectors.Converters;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.Json;
using System.Windows;
using GlobalHotKey;
using System.IO;

namespace CrosshairY
{
    /// <summary>
    /// Represents the main application window for CrosshairY, managing the UI, settings, and crosshair overlay.
    /// Handles user input for crosshair selection, size, opacity, and hotkey toggling.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// The overlay window that displays the crosshair on top of other applications.
        /// </summary>
        private OverlayWindow _overlayWindow;
        /// <summary>
        /// The application settings, including crosshair path, size, and opacity.
        /// </summary>
        private Settings _settings;
        /// <summary>
        /// List of crosshair image names (without .png) available in the assets/crosshairs folder.
        /// </summary>
        private List<string> _crosshairImages;
        /// <summary>
        /// Indicates whether the crosshair overlay is currently enabled (visible).
        /// </summary>
        private bool _isCrosshairEnabled;
        /// <summary>
        /// Path to the assets/crosshairs folder in the build output directory.
        /// </summary>
        private readonly string _crosshairFolder;
        /// <summary>
        /// The color used for the crosshair when selected, defaulting to red.
        /// </summary>
        private Color _selectedColor = Color.FromRgb(255, 0, 0);
        /// <summary>
        /// Gets the brush representing the selected color for the crosshair.
        /// </summary>
        public SolidColorBrush SelectedColorBrush => new SolidColorBrush(_selectedColor);
        /// <summary>
        /// Manages the registration and handling of global hotkeys.
        /// </summary>
        /// <remarks>This field holds an instance of <see cref="HotKeyManager"/>, which is responsible for
        /// managing global hotkey registrations and triggering associated actions when those hotkeys are
        /// pressed.</remarks>
        private HotKeyManager _hotKeyManager;
        private KeyGesture _toggleHotKeyGesture = new KeyGesture(Key.F1, ModifierKeys.Control);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            _overlayWindow = new OverlayWindow();
            _crosshairFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "crosshairs");
            _crosshairImages = LoadCrosshairLibrary();
            _settings = LoadSettings(); // only loads data

            InitializeComponent(); // initializes the UI, sliders now exist
            DataContext = this;

            ApplySettingsToUI(); // manually assign loaded settings to controls

            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.KeyPressed += HotKeyManager_KeyPressed;

            _hotKeyManager.Register(_toggleHotKeyGesture.Key, _toggleHotKeyGesture.Modifiers);

            Closed += (s, e) =>
            {
                _overlayWindow?.Close();
                _hotKeyManager.Dispose();
            };
        }

        /// <summary>
        /// Handles the <see cref="HotKeyManager.KeyPressed"/> event and toggles the application's state based on the
        /// specified hotkey gesture.
        /// </summary>
        /// <remarks>This method checks if the pressed hotkey matches the "ToggleOnOff" gesture defined in
        /// the hotkey map. If a match is found, it toggles the application's state between active and inactive,
        /// displaying a corresponding notification overlay. The method ensures that the notification is only displayed
        /// if no other notification is currently open.</remarks>
        /// <param name="sender">The source of the event. This parameter is typically not used.</param>
        /// <param name="e">The <see cref="KeyPressedEventArgs"/> containing the details of the pressed hotkey.</param>
        private void HotKeyManager_KeyPressed(object? sender, KeyPressedEventArgs e)
        {
            // Check if the pressed hotkey matches the "ToggleOnOff" gesture
            if (e.HotKey.Key == _toggleHotKeyGesture.Key && e.HotKey.Modifiers == _toggleHotKeyGesture.Modifiers)
            {
                // Toggle the application's state
                ToggleCrosshair();
            }
        }

        /// <summary>
        /// Applies the loaded settings to the UI elements, such as sliders and color preview.
        /// </summary>
        private void ApplySettingsToUI()
        {
            // Size and opacity
            SizeSlider.Value = _settings.Size;
            OpacitySlider.Value = _settings.Opacity;

            // RGB sliders
            RedSlider.Value = _settings.Red;
            GreenSlider.Value = _settings.Green;
            BlueSlider.Value = _settings.Blue;

            // Color preview and internal color
            _selectedColor = Color.FromRgb(_settings.Red, _settings.Green, _settings.Blue);
            OnPropertyChanged(nameof(SelectedColorBrush));

            // ComboBox selection (optional if needed)
            if (!string.IsNullOrEmpty(_settings.CrosshairPath))
                CrosshairComboBox.SelectedItem = _settings.CrosshairPath;

            // Update overlay and preview
            UpdateOverlay();
        }

        /// <summary>
        /// Handles the Loaded event of the window, initializing UI elements with settings.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Populate the ComboBox with crosshair image names
            CrosshairComboBox.ItemsSource = _crosshairImages;
            // Select the saved crosshair or the first one if none is saved
            CrosshairComboBox.SelectedItem = _settings.CrosshairPath;

            if (CrosshairComboBox.SelectedItem == null && _crosshairImages.Count > 0)
            {
                // Default to the first crosshair if no valid selection exists
                CrosshairComboBox.SelectedItem = _crosshairImages[0];
                _settings.CrosshairPath = _crosshairImages[0];
            }

            // Set slider values, ensuring they are within valid ranges
            SizeSlider.Value = Math.Clamp(_settings.Size, 5, 200);
            OpacitySlider.Value = Math.Clamp(_settings.Opacity, 0.1, 1.0);

            // Update the overlay with the current settings
            UpdateOverlay();
        }

        /// <summary>
        /// Handles the Closing event of the window, saving settings and cleaning up resources.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Save current settings to disk
            SaveSettings();
            // Unregister the global hotkey
            _hotKeyManager.Unregister(_toggleHotKeyGesture.Key, _toggleHotKeyGesture.Modifiers);
        }

        /// <summary>
        /// Handles the Click event of the Upload Image button, allowing users to upload a custom crosshair image.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUploadImageClick(object sender, RoutedEventArgs e)
        {
            // Configure the file dialog to accept only PNG images
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.svg)|*.png;*.svg|PNG Image (*.png)|*.png|SVG Image (*.svg)|*.svg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileNameWithExt = Path.GetFileName(openFileDialog.FileName);
                string destinationPath = Path.Combine(_crosshairFolder, fileNameWithExt);

                try
                {
                    Directory.CreateDirectory(_crosshairFolder);
                    File.Copy(openFileDialog.FileName, destinationPath, true);

                    _settings.CrosshairPath = fileNameWithExt;
                    CrosshairComboBox.SelectedItem = fileNameWithExt;

                    if (!_crosshairImages.Contains(fileNameWithExt))
                        _crosshairImages.Add(fileNameWithExt);

                    CrosshairComboBox.ItemsSource = null;
                    CrosshairComboBox.ItemsSource = _crosshairImages;
                    CrosshairComboBox.SelectedItem = fileNameWithExt;

                    UpdateOverlay();
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Failed to upload image: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Crosshair ComboBox, updating the selected crosshair.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCrosshairComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CrosshairComboBox.SelectedItem != null)
            {
                // Update settings with the selected crosshair name
                _settings.CrosshairPath = CrosshairComboBox.SelectedItem.ToString() ?? "";
                // Refresh the overlay and preview
                UpdateOverlay();
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the Size Slider, updating the crosshair size.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Avoid accessing UI elements before initialization
            if (SizeValueText == null)
                return;

            // Correct the value to nearest integer that's divisible by 2 to prevent going off center
            int correctedValue = (int)(SizeSlider.Value / 2) * 2;

            // Update settings with the new size
            _settings.Size = correctedValue;
            // Display the size in pixels
            SizeValueText.Text = $"{correctedValue}px";
            // Refresh the overlay with the new size
            UpdateOverlay();
        }

        /// <summary>
        /// Handles the ValueChanged event of the Opacity Slider, updating the crosshair opacity.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnOpacitySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Avoid accessing UI elements before initialization
            if (OpacityValueText == null)
                return;

            // Update settings with the new opacity
            _settings.Opacity = OpacitySlider.Value;
            // Display the opacity as a percentage
            OpacityValueText.Text = $"{(int)(OpacitySlider.Value * 100)}%";
            // Refresh the overlay with the new opacity
            UpdateOverlay();
        }

        /// <summary>
        /// Handles the ValueChanged event of the color sliders, updating the selected color and overlay.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnColorSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RedSlider == null || GreenSlider == null || BlueSlider == null || RedValueText == null || GreenValueText == null || BlueValueText == null)
                return; // Ensure UI elements are initialized

            byte r = (byte)RedSlider.Value;
            byte g = (byte)GreenSlider.Value;
            byte b = (byte)BlueSlider.Value;

            _selectedColor = Color.FromRgb(r, g, b);

            RedValueText.Text = r.ToString();
            GreenValueText.Text = g.ToString();
            BlueValueText.Text = b.ToString();

            OnPropertyChanged(nameof(SelectedColorBrush));

            UpdateOverlay(); // Update overlay immediately with new color
        }

        /// <summary>
        /// Handles the Click event of the Toggle Button, enabling or disabling the crosshair overlay.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnToggleButtonClick(object sender, RoutedEventArgs e)
        {
            // Toggle the crosshair visibility
            ToggleCrosshair();
        }

        /// <summary>
        /// Updates the crosshair overlay and preview image based on current settings.
        /// </summary>
        private void UpdateOverlay()
        {
            try
            {
                string crosshairPath = Path.Combine(_crosshairFolder, _settings.CrosshairPath);

                if (!string.IsNullOrEmpty(_settings.CrosshairPath) && File.Exists(crosshairPath))
                {
                    ImageSource previewSource;
                    string extension = Path.GetExtension(crosshairPath).ToLowerInvariant();

                    if (extension == ".svg")
                    {
                        WpfDrawingSettings settings = new WpfDrawingSettings
                        {
                            IncludeRuntime = true,
                            TextAsGeometry = false
                        };
                        using (FileSvgReader reader = new FileSvgReader(settings))
                        {
                            DrawingGroup drawing = reader.Read(crosshairPath);
                            if (drawing != null)
                            {
                                previewSource = new DrawingImage(drawing);
                                drawing.Freeze();
                            }
                            else
                            {
                                throw new Exception("Failed to load SVG for preview");
                            }
                        }
                    }
                    else // PNG
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(crosshairPath);
                        bitmap.EndInit();
                        bitmap.Freeze();
                        previewSource = bitmap;
                    }

                    _overlayWindow.UpdateCrosshair(crosshairPath, _settings.Size, _settings.Opacity, extension == ".svg" ? _selectedColor : null);
                    PreviewImage.Source = previewSource;

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

        /// <summary>
        /// Toggles the visibility of the crosshair overlay and updates UI elements accordingly.
        /// </summary>
        private void ToggleCrosshair()
        {
            // Toggle the enabled state
            _isCrosshairEnabled = !_isCrosshairEnabled;
            if (_isCrosshairEnabled)
            {
                // Update the overlay with the latest settings before showing to prevent flicker
                UpdateOverlay();

                // Show the overlay and update UI
                _overlayWindow.Show();
                ToggleButtonText.Text = "Disable Crosshair";
                StatusText.Text = "Crosshair Enabled";
            }
            else
            {
                // Hide the overlay and update UI
                _overlayWindow.Hide();
                ToggleButtonText.Text = "Enable Crosshair";
                StatusText.Text = "Crosshair Disabled";
            }
        }

        /// <summary>
        /// Loads the list of available crosshair image names from the assets/crosshairs folder.
        /// </summary>
        /// <returns>A list of crosshair image names (without .png extension).</returns>
        private List<string> LoadCrosshairLibrary()
        {
            List<string> images = new List<string>();

            if (Directory.Exists(_crosshairFolder))
            {
                // Get all PNG and SVG files and extract their names without extensions
                images.AddRange(Directory.GetFiles(_crosshairFolder, "*.png").Select(Path.GetFileName)!);
                images.AddRange(Directory.GetFiles(_crosshairFolder, "*.svg").Select(Path.GetFileName)!);
            }

            // Return distinct names as a collection expression
            return [.. images.Distinct()];
        }

        /// <summary>
        /// Finds a crosshair file by name in the assets/crosshairs folder, checking both PNG and SVG formats.
        /// </summary>
        /// <param name="fileName">The name of the crosshair file without extension.</param>
        /// <returns>The file name with extension if found, otherwise null.</returns>
        private string? FindCrosshairFile(string fileName)
        {
            foreach (string ext in new[] { ".png", ".svg" })
            {
                string path = Path.Combine(_crosshairFolder, fileName + ext);
                if (File.Exists(path))
                    return Path.GetFileName(path);
            }
            return null;
        }

        /// <summary>
        /// Loads user settings from the settings.json file in the AppData folder.
        /// </summary>
        /// <returns>The loaded or default <see cref="Settings"/> object.</returns>
        private Settings LoadSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairY", "settings.json");

            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                Settings settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();

                settings.Size = Math.Clamp(settings.Size, 5, 200);
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

        /// <summary>
        /// Saves the current settings to the settings.json file in the AppData folder.
        /// </summary>
        private void SaveSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairY", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);

            // Save current RGB color
            _settings.Red = (byte)RedSlider.Value;
            _settings.Green = (byte)GreenSlider.Value;
            _settings.Blue = (byte)BlueSlider.Value;

            string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(settingsPath, json);
        }
    }

    /// <summary>
    /// Represents the user settings for the CrosshairY application.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the name of the selected crosshair image (without .png extension).
        /// </summary>
        public string CrosshairPath { get; set; } = "";

        /// <summary>
        /// Gets or sets the size of the crosshair in pixels (10 to 200).
        /// </summary>
        public double Size { get; set; } = 50;

        /// <summary>
        /// Gets or sets the opacity of the crosshair (0.1 to 1.0).
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the red component of the crosshair color (0 to 255).
        /// </summary>
        public byte Red { get; set; } = 255;

        /// <summary>
        /// Gets or sets the green component of the crosshair color (0 to 255).
        /// </summary>
        public byte Green { get; set; } = 0;

        /// <summary>
        /// Gets or sets the blue component of the crosshair color (0 to 255).
        /// </summary>
        public byte Blue { get; set; } = 0;
    }
}