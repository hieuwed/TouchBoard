using System.Windows;
using System.Windows.Media;
using TouchBoard.Models;

namespace TouchBoard.Managers
{
    public static class ShapeManager
    {
        public static Geometry GetGeometry(ShapeType type)
        {
            string pathData = "";

            switch (type)
            {
                case ShapeType.Rectangle:
                    pathData = "M0,0 L1,0 L1,1 L0,1 Z";
                    break;
                case ShapeType.Triangle:
                    pathData = "M0.5,0 L1,1 L0,1 Z";
                    break;
                case ShapeType.Rhombus:
                    pathData = "M0.5,0 L1,0.5 L0.5,1 L0,0.5 Z";
                    break;
                case ShapeType.Hexagon:
                    pathData = "M0.25,0 L0.75,0 L1,0.5 L0.75,1 L0.25,1 L0,0.5 Z";
                    break;
                case ShapeType.Trapezoid:
                    pathData = "M0.2,0 L0.8,0 L1,1 L0,1 Z";
                    break;
                case ShapeType.Parallelogram:
                    pathData = "M0.2,0 L1,0 L0.8,1 L0,1 Z";
                    break;
                case ShapeType.Ellipse:
                case ShapeType.Circle:
                    // Ellipse using arc commands, normalized 0-1
                    pathData = "M0.5,0 A0.5,0.5 0 1,1 0.5,1 A0.5,0.5 0 1,1 0.5,0 Z";
                    break;
                case ShapeType.Semicircle:
                // 3D Shapes (Projections) - CHỈ CÁC CẠNH THẤY ĐƯỢC
                case ShapeType.Cube:
                    pathData = "M0,0.25 L0.75,0.25 L0.75,1 L0,1 Z M0,0.25 L0.25,0 L1,0 L0.75,0.25 M1,0 L1,0.75 L0.75,1";
                    break;
                case ShapeType.TriangularPrism:
                    pathData = "M0.5,0 L1,0.25 L1,1 L0,1 L0,0.25 Z M0.5,0 L0,0.25 M0.5,0 L1,0.25";
                    break;
                case ShapeType.Cylinder:
                    pathData = "M0,0.15 A0.5,0.15 0 0,1 1,0.15 A0.5,0.15 0 0,1 0,0.15 Z M0,0.15 L0,0.85 M1,0.15 L1,0.85 M0,0.85 A0.5,0.15 0 0,0 1,0.85";
                    break;
                case ShapeType.Cone:
                    pathData = "M0.5,0 L0,0.85 M0.5,0 L1,0.85 M0,0.85 A0.5,0.15 0 0,0 1,0.85";
                    break;
                case ShapeType.Sphere:
                    pathData = "M0.5,0 A0.5,0.5 0 0,1 0.5,1 A0.5,0.5 0 0,1 0.5,0 Z M0,0.5 A0.5,0.15 0 0,0 1,0.5 M0.5,0 A0.15,0.5 0 0,0 0.5,1";
                    break;
                case ShapeType.Frustum:
                    pathData = "M0.2,0.15 A0.3,0.08 0 0,1 0.8,0.15 A0.3,0.08 0 0,1 0.2,0.15 Z M0.2,0.15 L0,0.85 M0.8,0.15 L1,0.85 M0,0.85 A0.5,0.15 0 0,0 1,0.85";
                    break;
                case ShapeType.TriangularPyramid:
                    pathData = "M0.5,0 L0,0.8 M0.5,0 L1,0.8 M0.5,0 L0.5,1 M0,0.8 L0.5,1 M1,0.8 L0.5,1";
                    break;

                // Stickers
                case ShapeType.Star:
                    pathData = "M0.5,0 L0.61,0.35 L0.98,0.35 L0.68,0.57 L0.79,0.91 L0.5,0.7 L0.21,0.91 L0.32,0.57 L0.02,0.35 L0.39,0.35 Z";
                    break;
                case ShapeType.Checkmark:
                    pathData = "M0.1,0.5 L0.4,0.8 L0.9,0.1";
                    break;
                case ShapeType.Cross:
                    pathData = "M0.2,0.2 L0.8,0.8 M0.8,0.2 L0.2,0.8";
                    break;
                case ShapeType.Heart:
                    pathData = "M0.5,0.9 C0.5,0.9 0,0.5 0,0.25 C0,0.1 0.2,0 0.5,0.25 C0.8,0 1,0.1 1,0.25 C1,0.5 0.5,0.9 0.5,0.9 Z";
                    break;
                case ShapeType.Smiley:
                    pathData = "M0.5,0 A0.5,0.5 0 1,1 0.5,1 A0.5,0.5 0 1,1 0.5,0 Z M0.3,0.3 A0.05,0.05 0 1,1 0.3,0.4 A0.05,0.05 0 1,1 0.3,0.3 Z M0.7,0.3 A0.05,0.05 0 1,1 0.7,0.4 A0.05,0.05 0 1,1 0.7,0.3 Z M0.2,0.6 A0.3,0.2 0 0,0 0.8,0.6";
                    break;
                case ShapeType.Cloud:
                    pathData = "M0.3,0.4 A0.2,0.2 0 0,1 0.7,0.4 A0.15,0.15 0 0,1 0.9,0.5 A0.15,0.15 0 0,1 0.7,0.8 L0.3,0.8 A0.2,0.2 0 0,1 0.3,0.4 Z";
                    break;
                case ShapeType.Lightning:
                    pathData = "M0.45,0 L0.15,0.55 L0.45,0.55 L0.35,1 L0.85,0.4 L0.55,0.4 Z";
                    break;
                
                // Lines
                case ShapeType.Line:
                    pathData = "M0,0.5 L1,0.5";
                    break;
                case ShapeType.Arrow:
                    pathData = "M0,0.5 L0.9,0.5 M0.7,0.3 L0.9,0.5 L0.7,0.7";
                    break;
                case ShapeType.DashedLine:
                    pathData = "M0,0.5 L1,0.5";
                    break;

                default:
                    pathData = "M0,0 L1,0 L1,1 L0,1 Z";
                    break;
            }

            try
            {
                return Geometry.Parse(pathData);
            }
            catch
            {
                return Geometry.Parse("M0,0 L1,0 L1,1 L0,1 Z");
            }
        }

        /// <summary>
        /// Trả về Geometry nét đứt cho các cạnh khuất (bên trong) của hình 3D.
        /// Trả về null nếu không phải hình 3D.
        /// </summary>
        public static Geometry? GetHiddenGeometry(ShapeType type)
        {
            string? hiddenData = null;

            switch (type)
            {
                case ShapeType.Cube:
                    // 3 cạnh khuất phía sau: đứng + ngang dưới + ngang sau
                    hiddenData = "M0.25,0 L0.25,0.75 L0,1 M0.25,0.75 L1,0.75";
                    break;
                case ShapeType.TriangularPrism:
                    // Cạnh đáy khuất phía sau
                    hiddenData = "M0.5,0.75 L0,1 M0.5,0.75 L1,1 M0.5,0.75 L0.5,0";
                    break;
                case ShapeType.Cylinder:
                    hiddenData = "M0,0.85 A0.5,0.15 0 0,1 1,0.85";
                    break;
                case ShapeType.Cone:
                    hiddenData = "M0,0.85 A0.5,0.15 0 0,1 1,0.85";
                    break;
                case ShapeType.Sphere:
                    hiddenData = "M0.5,0 A0.15,0.5 0 0,1 0.5,1 M0,0.5 A0.5,0.15 0 0,1 1,0.5";
                    break;
                case ShapeType.Frustum:
                    hiddenData = "M0,0.85 A0.5,0.15 0 0,1 1,0.85";
                    break;
                case ShapeType.TriangularPyramid:
                    hiddenData = "M0,0.8 L1,0.8";
                    break;
            }

            if (hiddenData == null) return null;

            try
            {
                return Geometry.Parse(hiddenData);
            }
            catch
            {
                return null;
            }
        }

        public static System.Windows.Ink.StrokeCollection GenerateStrokes(ShapeType type, Rect bounds, System.Windows.Ink.DrawingAttributes da)
        {
            var strokes = new System.Windows.Ink.StrokeCollection();
            try
            {
                var geo = GetGeometry(type);
                if (geo != null)
                {
                    strokes.Add(ConvertGeometryToStrokes(geo, bounds, da, false));
                }

                var hiddenGeo = GetHiddenGeometry(type);
                if (hiddenGeo != null)
                {
                    strokes.Add(ConvertGeometryToStrokes(hiddenGeo, bounds, da, true));
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"GenerateStrokes Error: {ex.Message}");
            }

            return strokes;
        }

        private static System.Windows.Ink.StrokeCollection ConvertGeometryToStrokes(Geometry geo, Rect bounds, System.Windows.Ink.DrawingAttributes da, bool isDashed)
        {
            var strokes = new System.Windows.Ink.StrokeCollection();
            var flattened = geo.GetFlattenedPathGeometry(0.001, ToleranceType.Relative);

            foreach (var figure in flattened.Figures)
            {
                var points = new System.Windows.Input.StylusPointCollection();
                points.Add(new System.Windows.Input.StylusPoint(
                    figure.StartPoint.X * bounds.Width + bounds.Left, 
                    figure.StartPoint.Y * bounds.Height + bounds.Top));

                foreach (var segment in figure.Segments)
                {
                    if (segment is PolyLineSegment pls)
                    {
                        foreach (var pt in pls.Points)
                        {
                            points.Add(new System.Windows.Input.StylusPoint(pt.X * bounds.Width + bounds.Left, pt.Y * bounds.Height + bounds.Top));
                        }
                    }
                    else if (segment is LineSegment ls)
                    {
                        points.Add(new System.Windows.Input.StylusPoint(ls.Point.X * bounds.Width + bounds.Left, ls.Point.Y * bounds.Height + bounds.Top));
                    }
                }

                if (figure.IsClosed)
                {
                    points.Add(new System.Windows.Input.StylusPoint(
                        figure.StartPoint.X * bounds.Width + bounds.Left, 
                        figure.StartPoint.Y * bounds.Height + bounds.Top));
                }

                if (points.Count > 1)
                {
                    if (isDashed)
                    {
                        // Sinh ra các nét đứt ngắn
                        double dashLen = 8.0;
                        double gapLen = 6.0;
                        double currentLen = 0;
                        var dashPoints = new System.Windows.Input.StylusPointCollection();
                        dashPoints.Add(points[0]);
                        
                        for (int i = 0; i < points.Count - 1; i++)
                        {
                            var p1 = points[i];
                            var p2 = points[i+1];
                            double dx = p2.X - p1.X;
                            double dy = p2.Y - p1.Y;
                            double segLen = System.Math.Sqrt(dx*dx + dy*dy);
                            
                            double segRemaining = segLen;
                            double nx = dx / segLen;
                            double ny = dy / segLen;
                            
                            Point currentP = new Point(p1.X, p1.Y);

                            while (segRemaining > 0)
                            {
                                double step = isDrawingDash ? (dashLen - currentLen) : (gapLen - currentLen);
                                if (step > segRemaining) step = segRemaining;

                                currentP = new Point(currentP.X + nx * step, currentP.Y + ny * step);
                                currentLen += step;
                                segRemaining -= step;

                                if (isDrawingDash) dashPoints.Add(new System.Windows.Input.StylusPoint(currentP.X, currentP.Y));

                                if ((isDrawingDash && currentLen >= dashLen) || (!isDrawingDash && currentLen >= gapLen))
                                {
                                    if (isDrawingDash && dashPoints.Count > 1)
                                    {
                                        var stroke = new System.Windows.Ink.Stroke(dashPoints);
                                        stroke.DrawingAttributes = da.Clone();
                                        strokes.Add(stroke);
                                    }
                                    
                                    isDrawingDash = !isDrawingDash;
                                    currentLen = 0;
                                    dashPoints = new System.Windows.Input.StylusPointCollection();
                                    if (isDrawingDash) dashPoints.Add(new System.Windows.Input.StylusPoint(currentP.X, currentP.Y));
                                }
                            }
                        }
                        if (dashPoints.Count > 1)
                        {
                            var stroke = new System.Windows.Ink.Stroke(dashPoints);
                            stroke.DrawingAttributes = da.Clone();
                            strokes.Add(stroke);
                        }
                    }
                    else
                    {
                        var stroke = new System.Windows.Ink.Stroke(points);
                        stroke.DrawingAttributes = da.Clone();
                        stroke.DrawingAttributes.FitToCurve = true;
                        strokes.Add(stroke);
                    }
                }
            }
            return strokes;
        }

        private static bool isDrawingDash = true;
    }
}
