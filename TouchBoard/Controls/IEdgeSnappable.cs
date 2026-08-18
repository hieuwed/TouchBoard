using System.Collections.Generic;
using System.Windows;

namespace TouchBoard.Controls
{
    public interface IEdgeSnappable
    {
        /// <summary>
        /// Trả về danh sách các đoạn thẳng (cạnh thước) trong không gian tọa độ của Canvas cha.
        /// Tuple chứa: (Điểm đầu, Điểm cuối).
        /// </summary>
        IEnumerable<(Point P1, Point P2)> GetSnappingEdges();
    }
}
