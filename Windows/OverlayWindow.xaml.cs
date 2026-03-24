using System.Runtime.InteropServices;
using System.Windows.Interop;
using CrosshairY.Models;
using System.Windows;

namespace CrosshairY.Windows
{
    public partial class OverlayWindow : Window
    {
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public OverlayWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => MakeClickThrough();
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

        private void MakeClickThrough()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(hwnd, -20);
            SetWindowLong(hwnd, -20, exStyle | 0x80000 | 0x20 | 0x8);
            SetWindowPos(hwnd, new IntPtr(-1), 0, 0, 0, 0, 0x2 | 0x1 | 0x10);
        }
    }
}