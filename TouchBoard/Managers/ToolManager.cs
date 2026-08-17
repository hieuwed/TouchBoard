using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace TouchBoard.Managers
{
    public enum ToolMode { Pen, Select, EraserStroke, EraserPoint }

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
            _window.BtnEraserStrokeMode.Style = (Style)_window.FindResource("ToolButtonStyle");
            _window.BtnEraserPointMode.Style = (Style)_window.FindResource("ToolButtonStyle");

            var activeStyle = (Style)_window.FindResource("ActiveToolButtonStyle");

            // Hide selection context menu when switching modes
            _window.SelectionMenuButton.Visibility = Visibility.Collapsed;
            _window.SelectionPopup.IsOpen = false;

            switch (mode)
            {
                case ToolMode.Pen:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.None; // Tắt vẽ mặc định, nhường cho MultiTouchManager
                    _window.BtnPenMode.Style = activeStyle;
                    _window.TxtModeIcon.Text = "\uE76D";
                    _window.TxtModeIndicator.Text = "CHẾ ĐỘ VIẾT";

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
                    _window.TxtModeIcon.Text = "\uE825";
                    _window.TxtModeIndicator.Text = selCount > 0 ? $"ĐÃ CHỌN {selCount} NÉT VẼ" : "CHẾ ĐỘ CHỌN & THAO TÁC";

                    // Pen options remain visible for context and selected object editing
                    _window.PanelColors.Visibility = Visibility.Visible;
                    _window.PanelStrokeWidth.Visibility = Visibility.Visible;
                    break;

                case ToolMode.EraserStroke:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    _window.BtnEraserStrokeMode.Style = activeStyle;
                    _window.TxtModeIcon.Text = "\uE75C";
                    _window.TxtModeIndicator.Text = "TẨY NÉT";

                    // Hide pen options, hide delete
                    _window.PanelColors.Visibility = Visibility.Hidden;
                    _window.PanelStrokeWidth.Visibility = Visibility.Hidden;
                    _window.BtnDeleteSelected.IsEnabled = false;
                    _window.BtnDeleteSelected.Opacity = 0.4;
                    break;

                case ToolMode.EraserPoint:
                    _window.DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    _window.BtnEraserPointMode.Style = activeStyle;
                    _window.TxtModeIcon.Text = "\uE89A";
                    _window.TxtModeIndicator.Text = "TẨY ĐIỂM";

                    // Show stroke width for eraser size, hide colors
                    _window.PanelColors.Visibility = Visibility.Hidden;
                    _window.PanelStrokeWidth.Visibility = Visibility.Visible;
                    _window.BtnDeleteSelected.IsEnabled = false;
                    _window.BtnDeleteSelected.Opacity = 0.4;
                    break;
            }
        }
    }
}
