using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace TouchBoard.Managers
{
    public class StrokeWidthManager
    {
        private readonly MainWindow _window;
        private readonly ToolManager _toolManager;
        private readonly HistoryManager _historyManager;

        public double CurrentStrokeWidth { get; private set; } = 6;
        public Button? ActiveStrokeWidthButton { get; private set; }

        public StrokeWidthManager(MainWindow window, ToolManager toolManager, HistoryManager historyManager)
        {
            _window = window;
            _toolManager = toolManager;
            _historyManager = historyManager;

            ActiveStrokeWidthButton = _window.BtnStrokeMedium;
        }

        public void HandleStrokeWidthClick(object sender, Action applyDrawingAttributesCallback)
        {
            if (sender is not Button btn || btn.Tag is not string widthStr)
                return;

            if (!double.TryParse(widthStr, out double width))
                return;

            CurrentStrokeWidth = width;

            if (ActiveStrokeWidthButton != null)
                ActiveStrokeWidthButton.Style = (Style)_window.FindResource("ToolButtonStyle");

            btn.Style = (Style)_window.FindResource("ActiveToolButtonStyle");
            ActiveStrokeWidthButton = btn;

            applyDrawingAttributesCallback();

            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count > 0)
            {
                foreach (var stroke in selectedStrokes)
                {
                    var da = stroke.DrawingAttributes.Clone();
                    da.Width = width;
                    da.Height = width;
                    stroke.DrawingAttributes = da;
                }
                _historyManager.SaveState();
            }
            else
            {
                if (_toolManager.CurrentMode != ToolMode.Pen && _toolManager.CurrentMode != ToolMode.EraserPoint)
                    _toolManager.SwitchToMode(ToolMode.Pen);
            }
        }

        public void SyncToolbarWithSelectedStroke(double width, Action applyDrawingAttributesCallback)
        {
            foreach (UIElement child in _window.PanelStrokeWidth.Children)
            {
                if (child is Button btn && btn.Tag is string widthStr && double.TryParse(widthStr, out double w))
                {
                    if (Math.Abs(w - width) < 0.5)
                    {
                        if (ActiveStrokeWidthButton != null)
                            ActiveStrokeWidthButton.Style = (Style)_window.FindResource("ToolButtonStyle");

                        btn.Style = (Style)_window.FindResource("ActiveToolButtonStyle");
                        ActiveStrokeWidthButton = btn;
                        CurrentStrokeWidth = w;
                        break;
                    }
                }
            }
            applyDrawingAttributesCallback();
        }

        public void UpdateEraserShape()
        {
            double eraserSize = CurrentStrokeWidth * 3;
            _window.DrawingCanvas.EraserShape = new EllipseStylusShape(eraserSize, eraserSize);
        }
    }
}
