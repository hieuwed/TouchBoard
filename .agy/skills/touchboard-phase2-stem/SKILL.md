---
name: touchboard-phase2-stem
description: >-
  Skill chi tiết cho Phase 2: Công cụ Toán học (STEM Tools) và Máy tính.
  Trọng tâm: Kế thừa StemToolBase, Giao diện chuẩn QA-SmartTouch, và cơ chế
  Snap-to-Edge Realtime (Khóa cứng nét vẽ dọc mép thước ngay trong quá trình vẽ).
---

# Phase 2: STEM Tools & Máy tính (Calculator)

Tài liệu này đặc tả chi tiết kiến trúc, giao diện và logic của các công cụ toán học (Thước thẳng, Eke, Thước đo góc, Compa) và Máy tính, bám sát chặt chẽ yêu cầu từ dự án QA-SmartTouch.

## 1. Nền tảng chung (StemToolBase)
Tất cả 5 công cụ (Ruler, SetSquare, Protractor, Compass, Calculator) đều phải kế thừa từ lớp cơ sở `StemToolBase` (`Controls/StemToolBase.cs`) để đảm bảo tính đồng nhất:
- **Kéo thả (Drag):** Tái sử dụng logic di chuyển thông qua sự kiện `MouseDown/Move/Up` trên vùng `Body` của công cụ.
- **Xoay (Rotate):** Xử lý góc xoay tự do quanh tâm (`RenderTransformOrigin`).
- **Đóng (Close):** Cung cấp hàm đóng chuẩn, tự động dọn dẹp và hủy liên kết khỏi `StemManager` (hoặc `MainWindow`).

## 2. Giao diện (UI) - Kế thừa QA-SmartTouch
Giao diện các công cụ tuân thủ chặt chẽ bảng màu của hệ thống QA-SmartTouch:
- **Nền (Background):** Sử dụng các dải màu trong suốt để không che khuất bảng vẽ (VD: Thước thẳng `#CC2E86DE`). Máy tính dùng nền xám sáng thanh lịch (`#F1F2F6`).
- **Nút bấm (Buttons):** Thống nhất dùng màu xanh chủ đạo (`#2E86DE`) cho các nút Active/Primary và màu đỏ (`#EE5A6F`) cho nút Đóng (Close). Bo góc mềm mại (BorderRadius).
- **Mặt số, Vạch chia:** Rõ ràng, tối giản, tương phản cao (Màu trắng/Đen tùy nền).

## 3. Hệ thống Snap-to-Edge (Bắt dính từ tính 20px - Realtime)
> **Yêu cầu cốt lõi:** Khi vẽ cách mép thước < 20px, nét vẽ bị khóa cứng (constrain). Quá trình rê bút sẽ CHỈ tạo ra điểm nằm trên phương của cạnh thước, tạo cảm giác bị nam châm hút trong thời gian thực.

### Kiến trúc dự kiến:
1. **StemManager (Quản lý trạng thái):**
   - Theo dõi danh sách các Thước đang mở (đã implement interface `IEdgeSnappable`).
   - Cung cấp hàm `Point GetSnappedPoint(Point p, out bool isSnapped)` để `InkCanvas` gọi liên tục.

2. **Can thiệp quá trình vẽ (Realtime Constrain):**
   - *Thách thức của WPF:* `InkCanvas` có bộ máy vẽ mặc định (DynamicRenderer) không cho phép nắn điểm trực tiếp.
   - *Giải pháp:* Tắt chế độ vẽ mặc định của `InkCanvas` (`EditingMode = None`) hoặc can thiệp sâu qua Custom `DynamicRenderer`.
   - Bắt các sự kiện `StylusDown` (hoặc `MouseDown`), `StylusMove`, `StylusUp` trên `InkCanvas`.
   - Nếu `StylusDown` nằm trong phạm vi 20px của mép thước $\Rightarrow$ Kích hoạt trạng thái **Locked**.
   - Trong quá trình `StylusMove`, mọi tọa độ đầu vào đều bị **chiếu vuông góc (Project)** lên đường thẳng của mép thước $\Rightarrow$ Tạo điểm StylusPoint mới và chèn trực tiếp vào nét vẽ đang hiển thị.

## 4. Chi tiết các công cụ

### 4.1. Thước thẳng (`RulerOverlay`)
- Body dài hình chữ nhật. Cung cấp 2 cạnh thẳng (trên/dưới) cho hệ thống Snap-to-Edge.
- Chiều dài mặc định của thước và thanh thu phóng là 20cm.
- Trên thước không cần hiển thị số (chỉ giữ lại vạch chia).
- Phải có đánh dấu vị trí đặt bút trên mặt thước để người dùng biết chỗ đặt bút vào là kẻ được đường thẳng (ví dụ: đường viền xanh đậm báo hiệu vùng từ tính).

### 4.2. Eke (`SetSquareOverlay`)
- Body tam giác vuông. Cung cấp 2 cạnh góc vuông và cạnh huyền cho hệ thống Snap-to-Edge.

### 4.3. Thước đo góc (`ProtractorOverlay`)
- Body nửa vòng tròn. Chỉ cung cấp đoạn thẳng đáy cho hệ thống Snap-to-Edge.

### 4.4. Compa (`CompassOverlay`)
- Không dùng Snap-to-Edge. Hoạt động độc lập bằng cách kéo Handle Tâm và Handle Bán kính.
- Nút "Vẽ" đẩy trực tiếp Stroke vòng tròn hoàn hảo vào `InkCanvas`.

### 4.5. Máy tính (`CalculatorOverlay`)
- Máy tính nổi mini. Hỗ trợ phép tính toán học qua `DataTable().Compute()`.

---
*Tài liệu này được dùng làm tham chiếu để thiết kế lại hệ thống Snap-to-Edge. Mọi công cụ phải tuân theo tiêu chuẩn này.*
