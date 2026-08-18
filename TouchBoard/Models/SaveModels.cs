using System.Collections.Generic;

namespace TouchBoard.Models
{
    public class ProjectDto
    {
        public List<PageDto> Pages { get; set; } = new List<PageDto>();
    }

    public class PageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string BackgroundTheme { get; set; } = string.Empty;
        public string BackgroundPattern { get; set; } = string.Empty;
        public string StrokeData { get; set; } = string.Empty; // Strokes lưu dưới dạng Base64
        public List<string> ToolData { get; set; } = new List<string>(); // Dữ liệu các công cụ
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
