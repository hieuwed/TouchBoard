---
name: touchboard-pages
description: >-
  Skill chuyên biệt về Quản lý Đa Trang (Page Management) và Canvas Vô hạn trong TouchBoard.
  Kích hoạt khi người dùng hỏi về: tạo/xóa trang, đổi vị trí trang bằng kéo thả (drag & drop),
  xử lý hiển thị Thumbnail Preview của trang, đổi nền/loại bảng, hoặc xử lý Undo/Redo độc lập trên từng trang.
---

# TouchBoard — Quản Lý Trang & Canvas Vô Hạn

Đây là "runbook" chuyên sâu cho hệ thống Đa Trang (Multi-page) và Canvas vô hạn của dự án TouchBoard. Đọc kỹ trước khi chỉnh sửa bất kỳ module nào liên quan đến trang hoặc vùng vẽ.

---

## 1. Kiến trúc Đa Trang (Page Architecture)

Hệ thống trang được tách bạch rõ ràng giữa Model, Manager và UI:
- **`Models/PageModel.cs`**: Lưu trữ dữ liệu của mỗi trang, bao gồm: Tiêu đề, **`Pattern`** (loại kẻ: Plain/Grid/Ruled), **`Theme`** (màu nền: Dark/Light/Blackboard), `StrokeCollection` (nét vẽ), `UndoStack` và `RedoStack` (kiểu `Stack<byte[]>`).
- **`Managers/PageManager.cs`**: Quản lý danh sách `ObservableCollection<PageModel>`. Xử lý thêm, xóa, đổi chỗ (Move), đổi Pattern/Theme, và logic cốt lõi khi chuyển trang (`SwitchToPage`).
- **`Managers/HistoryManager.cs`**: Đã được nâng cấp để sử dụng Stack động. Khi chuyển trang, `PageManager` sẽ cung cấp Undo/Redo Stack của trang tương ứng cho `HistoryManager`.

---

## 2. Hệ thống Nền Trang (Pattern + Theme)

Nền của mỗi trang được tạo bởi **2 thuộc tính độc lập**, cho phép tổ hợp tự do:

### 2a. BackgroundPattern (Loại kẻ)
| Enum | Mô tả |
|------|--------|
| `Plain` | Trống trơn, không có đường kẻ |
| `Grid` | Ô ly (kẻ dọc + ngang, spacing 40px) |
| `Ruled` | Kẻ ngang (chỉ kẻ ngang, spacing 40px) |

### 2b. BackgroundTheme (Màu nền)
| Enum | Nền | Toolbar | Mực |
|------|-----|---------|-----|
| `Dark` | `#1E1E2E` (Catppuccin Mocha) | `#181825` | `#CDD6F4` (trắng xanh) |
| `Light` | `#EFF1F5` | `#DCE0E8` | `#4C4F69` (đen nhạt) |
| `Blackboard` | `#1B3A2D` (xanh bảng đen) | `#142E23` | `#E8E4D9` (trắng phấn) |

### Cách hoạt động (`BackgroundManager.SetBackground(pattern, theme)`)
1. Xác định bảng màu từ `theme` (bg, toolbar, border, ink, gridLine).
2. Gọi `ApplyTheme()` để gán màu nền canvas, toolbar, viền, và màu mực mặc định.
3. Nếu `pattern` là Grid/Ruled → vẽ `DrawingBrush` tiled với màu kẻ riêng của theme đó.

> **Lưu ý:** Mỗi Theme có màu kẻ riêng để luôn đảm bảo độ tương phản. Ví dụ: Theme Dark dùng kẻ `#2A2A3E`, Theme Light dùng kẻ `#CCD0DA`.

---

## 3. Giao diện (UI) và Thumbnail Previews

Danh sách trang nằm trong một Popup (`PagesPopup`) trồi lên từ nút bấm ở thanh Toolbar.

- **Preview Hình Thu Nhỏ (Live Binding):** Dùng `InkPresenter` bọc trong `Viewbox Stretch="Uniform"` Bind trực tiếp vào `Strokes` của `PageModel`.
  - *Lưu ý UI:* Background của Thumbnail được quy định bằng `BackgroundTypeToBrushConverter` dùng `BackgroundManager.GetThemeBgColor(theme)`.
  - `InkPresenter` phải có `IsHitTestVisible="False"` (đặt trên `Viewbox`) để không nuốt sự kiện chuột/touch.

- **Danh sách nằm ngang:** Popup dùng `ListBox` với `VirtualizingStackPanel Orientation="Horizontal"`.

### Tạo Trang Mới (UI)
Popup hiển thị **2 nhóm lựa chọn** riêng biệt:
1. **Loại bảng (Pattern):** 3 nút — Trống / Ô Ly / Kẻ ngang. Bấm chọn highlight bằng `ActiveToolButtonStyle`.
2. **Màu nền (Theme):** 3 nút — Tối / Sáng / Bảng đen.
3. Nút "✓ Tạo trang" gọi `PageManager.AddPage(pattern, theme)`.

### Đổi nền trang đã tạo (Panel-based, giống Tạo trang mới)
- **Nút Đổi nền** (icon `E771`): Góc trên-trái Thumbnail. Click vào sẽ:
  1. Tìm `PageModel` theo `pageId`, đọc `Pattern` và `Theme` hiện tại.
  2. Ẩn `PanelAddPageTypes`, hiện `PanelChangePageBg` — cấu trúc giống hệt panel Tạo trang mới.
  3. Highlight đúng nút Pattern/Theme tương ứng với trang đang chỉnh.
  4. Mỗi lần bấm nút Pattern/Theme → **áp dụng ngay** (`ChangePagePattern` / `ChangePageTheme`).
  5. Nút "✓ Áp dụng" hoặc "✕" → đóng panel.

---

## 4. Chuyển Trang & Hủy Vùng Chọn (Switch Page Flow)

Khi gọi `PageManager.SwitchToPage(int index)`:
1. ⚠️ **BẮT BUỘC:** Gọi `_window.DrawingCanvas.Select(new StrokeCollection())` để XÓA vùng chọn Lasso hiện tại. Nếu quên bước này, Menu Context (dấu ⋯) sẽ bị kẹt lại trên màn hình khi sang trang mới.
2. Lưu `_window.DrawingCanvas.Strokes` hiện tại vào `PageModel.Strokes` của trang cũ.
3. Cập nhật `CurrentPageIndex`.
4. Gọi `_historyManager.SetStacks(...)` để chuyển sang lịch sử Undo/Redo của trang mới.
5. Gán `_window.DrawingCanvas.Strokes = newPage.Strokes.Clone()`. Cần Clone để tránh tham chiếu vòng.
6. Gọi `BackgroundManager.SetBackground(pattern, theme)` để cập nhật nền tương ứng.

---

## 5. Kéo thả Trang kiểu Canva (Drag & Drop)

> **Quy tắc bắt buộc:** Mọi tương tác UI trong Popup trang phải hỗ trợ ĐỒNG THỜI cả **chuột (Mouse)** và **cảm ứng (Touch)**.

### 5a. Sự kiện chuột (Mouse)
- **`PreviewMouseLeftButtonDown`**: Ghi nhận `_dragStartPoint`.
- **`PreviewMouseMove`**: So sánh khoảng cách, nếu vượt ngưỡng → `DragDrop.DoDragDrop()`. Loại trừ click vào Button.
- **`PreviewMouseLeftButtonUp`**: Đóng popup nếu không phải kéo.

### 5b. Sự kiện cảm ứng (Touch)
- **`PreviewTouchDown`**: Ghi nhận `_touchDragStartPoint`, `_touchDragDeviceId`.
- **`PreviewTouchMove`**: So sánh khoảng cách > 15px → `DragDrop.DoDragDrop()`. Set `_touchDragInProgress = true`.
- **`PreviewTouchUp`**: Nếu `!_touchDragInProgress` → đóng popup (= tap nhẹ). Reset trạng thái.

### 5c. Hiển thị Insert Indicator (DragOver/Drop)
- **`DragOver`**: Xác định chuột ở nửa trái/phải của ListBoxItem đích:
  - Nửa trái → `LeftInsertIndicator.Visibility = Visible`
  - Nửa phải → `RightInsertIndicator.Visibility = Visible`
  - Dùng `FindVisualChild<ContentPresenter>` + `FindChildByName<Border>`.
- **`DragLeave`**: `ClearAllInsertIndicators()`.
- **`Drop`**: Tính `newIndex` từ nửa trái/phải + hướng kéo → `PageManager.MovePage()`.

**Cấu trúc DataTemplate mỗi trang:**
```
Grid (bao ngoài)
├── LeftInsertIndicator (Border, Width=3, Collapsed)
├── Main Thumbnail Border (InkPresenter, Title, nút Đổi Nền, nút Xóa)
└── RightInsertIndicator (Border, Width=3, Collapsed)
```

---

## 6. Cơ chế Pan/Zoom trên Canvas Vô Hạn

### 6a. Chuột (Mouse/Laptop)
- **Pan:** Chuột phải / Chuột giữa / `Space` + Chuột trái.
- **Zoom:** `Ctrl` + Lăn chuột.
- **Chuột (Mouse/Laptop):**
  - **Pan (Di chuyển):** Bấm giữ chuột phải, bấm giữ chuột giữa (con lăn), hoặc giữ `Space` + kéo chuột trái.
  - **Zoom (Thu phóng):** Giữ `Ctrl` + lăn chuột.
- **Cảm ứng (Touch) & Xung đột Đa điểm:**
  - **Vấn đề:** Để có thể dùng ngón tay Vuốt/Thu phóng, `IsManipulationEnabled` phải bật. Tuy nhiên nếu bật liên tục, nó sẽ nuốt mất các sự kiện chạm và làm hỏng khả năng vẽ bằng nhiều ngón tay cùng lúc (Multi-touch Pen).
  - **Cách xử lý:** 
    - Trong `ToolManager.cs`, thao tác Pan/Zoom bằng cảm ứng (Manipulation) **chỉ được bật (`true`) khi ở chế độ Chọn Vùng (`ToolMode.Select`)**. Lúc này người dùng có thể dùng 2 ngón tay chụm mở để thu phóng, tránh xung đột với bút viết.
    - Khi ở chế độ Viết (`ToolMode.Pen`) hoặc Tẩy, Manipulation bị **tắt (`false`)** để hệ thống ưu tiên nhận diện 100% các điểm chạm thành nét vẽ (cho phép nhiều học sinh cùng vẽ).
### 6b. Cảm ứng (Touch) — Tự code, KHÔNG dùng IsManipulationEnabled
- **Vấn đề:** `IsManipulationEnabled = true` nuốt sạch Touch 1 ngón → hỏng Lasso chọn vùng và vẽ đa ngón.
- **Giải pháp:**
  - `IsManipulationEnabled = false` **MỌI LÚC**.
  - Tự bắt `TouchDown/Move/Up` trong `NavigationManager.cs`.
  - Khi phát hiện **≥ 2 ngón tay** (trong chế độ `ToolMode.Select`): tính khoảng cách giữa 2 ngón → `ZoomAt()`, tính trung tâm → Pan.
  - Khi chỉ có 1 ngón → bỏ qua, nhường cho InkCanvas xử lý (vẽ/chọn).
