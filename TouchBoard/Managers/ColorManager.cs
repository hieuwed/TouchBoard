using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TouchBoard.Managers
{
    public class ColorManager
    {
        private readonly MainWindow _window;
        private readonly ToolManager _toolManager;
        private readonly HistoryManager _historyManager;
        private readonly StrokeWidthManager _strokeWidthManager;

        public string CurrentColorHex { get; private set; } = "#CDD6F4";
        public Button? ActiveColorButton { get; private set; }

        public ColorManager(MainWindow window, ToolManager toolManager, HistoryManager historyManager, StrokeWidthManager strokeWidthManager)
        {
            _window = window;
            _toolManager = toolManager;
            _historyManager = historyManager;
            _strokeWidthManager = strokeWidthManager;

            ActiveColorButton = _window.BtnColorBlack;
        }

        public void HandleColorClick(object sender)
        {
            if (sender is not Button btn || btn.Tag is not string colorHex)
                return;

            CurrentColorHex = colorHex;

            if (ActiveColorButton != null)
                ActiveColorButton.Style = (Style)_window.FindResource("ColorSwatchStyle");

            btn.Style = (Style)_window.FindResource("ActiveColorSwatchStyle");
            ActiveColorButton = btn;

            ApplyDrawingAttributes();

            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count > 0)
            {
                var newColor = (Color)ColorConverter.ConvertFromString(colorHex);
                foreach (var stroke in selectedStrokes)
                {
                    var da = stroke.DrawingAttributes.Clone();
                    da.Color = newColor;
                    stroke.DrawingAttributes = da;
                }
                _historyManager.SaveState();
            }
            else
            {
                if (_toolManager.CurrentMode != ToolMode.Pen)
                    _toolManager.SwitchToMode(ToolMode.Pen);
            }
        }

        public void SyncToolbarWithSelectedStroke(Color color)
        {
            foreach (UIElement child in _window.PanelColors.Children)
            {
                if (child is Button btn && btn.Tag is string hexStr)
                {
                    try
                    {
                        var btnColor = (Color)ColorConverter.ConvertFromString(hexStr);
                        if (btnColor == color)
                        {
                            if (ActiveColorButton != null)
                                ActiveColorButton.Style = (Style)_window.FindResource("ColorSwatchStyle");

                            btn.Style = (Style)_window.FindResource("ActiveColorSwatchStyle");
                            ActiveColorButton = btn;
                            CurrentColorHex = hexStr;
                            break;
                        }
                    }
                    catch { }
                }
            }
            ApplyDrawingAttributes();
        }

        public void ApplyDrawingAttributes()
        {
            var color = (Color)ColorConverter.ConvertFromString(CurrentColorHex);

            _window.DrawingCanvas.DefaultDrawingAttributes = new System.Windows.Ink.DrawingAttributes
            {
                Color = color,
                Width = _strokeWidthManager.CurrentStrokeWidth,
                Height = _strokeWidthManager.CurrentStrokeWidth,
                StylusTip = System.Windows.Ink.StylusTip.Ellipse,
                FitToCurve = true,
                IgnorePressure = true
            };
        }

        /// <summary>
        /// Updates the default ink color (first swatch) when background theme changes.
        /// This ensures the default pen color contrasts with the new background.
        /// </summary>
        public void SetDefaultInkColor(Color inkColor)
        {
            string hexColor = inkColor.ToString();
            
            // Update the first color button's visual and tag
            _window.BtnColorBlack.Background = new SolidColorBrush(inkColor);
            _window.BtnColorBlack.Tag = hexColor;

            // If the first color is currently active, update the drawing color too
            if (ActiveColorButton == _window.BtnColorBlack)
            {
                CurrentColorHex = hexColor;
                ApplyDrawingAttributes();
            }
        }
    }
}
