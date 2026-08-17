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

        public StrokeWidthManager(MainWindow window, ToolManager toolManager, HistoryManager historyManager)
        {
            _window = window;
            _toolManager = toolManager;
            _historyManager = historyManager;
        }

        public void HandleStrokeWidthChanged(double width, Action applyDrawingAttributesCallback)
        {
            CurrentStrokeWidth = width;
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
            CurrentStrokeWidth = width;
            _window.SliderStrokeWidth.Value = width;
            if (_window.SliderSelectionStrokeWidth != null)
                _window.SliderSelectionStrokeWidth.Value = width;

            applyDrawingAttributesCallback();
        }

        public void UpdateEraserShape()
        {
            double eraserSize = CurrentStrokeWidth * 3;
            _window.DrawingCanvas.EraserShape = new EllipseStylusShape(eraserSize, eraserSize);
        }
    }
}
