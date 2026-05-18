using CrosshairY.Managers;
using CrosshairY.Models;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private System.Windows.Media.Color _outlineColor;

        public System.Windows.Media.Color OutlineColor
        {
            get => _outlineColor;
            set
            {
                if (_outlineColor == value)
                    return;

                _outlineColor = value;

                // Sync into settings
                _crosshair.OutlineColorR = value.R;
                _crosshair.OutlineColorG = value.G;
                _crosshair.OutlineColorB = value.B;
                _crosshair.OutlineAlpha = value.A;

                // Notify / Update UI
                Notify();
                Notify(nameof(OutlineColorR));
                Notify(nameof(OutlineColorG));
                Notify(nameof(OutlineColorB));
                Notify(nameof(OutlineAlpha));
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

        public bool SquareStyle
        {
            get => _crosshair.SquareStyle;
            set
            {
                if (_crosshair.SquareStyle == value)
                    return;

                _crosshair.SquareStyle = value;
                SetAndRender(nameof(SquareStyle));
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

        public byte OutlineColorR
        {
            get => OutlineColor.R;
            set
            {
                if (OutlineColor.R == value)
                    return;

                OutlineColor = System.Windows.Media.Color.FromArgb(OutlineColor.A, value, OutlineColor.G, OutlineColor.B);
                SetAndRender(nameof(OutlineColorR));
            }
        }

        public byte OutlineColorG
        {
            get => OutlineColor.G;
            set
            {
                if (OutlineColor.G == value)
                    return;

                OutlineColor = System.Windows.Media.Color.FromArgb(OutlineColor.A, OutlineColor.R, value, OutlineColor.B);
                SetAndRender(nameof(OutlineColorG));
            }
        }

        public byte OutlineColorB
        {
            get => OutlineColor.B;
            set
            {
                if (OutlineColor.B == value)
                    return;

                OutlineColor = System.Windows.Media.Color.FromArgb(OutlineColor.A, OutlineColor.R, OutlineColor.G, value);
                SetAndRender(nameof(OutlineColorB));
            }
        }

        public byte OutlineAlpha
        {
            get => OutlineColor.A;
            set
            {
                if (OutlineColor.A == value)
                    return;

                OutlineColor = System.Windows.Media.Color.FromArgb(value, OutlineColor.R, OutlineColor.G, OutlineColor.B);
                SetAndRender(nameof(OutlineAlpha));
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

        private static string GetProposedText(System.Windows.Controls.TextBox textBox, string input)
        {
            string text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            return text.Insert(textBox.SelectionStart, input);
        }

        #region Event Handlers
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _crosshair = App.Settings.Crosshair == null ? new() : JsonSerializer.Deserialize<CrosshairSettings>(
                JsonSerializer.Serialize(App.Settings.Crosshair)!
            )!;

            CrosshairRenderer.Render(CrosshairCanvas, _crosshair);

            Color = System.Windows.Media.Color.FromArgb(_crosshair.Alpha, _crosshair.ColorR, _crosshair.ColorG, _crosshair.ColorB);
            OutlineColor = System.Windows.Media.Color.FromArgb(_crosshair.OutlineAlpha, _crosshair.OutlineColorR, _crosshair.OutlineColorG, _crosshair.OutlineColorB);

            Notify(nameof(Gap));
            Notify(nameof(Length));
            Notify(nameof(Thickness));
            Notify(nameof(OutlineThickness));
            Notify(nameof(Dot));
            Notify(nameof(TStyle));
            Notify(nameof(Outline));
            Notify(nameof(SquareStyle));
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
                    Notify(nameof(SquareStyle));
                    Notify(nameof(CrosshairName));
                    Notify(nameof(Description));
                }
            }
        }

        private async void OnSaveCrosshairButtonClicked(object sender, RoutedEventArgs e)
        {
            await CrosshairManager.Instance.UpdateCrosshair(_crosshair);
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
                OutlineColor = System.Windows.Media.Color.FromArgb(_crosshair.OutlineAlpha, _crosshair.OutlineColorR, _crosshair.OutlineColorG, _crosshair.OutlineColorB);

                Notify(nameof(Gap));
                Notify(nameof(Length));
                Notify(nameof(Thickness));
                Notify(nameof(OutlineThickness));
                Notify(nameof(Dot));
                Notify(nameof(TStyle));
                Notify(nameof(Outline));
                Notify(nameof(SquareStyle));
                Notify(nameof(CrosshairName));
                Notify(nameof(Description));
            }
        }

        private void OnGapSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            GapTextBox.SelectAll();
            GapTextBox.Focus();
        }
        private void OnLengthSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            LengthTextBox.SelectAll();
            LengthTextBox.Focus();
        }
        private void OnThicknessSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            ThicknessTextBox.SelectAll();
            ThicknessTextBox.Focus();
        }
        private void OnOutlineThicknessSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            OutlineThicknessTextBox.SelectAll();
            OutlineThicknessTextBox.Focus();
        }
        private void OnColorRSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            ColorRTextBox.SelectAll();
            ColorRTextBox.Focus();
        }
        private void OnColorGSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            ColorGTextBox.SelectAll();
            ColorGTextBox.Focus();
        }
        private void OnColorBSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            ColorBTextBox.SelectAll();
            ColorBTextBox.Focus();
        }
        private void OnColorASliderRightClick(object sender, MouseButtonEventArgs e)
        {
            ColorATextBox.SelectAll();
            ColorATextBox.Focus();
        }
        private void OnOutlineColorRSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            OutlineColorRTextBox.SelectAll();
            OutlineColorRTextBox.Focus();
        }
        private void OnOutlineColorGSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            OutlineColorGTextBox.SelectAll();
            OutlineColorGTextBox.Focus();
        }
        private void OnOutlineColorBSliderRightClick(object sender, MouseButtonEventArgs e)
        {
            OutlineColorBTextBox.SelectAll();
            OutlineColorBTextBox.Focus();
        }
        private void OnOutlineColorASliderRightClick(object sender, MouseButtonEventArgs e)
        {
            OutlineColorATextBox.SelectAll();
            OutlineColorATextBox.Focus();
        }

        private void OnRgbPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;

            // Block whitespace immediately
            if (string.IsNullOrWhiteSpace(e.Text))
            {
                e.Handled = true;
                return;
            }

            string newText = GetProposedText(textBox, e.Text);

            // Only allow digits
            if (!int.TryParse(newText, out int value))
            {
                e.Handled = true;
                return;
            }

            // Clamp instead of blocking
            if (value > 255)
            {
                textBox.Text = "255";
                textBox.CaretIndex = textBox.Text.Length;
                e.Handled = true;
            }
        }
        private void OnRgbPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Block anything that's not a digit or control key
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9) &&
                !(e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) &&
                e.Key != Key.Back &&
                e.Key != Key.Delete &&
                e.Key != Key.Left &&
                e.Key != Key.Right &&
                e.Key != Key.Tab &&
                e.Key != Key.End &&
                e.Key != Key.Home &&
                e.Key != Key.PageUp &&
                e.Key != Key.PageDown)
            {
                e.Handled = true;
            }
        }

        private void OnDecimalPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;

            string proposed = GetProposedText(textBox, e.Text).Replace(',', '.');

            // Allow empty or incomplete numbers
            if (string.IsNullOrEmpty(proposed) || Regex.IsMatch(proposed, @"^-?(\d+)?(\.\d*)?$"))
            {
                // If fully numeric, clamp immediately
                if (double.TryParse(proposed, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && textBox.Tag is Slider slider)
                {
                    if (value > slider.Maximum)
                    {
                        textBox.Text = slider.Maximum.ToString("F1", CultureInfo.InvariantCulture);
                        textBox.CaretIndex = textBox.Text.Length;
                        e.Handled = true;
                    }
                    else if (value < slider.Minimum)
                    {
                        textBox.Text = slider.Minimum.ToString("F1", CultureInfo.InvariantCulture);
                        textBox.CaretIndex = textBox.Text.Length;
                        e.Handled = true;
                    }
                }

                // Allow typing
                return;
            }

            // Block anything else (letters, symbols, etc.)
            e.Handled = true;
        }
        private void OnDecimalTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(System.Windows.DataFormats.Text))
            {
                System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
                string pasteText = ((string)e.DataObject.GetData(System.Windows.DataFormats.Text)).Replace(',', '.');

                // Get proposed text after paste
                string proposed = GetProposedText(textBox, pasteText);

                // Clamp if fully numeric
                if (double.TryParse(proposed, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) &&
                    textBox.Tag is Slider slider)
                {
                    if (value > slider.Maximum) value = slider.Maximum;
                    if (value < slider.Minimum) value = slider.Minimum;
                    textBox.Text = value.ToString("F1", CultureInfo.InvariantCulture);
                }
                else
                {
                    // For incomplete forms or empty, just allow the paste
                    textBox.Text = proposed;
                }

                textBox.CaretIndex = textBox.Text.Length;
                e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
        #endregion
    }
}