---
name: touchboard-pen-selection
description: >-
  Skill chuyên biệt cho giao diện bút vẽ (Pen UI) và hệ thống chọn vùng (Selection)
  của TouchBoard. Kích hoạt khi người dùng yêu cầu: điều chỉnh giao diện bút vẽ,
  thay đổi cách chọn màu/kích thước nét, thêm context menu (⋯) khi chọn vùng,
  sửa lỗi tương tác vùng chọn, hoặc bất kỳ thay đổi nào liên quan đến Pen UI
  và Selection trên DrawingCanvas.
---

# TouchBoard — Pen UI & Selection Context Menu

Đây là "runbook" chuyên sâu cho giao diện Bút vẽ và hệ thống Chọn vùng của TouchBoard.
Đọc kỹ trước khi chỉnh sửa bất kỳ dòng code nào.

> **🚨 RÀNG BUỘC QUAN TRỌNG:** KHÔNG ĐƯỢC tự động sửa đổi code hay file cấu hình.
> Bạn PHẢI giải thích rõ phương án, hiển thị code dự kiến thay đổi và yêu cầu
> người dùng REVIEW, xác nhận trước khi thực hiện bất kỳ hành động ghi file hay sửa đổi nào.

---

## 1. Kiến trúc dự án (Project Architecture)

### 1.1. Vị trí file

| File | Đường dẫn | Vai trò |
|---|---|---|
| XAML Layout | `TouchBoard/MainWindow.xaml` | Giao diện chính: InkCanvas, floating toolbar, status bar |
| Code-behind | `TouchBoard/MainWindow.xaml.cs` | Khởi tạo Manager, delegate event handler |
| Toolbar Styles | `TouchBoard/Styles/ToolbarStyles.xaml` | ResourceDictionary: ToolButtonStyle, ColorSwatchStyle, ComboBox styles |
| ToolManager | `TouchBoard/Managers/ToolManager.cs` | Quản lý chuyển mode (Pen/Select/EraserStroke/EraserPoint), cập nhật UI |
| ColorManager | `TouchBoard/Managers/ColorManager.cs` | Quản lý bảng màu, đồng bộ màu toolbar ↔ selected strokes |
| StrokeWidthManager | `TouchBoard/Managers/StrokeWidthManager.cs` | Quản lý độ dày nét (3/6/12px), đồng bộ với selected strokes |
| SelectionManager | `TouchBoard/Managers/SelectionManager.cs` | Xử lý SelectionChanged event, xóa nét đã chọn |
| HistoryManager | `TouchBoard/Managers/HistoryManager.cs` | Undo/Redo stack dùng ISF serialize |
| CanvasManager | `TouchBoard/Managers/CanvasManager.cs` | Clear canvas, toggle fullscreen |
| BackgroundManager | `TouchBoard/Managers/BackgroundManager.cs` | Chuyển đổi nền (Dark/White/Blackboard/Grid/Ruled) |
| ShortcutManager | `TouchBoard/Managers/ShortcutManager.cs` | Phím tắt (P/S/E/R/Delete/Ctrl+Z/Y/F11/Esc) |

### 1.2. Cấu trúc XAML — Canvas Layers (từ dưới lên)

```text
Grid (root)
├── InkCanvas (DrawingCanvas) — Bảng vẽ chính, EditingMode = Ink | Select | Erase
├── Border (Status Bar) — Hiển thị mode icon + text (góc trái trên, ZIndex=10)
└── Border (ToolbarBorder, ZIndex=20) — Floating toolbar dock, bottom-center
    └── WrapPanel
        ├── StackPanel [Mode Toggle: Pen / Select / EraserStroke / EraserPoint]
        ├── Separator
        ├── StackPanel [PanelColors: 6 nút tròn ColorSwatch]
        ├── Separator
        ├── StackPanel [PanelStrokeWidth: 3 nút S/M/L]
        ├── Separator
        ├── StackPanel [Actions: Undo / Redo / Delete / ClearAll / Fullscreen]
        ├── Separator
        └── ComboBox [CmbBackground: Dark/White/Blackboard/Grid/Ruled]
```

---

## 2. Hệ thống công cụ vẽ (Tool Modes)

`ToolManager.SwitchToMode(ToolMode mode)` xử lý:

| Mode | EditingMode | PanelColors | PanelStrokeWidth | Delete Button |
|---|---|---|---|---|
| `Pen` | `Ink` | ✅ Visible | ✅ Visible | ❌ Disabled |
| `Select` | `Select` | ✅ Visible | ✅ Visible | ✅ Enabled (nếu có selection) |
| `EraserStroke` | `EraseByStroke` | ❌ Hidden | ❌ Hidden | ❌ Disabled |
| `EraserPoint` | `EraseByPoint` | ❌ Hidden | ✅ Visible | ❌ Disabled |

> ⚠️ **Lưu ý:** Khi ở `Select` mode, PanelColors và PanelStrokeWidth vẫn hiển thị
> để cho phép thay đổi thuộc tính nét vẽ đã chọn (ColorManager.HandleColorClick
> và StrokeWidthManager.HandleStrokeWidthClick đều kiểm tra `GetSelectedStrokes()`).

---

## 3. Giao diện bút vẽ (Pen UI)

### 3.1. Color Palette (PanelColors)

6 nút tròn sử dụng `ColorSwatchStyle` / `ActiveColorSwatchStyle`:
- `#2F3542` (Đen) — Active mặc định
- `#F38BA8` (Đỏ hồng)
- `#A6E3A1` (Xanh lá)
- `#89B4FA` (Xanh dương)
- `#F9E2AF` (Vàng)
- `#FAB387` (Cam)

**Luồng xử lý khi click màu** (`ColorManager.HandleColorClick`):
1. Đổi style nút cũ → `ColorSwatchStyle`, nút mới → `ActiveColorSwatchStyle`
2. Gọi `ApplyDrawingAttributes()` — cập nhật `DefaultDrawingAttributes` của InkCanvas
3. Nếu có `selectedStrokes` → đổi màu nét đã chọn, `SaveState()`
4. Nếu không có selection → tự chuyển sang Pen mode

### 3.2. Stroke Width (PanelStrokeWidth)

3 nút S/M/L với `ToolButtonStyle` / `ActiveToolButtonStyle`:
- **S** (3px) — Nét mảnh
- **M** (6px) — Nét vừa, active mặc định
- **L** (12px) — Nét dày

Dùng Tag để truyền giá trị width.

### 3.3. Style Design (ToolbarStyles.xaml)

- **ToolButtonStyle**: 50×50, CornerRadius 10, Background `#F1F2F6`, hover `#E1E8ED`
- **ActiveToolButtonStyle**: kế thừa ToolButtonStyle, Background `#2E86DE`, Foreground White
- **ColorSwatchStyle**: 36×36, viền Ellipse + SelectionRing, hover ring
- **ActiveColorSwatchStyle**: 40×40, BorderBrush = AccentBrush (`#2E86DE`)
- **DangerButtonStyle**: Background `#EE5A6F`, hover `#FF6B80`

> ⚠️ **Khi điều chỉnh Pen UI:** Tuyệt đối KHÔNG thay đổi logic trong ColorManager,
> StrokeWidthManager, hoặc cách InkCanvas xử lý DrawingAttributes. Chỉ được phép
> thay đổi Style/Layout/Visual trong XAML và ToolbarStyles.xaml.

---

## 4. Selection & Context Menu (⋯)

### 4.1. Hệ thống chọn vùng hiện tại

`SelectionManager` lắng nghe `DrawingCanvas.SelectionChanged`:
- Cập nhật `BtnDeleteSelected` (enabled/disabled)
- Cập nhật Status Bar text ("ĐÃ CHỌN X NÉT VẼ")
- Đồng bộ toolbar (màu, kích thước) với nét vẽ đầu tiên được chọn

WPF `InkCanvas` ở mode `Select` tự động:
- Cho phép kéo lasso hoặc click để chọn nét
- Hiển thị selection adorner (đường nét đứt + 8 handle resize)
- Cho phép kéo di chuyển / resize nét đã chọn

### 4.2. Context Menu — Dấu ba chấm (⋯) khi có Selection

**Mục tiêu:** Khi có vùng chọn (1+ nét vẽ), hiển thị một nút ⋯ (ba chấm ngang)
ngay phía trên-phải của vùng chọn. Click vào nút này sẽ mở popup menu cho phép
thao tác nhanh với các vật thể đã chọn.

**Hướng dẫn triển khai:**

#### 4.2.1. Tạo SelectionContextMenu (XAML + Code)

- **File mới**: `TouchBoard/Controls/SelectionContextButton.xaml` + `.xaml.cs`
  (hoặc xử lý trực tiếp trong `SelectionManager`)
- Nút ⋯ là một `Border` / `Button` nổi trên InkCanvas, position = Canvas.SetLeft/SetTop
- Khi click → hiển thị `Popup` (WPF) hoặc `ContextMenu`

#### 4.2.2. Các mục trong menu gợi ý

| Mục | Icon (Segoe MDL2) | Action |
|---|---|---|
| Đổi màu | `E790` | Mở sub-menu chọn màu cho selected strokes |
| Đổi kích thước nét | `E8AB` | Mở sub-menu chọn S/M/L |
| Sao chép | `E8C8` | Copy selected strokes vào clipboard |
| Dán | `E77F` | Paste strokes từ clipboard |
| Xóa | `E74D` | Xóa selected strokes (gọi `DeleteSelectedStrokes()`) |
| Nhóm | `EE47` | (Tương lai) Nhóm các nét thành 1 đối tượng |

#### 4.2.3. Vị trí hiển thị nút ⋯

Trong `SelectionManager.DrawingCanvas_SelectionChanged`:
1. Khi `hasSelection == true`:
   - Tính bounding rect của selected strokes: `selectedStrokes.GetBounds()`
   - Đặt nút ⋯ tại vị trí `(bounds.Right + 8, bounds.Top - 8)` hoặc `(bounds.Right, bounds.Top - 40)`
   - Show nút ⋯
2. Khi `hasSelection == false`:
   - Hide nút ⋯
   - Đóng popup nếu đang mở

#### 4.2.4. Style nút ⋯ gợi ý

```xml
<Border x:Name="SelectionMenuButton"
        Visibility="Collapsed"
        Background="White"
        BorderBrush="#E1E8ED"
        BorderThickness="1"
        CornerRadius="16"
        Padding="6,2"
        Cursor="Hand"
        Panel.ZIndex="30">
    <Border.Effect>
        <DropShadowEffect Color="#CCCCCC" BlurRadius="6" Opacity="0.2" ShadowDepth="1"/>
    </Border.Effect>
    <TextBlock Text="⋯" FontSize="18" FontWeight="Bold"
               Foreground="#2F3542" HorizontalAlignment="Center"/>
</Border>
```

### 4.3. Các ràng buộc bảo toàn

Khi triển khai context menu, **PHẢI đảm bảo**:

1. ❌ **KHÔNG thay đổi** cách `InkCanvas.EditingMode` hoạt động
2. ❌ **KHÔNG thay đổi** logic `HistoryManager` (Undo/Redo stack)
3. ❌ **KHÔNG thay đổi** cách `DrawingCanvas.StrokeCollected` / `StrokeErased` hoạt động
4. ❌ **KHÔNG thay đổi** logic bên trong `ToolManager.SwitchToMode`
5. ❌ **KHÔNG thay đổi** Background pattern logic (`BackgroundManager`)
6. ✅ **ĐƯỢC PHÉP** thêm UI element mới vào `MainWindow.xaml` (nút ⋯, popup)
7. ✅ **ĐƯỢC PHÉP** mở rộng `SelectionManager` để quản lý context menu
8. ✅ **ĐƯỢC PHÉP** thêm file mới trong `Controls/` hoặc `Managers/`
9. ✅ **ĐƯỢC PHÉP** thêm style mới vào `ToolbarStyles.xaml`

---

## 5. Hệ thống Undo/Redo

- Stack-based ISF serialization trong `HistoryManager`
- Auto-save khi: `StrokeCollected`, `StrokeErased`, `SelectionMoved`, `SelectionResized`
- Manual save khi: `DeleteSelectedStrokes()`, `ClearAll()`
- **Bất kỳ thao tác nào thay đổi strokes từ context menu đều phải gọi `_historyManager.SaveState()`**

---

## 6. Lưu ý thiết kế giao diện

- **Light Theme**: Toolbar nền trắng, icon dạng SVG Path (fill `#2F3542`), accent `#2E86DE`
- **Touch-friendly**: Nút tối thiểu 44×44 pixel, phù hợp màn hình cảm ứng lớn
- **Hover/Press feedback**: Dùng Trigger trong ControlTemplate, KHÔNG gán cứng Background
- **Popup direction**: Popup nên mở lên trên (Placement="Top") vì toolbar ở dưới cùng
- Nút ⋯ context menu nên có animation fade-in khi xuất hiện

---

## 7. Quy tắc lập trình

- **Bảo toàn code hiện tại.** Không dùng Replace All mù quáng
- **Xác định Line range chính xác** khi dùng Replace
- **Trình bày Code Block đề xuất** và chờ User gật đầu trước khi thực thi
- **Tuân thủ Manager Pattern**: Logic nằm trong Manager, MainWindow chỉ delegate event
- **Naming convention**: Tiếng Anh cho code (class, method, variable), tiếng Việt cho UI text/tooltip

---

## 8. Phím tắt liên quan

| Phím | Hành động |
|---|---|
| `P` | Chuyển sang Pen mode |
| `S` | Chuyển sang Select mode |
| `E` | Chuyển sang EraserStroke |
| `R` | Chuyển sang EraserPoint |
| `Delete` | Xóa nét đã chọn (khi đang ở Select mode) |
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `F11` | Toggle fullscreen |
| `Escape` | Thoát fullscreen hoặc về Pen mode |
