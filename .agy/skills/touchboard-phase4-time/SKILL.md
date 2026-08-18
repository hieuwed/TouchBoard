---
name: touchboard-phase4-time
description: >-
  Skill chi tiết cho Phase 4: Công cụ Quản lý Thời gian (Time Tools).
  Bao gồm kiến trúc UI và logic đếm ngược (Countdown) và bấm giờ (Stopwatch).
---

# Phase 4: Công cụ Quản lý Thời gian (Time Tools)

Tài liệu đặc tả kiến trúc và cách thức triển khai các công cụ hỗ trợ giáo viên quản lý thời gian trên bảng tương tác.

## 1. Công cụ Đếm ngược (Countdown Timer)
Sử dụng khi giáo viên muốn giao bài tập có giới hạn thời gian.

### 1.1. Giao diện (UI)
- Nổi (Overlay) trên màn hình chính, có thể kéo di chuyển.
- Hình dáng: Hình chữ nhật bo góc tròn hoặc hình đồng hồ.
- Chế độ hiển thị:
  - **Setup Mode:** Các ô quay (Spinners) hoặc nút `+ / -` để thiết lập Phút và Giây.
  - **Running Mode:** Chữ số đếm ngược khổng lồ (Digital Clock style). Có thanh tiến trình (ProgressBar) chạy vòng tròn hoặc ngang.

### 1.2. Tính năng & Logic
- Nút Start, Pause, Reset.
- Hỗ trợ chọn âm thanh báo thức khi hết giờ (Ring).
- Nút `+1 min`, `+5 mins` tiện lợi.
- Khi thời gian < 10 giây, số đổi sang màu đỏ để cảnh báo.

## 2. Đồng hồ bấm giờ (Stopwatch)
Sử dụng khi học sinh thi đua, giáo viên cần tính xem mất bao lâu để hoàn thành.

### 2.1. Giao diện (UI)
- Nổi (Overlay), có thể thu gọn (Minify) để không chiếm diện tích.
- Hiển thị Định dạng: `MM:SS:ms` (Phút : Giây : Phần trăm giây).

### 2.2. Tính năng & Logic
- Nút Start, Pause, Reset.
- Nút **Lap (Cờ)** để ghi lại các mốc thời gian (hiển thị thành danh sách xổ xuống).

## 3. Kiến trúc hệ thống
- Sử dụng `System.Windows.Threading.DispatcherTimer` cho độ chính xác cao trong UI thread.
- Các Overlay này sẽ kế thừa từ `StemToolBase` hoặc một `FloatingToolBase` tương tự để tái sử dụng tính năng Drag-to-move và Nút Đóng.

## 4. Trạng thái thực hiện

- [ ] Lớp cơ sở (Base) hoặc tái sử dụng StemToolBase
- [ ] Giao diện đếm ngược (Countdown UI)
- [ ] Logic đếm ngược & m Nhạc
- [ ] Giao diện bấm giờ (Stopwatch UI)
- [ ] Logic bấm giờ & Laps
