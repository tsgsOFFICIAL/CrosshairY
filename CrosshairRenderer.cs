using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using CrosshairY.Models;

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
            bool centerOnly = s.Length == 0;

            PenLineCap cap = s.SquareStyle ? PenLineCap.Square : PenLineCap.Round;

            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(s.Alpha, s.ColorR, s.ColorG, s.ColorB);
            SolidColorBrush brush = new SolidColorBrush(color);
            SolidColorBrush outlineBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(s.OutlineAlpha, s.OutlineColorR, s.OutlineColorG, s.OutlineColorB));

            // Outline first – now using full outside growth via thicker centered stroke
            if (s.Outline && !centerOnly)
            {
                double outlineTotalThick = s.Thickness + s.OutlineThickness * 2;  // ← key: *2 so full OutlineThickness per side

                // Left arm outline
                AddLine(canvas, centerX - halfLen - gap, centerY, centerX - gap, centerY, outlineTotalThick, outlineBrush, cap);

                // Right arm outline
                AddLine(canvas, centerX + gap, centerY, centerX + halfLen + gap, centerY, outlineTotalThick, outlineBrush, cap);

                if (!s.TStyle)
                {
                    // Top arm outline
                    AddLine(canvas, centerX, centerY - halfLen - gap, centerX, centerY - gap, outlineTotalThick, outlineBrush, cap);
                }

                // Bottom arm outline
                AddLine(canvas, centerX, centerY + gap, centerX, centerY + halfLen + gap, outlineTotalThick, outlineBrush, cap);
            }

            if (!centerOnly)
            {
                // Main lines
                AddLine(canvas, centerX - halfLen - gap, centerY, centerX - gap, centerY, s.Thickness, brush, cap); // left
                AddLine(canvas, centerX + gap, centerY, centerX + halfLen + gap, centerY, s.Thickness, brush, cap); // right

                if (!s.TStyle)
                    AddLine(canvas, centerX, centerY - halfLen - gap, centerX, centerY - gap, s.Thickness, brush, cap); // top

                AddLine(canvas, centerX, centerY + gap, centerX, centerY + halfLen + gap, s.Thickness, brush, cap); // bottom
            }

            // Dot with outline
            if (s.Dot)
            {
                double dotSize = s.Thickness * 2;
                double halfThickness = s.Thickness;

                if (s.SquareStyle)
                {
                    dotSize /= 2;
                    halfThickness /= 2;

                    // Square dot
                    if (s.Outline)
                    {
                        System.Windows.Shapes.Rectangle dotOutlineRect = new System.Windows.Shapes.Rectangle
                        {
                            Width = dotSize + s.OutlineThickness * 2,
                            Height = dotSize + s.OutlineThickness * 2,
                            Stroke = outlineBrush,
                            StrokeThickness = s.OutlineThickness,
                            Fill = null
                        };
                        Canvas.SetLeft(dotOutlineRect, centerX - halfThickness - s.OutlineThickness);
                        Canvas.SetTop(dotOutlineRect, centerY - halfThickness - s.OutlineThickness);
                        canvas.Children.Add(dotOutlineRect);
                    }

                    System.Windows.Shapes.Rectangle dotRect = new System.Windows.Shapes.Rectangle { Width = dotSize, Height = dotSize, Fill = brush };
                    Canvas.SetLeft(dotRect, centerX - halfThickness);
                    Canvas.SetTop(dotRect, centerY - halfThickness);
                    canvas.Children.Add(dotRect);
                }
                else
                {
                    // Round dot
                    if (s.Outline)
                    {
                        Ellipse dotOutline = new Ellipse
                        {
                            Width = dotSize + s.OutlineThickness * 2,
                            Height = dotSize + s.OutlineThickness * 2,
                            Stroke = outlineBrush,
                            StrokeThickness = s.OutlineThickness,
                            Fill = null
                        };
                        Canvas.SetLeft(dotOutline, centerX - halfThickness - s.OutlineThickness);
                        Canvas.SetTop(dotOutline, centerY - halfThickness - s.OutlineThickness);
                        canvas.Children.Add(dotOutline);
                    }

                    Ellipse dot = new Ellipse { Width = dotSize, Height = dotSize, Fill = brush };
                    Canvas.SetLeft(dot, centerX - halfThickness);
                    Canvas.SetTop(dot, centerY - halfThickness);
                    canvas.Children.Add(dot);
                }
            }
        }

        private static void AddLine(Canvas c, double x1, double y1, double x2, double y2, double thick, System.Windows.Media.Brush b, PenLineCap cap = PenLineCap.Round)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = b,
                StrokeThickness = thick,
                StrokeStartLineCap = cap,
                StrokeEndLineCap = cap
            };
            c.Children.Add(line);
        }
    }
}