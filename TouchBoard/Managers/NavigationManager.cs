using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace TouchBoard.Managers
{
    public class NavigationManager
    {
        private readonly MainWindow _window;
        
        private ScaleTransform _scaleTransform;
        private TranslateTransform _translateTransform;
        private TransformGroup _transformGroup;

        private bool _isPanning = false;
        private Point _panStartPoint;
        private Point _translateStartPoint;

        private System.Collections.Generic.Dictionary<int, Point> _activeTouches = new System.Collections.Generic.Dictionary<int, Point>();
        private double _lastDistance = 0;
        private Point _lastCenter = new Point();

        public NavigationManager(MainWindow window)
        {
            _window = window;

            _scaleTransform = new ScaleTransform(1.0, 1.0);
            _translateTransform = new TranslateTransform(50000, 50000);

            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);

            _window.DrawingCanvas.RenderTransform = _transformGroup;
            _window.DrawingCanvas.RenderTransformOrigin = new Point(0, 0);

            // Xử lý bằng Chuột (Space + Left drag hoặc Middle click)
            _window.PreviewKeyDown += Window_PreviewKeyDown;
            _window.PreviewKeyUp += Window_PreviewKeyUp;
            _window.PreviewMouseWheel += Window_PreviewMouseWheel;
            _window.DrawingCanvas.PreviewMouseDown += DrawingCanvas_PreviewMouseDown;
            _window.DrawingCanvas.PreviewMouseMove += DrawingCanvas_PreviewMouseMove;
            _window.DrawingCanvas.PreviewMouseUp += DrawingCanvas_PreviewMouseUp;

            // Xử lý bằng Cảm ứng (2 ngón) - Thay vì dùng Manipulation, ta dùng Touch trực tiếp
            _window.DrawingCanvas.IsManipulationEnabled = false;
            _window.DrawingCanvas.PreviewTouchDown += DrawingCanvas_PreviewTouchDown;
            _window.DrawingCanvas.PreviewTouchMove += DrawingCanvas_PreviewTouchMove;
            _window.DrawingCanvas.PreviewTouchUp += DrawingCanvas_PreviewTouchUp;
        }

        // =====================================
        // ZOOM (MOUSE WHEEL)
        // =====================================
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double zoomDelta = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
                ZoomAt(zoomDelta, e.GetPosition(_window));
                e.Handled = true;
            }
        }

        private void ZoomAt(double zoomFactor, Point center)
        {
            // Tính toán giới hạn scale
            double newScaleX = _scaleTransform.ScaleX * zoomFactor;
            double newScaleY = _scaleTransform.ScaleY * zoomFactor;

            if (newScaleX < 0.1 || newScaleX > 10.0) return;

            // Chuyển đổi điểm center từ tọa độ màn hình sang tọa độ trước khi biến đổi
            Point relativePoint = _window.DrawingCanvas.TransformToVisual(_window).Inverse.Transform(center);

            _scaleTransform.ScaleX = newScaleX;
            _scaleTransform.ScaleY = newScaleY;

            // Cập nhật translate để giữ nguyên vị trí con trỏ chuột
            Point newPoint = _window.DrawingCanvas.TransformToVisual(_window).Transform(relativePoint);
            _translateTransform.X += center.X - newPoint.X;
            _translateTransform.Y += center.Y - newPoint.Y;
        }

        // =====================================
        // PAN (MOUSE DRAG)
        // =====================================
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !_isPanning)
            {
                _window.Cursor = Cursors.Hand;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _window.Cursor = Cursors.Arrow;
            }
        }

        private void DrawingCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed || 
                e.MiddleButton == MouseButtonState.Pressed || 
                (Keyboard.IsKeyDown(Key.Space) && e.LeftButton == MouseButtonState.Pressed))
            {
                _isPanning = true;
                _panStartPoint = e.GetPosition(_window);
                _translateStartPoint = new Point(_translateTransform.X, _translateTransform.Y);
                _window.DrawingCanvas.CaptureMouse();
                e.Handled = true;
            }
        }

        private void DrawingCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                Point currentPoint = e.GetPosition(_window);
                Vector delta = currentPoint - _panStartPoint;

                _translateTransform.X = _translateStartPoint.X + delta.X;
                _translateTransform.Y = _translateStartPoint.Y + delta.Y;
                e.Handled = true;
            }
        }

        private void DrawingCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning && 
                (e.RightButton == MouseButtonState.Released || 
                 e.MiddleButton == MouseButtonState.Released || 
                 e.LeftButton == MouseButtonState.Released))
            {
                _isPanning = false;
                _window.DrawingCanvas.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        // =====================================
        // TOUCH (MANUAL 2-FINGER ZOOM/PAN)
        // =====================================
        private void DrawingCanvas_PreviewTouchDown(object? sender, TouchEventArgs e)
        {
            if (_window.ToolManager?.CurrentMode != ToolMode.Select) return;
            
            _activeTouches[e.TouchDevice.Id] = e.GetTouchPoint(_window).Position;
            if (_activeTouches.Count >= 2)
            {
                var points = System.Linq.Enumerable.ToArray(_activeTouches.Values);
                _lastDistance = (points[0] - points[1]).Length;
                _lastCenter = new Point((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);
                
                // Nhả capture để hủy Lasso đang vẽ dở của ngón 1
                _window.DrawingCanvas.ReleaseMouseCapture();
                _window.DrawingCanvas.ReleaseStylusCapture();
                
                e.Handled = true; // Chặn InkCanvas xử lý tiếp ngón thứ 2
            }
        }

        private void DrawingCanvas_PreviewTouchMove(object? sender, TouchEventArgs e)
        {
            if (_window.ToolManager?.CurrentMode != ToolMode.Select) return;

            if (_activeTouches.ContainsKey(e.TouchDevice.Id))
            {
                _activeTouches[e.TouchDevice.Id] = e.GetTouchPoint(_window).Position;

                if (_activeTouches.Count >= 2)
                {
                    var points = System.Linq.Enumerable.ToArray(_activeTouches.Values);
                    double currentDistance = (points[0] - points[1]).Length;
                    Point currentCenter = new Point((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);

                    if (_lastDistance > 0)
                    {
                        // Calculate zoom
                        double zoomDelta = currentDistance / _lastDistance;
                        ZoomAt(zoomDelta, currentCenter);

                        // Calculate pan
                        Vector panDelta = currentCenter - _lastCenter;
                        _translateTransform.X += panDelta.X;
                        _translateTransform.Y += panDelta.Y;
                    }

                    _lastDistance = currentDistance;
                    _lastCenter = currentCenter;
                    e.Handled = true; // Chặn InkCanvas không cho Lasso hay Move object
                }
            }
        }

        private void DrawingCanvas_PreviewTouchUp(object? sender, TouchEventArgs e)
        {
            if (_activeTouches.ContainsKey(e.TouchDevice.Id))
            {
                bool wasMultiTouch = _activeTouches.Count >= 2;
                _activeTouches.Remove(e.TouchDevice.Id);
                
                if (_activeTouches.Count < 2)
                {
                    _lastDistance = 0;
                }
                
                if (wasMultiTouch && _window.ToolManager?.CurrentMode == ToolMode.Select)
                {
                    e.Handled = true; // Chặn sự kiện TouchUp để khỏi click nhầm sau khi zoom
                }
            }
        }
    }
}
