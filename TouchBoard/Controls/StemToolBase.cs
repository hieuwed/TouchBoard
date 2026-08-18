using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TouchBoard.Controls
{
    public abstract class StemToolBase : UserControl, ISerializableTool
    {
        protected Canvas _parentCanvas;
        protected bool _isDragging = false;
        protected bool _isRotating = false;
        protected Point _dragStartPoint;
        protected double _startAngle;
        protected Vector _startVector;

        // Visual Transforms
        protected ScaleTransform _scaleTransform;
        protected RotateTransform _rotateTransform;
        protected TranslateTransform _translateTransform;

        public event EventHandler ToolClosed;

        public StemToolBase()
        {
            _scaleTransform = new ScaleTransform();
            _rotateTransform = new RotateTransform();
            _translateTransform = new TranslateTransform();

            TransformGroup group = new TransformGroup();
            group.Children.Add(_scaleTransform);
            group.Children.Add(_rotateTransform);
            group.Children.Add(_translateTransform);

            this.RenderTransform = group;
        }

        public void Initialize(Canvas parentCanvas, Point initialPosition)
        {
            _parentCanvas = parentCanvas;
            Canvas.SetLeft(this, initialPosition.X);
            Canvas.SetTop(this, initialPosition.Y);
        }

        // ==========================================
        // DRAG LOGIC
        // ==========================================
        protected void OnBodyMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_parentCanvas == null) return;
            _isDragging = true;
            _dragStartPoint = e.GetPosition(_parentCanvas);
            Mouse.Capture(sender as UIElement);
            e.Handled = true;
        }

        protected void OnBodyMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _parentCanvas == null) return;

            Point currentPoint = e.GetPosition(_parentCanvas);
            double dx = currentPoint.X - _dragStartPoint.X;
            double dy = currentPoint.Y - _dragStartPoint.Y;

            double currentLeft = Canvas.GetLeft(this);
            double currentTop = Canvas.GetTop(this);

            if (double.IsNaN(currentLeft)) currentLeft = 0;
            if (double.IsNaN(currentTop)) currentTop = 0;

            Canvas.SetLeft(this, currentLeft + dx);
            Canvas.SetTop(this, currentTop + dy);

            _dragStartPoint = currentPoint;
            e.Handled = true;
        }

        protected void OnBodyMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null);
                e.Handled = true;
            }
        }

        // ==========================================
        // ROTATE LOGIC
        // ==========================================
        protected void OnRotateHandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_parentCanvas == null) return;
            _isRotating = true;
            
            // Lấy tọa độ gốc xoay trên parent Canvas
            Point centerInCanvas = this.TransformToAncestor(_parentCanvas).Transform(new Point(this.ActualWidth * this.RenderTransformOrigin.X, this.ActualHeight * this.RenderTransformOrigin.Y));
            Point currentPoint = e.GetPosition(_parentCanvas);

            _startVector = currentPoint - centerInCanvas;
            _startAngle = _rotateTransform.Angle;

            Mouse.Capture(sender as UIElement);
            e.Handled = true;
        }

        protected void OnRotateHandleMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRotating || _parentCanvas == null) return;

            Point centerInCanvas = this.TransformToAncestor(_parentCanvas).Transform(new Point(this.ActualWidth * this.RenderTransformOrigin.X, this.ActualHeight * this.RenderTransformOrigin.Y));
            Point currentPoint = e.GetPosition(_parentCanvas);

            Vector currentVector = currentPoint - centerInCanvas;
            
            double angle = Vector.AngleBetween(_startVector, currentVector);
            _rotateTransform.Angle = _startAngle + angle;

            OnAngleChanged(_rotateTransform.Angle);
            e.Handled = true;
        }

        protected void OnRotateHandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isRotating)
            {
                _isRotating = false;
                Mouse.Capture(null);
                e.Handled = true;
            }
        }

        protected virtual void OnAngleChanged(double newAngle)
        {
            // Cho phép lớp con override để cập nhật UI (Ví dụ: txtAngle.Text = ...)
        }

        // ==========================================
        // CLOSE LOGIC
        // ==========================================
        protected void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            if (_parentCanvas != null && _parentCanvas.Children.Contains(this))
            {
                _parentCanvas.Children.Remove(this);
            }
            ToolClosed?.Invoke(this, EventArgs.Empty);
        }

        // ==========================================
        // SERIALIZATION
        // ==========================================
        public virtual string Serialize()
        {
            var data = new
            {
                Type = this.GetType().FullName,
                PositionX = Canvas.GetLeft(this),
                PositionY = Canvas.GetTop(this),
                Width = this.Width,
                Height = this.Height,
                Angle = _rotateTransform != null ? _rotateTransform.Angle : 0
            };
            return System.Text.Json.JsonSerializer.Serialize(data);
        }

        public virtual void Deserialize(string json)
        {
            var doc = System.Text.Json.JsonDocument.Parse(json).RootElement;

            double posX = doc.GetProperty("PositionX").GetDouble();
            double posY = doc.GetProperty("PositionY").GetDouble();
            Canvas.SetLeft(this, double.IsNaN(posX) ? 0 : posX);
            Canvas.SetTop(this, double.IsNaN(posY) ? 0 : posY);

            this.Width = doc.GetProperty("Width").GetDouble();
            this.Height = doc.GetProperty("Height").GetDouble();

            if (doc.TryGetProperty("Angle", out var angleElement))
            {
                if (_rotateTransform != null)
                {
                    _rotateTransform.Angle = angleElement.GetDouble();
                    OnAngleChanged(_rotateTransform.Angle);
                }
            }
        }
    }
}
