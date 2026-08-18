---
name: touchboard-save-export
description: Quy chuẩn triển khai Lưu/Mở dự án (.tbproj), AutoSave và Xuất PDF cho TouchBoard. Tham khảo WPF InkCanvas ISF format, kiến trúc DrawingStorageService và chuẩn UX Umind/IPro.
---

# 📦 Skill: Lưu trữ, Mở và Xuất File (Save · Load · Auto-Save · PDF Export)

## 1. Nguyên tắc lưu trữ cốt lõi (Core Storage Principle)

### 1.1 Lưu nét vẽ (Strokes)
**QUAN TRỌNG:** WPF `InkCanvas` có thể lưu/khôi phục `StrokeCollection` bằng định dạng **ISF (Ink Serialized Format)** cực nhỏ và nhanh. Đây là phương pháp tối ưu nhất, **KHÔNG** chuyển thành ảnh để lưu.

```csharp
// LƯU — lưu trực tiếp từ InkCanvas vào MemoryStream rồi Base64 hóa vào JSON
using var ms = new MemoryStream();
_inkCanvas.Strokes.Save(ms);   // định dạng ISF nhị phân
string base64 = Convert.ToBase64String(ms.ToArray());

// MỞ — khôi phục từ Base64 → MemoryStream → StrokeCollection
byte[] bytes = Convert.FromBase64String(base64);
using var ms = new MemoryStream(bytes);
page.Strokes = new StrokeCollection(ms);
_inkCanvas.Strokes = page.Strokes.Clone();
```

### 1.2 Format file dự án (.tbproj)
File là JSON, bên trong mỗi trang chứa:
- `Id`, `Title`, `BackgroundTheme`, `BackgroundPattern`
- `StrokeData` (string Base64 của ISF binary)
- `ToolData` (List<string> JSON — mỗi string là dữ liệu 1 STEM Tool)

Cấu trúc thư mục lưu trữ mặc định:
```
%UserProfile%\Documents\TouchBoard\
├── Projects\          ← file .tbproj do người dùng lưu
└── AutoSave\          ← file tự lưu ngầm (current_session.tbproj)
```

---

## 2. AutoSave (Tự động lưu ngầm)

AutoSave chạy sau mỗi **30 giây** kể từ lần thay đổi cuối, lưu vào file cố định:
`%UserProfile%\Documents\TouchBoard\AutoSave\current_session.tbproj`

**Khi khởi động app:**
```csharp
string autoSavePath = GetAutoSavePath();
if (File.Exists(autoSavePath))
{
    // Hỏi người dùng có muốn khôi phục không
    var result = MessageBox.Show(
        "Có bản vẽ chưa được lưu. Bạn có muốn khôi phục không?",
        "Khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
    if (result == MessageBoxResult.Yes)
    {
        _saveLoadManager.LoadProject(autoSavePath);
    }
}
```

**Kích hoạt AutoSave (DispatcherTimer):**
```csharp
// Khởi tạo trong MainWindow.cs
_autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
_autoSaveTimer.Tick += (s, e) => _saveLoadManager.AutoSave();

// Reset timer mỗi khi có nét vẽ mới
_inkCanvas.StrokeCollected += (s, e) => ResetAutoSaveTimer();
```

---

## 3. Quy trình Load Project — Tránh bẫy SwitchToPage Overwrite

### ⚠️ BẪY THƯỜNG GẶP
`PageManager.SwitchToPage(index)` luôn **lưu canvas hiện tại vào `Pages[CurrentPageIndex]`** trước khi tải trang mới.

Nếu sau `Pages.Clear()` ta thêm lại pages từ file rồi gọi `SwitchToPage(0)`:
- `CurrentPageIndex == 0`, `Pages[0]` vừa được nạp từ file
- Bước "save current" sẽ ghi đè `Pages[0].Strokes` bằng **canvas trắng hiện tại**
- Kết quả: dữ liệu vừa nạp bị xóa hoàn toàn → người dùng thấy bảng trắng!

**Cách fix bắt buộc:**
```csharp
// Sau khi Pages được thêm đủ từ file:

// 1. Lưu strokes trang đầu trước khi SwitchToPage ghi đè
var firstStrokes = _pageManager.Pages[0].Strokes?.Clone() ?? new StrokeCollection();

// 2. Gọi SwitchToPage (nó sẽ ghi đè Pages[0] bằng canvas trắng)
_pageManager.SwitchToPage(0);

// 3. Khôi phục lại đúng strokes từ file
_pageManager.Pages[0].Strokes = firstStrokes;
_inkCanvas.Strokes = firstStrokes.Clone();
```

### Hành vi khi khởi động (Startup Behavior)

| Tình huống | Hành vi đúng |
|---|---|
| Lần đầu mở app | Bảng trắng — 1 trang mặc định |
| Có AutoSave, người dùng chọn "Có" | Load AutoSave → thay thế toàn bộ nội dung |
| Có AutoSave, người dùng chọn "Không" | Xóa AutoSave, giữ bảng trắng |
| Người dùng bấm "Mở dự án" | Load file → thay thế toàn bộ nội dung |

> **KHÔNG** tự động load file khi khởi động mà không có sự đồng ý của người dùng.

### Pattern Save vs Save As vs QuickSave

```
SaveLoadManager.CurrentFilePath = null   → Dự án mới chưa lưu (IsNewProject = true)
SaveLoadManager.CurrentFilePath = "..."  → Đang làm việc với file đã có
```

| Hành động | Logic |
|---|---|
| 💾 Lưu dự án (Ctrl+S) | `IsNewProject` → hiện dialog \| `HasFile` → `QuickSave()` vào file cũ |
| 📋 Lưu thành... | Luôn hiện dialog → lưu sang file mới |
| 📂 Mở dự án | Load → set `CurrentFilePath` |
| 📄 Xuất PDF | Tên mặc định = `Path.GetFileNameWithoutExtension(CurrentFilePath)` |

> **Khi xuất PDF**: tên file mặc định phải giống tên dự án đang mở, không dùng timestamp.  
> `AutoSavePath` **KHÔNG** được set làm `CurrentFilePath` — tránh việc lưu đè AutoSave.

---

## 4. UI/UX theo chuẩn Umind/IPro

- **Vị trí:** 3 tính năng (Lưu, Mở, Xuất PDF) nằm trong **Popup ⚙️ Cài đặt** ở cuối Toolbar nổi — **KHÔNG** để ngoài Toolbar chính để giữ không gian vẽ thoáng.
- **Popup ⚙️ menu items:**
  - 💾 Lưu dự án (`Ctrl+S`)
  - 📂 Mở dự án (`Ctrl+O`)
  - ───── (Separator)
  - 📄 Xuất PDF
- **Feedback:** Hiển thị `MessageBox` sau mỗi thao tác thành công. Khi xuất PDF, dùng `Mouse.OverrideCursor = Cursors.Wait` để báo hiệu đang xử lý.
- **Dialog Xuất PDF:** Cung cấp 3 tuỳ chọn: Tất cả trang / Trang hiện tại / Tùy chỉnh (ví dụ `1, 3, 5-7`).

### 📌 UX quan trọng: Bàn phím ảo (Touch Keyboard) khi nhập tên file

**Vấn đề:** Phần mềm chạy trên màn hình cảm ứng (IR Touchscreen, Smart Board). Khi người dùng cần nhập tên file để Lưu/Xuất PDF, `SaveFileDialog` của Windows không đảm bảo hiện bàn phím ảo tự động vì nó là dialog hệ thống.

**Phân tích các lựa chọn:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| Dùng `SaveFileDialog` của Windows | Quen thuộc, có explorer duyệt thư mục | Không chắc bàn phím ảo tự hiện trên Smart Board |
| Dialog tự thiết kế (WPF) | Kiểm soát hoàn toàn, to/đẹp cho cảm ứng | Không có explorer duyệt thư mục |
| Tự động đặt tên + cho đổi tên sau | Đơn giản nhất cho cảm ứng | Người dùng phải vào hệ thống để đổi tên |

**Khuyến nghị cho TouchBoard:**

✅ **Sử dụng dialog tự thiết kế (WPF)** hiển thị:
- Ô `TextBox` lớn để nhập tên file (font size ≥ 18)
- `InputScope` đặt thành `Text` để Windows tự kích hoạt bàn phím ảo đúng cách
- Nút "Bàn phím" (nếu muốn) để gọi `TabTip.exe` thủ công
- Nút "Đồng ý" / "Hủy" kích thước lớn, thân thiện cảm ứng (tối thiểu 60px height)

**Code kích hoạt bàn phím ảo thủ công (nếu cần):**
```csharp
// Gọi bàn phím cảm ứng Windows (TabTip) nếu không tự hiện
private void InvokeTouchKeyboard()
{
    string tabTipPath = @"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe";
    if (File.Exists(tabTipPath))
        System.Diagnostics.Process.Start(tabTipPath);
}

// Hoặc dùng InputScope trong XAML để Windows tự trigger bàn phím:
// <TextBox InputScope="Text" FontSize="20" Height="50" />
```

**Cấu trúc dialog lưu file cho màn hình cảm ứng:**
```
┌────────────────────────────────────────┐
│  💾 Lưu dự án                          │
│                                        │
│  Tên file:                             │
│  ┌──────────────────────────────────┐  │
│  │  Bai_giang_Toan_01              │  │  ← TextBox lớn, InputScope=Text
│  └──────────────────────────────────┘  │
│                                        │
│  Thư mục lưu: Documents/TouchBoard    │
│                                        │
│     [  Hủy bỏ  ]   [  💾 Lưu  ]      │  ← Nút cao ≥60px
└────────────────────────────────────────┘
```

File sẽ được lưu vào `%Documents%\TouchBoard\Projects\<tên file>.tbproj` mà không cần dialog duyệt thư mục phức tạp.

---

## 4. Xuất PDF (PDF Export) — Dùng PdfSharp thuần

**KHÔNG dùng MigraDoc** (gây lỗi Font Resolver khi không có Global Font Resolver trong .NET 8).
Dùng **`PdfSharp.Drawing`** vẽ thẳng ảnh vào trang PDF:

```csharp
// Quy trình
var doc = new PdfDocument();
foreach (var pageIndex in selectedPages)
{
    // 1. Switch sang trang đó và UpdateLayout
    _pageManager.SwitchToPage(pageIndex);
    canvas.UpdateLayout();

    // 2. Render WPF canvas thành bitmap (trên UI Thread)
    var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
    rtb.Render(canvas);

    // 3. Lưu bitmap ra file tạm (.png)
    string tempFile = Path.GetTempFileName() + ".png";
    var encoder = new PngBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(rtb));
    using (var fs = File.Create(tempFile)) encoder.Save(fs);

    // 4. Thêm trang PDF kích thước A4 ngang và vẽ ảnh vào
    var pdfPage = doc.AddPage();
    pdfPage.Width  = XUnit.FromPoint(842);  // A4 Landscape
    pdfPage.Height = XUnit.FromPoint(595);
    using var gfx = XGraphics.FromPdfPage(pdfPage);
    using var xImg = XImage.FromFile(tempFile);
    gfx.DrawImage(xImg, 0, 0, pdfPage.Width.Point, pdfPage.Height.Point);

    // 5. Dọn file tạm
    File.Delete(tempFile);
}
doc.Save(filePath);
```

---

## 5. Giao diện ISerializableTool (cho STEM Tools)

```csharp
public interface ISerializableTool
{
    string Serialize();          // Trả về JSON string chứa trạng thái
    void Deserialize(string json); // Phục hồi trạng thái từ JSON string
}
```

**Lưu ý:** `System.Text.Json` không serialize được `object` / `Anonymous Type` khi đọc lại. Luôn ép kiểu và serialize thành `string` trước khi đặt vào `ToolData`.

---

## 6. Ràng buộc & Bẫy kỹ thuật

| Vấn đề | Giải pháp |
|---|---|
| `PdfSharpCore` báo lỗi ImageSource | Dùng `PDFsharp-MigraDoc` hoặc `PdfSharp` (gói chính thức Windows) |
| `MigraDoc` báo "Font 'Courier New' cannot be resolved" | **Không dùng MigraDoc** — dùng `XGraphics.DrawImage()` thuần |
| `XUnit` cảnh báo implicit cast obsolete | Dùng `.Point` property: `pdfPage.Width.Point` |
| `RenderTargetBitmap` phải chạy trên UI Thread | Gọi từ Click handler hoặc `Dispatcher.Invoke` |
| `System.Text.Json` serialize `Anonymous Type` thành `{}` rỗng | Serialize Tool ra `string` trước, không dùng `List<object>` |
| AutoSave có thể block UI | Dùng `DispatcherTimer` (chạy trên UI Thread nhưng không block) hoặc `Task.Run` + `Dispatcher.Invoke` cho IO |
