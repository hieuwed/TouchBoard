---
name: touchboard-tools
description: >-
  Skill quản lý các công cụ mở rộng (STEM Tools) và chức năng Chèn (Insert) trên bảng vẽ TouchBoard.
  Bao gồm các tính năng: Chèn ảnh, Thước thẳng (Ruler), Thước đo góc (Protractor),
  Eke (Set Square), và Compa (Compass).
---

# TouchBoard Tools & Insert Module

Skill này quản lý việc triển khai và theo dõi tiến độ của Menu "Chèn" (Insert) cùng các công cụ mở rộng (STEM Tools & Utilities).

## 🚀 Lộ trình Phát triển (Phases)

Mỗi module dưới đây sẽ được phát triển theo từng giai đoạn. Hệ thống sẽ tự động cập nhật trạng thái khi hoàn thành.

### Phase 1: Kiến trúc Menu Chèn (Insert Menu)
- [x] Tạo UI cơ bản cho Menu Chèn (`InsertPopup`).
- [ ] Tích hợp tính năng chèn Hình ảnh (Media).

### Phase 2: Các Công cụ Toán học (STEM Tools)
- [ ] **Thước thẳng (`RulerTool`)**: Đo và vẽ đoạn thẳng.
- [ ] **Eke (`SetSquareTool`)**: Thước tam giác vuông 45/60.
- [ ] **Thước đo góc (`ProtractorTool`)**: Đo góc.
- [ ] **Compa (`CompassTool`)**: Vẽ đường tròn tâm tuỳ chỉnh.

### Phase 3: Công cụ Quản lý Thời gian (Time Tools)
- [ ] **Đồng hồ bấm giờ / Đếm ngược (Stopwatch & Timer)**: Widget nổi đếm thời gian.
- [ ] **Lịch (Calendar)**: Widget hiển thị ngày tháng, ghi chú sự kiện.

### Phase 4: Công cụ Tập trung (Focus Tools)
- [ ] **Đèn soi (Spotlight)**: Làm tối màn hình, soi một vùng nhỏ.
- [ ] **Che màn hình (Screen Shade)**: Phủ màn che bài giảng.

### Phase 5: Công cụ Đồ hoạ & Biểu diễn (Graphics & Diagrams)
- [ ] **Vẽ Bảng (Table)**: Tạo bảng lưới kéo thả co giãn.
- [ ] **Các hình học cơ bản (Geometry Shapes)**: Các hình học 2D/3D điều khiển bằng Control Points.
- [ ] **Sơ đồ tư duy (Mindmaps)**: Tạo các Node và đường nối tự động layout.

---

## Cơ chế hoạt động của UI (Chưa hoàn thành)
- Bất kỳ module nào ở trên đang ở trạng thái `[ ]` (chưa hoàn thành), khi nhấn vào nút bấm tương ứng trên giao diện `MainWindow` sẽ hiển thị hộp thoại thông báo: *"Tính năng này đang được phát triển!"*.
- Khi hoàn thành tính năng, nút bấm sẽ gọi trực tiếp module thay vì hiện thông báo, đồng thời trạng thái trong file này sẽ chuyển sang `[x]`.

## Hướng dẫn bảo trì
- Khi nâng cấp hoặc chuyển đổi hệ thống vẽ (DrawingCanvas) sang một Engine khác, cần cập nhật lại reference ở trong các file `Tools/*.xaml.cs` (phần xử lý MouseDown/MouseMove/MouseUp chuyển đổi toạ độ Screen -> Canvas).
