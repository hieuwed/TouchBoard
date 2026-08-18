---
name: touchboard-phase5-media
description: >-
  Skill chi tiết cho Phase 5: Quản lý Ảnh & Tài liệu (Media).
  Bao gồm logic chèn ảnh vào bảng vẽ, biến ảnh thành đối tượng có thể di chuyển, 
  thu phóng, xoay và tương tác với undo/redo.
---

# Phase 5: Quản lý Ảnh & Tài liệu (Media)

Tài liệu này mô tả kiến trúc của tính năng chèn ảnh và quản lý đối tượng đa phương tiện trên bảng viết.

## 1. Mục tiêu
- Giáo viên có thể chèn hình ảnh (.jpg, .png, .gif) vào bảng để làm học liệu.
- Hình ảnh là một đối tượng độc lập, có thể chọn, di chuyển, phóng to thu nhỏ.
- Mọi thao tác đều được ghi nhận vào hệ thống Undo/Redo (HistoryManager).

## 2. Kiến trúc & Tương tác

### 2.1. Cấu trúc Đối tượng Ảnh (`ImageObject` hoặc `BoardImage`)
- Hình ảnh không được vẽ chìm vào nền mà được thêm dưới dạng `UIElement` (thường là thẻ `<Image>`) nổi trên `Canvas`.
- Giống như Shape ở Phase 3, Image cần được bọc trong một Container hoặc hỗ trợ gắn Adorner để có các điểm Neo (Handles) thu phóng.

### 2.2. Tích hợp SelectionManager
- Tương tự Phase 3, `SelectionManager` cần hỗ trợ Hit-test và chọn `UIElement`.
- Khi nhấp vào ảnh, hiện Bounding Box có 8 điểm neo (Resize) và 1 điểm neo (Rotate).
- Hỗ trợ thao tác cảm ứng đa điểm (Pinch-to-zoom) trực tiếp trên bức ảnh.

### 2.3. Menu Context của Ảnh
Khi chọn ảnh, nút `(⋯)` sẽ mở menu:
- Nhân bản (Duplicate)
- Khóa vị trí (Lock) - để học sinh viết vẽ lên trên mà không lỡ tay kéo ảnh đi.
- Xóa (Delete)
- Sắp xếp (Z-Index): Đưa lên trên cùng, đưa xuống dưới cùng.

## 3. Trạng thái thực hiện

- [x] Tính năng Chèn ảnh từ máy tính (ImageManager cơ bản)
- [ ] Bọc Ảnh thành Đối tượng có thể thao tác (Adorner/Container)
- [ ] Tích hợp SelectionManager cho Ảnh
- [ ] Tính năng Pinch-to-zoom cho Ảnh
- [ ] Undo/Redo cho Ảnh
- [ ] Context Menu (Khóa, Z-Index)
