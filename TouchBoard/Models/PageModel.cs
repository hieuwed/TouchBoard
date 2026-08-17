using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using TouchBoard.Managers;

namespace TouchBoard.Models
{
    public class PageModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "Trang 1";

        /// <summary>
        /// Loại kẻ: Plain (trống), Grid (ô ly), Ruled (kẻ ngang).
        /// </summary>
        public BackgroundPattern Pattern { get; set; } = BackgroundPattern.Plain;

        /// <summary>
        /// Màu nền: Dark, Light, Blackboard.
        /// </summary>
        public BackgroundTheme Theme { get; set; } = BackgroundTheme.Dark;
        
        // Dữ liệu nội dung của trang
        public StrokeCollection Strokes { get; set; } = new StrokeCollection();
        
        // Lịch sử Undo/Redo riêng của trang
        public Stack<byte[]> UndoStack { get; set; } = new Stack<byte[]>();
        public Stack<byte[]> RedoStack { get; set; } = new Stack<byte[]>();

        public PageModel()
        {
        }
    }
}
