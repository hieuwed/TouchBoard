using System;

namespace TouchBoard.Controls
{
    /// <summary>
    /// Giao diện cho phép các công cụ có thể được lưu trữ và khôi phục trạng thái.
    /// </summary>
    public interface ISerializableTool
    {
        /// <summary>
        /// Trả về chuỗi JSON chứa trạng thái hiện tại của công cụ.
        /// </summary>
        string Serialize();

        /// <summary>
        /// Phục hồi trạng thái công cụ từ chuỗi JSON.
        /// </summary>
        void Deserialize(string json);
    }
}
