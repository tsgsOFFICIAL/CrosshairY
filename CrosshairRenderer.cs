using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace CrosshairY
{
    public static class CrosshairRenderer
    {
        public static void Render(Canvas canvas, CrosshairSettings s)
        {
            canvas.Children.Clear();
            double centerX = canvas.ActualWidth / 2;
            double centerY = canvas.ActualHeight / 2;
            double halfLen = s.Length / 2;
            double gap = s.Gap;

            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(s.Alpha, s.ColorR, s.ColorG, s.ColorB);
            SolidColorBrush brush = new SolidColorBrush(color);
            SolidColorBrush outlineBrush = System.Windows.Media.Brushes.Black;

            // Outline first (thicker)
            if (s.Outline)
            {
                double ot = s.OutlineThickness + s.Thickness;
                AddLine(canvas, centerX - halfLen - gap, centerY, centerX - gap, centerY, ot, outlineBrush); // left
                AddLine(canvas, centerX + gap, centerY, centerX + halfLen + gap, centerY, ot, outlineBrush); // right
                if (!s.TStyle) AddLine(canvas, centerX, centerY - halfLen - gap, centerX, centerY - gap, ot, outlineBrush); // top
                AddLine(canvas, centerX, centerY + gap, centerX, centerY + halfLen + gap, ot, outlineBrush); // bottom
            }

            // Main lines
            AddLine(canvas, centerX - halfLen - gap, centerY, centerX - gap, centerY, s.Thickness, brush); // left
            AddLine(canvas, centerX + gap, centerY, centerX + halfLen + gap, centerY, s.Thickness, brush); // right
            if (!s.TStyle) AddLine(canvas, centerX, centerY - halfLen - gap, centerX, centerY - gap, s.Thickness, brush); // top
            AddLine(canvas, centerX, centerY + gap, centerX, centerY + halfLen + gap, s.Thickness, brush); // bottom

            // Dot
            if (s.Dot)
            {
                Ellipse dot = new Ellipse { Width = s.Thickness * 2, Height = s.Thickness * 2, Fill = brush };
                Canvas.SetLeft(dot, centerX - s.Thickness);
                Canvas.SetTop(dot, centerY - s.Thickness);
                canvas.Children.Add(dot);
            }
        }

        private static void AddLine(Canvas c, double x1, double y1, double x2, double y2, double thick, System.Windows.Media.Brush b)
        {
            Line line = new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = b, StrokeThickness = thick, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
            c.Children.Add(line);
        }
    }
}