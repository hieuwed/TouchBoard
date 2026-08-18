using System;
using System.Collections.Generic;
using System.Windows;
using TouchBoard.Controls;

namespace TouchBoard.Managers
{
    public class StemManager
    {
        private List<IEdgeSnappable> _activeTools = new List<IEdgeSnappable>();
        private const double SNAP_THRESHOLD = 20.0;

        public void RegisterTool(IEdgeSnappable tool)
        {
            if (!_activeTools.Contains(tool))
            {
                _activeTools.Add(tool);
            }
        }

        public void UnregisterTool(IEdgeSnappable tool)
        {
            _activeTools.Remove(tool);
        }

        public Point GetSnappedPointAndEdge(Point p, out bool isSnapped, out (Point, Point) snappedEdge)
        {
            isSnapped = false;
            snappedEdge = (new Point(), new Point());
            Point bestPoint = p;
            double minDistance = SNAP_THRESHOLD;

            foreach (var tool in _activeTools)
            {
                foreach (var edge in tool.GetSnappingEdges())
                {
                    Point projected = ProjectPointOnLineSegment(edge.P1, edge.P2, p);
                    double dist = (projected - p).Length;

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestPoint = projected;
                        isSnapped = true;
                        snappedEdge = edge;
                    }
                }
            }

            return bestPoint;
        }

        public static Point ProjectPointOnLineSegment(Point p1, Point p2, Point p)
        {
            Vector lineVec = p2 - p1;
            Vector pointVec = p - p1;

            double lineLengthSquared = lineVec.LengthSquared;
            if (lineLengthSquared == 0) return p1;

            double t = Vector.Multiply(pointVec, lineVec) / lineLengthSquared;
            
            // Nếu muốn giới hạn không cho vẽ vượt quá chiều dài thước, bỏ comment 2 dòng dưới:
            // if (t < 0) return p1;
            // if (t > 1) return p2;

            return p1 + t * lineVec;
        }
    }
}
