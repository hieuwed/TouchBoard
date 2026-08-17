using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Media;

namespace TouchBoard.Managers
{
    public class MultiTouchManager
    {
        private readonly MainWindow _window;
        private readonly ToolManager _toolManager;
        private readonly HistoryManager _historyManager;

        private readonly Dictionary<int, Stroke> _activeTouches = new Dictionary<int, Stroke>();
        private readonly Dictionary<int, Stroke> _activeStyluses = new Dictionary<int, Stroke>();
        private Stroke? _mouseStroke;

        public MultiTouchManager(MainWindow window, ToolManager toolManager, HistoryManager historyManager)
        {
            _window = window;
            _toolManager = toolManager;
            _historyManager = historyManager;

            // 1. Touch events for Multi-touch (Fingers)
            _window.DrawingCanvas.TouchDown += DrawingCanvas_TouchDown;
            _window.DrawingCanvas.TouchMove += DrawingCanvas_TouchMove;
            _window.DrawingCanvas.TouchUp += DrawingCanvas_TouchUp;
            _window.DrawingCanvas.TouchLeave += DrawingCanvas_TouchUp; // Fallback

            // 2. Stylus events for Active Pens (Pressure sensitive)
            _window.DrawingCanvas.StylusDown += DrawingCanvas_StylusDown;
            _window.DrawingCanvas.StylusMove += DrawingCanvas_StylusMove;
            _window.DrawingCanvas.StylusUp += DrawingCanvas_StylusUp;
            _window.DrawingCanvas.StylusOutOfRange += DrawingCanvas_StylusUp;

            // 3. Mouse events for regular mouse fallback
            _window.DrawingCanvas.MouseDown += DrawingCanvas_MouseDown;
            _window.DrawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            _window.DrawingCanvas.MouseUp += DrawingCanvas_MouseUp;
            _window.DrawingCanvas.MouseLeave += DrawingCanvas_MouseUp; // Fallback
        }

        // ==========================================
        // HELPER: HIT TEST VALIDATION
        // ==========================================
        private bool IsHitValidForCanvas(Point posScreen)
        {
            var hit = VisualTreeHelper.HitTest(_window, posScreen);
            if (hit?.VisualHit == null) return false;

            DependencyObject? current = hit.VisualHit;
            while (current != null)
            {
                if (current == _window.InfiniteCanvasContainer)
                    return true;
                
                if (current is Visual || current is System.Windows.Media.Media3D.Visual3D)
                    current = VisualTreeHelper.GetParent(current);
                else
                    current = LogicalTreeHelper.GetParent(current);
            }
            return false;
        }

        // ==========================================
        // 1. TOUCH HANDLING (MULTI-TOUCH)
        // ==========================================
        private void DrawingCanvas_TouchDown(object? sender, TouchEventArgs e)
        {
            if (_toolManager.CurrentMode != ToolMode.Pen) return;

            if (!IsHitValidForCanvas(e.GetTouchPoint(_window).Position)) return;

            e.Handled = true; // Prevent promotion to mouse/stylus to avoid double drawing

            var touchPoint = e.GetTouchPoint(_window.DrawingCanvas);
            var stroke = new Stroke(new StylusPointCollection(new[] { new StylusPoint(touchPoint.Position.X, touchPoint.Position.Y) }))
            {
                DrawingAttributes = _window.DrawingCanvas.DefaultDrawingAttributes.Clone()
            };

            _activeTouches[e.TouchDevice.Id] = stroke;
            _window.DrawingCanvas.Strokes.Add(stroke);
            e.TouchDevice.Capture(_window.DrawingCanvas);
        }

        private void DrawingCanvas_TouchMove(object? sender, TouchEventArgs e)
        {
            if (_toolManager.CurrentMode != ToolMode.Pen) return;

            if (_activeTouches.TryGetValue(e.TouchDevice.Id, out var stroke))
            {
                var touchPoint = e.GetTouchPoint(_window.DrawingCanvas);
                stroke.StylusPoints.Add(new StylusPoint(touchPoint.Position.X, touchPoint.Position.Y));
                e.Handled = true;
            }
        }

        private void DrawingCanvas_TouchUp(object? sender, TouchEventArgs e)
        {
            if (_activeTouches.TryGetValue(e.TouchDevice.Id, out var stroke))
            {
                _activeTouches.Remove(e.TouchDevice.Id);
                e.TouchDevice.Capture(null);
                _historyManager.SaveState();
                e.Handled = true;
            }
        }

        // ==========================================
        // 2. STYLUS HANDLING (ACTIVE PEN)
        // ==========================================
        private void DrawingCanvas_StylusDown(object? sender, StylusDownEventArgs e)
        {
            if (_toolManager.CurrentMode != ToolMode.Pen) return;

            if (!IsHitValidForCanvas(e.GetPosition(_window))) return;
            if (e.Handled) return; // Ignore if Touch already handled it
            
            e.Handled = true;

            var incomingPts = e.GetStylusPoints(_window.DrawingCanvas);
            var pts = new StylusPointCollection(incomingPts.Description);
            pts.Add(incomingPts[0]);
            
            var stroke = new Stroke(pts) 
            { 
                DrawingAttributes = _window.DrawingCanvas.DefaultDrawingAttributes.Clone() 
            };

            _activeStyluses[e.StylusDevice.Id] = stroke;
            _window.DrawingCanvas.Strokes.Add(stroke);
            e.StylusDevice.Capture(_window.DrawingCanvas);
        }

        private void DrawingCanvas_StylusMove(object? sender, StylusEventArgs e)
        {
            if (_toolManager.CurrentMode != ToolMode.Pen) return;

            if (_activeStyluses.TryGetValue(e.StylusDevice.Id, out var stroke))
            {
                stroke.StylusPoints.Add(e.GetStylusPoints(_window.DrawingCanvas));
                e.Handled = true;
            }
        }

        private void DrawingCanvas_StylusUp(object? sender, StylusEventArgs e)
        {
            if (_activeStyluses.TryGetValue(e.StylusDevice.Id, out var stroke))
            {
                _activeStyluses.Remove(e.StylusDevice.Id);
                e.StylusDevice.Capture(null);
                _historyManager.SaveState();
                e.Handled = true;
            }
        }

        // ==========================================
        // 3. MOUSE HANDLING (FALLBACK)
        // ==========================================
        private void DrawingCanvas_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (_toolManager.CurrentMode != ToolMode.Pen) return;
            if (e.Handled || e.StylusDevice != null) return; // Ignore if handled by Touch/Stylus
            
            if (!IsHitValidForCanvas(e.GetPosition(_window))) return;
            
            var pos = e.GetPosition(_window.DrawingCanvas);
            _mouseStroke = new Stroke(new StylusPointCollection(new[] { new StylusPoint(pos.X, pos.Y) }))
            {
                DrawingAttributes = _window.DrawingCanvas.DefaultDrawingAttributes.Clone()
            };
            
            _window.DrawingCanvas.Strokes.Add(_mouseStroke);
            Mouse.Capture(_window.DrawingCanvas);
        }

        private void DrawingCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_toolManager.CurrentMode != ToolMode.Pen) return;

            if (_mouseStroke != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(_window.DrawingCanvas);
                _mouseStroke.StylusPoints.Add(new StylusPoint(pos.X, pos.Y));
            }
        }

        private void DrawingCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_mouseStroke != null)
            {
                _mouseStroke = null;
                Mouse.Capture(null);
                _historyManager.SaveState();
            }
        }
    }
}
