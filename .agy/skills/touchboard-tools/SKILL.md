---
name: touchboard-tools
description: >-
  Master Skill quản lý lộ trình phát triển (Roadmap) và kiến trúc tổng thể cho các công cụ mở rộng,
  Menu Chèn (Insert), Công cụ Toán học (STEM), Hình học, và Máy tính trên bảng vẽ TouchBoard.
  Sử dụng skill này để xem tiến độ và điều hướng đến các skill con (sub-skills) của từng Phase.
---

# Lộ trình Phát triển Công cụ & Menu Chèn (TouchBoard Tools Roadmap)

Đây là **Master Skill** quản lý cấu trúc tổng thể của các công cụ. Để phát triển tính năng chi tiết, vui lòng tạo/sử dụng các **Skill con (Sub-skills)** tương ứng với từng Phase.

## Cơ chế "Đang phát triển"
- Module nào còn `[ ]` → nút bấm tương ứng trong giao diện (VD: `InsertPopup`) gọi `BtnUnderConstruction_Click` → hiện thông báo *"Tính năng này đang được phát triển!"*.
- Khi hoàn thành → thay handler thành gọi trực tiếp công cụ, cập nhật thành `[x]`.

---

## 📅 Roadmap & Tiến độ (Checklist)

### Phase 1: Menu Chèn Cơ bản
> Trạng thái: **Hoàn thành**
- [x] Kiến trúc `InsertPopup` (Menu Chèn) ở thanh công cụ dưới.

### Phase 2: Các Công cụ Toán học (STEM Tools)
> Skill con: `D:\Document\_Projects\TouchBoard\.agy\skills\touchboard-phase2-stem\SKILL.md`
- [x] **Thước thẳng (`RulerOverlay`)**: Đo và vẽ đoạn thẳng, kéo/xoay tự do (Overlay 25 Z-Index).
- [x] **Eke (`SetSquareOverlay`)**: Thước tam giác vuông 45-45-90.
- [x] **Thước đo góc (`ProtractorOverlay`)**: Đo góc 0-180°.
- [x] **Compa (`CompassOverlay`)**: Vẽ đường tròn tâm O với bán kính tuỳ chỉnh trực tiếp lên InkCanvas.
- [x] **Máy tính (Calculator)**: Tích hợp máy tính mini nổi trên màn hình giúp tính toán nhanh trong giờ học toán. (MỚI)

### Phase 3: Hình học có sẵn (Predefined Shapes)
> Skill con: `D:\Document\_Projects\TouchBoard\.agy\skills\touchboard-phase3-shapes\SKILL.md`
- [ ] **Menu Hình học**: Bảng chọn hình học nổi (Popup) gồm 3 nhóm chính.
- [ ] **Hình học phẳng (2D Shapes)**:
  - Đường thẳng, mũi tên, đứt nét.
  - Chữ nhật, tam giác, hình thoi, elip, hình tròn, lục giác, hình thang, bán nguyệt, hình bình hành.
- [ ] **Stickers/Icons**: Các biểu tượng thường dùng (Ngôi sao, tick...).
- [ ] **Hình học không gian (3D Shapes)**:
  - Mặt cầu, bán cầu, hình trụ, nón cụt (frustum).
  - Hình nón, chóp tam giác, lăng trụ tam giác, hình lập phương.
- [ ] **Tương tác Hình học**: Hình được chèn vào bảng là Object, hỗ trợ chọn, đổi màu nét, tô màu nền, đổi kích thước (tương tự Image/Stroke Selection).

### Phase 4: Công cụ Quản lý Thời gian (Time Tools)
> Skill con: `D:\Document\_Projects\TouchBoard\.agy\skills\touchboard-phase4-time\SKILL.md`
- [ ] **Đồng hồ đếm ngược (Countdown Timer)**.
- [ ] **Đồng hồ bấm giờ (Stopwatch)**.

### Phase 5: Quản lý Ảnh & Tài liệu (Media)
> Skill con: `D:\Document\_Projects\TouchBoard\.agy\skills\touchboard-phase5-media\SKILL.md`
> Trạng thái: **Hoàn thành**
- [x] **Quản lý Ảnh (`ImageManager`)**: Chọn ảnh `.png, .jpg` từ máy tính, kéo thả, thu phóng, xóa ảnh trên `DrawingCanvas`.

---

## 🛠 Hướng dẫn phát triển Phase mới

Mỗi khi bắt tay vào làm một Phase mới (ví dụ Phase 3: Shapes), bạn cần:
1. Tạo một thư mục skill mới (ví dụ: `touchboard-phase3-shapes`).
2. Viết file `SKILL.md` con chứa đặc tả chi tiết về logic, kiến trúc (ví dụ: Hình học dùng `Polygon`, `Ellipse` hay custom `Path`), cách tích hợp vào `SelectionManager`.
3. Cập nhật đường dẫn skill con vào file Master Skill này.
4. Triển khai code và cập nhật trạng thái `[x]` ở cả Master và Sub-skill.
