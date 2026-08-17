---
name: touchboard-eraser
description: >-
  Skill quản lý công cụ Tẩy (Eraser) bao gồm: Tẩy Điểm (Point), Tẩy Nét (Stroke) và
  Nhận diện xóa bằng Lòng bàn tay (Palm Eraser) trong dự án TouchBoard.
---

# TouchBoard — Công cụ Tẩy (Eraser) & Nhận diện Lòng bàn tay (Palm Rejection/Eraser)

Đây là tài liệu đặc tả và hướng dẫn bảo trì cho công cụ Tẩy của hệ thống TouchBoard. Cần tuân thủ chặt chẽ khi nâng cấp hoặc sửa đổi các tính năng xóa.

---

## 1. Cơ chế Tẩy (Eraser Modes)
TouchBoard hỗ trợ 2 loại tẩy chính, được điều khiển thông qua `ToolManager.CurrentMode`:

1. **`EraserStroke` (Tẩy Nét)**:
   - *Logic*: Chạm vào bất kỳ phần nào của một nét vẽ, toàn bộ nét vẽ đó sẽ bị xóa.
   - *EditingMode*: `InkCanvasEditingMode.EraseByStroke`
   - *Ứng dụng*: Xóa nhanh các đối tượng lớn, chữ viết.

2. **`EraserPoint` (Tẩy Điểm)**:
   - *Logic*: Chỉ xóa phần mực trực tiếp nằm dưới diện tích của cục tẩy, giữ lại các phần nét chưa bị chạm tới.
   - *EditingMode*: `InkCanvasEditingMode.EraseByPoint`
   - *Kích thước (EraserShape)*: Được tuỳ chỉnh qua `Slider` trong `EraserSettingsPopup`, dùng `EllipseStylusShape(size, size)` gắn vào `DrawingCanvas.EraserShape`.

---

## 2. Giao diện (EraserSettingsPopup)
Tương tự như cài đặt Bút (PenSettings), nút bấm trên thanh Toolbar gộp chung cho cả 2 loại Tẩy:
- **Click 1 lần**: Kích hoạt chế độ Tẩy (sử dụng loại tẩy được lưu gần nhất, mặc định là Tẩy Nét).
- **Click lần 2 (Double Click)**: Xổ ra `EraserSettingsPopup` chứa:
  - Nút chọn Tẩy Nét.
  - Nút chọn Tẩy Điểm.
  - Thanh trượt (Slider) để thay đổi độ to/nhỏ của Tẩy Điểm (giá trị 10px - 200px).

---

## 3. Ghi chú bảo trì
- **Tốc độ xử lý**: Gọi `HitTest` nhiều lần trong sự kiện `TouchMove` có thể gây giảm hiệu năng nếu số lượng nét quá lớn, hãy đảm bảo `HitTest` chỉ quét trên tiết diện cần thiết.
- **Undo cho Tẩy Điểm (EraseByPoint)**: Khi sử dụng `EraseByPoint`, WPF liên tục "cắt" nét vẽ và kích hoạt sự kiện thay đổi. Để tránh việc Undo trả lại từng pixel (Undo spam), `HistoryManager.cs` sử dụng một bộ đếm `DispatcherTimer` (Debounce 400ms) để gom các sự kiện thay đổi lại và chỉ lưu 1 trạng thái duy nhất sau khi người dùng ngừng kéo tẩy.
- **Xung đột trạng thái khi đổi trang**: Khi `PageManager` đổi trang, nó sẽ gọi lệnh làm sạch vùng chọn `DrawingCanvas.Select(empty)`. Hàm này của WPF âm thầm ép `EditingMode` của `InkCanvas` chuyển về `Select`. Do đó, sự kiện `OnPageChanged` trong `MainWindow.xaml.cs` bắt buộc phải gọi lại `_toolManager.SwitchToMode(_toolManager.CurrentMode)` để đồng bộ lại chức năng thao tác trên bảng vẽ với giao diện UI hiện tại.
