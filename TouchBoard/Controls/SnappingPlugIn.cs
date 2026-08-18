using System.Windows.Input.StylusPlugIns;
using System.Windows.Input;
using TouchBoard.Managers;
using System.Windows;
using System;
using System.Windows.Controls;

namespace TouchBoard.Controls
{
    /// <summary>
    /// StylusPlugIn chỉ can thiệp được Stylus/Touch (KHÔNG can thiệp được Mouse).
    /// Để hỗ trợ cả Mouse, ta cần bổ sung logic ở MainWindow (PreviewMouse events).
    /// Class này chịu trách nhiệm cho phần Stylus/Touch realtime.
    /// </summary>
    public class SnappingPlugIn : StylusPlugIn
    {
        private StemManager _stemManager;
        private bool _isCurrentlySnapping = false;
        private (Point P1, Point P2) _snappedEdge;

        public StemManager StemManager 
        { 
            get => _stemManager; 
            set => _stemManager = value; 
        }

        public bool IsCurrentlySnapping => _isCurrentlySnapping;
        public (Point P1, Point P2) SnappedEdge => _snappedEdge;

        public SnappingPlugIn(StemManager stemManager)
        {
            _stemManager = stemManager;
        }

        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            if (_stemManager == null || rawStylusInput.GetStylusPoints().Count == 0)
            {
                base.OnStylusDown(rawStylusInput);
                return;
            }

            StylusPoint firstPoint = rawStylusInput.GetStylusPoints()[0];
            Point p = new Point(firstPoint.X, firstPoint.Y);
            
            Point snapped = _stemManager.GetSnappedPointAndEdge(p, out _isCurrentlySnapping, out _snappedEdge);
            
            if (_isCurrentlySnapping)
            {
                StylusPointCollection newPoints = new StylusPointCollection(rawStylusInput.GetStylusPoints().Description);
                foreach (var pt in rawStylusInput.GetStylusPoints())
                {
                    StylusPoint snappedPt = pt;
                    snappedPt.X = snapped.X;
                    snappedPt.Y = snapped.Y;
                    newPoints.Add(snappedPt);
                }
                rawStylusInput.SetStylusPoints(newPoints);
            }
            else
            {
                _snappedEdge = default;
            }
            
            base.OnStylusDown(rawStylusInput);
        }

        protected override void OnStylusMove(RawStylusInput rawStylusInput)
        {
            if (_isCurrentlySnapping && _stemManager != null)
            {
                StylusPointCollection newPoints = new StylusPointCollection(rawStylusInput.GetStylusPoints().Description);
                foreach (var pt in rawStylusInput.GetStylusPoints())
                {
                    Point p = new Point(pt.X, pt.Y);
                    Point snapped = StemManager.ProjectPointOnLineSegment(_snappedEdge.P1, _snappedEdge.P2, p);
                    
                    StylusPoint newPt = pt;
                    newPt.X = snapped.X;
                    newPt.Y = snapped.Y;
                    newPoints.Add(newPt);
                }
                rawStylusInput.SetStylusPoints(newPoints);
            }
            
            base.OnStylusMove(rawStylusInput);
        }

        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            if (_isCurrentlySnapping && _stemManager != null)
            {
                StylusPointCollection newPoints = new StylusPointCollection(rawStylusInput.GetStylusPoints().Description);
                foreach (var pt in rawStylusInput.GetStylusPoints())
                {
                    Point p = new Point(pt.X, pt.Y);
                    Point snapped = StemManager.ProjectPointOnLineSegment(_snappedEdge.P1, _snappedEdge.P2, p);
                    
                    StylusPoint newPt = pt;
                    newPt.X = snapped.X;
                    newPt.Y = snapped.Y;
                    newPoints.Add(newPt);
                }
                rawStylusInput.SetStylusPoints(newPoints);
            }

            _isCurrentlySnapping = false;
            _snappedEdge = default;
            base.OnStylusUp(rawStylusInput);
        }
    }
}
