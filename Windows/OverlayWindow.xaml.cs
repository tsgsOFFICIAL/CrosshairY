using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using CrosshairY.Models;
using System.Windows;
using System.Windows.Threading;

namespace CrosshairY.Windows
{
    public partial class OverlayWindow : Window
    {
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Win32 constants for extended window styles
        private const int GWL_EXSTYLE = -20; // Index for extended window styles
        private const int WS_EX_LAYERED = 0x80000; // Enables layered window for opacity
        private const int WS_EX_TRANSPARENT = 0x20; // Makes window transparent to mouse input
        private const int WS_EX_TOPMOST = 0x8; // Keeps window always on top
        // Win32 constants for SetWindowPos flags
        private const int SWP_NOMOVE = 0x2; // Do not change window position
        private const int SWP_NOSIZE = 0x1; // Do not change window size
        private const int SWP_NOACTIVATE = 0x10; // Do not activate the window
        private const int WS_EX_TOOLWINDOW = 0x80; // hides from Alt+Tab

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Handle for topmost z-order

        // Other topmost windows (e.g. borderless games) can push the overlay behind them, so re-assert periodically
        private readonly DispatcherTimer _topmostTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        public OverlayWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => MakeClickThrough();

            _topmostTimer.Tick += (_, _) => AssertTopmost();
            IsVisibleChanged += (_, e) =>
            {
                if ((bool)e.NewValue)
                {
                    AssertTopmost();
                    _topmostTimer.Start();
                }
                else
                {
                    _topmostTimer.Stop();
                }
            };
        }

        public void UpdateCrosshair(CrosshairSettings crosshair)
        {
            if (!IsLoaded)
            {
                Loaded += (_, _) => UpdateCrosshair(crosshair);
                return;
            }
         
            double correctedWidth, correctedHeight;
            
            if (crosshair.Outline)
                correctedWidth = correctedHeight = (crosshair.Length + crosshair.Gap + crosshair.Thickness + crosshair.OutlineThickness) * 2 + 50;
            else
                correctedWidth = correctedHeight = (crosshair.Length + crosshair.Gap + crosshair.Thickness) * 2 + 50;

            Width = (int)(correctedWidth / 2) * 2;
            Height = (int)(correctedHeight / 2) * 2;

            CrosshairRenderer.Render(OverlayCanvas, crosshair);

            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        }

        public void ToggleVisibility() => Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

        private void AssertTopmost()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero)
                return;

            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void MakeClickThrough()
        {
            try
            {
                // Get the window handle for Win32 API operations
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero)
                    return;

                // Combine extended styles: layered for opacity, transparent for click-through, topmost for z-order
                int extendedStyle = WS_EX_LAYERED
                          | WS_EX_TRANSPARENT
                          | WS_EX_TOPMOST
                          | WS_EX_TOOLWINDOW;

                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                int result = SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | extendedStyle);

                // Verify the applied styles for debugging
                int currentStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                // Set the window to always be on top without activating or resizing it
                AssertTopmost();
            }
            catch (Exception ex)
            {
                // Log any unexpected errors during style application
                Debug.WriteLine($"SetWindowStyle error: {ex.Message}");
            }
        }
    }
}