using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace TouchBoard.Managers
{
    public enum ToolMode { Pen, Select, EraserStroke, EraserPoint, Shape }

    public class ToolManager
    {
        private readonly MainWindow _window;
        public ToolMode CurrentMode { get; private set; } = ToolMode.Pen;

        public ToolManager(MainWindow window)
        {
            _window = window;
        }

        public void SwitchToMode(ToolMode mode)
        {
            CurrentMode = mode;

            // Reset all tool button styles
            _window.BtnPenMode.Style = (Style)_window.FindResource("ToolButtonStyle");
            _window.BtnSelectMode.Style = (Style)_window.FindResource("ToolButtonStyle");
            _window.BtnEraserMode.Style = (Style)_window.FindResource("ToolButtonStyle");

            var activeStyle = (Style)_window.FindResource("ActiveToolButtonStyle");

            // Hide selection context menu when switching modes
            _window.SelectionMenuButton.Visibility = Visibility.Collapsed;
            _window.SelectionPopup.IsOpen = false;

            switch (mode)
            {
                case ToolMode.Pen:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.None; // Tắt vẽ mặc định, nhường cho MultiTouchManager
                    _window.BtnPenMode.Style = activeStyle;

                    // Show pen options, hide delete
                    _window.PanelColors.Visibility = Visibility.Visible;
                    _window.PanelStrokeWidth.Visibility = Visibility.Visible;
                    _window.BtnDeleteSelected.IsEnabled = false;
                    _window.BtnDeleteSelected.Opacity = 0.4;
                    break;

                case ToolMode.Select:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
                    _window.BtnSelectMode.Style = activeStyle;
                    var selCount = _window.DrawingCanvas.GetSelectedStrokes().Count;

                    // Pen options remain visible for context and selected object editing
                    _window.PanelColors.Visibility = Visibility.Visible;
                    _window.PanelStrokeWidth.Visibility = Visibility.Visible;
                    break;

                case ToolMode.EraserStroke:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    _window.BtnEraserMode.Style = activeStyle;

                    // Hide pen options, hide delete
                    _window.PanelColors.Visibility = Visibility.Hidden;
                    _window.PanelStrokeWidth.Visibility = Visibility.Hidden;
                    _window.BtnDeleteSelected.IsEnabled = false;
                    _window.BtnDeleteSelected.Opacity = 0.4;
                    break;

                case ToolMode.EraserPoint:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    _window.BtnEraserMode.Style = activeStyle;

                    // Show stroke width for eraser size, hide colors
                    _window.PanelColors.Visibility = Visibility.Hidden;
                    _window.PanelStrokeWidth.Visibility = Visibility.Visible;
                    _window.BtnDeleteSelected.IsEnabled = false;
                    _window.BtnDeleteSelected.Opacity = 0.4;
                    break;

                case ToolMode.Shape:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
                    // Giữ nguyên hiển thị màu sắc và nét để user thấy màu sẽ được áp dụng
                    _window.PanelColors.Visibility = Visibility.Visible;
                    _window.PanelStrokeWidth.Visibility = Visibility.Visible;
                    _window.BtnDeleteSelected.IsEnabled = false;
                    _window.BtnDeleteSelected.Opacity = 0.4;
                    _window.Cursor = Cursors.Cross; // Đổi con trỏ chuột thành dấu thập
                    break;
            }
        }
    }
}
