using System.Windows.Input;
using System.Windows;

namespace CrosshairY.Windows
{
    /// <summary>
    /// Interaction logic for HotkeyDialog.xaml
    /// </summary>
    public partial class HotkeyDialog : Window
    {
        public KeyGesture? SelectedHotkey { get; private set; }

        public HotkeyDialog()
        {
            InitializeComponent();
            HotkeyText.Text = "Press a key combination";
            KeyDown += HotkeyDialog_KeyDown;
        }

        private void HotkeyDialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Prevent the event from bubbling up further if we handle it
            e.Handled = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Key pressed: {e.Key}, Modifiers: {Keyboard.Modifiers}");

                // Ignore modifier keys by themselves (Ctrl, Alt, Shift, etc.)
                if (IsModifierKey(e.Key))
                {
                    return;
                }

                // Also ignore Escape (used for cancel)
                if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                    Close();
                    return;
                }

                ModifierKeys modifiers = Keyboard.Modifiers;

                // Create the hotkey
                SelectedHotkey = new KeyGesture(e.Key, modifiers);

                // Update display text
                HotkeyText.Text = FormatHotkeyDisplay(modifiers, e.Key);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hotkey capture error: {ex.Message}");
                DialogResult = false;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private static bool IsModifierKey(Key key)
        {
            return key is Key.LeftCtrl or Key.RightCtrl or
                   Key.LeftAlt or Key.RightAlt or
                   Key.LeftShift or Key.RightShift or
                   Key.LWin or Key.RWin or
                   Key.System; // AltGr often appears as System
        }

        private static string FormatHotkeyDisplay(ModifierKeys modifiers, Key key)
        {
            if (modifiers == ModifierKeys.None)
                return key.ToString();

            return $"{modifiers} + {key}";
        }
    }
}
