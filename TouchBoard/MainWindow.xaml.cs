using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace TouchBoard
{
    /// <summary>
    /// Touch Board - Interactive Whiteboard Application
    /// Two modes: Pen (Ink) and Select (Move / Resize / Delete)
    /// Optimized for touchscreen interactive displays.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Current state ──────────────────────────────────
        private enum ToolMode { Pen, Select, EraserStroke, EraserPoint }
        private ToolMode _currentMode = ToolMode.Pen;
        private string _currentColorHex = "#CDD6F4";
        private double _currentStrokeWidth = 6;
        private bool _isFullscreen = false;

        private HistoryManager _historyManager;

        // ── References to buttons for active-state styling ─
        private Button? _activeColorButton;
        private Button? _activeStrokeWidthButton;

        // ── Resource Styles ────────────────────────────────
        private Style ToolButtonStyle => (Style)FindResource("ToolButtonStyle");
        private Style ActiveToolButtonStyle => (Style)FindResource("ActiveToolButtonStyle");
        private Style ColorSwatchStyle => (Style)FindResource("ColorSwatchStyle");
        private Style ActiveColorSwatchStyle => (Style)FindResource("ActiveColorSwatchStyle");

        public MainWindow()
        {
            InitializeComponent();

            // Set initial active states
            _activeColorButton = BtnColorBlack;
            _activeStrokeWidthButton = BtnStrokeMedium;

            // Apply initial drawing attributes
            ApplyDrawingAttributes();

            // Initialize HistoryManager for Undo/Redo
            _historyManager = new HistoryManager(DrawingCanvas);
            _historyManager.StateChanged += UpdateUndoRedoButtons;
            UpdateUndoRedoButtons();
        }

        private void UpdateUndoRedoButtons()
        {
            BtnUndo.IsEnabled = _historyManager.CanUndo;
            BtnUndo.Opacity = _historyManager.CanUndo ? 1.0 : 0.4;

            BtnRedo.IsEnabled = _historyManager.CanRedo;
            BtnRedo.Opacity = _historyManager.CanRedo ? 1.0 : 0.4;
        }

        // ═══════════════════════════════════════════════════
        // MODE SWITCHING
        // ═══════════════════════════════════════════════════

        private void BtnPenMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchToMode(ToolMode.Pen);
        }

        private void BtnSelectMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchToMode(ToolMode.Select);
        }

        private void BtnEraserStrokeMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchToMode(ToolMode.EraserStroke);
        }

        private void BtnEraserPointMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchToMode(ToolMode.EraserPoint);
        }

        private void SwitchToMode(ToolMode mode)
        {
            _currentMode = mode;

            // Reset all tool button styles
            BtnPenMode.Style = ToolButtonStyle;
            BtnSelectMode.Style = ToolButtonStyle;
            BtnEraserStrokeMode.Style = ToolButtonStyle;
            BtnEraserPointMode.Style = ToolButtonStyle;

            switch (mode)
            {
                case ToolMode.Pen:
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    BtnPenMode.Style = ActiveToolButtonStyle;
                    TxtModeIndicator.Text = "✏️ CHẾ ĐỘ VIẾT";

                    // Show pen options, hide delete
                    PanelColors.Visibility = Visibility.Visible;
                    PanelStrokeWidth.Visibility = Visibility.Visible;
                    BtnDeleteSelected.IsEnabled = false;
                    BtnDeleteSelected.Opacity = 0.4;
                    break;

                case ToolMode.Select:
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
                    BtnSelectMode.Style = ActiveToolButtonStyle;
                    TxtModeIndicator.Text = "👆 CHẾ ĐỘ CHỌN & THAO TÁC";

                    // Pen options remain visible but contextually less important
                    PanelColors.Visibility = Visibility.Visible;
                    PanelStrokeWidth.Visibility = Visibility.Visible;
                    break;

                case ToolMode.EraserStroke:
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    BtnEraserStrokeMode.Style = ActiveToolButtonStyle;
                    TxtModeIndicator.Text = "🧽 TẨY NÉT";

                    // Hide pen options, hide delete
                    PanelColors.Visibility = Visibility.Hidden;
                    PanelStrokeWidth.Visibility = Visibility.Hidden;
                    BtnDeleteSelected.IsEnabled = false;
                    BtnDeleteSelected.Opacity = 0.4;
                    break;

                case ToolMode.EraserPoint:
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    BtnEraserPointMode.Style = ActiveToolButtonStyle;
                    TxtModeIndicator.Text = "🧼 TẨY ĐIỂM";

                    // Show stroke width for eraser size, hide colors
                    PanelColors.Visibility = Visibility.Hidden;
                    PanelStrokeWidth.Visibility = Visibility.Visible;
                    BtnDeleteSelected.IsEnabled = false;
                    BtnDeleteSelected.Opacity = 0.4;
                    break;
            }
        }

        // ═══════════════════════════════════════════════════
        // COLOR SELECTION
        // ═══════════════════════════════════════════════════

        private void BtnColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string colorHex)
                return;

            _currentColorHex = colorHex;

            // Update active color button visual
            if (_activeColorButton != null)
                _activeColorButton.Style = ColorSwatchStyle;

            btn.Style = ActiveColorSwatchStyle;
            _activeColorButton = btn;

            ApplyDrawingAttributes();

            // Auto-switch to Pen mode when color is selected
            if (_currentMode != ToolMode.Pen)
                SwitchToMode(ToolMode.Pen);
        }

        // ═══════════════════════════════════════════════════
        // STROKE WIDTH SELECTION
        // ═══════════════════════════════════════════════════

        private void BtnStrokeWidth_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string widthStr)
                return;

            if (!double.TryParse(widthStr, out double width))
                return;

            _currentStrokeWidth = width;

            // Update active stroke width button visual
            if (_activeStrokeWidthButton != null)
                _activeStrokeWidthButton.Style = ToolButtonStyle;

            btn.Style = ActiveToolButtonStyle;
            _activeStrokeWidthButton = btn;

            ApplyDrawingAttributes();

            // Auto-switch to Pen mode when stroke width is selected, UNLESS we are in EraserPoint mode
            if (_currentMode != ToolMode.Pen && _currentMode != ToolMode.EraserPoint)
                SwitchToMode(ToolMode.Pen);
        }

        // ═══════════════════════════════════════════════════
        // APPLY DRAWING ATTRIBUTES
        // ═══════════════════════════════════════════════════

        private void ApplyDrawingAttributes()
        {
            var color = (Color)ColorConverter.ConvertFromString(_currentColorHex);

            DrawingCanvas.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = color,
                Width = _currentStrokeWidth,
                Height = _currentStrokeWidth,
                StylusTip = StylusTip.Ellipse,
                FitToCurve = true,
                IgnorePressure = true
            };

            // Update EraserShape for EraserPoint mode (make it a bit larger than pen stroke)
            double eraserSize = _currentStrokeWidth * 3;
            DrawingCanvas.EraserShape = new EllipseStylusShape(eraserSize, eraserSize);
        }

        // ═══════════════════════════════════════════════════
        // SELECTION EVENTS & DELETE
        // ═══════════════════════════════════════════════════

        private void DrawingCanvas_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = DrawingCanvas.GetSelectedStrokes().Count > 0 ||
                                DrawingCanvas.GetSelectedElements().Count > 0;

            BtnDeleteSelected.IsEnabled = hasSelection;
            BtnDeleteSelected.Opacity = hasSelection ? 1.0 : 0.4;
        }

        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedStrokes();
        }

        private void DeleteSelectedStrokes()
        {
            var selectedStrokes = DrawingCanvas.GetSelectedStrokes();
            if (selectedStrokes.Count > 0)
            {
                DrawingCanvas.Strokes.Remove(selectedStrokes);
            }

            var selectedElements = DrawingCanvas.GetSelectedElements().Cast<UIElement>().ToList();
            foreach (var element in selectedElements)
            {
                DrawingCanvas.Children.Remove(element);
            }

            _historyManager.SaveState();
        }

        // ═══════════════════════════════════════════════════
        // CLEAR ALL
        // ═══════════════════════════════════════════════════

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.Strokes.Count == 0 && DrawingCanvas.Children.Count == 0)
                return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa toàn bộ bảng?",
                "Xác nhận Xóa Sạch",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DrawingCanvas.Strokes.Clear();
                DrawingCanvas.Children.Clear();
                _historyManager.SaveState();
            }
        }

        // ═══════════════════════════════════════════════════
        // UNDO / REDO
        // ═══════════════════════════════════════════════════

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            _historyManager.Undo();
        }

        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            _historyManager.Redo();
        }

        // ═══════════════════════════════════════════════════
        // FULLSCREEN TOGGLE
        // ═══════════════════════════════════════════════════

        private void BtnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        private void ToggleFullscreen()
        {
            _isFullscreen = !_isFullscreen;

            if (_isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.CanResize;
            }
        }

        // ═══════════════════════════════════════════════════
        // KEYBOARD SHORTCUTS
        // ═══════════════════════════════════════════════════

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // P = Pen mode
                case Key.P:
                    SwitchToMode(ToolMode.Pen);
                    e.Handled = true;
                    break;

                // S = Select mode
                case Key.S:
                    SwitchToMode(ToolMode.Select);
                    e.Handled = true;
                    break;

                // E = Eraser Stroke mode
                case Key.E:
                    SwitchToMode(ToolMode.EraserStroke);
                    e.Handled = true;
                    break;

                // R = Eraser Point mode
                case Key.R:
                    SwitchToMode(ToolMode.EraserPoint);
                    e.Handled = true;
                    break;

                // Delete = Delete selected strokes
                case Key.Delete:
                    if (_currentMode == ToolMode.Select)
                        DeleteSelectedStrokes();
                    e.Handled = true;
                    break;

                // Undo (Ctrl+Z)
                case Key.Z:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        _historyManager.Undo();
                        e.Handled = true;
                    }
                    break;

                // Redo (Ctrl+Y)
                case Key.Y:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        _historyManager.Redo();
                        e.Handled = true;
                    }
                    break;

                // F11 = Toggle fullscreen
                case Key.F11:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;

                // Escape = Exit fullscreen or switch to Pen
                case Key.Escape:
                    if (_isFullscreen)
                        ToggleFullscreen();
                    else
                        SwitchToMode(ToolMode.Pen);
                    e.Handled = true;
                    break;
            }
        }
    }
}