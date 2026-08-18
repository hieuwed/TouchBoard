using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TouchBoard.Controls;
using TouchBoard.Models;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace TouchBoard.Managers
{
    public class SaveLoadManager
    {
        private readonly Canvas _mainContainer;
        private readonly InkCanvas _inkCanvas;
        private readonly PageManager _pageManager;

        private DispatcherTimer? _autoSaveTimer;
        private bool _hasUnsavedChanges = false;

        /// <summary>Đường dẫn file đang mở. Null = chưa lưu lần nào (dự án mới).</summary>
        public string? CurrentFilePath { get; private set; } = null;

        /// <summary>True nếu đang làm việc với dự án mới chưa lưu lần nào.</summary>
        public bool IsNewProject => CurrentFilePath == null;

        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "TouchBoard");
        private static readonly string AutoSavePath = Path.Combine(AppDataDir, "AutoSave", "current_session.tbproj");

        public SaveLoadManager(Canvas mainContainer, InkCanvas inkCanvas, PageManager pageManager)
        {
            _mainContainer = mainContainer;
            _inkCanvas = inkCanvas;
            _pageManager = pageManager;

            // Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(Path.Combine(AppDataDir, "Projects"));
            Directory.CreateDirectory(Path.Combine(AppDataDir, "AutoSave"));

            // AutoSave mỗi 30s nếu có thay đổi
            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoSaveTimer.Tick += (s, e) => { if (_hasUnsavedChanges) AutoSave(); };
            _autoSaveTimer.Start();

            // Đánh dấu thay đổi khi có nét vẽ mới
            _inkCanvas.StrokeCollected += (s, e) => _hasUnsavedChanges = true;
            _inkCanvas.StrokeErased += (s, e) => _hasUnsavedChanges = true;
        }

        // ==========================================
        // AUTO SAVE
        // ==========================================
        public void AutoSave()
        {
            try
            {
                SaveProjectInternal(AutoSavePath);
                _hasUnsavedChanges = false;
            }
            catch { /* AutoSave thất bại thì bỏ qua */ }
        }

        public bool HasPendingAutoSave() => File.Exists(AutoSavePath);

        public void DeleteAutoSave()
        {
            try { if (File.Exists(AutoSavePath)) File.Delete(AutoSavePath); } catch { }
        }

        // ==========================================
        // 1. SAVE PROJECT
        // ==========================================
        /// <summary>
        /// Lưu vào file chỉ định. Ghi nhớ đường dẫn làm CurrentFilePath.
        /// </summary>
        public void SaveProject(string filePath)
        {
            SaveProjectInternal(filePath);
            CurrentFilePath = filePath;        // Ghi nhớ để lần sau Ctrl+S lưu thẳng vào đây
            DeleteAutoSave();
            _hasUnsavedChanges = false;
        }

        /// <summary>
        /// Lưu nhanh vào CurrentFilePath (Ctrl+S). Trả về false nếu chưa có file → cần gọi SaveProject.
        /// </summary>
        public bool QuickSave()
        {
            if (CurrentFilePath == null) return false;
            SaveProject(CurrentFilePath);
            return true;
        }

        private void SaveProjectInternal(string filePath)
        {
            // Flush nét vẽ trang hiện tại vào PageModel trước khi lưu
            if (_pageManager.Pages.Count > 0)
            {
                _pageManager.Pages[_pageManager.CurrentPageIndex].Strokes =
                    _inkCanvas.Strokes.Clone();
            }

            // Thu thập STEM Tools trên canvas hiện tại
            var currentTools = new List<string>();
            foreach (UIElement child in _mainContainer.Children)
            {
                if (child is ISerializableTool tool)
                    currentTools.Add(tool.Serialize());
            }

            var project = new ProjectDto { Pages = new List<PageDto>() };
            for (int i = 0; i < _pageManager.Pages.Count; i++)
            {
                var page = _pageManager.Pages[i];
                var dto = new PageDto
                {
                    Id = page.Id.ToString(),
                    Title = page.Title,
                    BackgroundTheme = page.Theme.ToString(),
                    BackgroundPattern = page.Pattern.ToString(),
                    Width = _mainContainer.ActualWidth,
                    Height = _mainContainer.ActualHeight,
                    // Lưu nét vẽ theo ISF format → Base64
                    StrokeData = StrokesToBase64(page.Strokes),
                    // Chỉ trang hiện tại mới có Tools trên canvas
                    ToolData = (i == _pageManager.CurrentPageIndex) ? currentTools : new List<string>()
                };
                project.Pages.Add(dto);
            }

            string json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // ==========================================
        // 2. LOAD PROJECT
        // ==========================================
        public void LoadProject(string filePath)
        {
            if (!File.Exists(filePath)) return;

            string json = File.ReadAllText(filePath);
            var project = JsonSerializer.Deserialize<ProjectDto>(json);
            if (project == null || project.Pages.Count == 0) return;

            // Ghi nhớ đường dẫn file vừa mở → Ctrl+S sẽ lưu thẳng vào đây
            // Ngoại trừ AutoSave (không coi đó là "file hiện tại")
            if (filePath != AutoSavePath)
                CurrentFilePath = filePath;

            // Xoá Tools STEM cũ trên canvas
            for (int j = _mainContainer.Children.Count - 1; j >= 0; j--)
            {
                if (_mainContainer.Children[j] is ISerializableTool)
                    _mainContainer.Children.RemoveAt(j);
            }

            // Xoá tất cả trang cũ
            _pageManager.Pages.Clear();

            for (int i = 0; i < project.Pages.Count; i++)
            {
                var dto = project.Pages[i];

                Enum.TryParse(dto.BackgroundPattern, out BackgroundPattern pattern);
                Enum.TryParse(dto.BackgroundTheme, out BackgroundTheme theme);

                var page = new PageModel
                {
                    Id = Guid.TryParse(dto.Id, out var gid) ? gid : Guid.NewGuid(),
                    Title = dto.Title,
                    Pattern = pattern,
                    Theme = theme,
                    // Khôi phục nét vẽ từ Base64 → ISF stream
                    Strokes = Base64ToStrokes(dto.StrokeData)
                };

                _pageManager.Pages.Add(page);

                // Khôi phục STEM Tools (chỉ trang đầu tiên khi load xong rồi switch)
                if (i == 0 && dto.ToolData?.Count > 0)
                {
                    RestoreTools(dto.ToolData);
                }
            }

            // Chuyển về trang đầu tiên — nhưng phải bảo vệ strokes vừa nạp từ file
            // vì SwitchToPage(0) sẽ ghi đè Pages[0].Strokes bằng canvas hiện tại (trắng)
            if (_pageManager.Pages.Count > 0)
            {
                // Bước 1: Lưu lại strokes đúng từ file trước khi SwitchToPage làm hỏng
                var firstStrokes = _pageManager.Pages[0].Strokes?.Clone() ?? new StrokeCollection();

                // Bước 2: SwitchToPage sẽ apply background, fire events — nhưng sẽ ghi đè Pages[0]
                _pageManager.SwitchToPage(0);

                // Bước 3: Khôi phục lại strokes đúng từ file vào cả Pages[0] lẫn InkCanvas
                _pageManager.Pages[0].Strokes = firstStrokes;
                _inkCanvas.Strokes = firstStrokes.Clone();
            }

            _hasUnsavedChanges = false;
        }

        // ==========================================
        // 3. EXPORT TO PDF (dùng PdfSharp thuần — KHÔNG dùng MigraDoc)
        // ==========================================
        public void ExportToPdf(string filePath, int[]? pageIndices = null)
        {
            if (_pageManager.Pages.Count == 0) return;

            if (pageIndices == null || pageIndices.Length == 0)
            {
                pageIndices = new int[_pageManager.Pages.Count];
                for (int i = 0; i < _pageManager.Pages.Count; i++) pageIndices[i] = i;
            }

            var doc = new PdfDocument();
            int originalIndex = _pageManager.CurrentPageIndex;
            var tempFiles = new List<string>();

            try
            {
                foreach (int idx in pageIndices)
                {
                    if (idx < 0 || idx >= _pageManager.Pages.Count) continue;

                    // Chuyển sang trang cần xuất để InkCanvas hiển thị đúng nét vẽ
                    if (_pageManager.CurrentPageIndex != idx)
                    {
                        _pageManager.SwitchToPage(idx);
                        _mainContainer.UpdateLayout();
                    }

                    // Render WPF Canvas → file PNG tạm (dùng WPF encoder thuần, không cần System.Drawing)
                    string tmpFile = Path.Combine(Path.GetTempPath(), $"tb_export_{Guid.NewGuid()}.png");
                    RenderCanvasToPngFile(_mainContainer, tmpFile);
                    tempFiles.Add(tmpFile);

                    // Thêm trang PDF A4 ngang và vẽ ảnh vào toàn trang
                    var pdfPage = doc.AddPage();
                    pdfPage.Width  = XUnit.FromPoint(842); // A4 Landscape: 297mm × 210mm
                    pdfPage.Height = XUnit.FromPoint(595);

                    using var gfx = XGraphics.FromPdfPage(pdfPage);
                    using var xImg = XImage.FromFile(tmpFile);
                    // Dùng .Point thay vì implicit cast (tránh warning CS0618)
                    gfx.DrawImage(xImg, 0, 0, pdfPage.Width.Point, pdfPage.Height.Point);
                }

                doc.Save(filePath);
            }
            finally
            {
                // Khôi phục trang gốc
                if (_pageManager.CurrentPageIndex != originalIndex)
                    _pageManager.SwitchToPage(originalIndex);

                // Dọn file tạm
                foreach (var f in tempFiles)
                    try { File.Delete(f); } catch { }
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================

        /// <summary>Lưu StrokeCollection → ISF binary → Base64 string</summary>
        private static string StrokesToBase64(StrokeCollection strokes)
        {
            if (strokes == null || strokes.Count == 0) return string.Empty;
            using var ms = new MemoryStream();
            strokes.Save(ms); // WPF ISF format — cực nhanh và nhỏ
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>Khôi phục StrokeCollection từ Base64 string → ISF binary</summary>
        private static StrokeCollection Base64ToStrokes(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return new StrokeCollection();
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(bytes);
                return new StrokeCollection(ms);
            }
            catch { return new StrokeCollection(); }
        }

        /// <summary>Render WPF Canvas thành file PNG — KHÔNG dùng System.Drawing</summary>
        private static void RenderCanvasToPngFile(FrameworkElement element, string outputPath)
        {
            int width  = (int)Math.Max(element.ActualWidth,  1920);
            int height = (int)Math.Max(element.ActualHeight, 1080);

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var fs = File.Create(outputPath);
            encoder.Save(fs);
        }

        /// <summary>Khôi phục các STEM Tools từ JSON string list</summary>
        private void RestoreTools(List<string> toolDataList)
        {
            foreach (string toolJson in toolDataList)
            {
                try
                {
                    var root = JsonDocument.Parse(toolJson).RootElement;
                    var typeName = root.GetProperty("Type").GetString();
                    if (string.IsNullOrEmpty(typeName)) continue;

                    var type = Type.GetType(typeName);
                    if (type == null) continue;

                    if (Activator.CreateInstance(type) is not UserControl toolCtrl) continue;
                    if (toolCtrl is not ISerializableTool serializable) continue;

                    serializable.Deserialize(toolJson);

                    if (toolCtrl is StemToolBase stemTool)
                    {
                        double x = Canvas.GetLeft(toolCtrl); if (double.IsNaN(x)) x = 100;
                        double y = Canvas.GetTop(toolCtrl);  if (double.IsNaN(y)) y = 100;

                        // Gọi Initialize nếu có, nếu không thì Add thẳng
                        var initMethod = type.GetMethod("Initialize",
                            new[] { typeof(Canvas), typeof(Point) });
                        if (initMethod != null)
                            initMethod.Invoke(stemTool, new object[] { _mainContainer, new Point(x, y) });
                        else
                            _mainContainer.Children.Add(toolCtrl);
                    }
                    else
                    {
                        _mainContainer.Children.Add(toolCtrl);
                    }
                }
                catch { /* bỏ qua tool lỗi */ }
            }
        }
    }
}
