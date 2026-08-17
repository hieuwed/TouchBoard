using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TouchBoard.Managers
{
    /// <summary>
    /// Background Pattern: loại kẻ trên bảng.
    /// </summary>
    public enum BackgroundPattern { Plain, Grid, Ruled }

    /// <summary>
    /// Background Theme: màu nền của bảng.
    /// </summary>
    public enum BackgroundTheme { Dark, Light, Blackboard }

    /// <summary>
    /// Manages canvas background by combining a Theme (color palette) with a Pattern (grid/ruled/plain).
    /// </summary>
    public class BackgroundManager
    {
        private readonly MainWindow _window;
        private readonly ToolManager _toolManager;
        private readonly ColorManager _colorManager;
        public BackgroundPattern CurrentPattern { get; private set; } = BackgroundPattern.Plain;
        public BackgroundTheme CurrentTheme { get; private set; } = BackgroundTheme.Dark;

        // ============================
        // THEME COLOR PALETTES
        // ============================

        // Dark theme (Catppuccin Mocha)
        private static readonly Color DarkBg = (Color)ColorConverter.ConvertFromString("#1E1E2E")!;
        private static readonly Color DarkToolbar = (Color)ColorConverter.ConvertFromString("#181825")!;
        private static readonly Color DarkBorder = (Color)ColorConverter.ConvertFromString("#313244")!;
        private static readonly Color DarkInk = (Color)ColorConverter.ConvertFromString("#CDD6F4")!;
        private static readonly Color DarkGridLine = (Color)ColorConverter.ConvertFromString("#2A2A3E")!;

        // Light theme
        private static readonly Color LightBg = (Color)ColorConverter.ConvertFromString("#EFF1F5")!;
        private static readonly Color LightToolbar = (Color)ColorConverter.ConvertFromString("#DCE0E8")!;
        private static readonly Color LightBorder = (Color)ColorConverter.ConvertFromString("#BCC0CC")!;
        private static readonly Color LightInk = (Color)ColorConverter.ConvertFromString("#4C4F69")!;
        private static readonly Color LightGridLine = (Color)ColorConverter.ConvertFromString("#CCD0DA")!;

        // Blackboard theme
        private static readonly Color BlackboardBg = (Color)ColorConverter.ConvertFromString("#1B3A2D")!;
        private static readonly Color BlackboardToolbar = (Color)ColorConverter.ConvertFromString("#142E23")!;
        private static readonly Color BlackboardBorder = (Color)ColorConverter.ConvertFromString("#2D5A47")!;
        private static readonly Color BlackboardInk = (Color)ColorConverter.ConvertFromString("#E8E4D9")!;
        private static readonly Color BlackboardGridLine = (Color)ColorConverter.ConvertFromString("#24503D")!;

        public BackgroundManager(MainWindow window, ToolManager toolManager, ColorManager colorManager)
        {
            _window = window;
            _toolManager = toolManager;
            _colorManager = colorManager;

            _window.DrawingCanvas.SizeChanged += (s, e) => RedrawBackgroundPattern();
        }

        /// <summary>
        /// Sets the canvas background by combining a theme and a pattern.
        /// </summary>
        public void SetBackground(BackgroundPattern pattern, BackgroundTheme theme)
        {
            CurrentPattern = pattern;
            CurrentTheme = theme;

            // 1. Apply theme colors
            var (bg, toolbar, border, ink, gridLine) = GetThemeColors(theme);
            ApplyTheme(bg, toolbar, border, ink);

            // 2. Draw pattern on top (if any)
            switch (pattern)
            {
                case BackgroundPattern.Grid:
                    DrawGrid(bg, gridLine);
                    break;
                case BackgroundPattern.Ruled:
                    DrawRuledLines(bg, gridLine);
                    break;
                case BackgroundPattern.Plain:
                default:
                    // Solid color only, already applied
                    break;
            }
        }

        /// <summary>
        /// Overload for backward compatibility — sets both from current state.
        /// </summary>
        public void SetBackground(BackgroundTheme theme)
        {
            SetBackground(CurrentPattern, theme);
        }

        public void SetBackground(BackgroundPattern pattern)
        {
            SetBackground(pattern, CurrentTheme);
        }

        private (Color bg, Color toolbar, Color border, Color ink, Color gridLine) GetThemeColors(BackgroundTheme theme)
        {
            switch (theme)
            {
                case BackgroundTheme.Light:
                    return (LightBg, LightToolbar, LightBorder, LightInk, LightGridLine);
                case BackgroundTheme.Blackboard:
                    return (BlackboardBg, BlackboardToolbar, BlackboardBorder, BlackboardInk, BlackboardGridLine);
                case BackgroundTheme.Dark:
                default:
                    return (DarkBg, DarkToolbar, DarkBorder, DarkInk, DarkGridLine);
            }
        }

        /// <summary>
        /// Returns the background color for a given theme (used by thumbnail converter).
        /// </summary>
        public static Color GetThemeBgColor(BackgroundTheme theme)
        {
            switch (theme)
            {
                case BackgroundTheme.Light: return LightBg;
                case BackgroundTheme.Blackboard: return BlackboardBg;
                case BackgroundTheme.Dark:
                default: return DarkBg;
            }
        }

        private void ApplyTheme(Color canvasBg, Color toolbarBg, Color borderColor, Color inkColor)
        {
            _window.Background = new SolidColorBrush(canvasBg);
            _window.DrawingCanvas.Background = new SolidColorBrush(canvasBg);

            if (_window.ToolbarBorder != null)
            {
                _window.ToolbarBorder.Background = new SolidColorBrush(toolbarBg);
                _window.ToolbarBorder.BorderBrush = new SolidColorBrush(borderColor);
            }

            _window.DrawingCanvas.DefaultDrawingAttributes.Color = inkColor;
            _colorManager.SetDefaultInkColor(inkColor);
        }

        private void DrawGrid(Color bgColor, Color lineColor)
        {
            double spacing = 40;

            var pen = new Pen(new SolidColorBrush(lineColor), 0.8);
            pen.Freeze();

            var geometry = new GeometryGroup();
            geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(spacing, 0)));
            geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, spacing)));

            var drawing = new GeometryDrawing(null, pen, geometry);

            // Background fill for the tile
            var bgDrawing = new GeometryDrawing(
                new SolidColorBrush(bgColor),
                null,
                new RectangleGeometry(new Rect(0, 0, spacing, spacing)));

            var group = new DrawingGroup();
            group.Children.Add(bgDrawing);
            group.Children.Add(drawing);

            var brush = new DrawingBrush(group)
            {
                Viewport = new Rect(0, 0, spacing, spacing),
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
            brush.Freeze();

            _window.DrawingCanvas.Background = brush;
        }

        private void DrawRuledLines(Color bgColor, Color lineColor)
        {
            double spacing = 40;

            var pen = new Pen(new SolidColorBrush(lineColor), 0.8);
            pen.Freeze();

            var geometry = new GeometryGroup();
            geometry.Children.Add(new LineGeometry(new Point(0, 0), new Point(spacing, 0)));

            var drawing = new GeometryDrawing(null, pen, geometry);

            var bgDrawing = new GeometryDrawing(
                new SolidColorBrush(bgColor),
                null,
                new RectangleGeometry(new Rect(0, 0, spacing, spacing)));

            var group = new DrawingGroup();
            group.Children.Add(bgDrawing);
            group.Children.Add(drawing);

            var brush = new DrawingBrush(group)
            {
                Viewport = new Rect(0, 0, spacing, spacing),
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
            brush.Freeze();

            _window.DrawingCanvas.Background = brush;
        }

        private void ClearBackgroundPattern() { }

        private void RedrawBackgroundPattern() { }
    }
}
