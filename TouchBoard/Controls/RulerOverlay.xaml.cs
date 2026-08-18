using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Ink;

namespace TouchBoard.Controls
{
    public partial class RulerOverlay : StemToolBase, IEdgeSnappable
    {
        private MainWindow? _cachedMainWindow;

        private MainWindow? FindMainWindow()
        {
            if (_cachedMainWindow != null) return _cachedMainWindow;
            DependencyObject? parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is MainWindow mw) { _cachedMainWindow = mw; return mw; }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        // IEdgeSnappable — trả về 2 cạnh thước (trên/dưới) trong hệ tọa độ DrawingCanvas
        public IEnumerable<(Point P1, Point P2)> GetSnappingEdges()
        {
            var mw = FindMainWindow();
            if (mw == null) yield break;

            // Phần thân thước: bắt đầu ở Y=42 (sau 42px control panel), cao 60px
            const double yTop    = 42.0;   // mép trên thân thước
            const double yBottom = 102.0;  // mép dưới thân thước
            double xLeft  = 16;            // sau left handle
            double xRight = ActualWidth - 16; // trước right handle

            // Chuyển từ local → screen → DrawingCanvas (tự xử lý xoay & zoom)
            Point topL = mw.DrawingCanvas.PointFromScreen(PointToScreen(new Point(xLeft,  yTop)));
            Point topR = mw.DrawingCanvas.PointFromScreen(PointToScreen(new Point(xRight, yTop)));
            yield return (topL, topR);

            Point botL = mw.DrawingCanvas.PointFromScreen(PointToScreen(new Point(xLeft,  yBottom)));
            Point botR = mw.DrawingCanvas.PointFromScreen(PointToScreen(new Point(xRight, yBottom)));
            yield return (botL, botR);
        }

        private const double CM_TO_PIXELS = 32; // 1cm = 32px
        private double _lengthCm = 20;
        private bool _isResizingLeft = false;
        private bool _isResizingRight = false;
        private Point _resizeStartPoint;
        private double _resizeStartWidth;
        private double _resizeStartLeft;

        public RulerOverlay()
        {
            InitializeComponent();
            this.Width = 32 + (_lengthCm * CM_TO_PIXELS);
            DrawRulerMarks();
            this.Loaded += RulerOverlay_Loaded;
        }

        private void RulerOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            DrawRulerMarks();
        }

        private void DrawRulerMarks()
        {
            RulerCanvas.Children.Clear();
            
            // Width of the ruler canvas (excluding handles)
            double canvasWidth = _lengthCm * CM_TO_PIXELS;
            double height = 60; // Chiều cao của Ruler Body

            // Vẽ các vạch chia trên và dưới
            for (double x = 0; x <= canvasWidth; x += CM_TO_PIXELS / 10) // mm
            {
                bool isCm = (Math.Abs(x % CM_TO_PIXELS) < 0.1);
                bool isHalfCm = (Math.Abs((x % CM_TO_PIXELS) - (CM_TO_PIXELS / 2)) < 0.1);
                
                double markHeight = isCm ? 15 : (isHalfCm ? 10 : 5);
                double strokeThickness = isCm ? 1.5 : 1.0;

                // Vạch mép trên
                Line topLine = new Line
                {
                    X1 = x, Y1 = 0,
                    X2 = x, Y2 = markHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness
                };
                RulerCanvas.Children.Add(topLine);

                // Vạch mép dưới
                Line bottomLine = new Line
                {
                    X1 = x, Y1 = height,
                    X2 = x, Y2 = height - markHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = strokeThickness
                };
                RulerCanvas.Children.Add(bottomLine);
            }
        }

        protected override void OnAngleChanged(double newAngle)
        {
            if (txtAngle != null)
            {
                txtAngle.Text = $"{(int)Math.Round(newAngle)}°";
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RulerMainBorder != null)
            {
                RulerMainBorder.Opacity = e.NewValue;
            }
        }

        // ==============================================
        // RESIZING LOGIC
        // ==============================================
        private void LeftHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_parentCanvas == null) return;
            _isResizingLeft = true;
            _resizeStartPoint = e.GetPosition(_parentCanvas);
            _resizeStartWidth = this.Width;
            _resizeStartLeft = Canvas.GetLeft(this);
            Mouse.Capture(LeftHandle);
            e.Handled = true;
        }

        private void LeftHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isResizingLeft || _parentCanvas == null) return;
            Point currentPoint = e.GetPosition(_parentCanvas);
            
            // Tính toán dx theo trục X (bỏ qua xoay vì đang kéo trên mặt phẳng cha)
            // Tuy nhiên, nếu bị xoay, khoảng cách này cần được tính theo vector chiều ngang của thước
            // Để đơn giản, ta tính dx dựa trên tọa độ Local
            Point localCurrent = e.GetPosition(this);
            Point localStart = LeftHandle.PointFromScreen(_parentCanvas.PointToScreen(_resizeStartPoint)); 
            
            double dx = localCurrent.X; // Điểm chuột hiện tại trên thước (trục X)
            
            double newWidth = this.Width - dx;
            if (newWidth >= 320 && newWidth <= 1600) // Min 10cm, Max 50cm
            {
                this.Width = newWidth;
                _lengthCm = (newWidth - 32) / CM_TO_PIXELS;
                DrawRulerMarks();
                
                // Di chuyển vị trí thước để điểm phải giữ nguyên (nhưng cần cẩn thận với RotateTransform)
                // Hiện tại ta chỉ thay đổi Width. Tâm RenderTransformOrigin là 0.5,0.5 nên nó sẽ giãn ra 2 bên.
                // Để kéo từ mép trái, ta cần chỉnh Canvas.Left và Canvas.Top
                // Việc xử lý resizing có Transform khá phức tạp. Ta có thể tạm thời chỉ update Width 
                // và bỏ qua việc cố định vị trí mép phải nếu chưa cần thiết hoàn hảo.
            }
            e.Handled = true;
        }

        private void LeftHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizingLeft)
            {
                _isResizingLeft = false;
                Mouse.Capture(null);
                e.Handled = true;
            }
        }

        private void RightHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_parentCanvas == null) return;
            _isResizingRight = true;
            _resizeStartPoint = e.GetPosition(this);
            _resizeStartWidth = this.Width;
            Mouse.Capture(RightHandle);
            e.Handled = true;
        }

        private void RightHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isResizingRight) return;
            Point currentPoint = e.GetPosition(this);
            
            double dx = currentPoint.X - _resizeStartPoint.X;
            double newWidth = _resizeStartWidth + dx;
            
            if (newWidth >= 320 && newWidth <= 1600) // Min 10cm, Max 50cm
            {
                this.Width = newWidth;
                _lengthCm = (newWidth - 32) / CM_TO_PIXELS;
                DrawRulerMarks();
            }
            e.Handled = true;
        }

        private void RightHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizingRight)
            {
                _isResizingRight = false;
                Mouse.Capture(null);
                e.Handled = true;
            }
        }

        // ==============================================
        // DRAWING LOGIC (Bottom Draw Zone) - Hỗ trợ Mouse + Touch
        // ==============================================
        private bool _isDrawing = false;
        private Point? _drawStartPoint = null;
        private Line _previewLine;
        private object _currentInputDevice = null; // Tránh xung đột giữa touch và mouse (ví dụ: touch tạo ra cả event touch và mouse)
        private InkCanvasEditingMode? _previousEditingMode = null;

        private void BeginDraw(Point startPoint, object inputDevice)
        {
            if (_parentCanvas == null || _isDrawing) return;

            var mainWindow = FindMainWindow();
            if (mainWindow != null)
            {
                // Chế độ chọn vùng: không can thiệp — để SelectionManager xử lý
                if (mainWindow.DrawingCanvas.EditingMode == InkCanvasEditingMode.Select)
                    return;

                _previousEditingMode = mainWindow.DrawingCanvas.EditingMode;
                mainWindow.DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
            }

            _isDrawing = true;
            _currentInputDevice = inputDevice;
            _drawStartPoint = startPoint;

            _previewLine = new Line
            {
                X1 = startPoint.X,
                Y1 = 60, // bám sát mép dưới ruler body
                X2 = startPoint.X,
                Y2 = 60,
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
            PreviewCanvas.Children.Add(_previewLine);
        }

        private void UpdateDraw(Point currentPoint, object inputDevice)
        {
            if (!_isDrawing || _previewLine == null || _currentInputDevice != inputDevice) return;
            _previewLine.X2 = currentPoint.X;
        }

        private void EndDraw(Point endPoint, object inputDevice)
        {
            if (!_isDrawing || _currentInputDevice != inputDevice) return;

            _isDrawing = false;
            _currentInputDevice = null;

            if (_previewLine != null)
            {
                PreviewCanvas.Children.Remove(_previewLine);
                _previewLine = null;
            }

            if (_drawStartPoint.HasValue && Math.Abs(endPoint.X - _drawStartPoint.Value.X) > 5)
                DrawStraightLineOnMainCanvas(_drawStartPoint.Value.X, endPoint.X);

            _drawStartPoint = null;

            // Restore EditingMode (bug fix: trước đây bị thiếu restore nên EditingMode bị kẹt ở None)
            RestoreEditingMode();
        }

        /// <summary>Gọi khi đóng thước hoặc mất capture — đảm bảo EditingMode luôn được hồi phục.</summary>
        public void CancelDraw()
        {
            if (!_isDrawing) return;
            _isDrawing = false;
            _currentInputDevice = null;
            if (_previewLine != null) { PreviewCanvas.Children.Remove(_previewLine); _previewLine = null; }
            _drawStartPoint = null;
            RestoreEditingMode();
        }

        private void RestoreEditingMode()
        {
            if (!_previousEditingMode.HasValue) return;
            var mw = FindMainWindow();
            if (mw != null) mw.DrawingCanvas.EditingMode = _previousEditingMode.Value;
            _previousEditingMode = null;
        }

        // --- Mouse Events ---
        private void BottomDrawZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Không can thiệp khi Selection mode (BeginDraw sẽ tự check và return)
            BeginDraw(e.GetPosition(this), e.MouseDevice);
            if (_isDrawing) // chỉ capture khi thực sự vẽ
            {
                Mouse.Capture(BottomDrawZone);
                e.Handled = true;
            }
        }

        private void BottomDrawZone_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateDraw(e.GetPosition(this), e.MouseDevice);
        }

        private void BottomDrawZone_MouseUp(object sender, MouseButtonEventArgs e)
        {
            bool wasDrawing = _isDrawing;
            EndDraw(e.GetPosition(this), e.MouseDevice);
            Mouse.Capture(null);
            if (wasDrawing) e.Handled = true;
        }

        private void BottomDrawZone_MouseLeave(object sender, MouseEventArgs e)
        {
            // Khi rời vùng mà không release (mất capture) → hủy draw
            if (_isDrawing && !Mouse.Captured.Equals(BottomDrawZone))
                CancelDraw();
        }

        // --- Touch Events ---
        private void BottomDrawZone_TouchDown(object sender, TouchEventArgs e)
        {
            BeginDraw(e.GetTouchPoint(this).Position, e.TouchDevice);
            if (_isDrawing) // chỉ capture khi thực sự vẽ
            {
                BottomDrawZone.CaptureTouch(e.TouchDevice);
                e.Handled = true;
            }
        }

        private void BottomDrawZone_TouchMove(object sender, TouchEventArgs e)
        {
            UpdateDraw(e.GetTouchPoint(this).Position, e.TouchDevice);
            if (_isDrawing) e.Handled = true;
        }

        private void BottomDrawZone_TouchUp(object sender, TouchEventArgs e)
        {
            bool wasDrawing = _isDrawing;
            EndDraw(e.GetTouchPoint(this).Position, e.TouchDevice);
            BottomDrawZone.ReleaseTouchCapture(e.TouchDevice);
            if (wasDrawing) e.Handled = true;
        }

        private void BottomDrawZone_TouchLeave(object sender, TouchEventArgs e)
        {
            if (_isDrawing) CancelDraw(); // hủy nếu mất touch
        }
        
        private void DrawStraightLineOnMainCanvas(double startXLocal, double endXLocal)
        {
            if (_parentCanvas == null) return;

            var mainWindow = FindMainWindow(); // dùng cache thay vì tìm lại
            if (mainWindow == null) return;

            const double rulerBodyHeight = 60;
            Point startLocal = new Point(startXLocal, rulerBodyHeight);
            Point endLocal   = new Point(endXLocal,   rulerBodyHeight);

            Point startOnScreen = this.PointToScreen(startLocal);
            Point endOnScreen   = this.PointToScreen(endLocal);

            Point startOnCanvas = mainWindow.DrawingCanvas.PointFromScreen(startOnScreen);
            Point endOnCanvas   = mainWindow.DrawingCanvas.PointFromScreen(endOnScreen);

            var drawingAttributes = mainWindow.DrawingCanvas.DefaultDrawingAttributes.Clone();
            drawingAttributes.FitToCurve = false;

            var points = new System.Windows.Input.StylusPointCollection();
            points.Add(new System.Windows.Input.StylusPoint(startOnCanvas.X, startOnCanvas.Y));
            points.Add(new System.Windows.Input.StylusPoint(endOnCanvas.X,   endOnCanvas.Y));

            var straightStroke = new System.Windows.Ink.Stroke(points, drawingAttributes);
            mainWindow.DrawingCanvas.Strokes.Add(straightStroke);

            // Restore EditingMode sau khi vẽ xong
            RestoreEditingMode();
        }
    }
}
