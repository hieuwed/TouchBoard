---
name: TouchBoard Roadmap
description: Lộ trình phát triển các tính năng mở rộng (Phases) cho dự án TouchBoard (Whiteboard).
---

# TouchBoard Roadmap (Kế hoạch Phát triển Mở rộng)

Skill này cung cấp lộ trình (Roadmap) và định hướng phát triển các tính năng nâng cao cho dự án TouchBoard. Agent cần bám sát các nhóm tính năng (Phases) dưới đây khi nhận yêu cầu thêm tính năng mới từ người dùng.

> [!NOTE]
> Mã nguồn hiện đã được refactor theo cấu trúc modular (chia thành các Manager). Việc thêm các tính năng mới nên được thực hiện bằng cách tạo thêm các Manager tương ứng (ví dụ: `ExportManager`, `ScreenCaptureManager`) thay vì viết dồn vào `MainWindow.xaml.cs`.

## Các Tính Năng Đề Xuất (Phases)

Các tính năng được chia thành từng nhóm để dễ quản lý và triển khai:

### ✅ Nhóm 1: Quản lý Lịch sử (Undo / Redo) [Đã hoàn thành ✅]
- Theo dõi các sự kiện: Thêm nét (`StrokesChanged`), Xóa nét, Di chuyển/Thay đổi kích thước (`SelectionMoved`, `SelectionResized`).
- Xây dựng ngăn xếp (Stack) Undo và Redo (`HistoryManager.cs`).
- Thêm 2 nút **Hoàn tác (Undo)** ↩️ và **Làm lại (Redo)** ↪️ lên thanh công cụ (hỗ trợ phím tắt `Ctrl+Z`, `Ctrl+Y`).

### ✅ Nhóm 2: Công cụ Tẩy (Eraser) nâng cao [Đã hoàn thành ✅]
- **Tẩy theo điểm (`EraseByPoint`)**: Xóa chính xác vùng chạm vào (giống cục tẩy thật). Đã hỗ trợ đổi kích thước qua `StrokeWidthManager`.
- **Tẩy theo nét (`EraseByStroke`)**: Chạm vào nét nào là xóa toàn bộ nét đó (nhanh hơn).

### ✅ Nhóm 3: Thay đổi màu nét bút của đối tượng được chọn (Change Selected Strokes Color) [Đã hoàn thành ✅]
- **Chọn và Đổi màu (`Change Color`)**: Dùng công cụ Chọn (Selection Tool) để chọn nét vẽ, sau đó bấm chọn màu mới trên bảng màu để đổi màu (`ColorManager`).
- **Đổi độ dày nét (`Change Thickness`)**: Đã hỗ trợ thay đổi độ dày nét cho các nét vẽ đang chọn (`StrokeWidthManager`).

### Nhóm 4: Chụp màn hình & Quay màn hình (Screen Capture & Screen Recording) [Đang chờ triển khai ⏳]
- **Chụp màn hình (`Screen Snipping / Capture`)**: 
  - Hỗ trợ chụp một vùng chọn hoặc toàn bộ màn hình (bảng / ứng dụng khác trên máy tính).
  - Tự động copy ảnh chụp vào bộ nhớ tạm (Clipboard) và hỗ trợ **Dán (`Paste`)** trực tiếp lên bảng viết.
  - Ảnh sau khi dán lên bảng trở thành đối tượng tương tác: có thể **di chuyển, thu phóng (resize), xoay, và vẽ/ghi chú đè lên**.
- **Quay màn hình (`Screen Recording`)**:
  - Hỗ trợ quay video các thao tác trên bảng kèm âm thanh (Microphone).
  - Thanh điều khiển quay video: Bắt đầu (Record), Tạm dừng (Pause), Tiếp tục (Resume) và Dừng & Xuất video (định dạng `.mp4`).

### Nhóm 5: Lưu và Xuất file (Save / Export) [Đang chờ triển khai ⏳]
- **Lưu / Mở file chuẩn (`.isf`)**: Lưu toàn bộ nét vẽ và đối tượng dưới dạng vector để lần sau mở ra chỉnh sửa tiếp.
- **Xuất hình ảnh (`.png`, `.jpg`)**: Lưu lại trạng thái bảng hiện tại thành ảnh tĩnh để chia sẻ (kết xuất cả background).

### ✅ Nhóm 6: Nền bảng đa dạng (Canvas Backgrounds) [Đã hoàn thành ✅]
- Cung cấp nút chuyển đổi nền: Tối (Dark, mặc định), Trắng (White), Bảng đen (Blackboard), Lưới ô ly (Grid), Kẻ ngang (Ruled).
- Quản lý bởi `BackgroundManager.cs`. Tự động đổi màu thanh công cụ và màu mực mặc định theo nền.

### ✅ Nhóm 8: Đa điểm chạm (Multi-Touch Drawing) [Đã hoàn thành ✅]
- Hỗ trợ vẽ đồng thời bằng nhiều ngón tay trên màn hình Android (OPS) và Laptop Windows.
- Tự động nhận diện và xử lý độc lập các nguồn nhập liệu: Cảm ứng (Touch), Bút (Stylus) và Chuột (Mouse).
- Tích hợp liền mạch với hệ thống Undo/Redo thông qua `MultiTouchManager`.

### Nhóm 7: Thêm Đối tượng nâng cao (Shapes, Text, Image) [Đang chờ triển khai ⏳]
- Vẽ hình khối cơ bản (Vuông, Tròn, Đường thẳng, Mũi tên).
- Chèn hộp văn bản (Text Box) để gõ chữ bằng bàn phím.
- Chèn hình ảnh (Import Image) từ máy tính vào bảng để ghi chú đè lên.

### Nhóm 9: Canvas Vô Hạn & Quản Lý Đa Trang (Infinite Canvas & Pages) [Đang chờ triển khai ⏳]
- Bảng vẽ vô hạn (Infinite Canvas): Cho phép người dùng cuộn (Pan) và thu phóng (Zoom) không giới hạn không gian vẽ.
  - **Cơ chế di chuyển (Pan/Zoom) đề xuất:**
    - *Với Chuột (Mouse):* Giữ phím `Space` + Kéo chuột trái (hoặc bấm giữ Chuột giữa / Chuột phải) để di chuyển (Pan). Lăn con trỏ chuột (kết hợp `Ctrl`) để Thu/Phóng (Zoom).
    - *Với Màn hình cảm ứng (Android OPS/Touch):* Sử dụng thao tác 2 ngón tay (Two-finger Pan & Pinch-to-zoom) để cuộn và thu phóng. Một ngón tay vẫn giữ nguyên chức năng vẽ/chọn để không bị xung đột.
- Quản lý đa trang trực quan: Xây dựng một giao diện (UI) quản lý danh sách trang đẹp mắt, thay thế việc chọn nền trực tiếp trên Toolbar.
- Chọn loại trang trước khi tạo: Khi bấm tạo trang mới, hiển thị tuỳ chọn loại nền (Trắng, Đen, Ô ly, Kẻ ngang...).
- Sắp xếp và di chuyển trang: Hỗ trợ tính năng kéo thả (Drag & Drop) qua lại để dễ dàng thay đổi thứ tự các trang (VD: kéo trang 2 lên vị trí trang 1).

---

## Nguyên tắc Triển Khai Tính Năng Mới
1. Luôn tuân thủ kiến trúc Modular (Tạo file Manager mới trong thư mục `Managers/` nếu cần).
2. Tách bạch giao diện và logic (Ví dụ: Thêm resource vào `Styles/ToolbarStyles.xaml`).
3. Cập nhật `ShortcutManager` nếu có phím tắt mới.
4. Cập nhật `HistoryManager` nếu tính năng mới làm thay đổi trạng thái bảng vẽ cần Undo/Redo.
