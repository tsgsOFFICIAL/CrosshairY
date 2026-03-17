using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;

namespace CrosshairY
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

        public void UpdateCrosshair(CrosshairSettings s)
        {
            if (!IsLoaded)
            {
                Loaded += (_, _) => UpdateCrosshair(s);
                return;
            }
            double correctedWidth, correctedHeight;
            if (s.Outline)
                correctedWidth = correctedHeight = (s.Length + s.Gap + s.Thickness + s.OutlineThickness) * 2 + 50;
            else
                correctedWidth = correctedHeight = (s.Length + s.Gap + s.Thickness) * 2 + 50;

            Width = (int)(correctedWidth / 2) * 2;
            Height = (int)(correctedHeight / 2) * 2;

            CrosshairRenderer.Render(OverlayCanvas, s);

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