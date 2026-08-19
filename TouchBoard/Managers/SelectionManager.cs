using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace TouchBoard.Managers
{
    public class SelectionManager
    {
        private readonly MainWindow _window;
        private readonly ToolManager _toolManager;
        private readonly HistoryManager _historyManager;
        private readonly ColorManager _colorManager;
        private readonly StrokeWidthManager _strokeWidthManager;

        public SelectionManager(MainWindow window, ToolManager toolManager, HistoryManager historyManager, ColorManager colorManager, StrokeWidthManager strokeWidthManager)
        {
            _window = window;
            _toolManager = toolManager;
            _historyManager = historyManager;
            _colorManager = colorManager;
            _strokeWidthManager = strokeWidthManager;

            _window.DrawingCanvas.SelectionChanged += DrawingCanvas_SelectionChanged;
            _window.DrawingCanvas.SelectionMoved += (s, e) => UpdateContextMenuPosition();
            _window.DrawingCanvas.SelectionResized += (s, e) => UpdateContextMenuPosition();
        }

        private void DrawingCanvas_SelectionChanged(object? sender, EventArgs e)
        {
            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            bool hasSelection = selectedStrokes.Count > 0 ||
                                _window.DrawingCanvas.GetSelectedElements().Count > 0;

            _window.BtnDeleteSelected.IsEnabled = hasSelection;
            _window.BtnDeleteSelected.Opacity = hasSelection ? 1.0 : 0.4;



            if (selectedStrokes.Count > 0)
            {
                var stroke = selectedStrokes[0];
                _colorManager.SyncToolbarWithSelectedStroke(stroke.DrawingAttributes.Color);
                _strokeWidthManager.SyncToolbarWithSelectedStroke(stroke.DrawingAttributes.Width, () => 
                {
                    _colorManager.ApplyDrawingAttributes();
                    _strokeWidthManager.UpdateEraserShape();
                });
            }

            // Show/hide the ⋯ context menu button
            if (hasSelection)
            {
                UpdateContextMenuPosition();
                _window.SelectionMenuButton.Visibility = Visibility.Visible;
                _window.SelectionRotateThumb.Visibility = Visibility.Visible;
            }
            else
            {
                _window.SelectionMenuButton.Visibility = Visibility.Collapsed;
                _window.SelectionRotateThumb.Visibility = Visibility.Collapsed;
                _window.SelectionPopup.IsOpen = false;
            }
        }

        /// <summary>
        /// Repositions the ⋯ button at the top-right corner of the selection bounding box.
        /// </summary>
        private void UpdateContextMenuPosition()
        {
            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count == 0)
                return;

            var strokeBounds = selectedStrokes.GetBounds();

            // Chuyển đổi tọa độ bounds từ hệ quy chiếu của DrawingCanvas sang SelectionOverlay
            var bounds = _window.DrawingCanvas.TransformToVisual(_window.SelectionOverlay).TransformBounds(strokeBounds);

            // Position at top-right of selection bounds
            double left = bounds.Right + 8;
            double top = bounds.Top - 8;

            // Clamp to window bounds so the button stays visible
            double windowWidth = _window.ActualWidth;
            if (windowWidth > 0 && left + 48 > windowWidth)
                left = bounds.Left - 48;
            if (top < 0)
                top = bounds.Top;

            Canvas.SetLeft(_window.SelectionMenuButton, left);
            Canvas.SetTop(_window.SelectionMenuButton, top);

            // Đặt nút Rotate ngay dưới nút ContextMenu
            Canvas.SetLeft(_window.SelectionRotateThumb, left + 4);
            Canvas.SetTop(_window.SelectionRotateThumb, top + _window.SelectionMenuButton.Height + 8);
        }

        /// <summary>
        /// Toggles the context popup open/closed.
        /// </summary>
        public void ToggleContextMenu()
        {
            _window.SelectionPopup.IsOpen = !_window.SelectionPopup.IsOpen;
        }

        /// <summary>
        /// Changes the color of all selected strokes and syncs the main toolbar.
        /// </summary>
        public void ChangeSelectionColor(string colorHex)
        {
            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count == 0) return;

            var newColor = (Color)ColorConverter.ConvertFromString(colorHex);
            foreach (var stroke in selectedStrokes)
            {
                var da = stroke.DrawingAttributes.Clone();
                da.Color = newColor;
                stroke.DrawingAttributes = da;
            }
            _historyManager.SaveState();
            _colorManager.SyncToolbarWithSelectedStroke(newColor);
        }

        /// <summary>
        /// Changes the stroke width of all selected strokes.
        /// </summary>
        public void ChangeSelectionStrokeWidth(double width)
        {
            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count == 0) return;

            foreach (var stroke in selectedStrokes)
            {
                var da = stroke.DrawingAttributes.Clone();
                da.Width = width;
                da.Height = width;
                stroke.DrawingAttributes = da;
            }
            _historyManager.SaveState();
        }

        /// <summary>
        /// Copies the current selection to the clipboard using InkCanvas built-in support.
        /// </summary>
        public void CopySelection()
        {
            _window.DrawingCanvas.CopySelection();
        }

        public void DeleteSelectedStrokes()
        {
            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count > 0)
            {
                _window.DrawingCanvas.Strokes.Remove(selectedStrokes);
            }

            var selectedElements = _window.DrawingCanvas.GetSelectedElements().Cast<UIElement>().ToList();
            foreach (var element in selectedElements)
            {
                _window.DrawingCanvas.Children.Remove(element);
            }

            _historyManager.SaveState();
            _window.SelectionMenuButton.Visibility = Visibility.Collapsed;
            _window.SelectionPopup.IsOpen = false;
        }

        public void RotateSelectedStrokes(double angleInDegrees)
        {
            var selectedStrokes = _window.DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count == 0) return;

            var bounds = selectedStrokes.GetBounds();
            double centerX = bounds.X + bounds.Width / 2;
            double centerY = bounds.Y + bounds.Height / 2;

            var matrix = new System.Windows.Media.Matrix();
            matrix.RotateAt(angleInDegrees, centerX, centerY);
            
            var strokesToSelect = new System.Windows.Ink.StrokeCollection();
            
            foreach (var stroke in selectedStrokes)
            {
                stroke.Transform(matrix, false);
                strokesToSelect.Add(stroke);
            }
            
            _window.DrawingCanvas.Select(strokesToSelect);
        }
    }
}
