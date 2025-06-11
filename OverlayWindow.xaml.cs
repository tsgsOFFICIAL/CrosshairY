using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using SharpVectors.Renderers.Wpf;
using SharpVectors.Converters;
using System.Windows.Interop;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace CrosshairY
{
    /// <summary>
    /// Represents the overlay window that displays the crosshair image on top of all other windows.
    /// This window is transparent to mouse input, always on top, and supports opacity and size adjustments.
    /// </summary>
    public partial class OverlayWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayWindow"/> class.
        /// </summary>
        public OverlayWindow()
        {
            InitializeComponent();
            // Apply window styles after the window is fully loaded to ensure the handle is available
            Loaded += (s, e) => SetWindowStyle();
        }

        /// <summary>
        /// Updates the crosshair image, size, and position, and centers the window on the screen.
        /// </summary>
        /// <param name="imagePath">The image path to the crosshair overlay (PNG).</param>
        /// <param name="size">The width and height of the crosshair in pixels.</param>
        /// <param name="opacity">The opacity of the crosshair (0.1 to 1.0).</param>
        public void UpdateCrosshair(string imagePath, double size, double opacity, Color? fillColor = null)
        {
            try
            {
                // Determine file type
                string extension = Path.GetExtension(imagePath).ToLowerInvariant();
                bool isSvg = extension == ".svg";

                // Clear current image
                CrosshairImage.Source = null;

                if (isSvg)
                {
                    WpfDrawingSettings settings = new WpfDrawingSettings { IncludeRuntime = true, TextAsGeometry = false };
                    using (FileSvgReader reader = new FileSvgReader(settings))
                    {
                        DrawingGroup drawing = reader.Read(imagePath);
                        if (drawing != null)
                        {
                            if (fillColor.HasValue)
                            {
                                // Apply fill color to all geometries in the drawing
                                ApplyFillColor(drawing, fillColor.Value);
                            }

                            CrosshairImage.Source = new DrawingImage(drawing);
                            drawing.Freeze();
                        }
                        else
                        {
                            Debug.WriteLine($"Failed to load SVG: {imagePath}");
                        }
                    }
                }
                else // PNG
                {
                    if (!CrosshairImage.Source?.ToString().Replace("/", "\\").Contains(imagePath, StringComparison.OrdinalIgnoreCase) ?? true)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath);
                        bitmap.EndInit();
                        bitmap.Freeze();
                        CrosshairImage.Source = bitmap;
                    }
                }

                // Update window properties
                Width = size;
                Height = size;
                Opacity = opacity;

                // Center the window
                Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

                // Force visual update
                CrosshairImage.InvalidateVisual();
                UpdateLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateCrosshair error: {ex.Message}");
                CrosshairImage.Source = null;
            }
        }

        // Helper to apply fill color recursively
        private void ApplyFillColor(Drawing drawing, Color color)
        {
            Debug.WriteLine($"Processing drawing type: {drawing.GetType().Name}");
            if (drawing is GeometryDrawing geometryDrawing)
            { // Only set fill if the original SVG has a fill (preserve fill="none")
                if (geometryDrawing.Brush != null)
                {
                    geometryDrawing.Brush = new SolidColorBrush(color);
                } // Set stroke color
                if (geometryDrawing.Pen != null)
                {
                    geometryDrawing.Pen = new Pen(new SolidColorBrush(color), geometryDrawing.Pen.Thickness);
                }
                else
                {
                    geometryDrawing.Pen = new Pen(new SolidColorBrush(color), 1.0);
                }
                Debug.WriteLine($"Applied color {color} to GeometryDrawing (Fill: {geometryDrawing.Brush?.ToString() ?? "null "}, Stroke: {geometryDrawing.Pen?.Brush?.ToString() ?? "null "})");
            }
            else if (drawing is DrawingGroup drawingGroup)
            {
                foreach (Drawing child in drawingGroup.Children)
                {
                    ApplyFillColor(child, color);
                }
            }
            else
            {
                Debug.WriteLine($"Skipped unsupported drawing type: {drawing.GetType().Name}");
            }
        }

        /// <summary>
        /// Configures the window to be always on top, transparent to mouse input, and layered for opacity support.
        /// Uses Win32 API calls to set extended window styles and position.
        /// </summary>
        private void SetWindowStyle()
        {
            try
            {
                // Get the window handle for Win32 API operations
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero)
                    return;


                // Combine extended styles: layered for opacity, transparent for click-through, topmost for z-order
                int extendedStyle = WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST;
                int result = SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);

                // Verify the applied styles for debugging
                int currentStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                // Set the window to always be on top without activating or resizing it
                bool posResult = SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
            catch (Exception ex)
            {
                // Log any unexpected errors during style application
                Debug.WriteLine($"SetWindowStyle error: {ex.Message}");
            }
        }

        // Win32 constants for extended window styles
        private const int GWL_EXSTYLE = -20; // Index for extended window styles
        private const int WS_EX_LAYERED = 0x80000; // Enables layered window for opacity
        private const int WS_EX_TRANSPARENT = 0x20; // Makes window transparent to mouse input
        private const int WS_EX_TOPMOST = 0x8; // Keeps window always on top
        // Win32 constants for SetWindowPos flags
        private const int SWP_NOMOVE = 0x2; // Do not change window position
        private const int SWP_NOSIZE = 0x1; // Do not change window size
        private const int SWP_NOACTIVATE = 0x10; // Do not activate the window
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Handle for topmost z-order

        // Win32 API imports
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex); // Retrieves window style information

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); // Sets window style information

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); // Sets window position and z-order
    }
}