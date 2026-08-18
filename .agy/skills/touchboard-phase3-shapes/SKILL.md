---
name: touchboard-phase3-shapes
description: >-
  Skill chi tiết cho Phase 3: Hình học có sẵn (Predefined Shapes).
  Định nghĩa kiến trúc chèn, vẽ và tương tác (chọn, di chuyển, thu phóng, đổi màu) 
  với các hình học 2D, 3D và Stickers trên bảng vẽ.
---

# Phase 3: Hình học có sẵn (Predefined Shapes)

Tài liệu đặc tả kiến trúc và cách thức triển khai module Hình học (Shapes) cho TouchBoard, tương tự tính năng chèn hình của bảng SmartTouch.

## 1. Phân loại Hình học (Shape Categories)

Hệ thống sẽ cung cấp một Popup Menu (`ShapeMenuPopup`) chứa 3 tab/nhóm:

### 1.1. Hình học 2D (2D Shapes)
- **Đường nét:** Đường thẳng (Line), Mũi tên (Arrow), Đường đứt nét (Dashed Line).
- **Hình cơ bản:** Chữ nhật (Rectangle), Tam giác (Triangle), Hình thoi (Rhombus), Elip (Ellipse), Hình tròn (Circle).
- **Hình đa giác:** Lục giác (Hexagon), Hình thang (Trapezoid), Bình hành (Parallelogram), Bán nguyệt (Semicircle).

### 1.2. Hình học không gian (3D Shapes)
- **Cơ bản:** Mặt cầu (Sphere), Hình lập phương (Cube), Lăng trụ tam giác (Triangular Prism).
- **Trụ & Nón:** Hình trụ (Cylinder), Hình nón (Cone), Nón cụt (Frustum).
- **Chóp:** Chóp tam giác (Triangular Pyramid).

### 1.3. Stickers / Icons
- Ngôi sao (Star), Dấu Tick (Checkmark), Dấu X (Cross), v.v.

## 2. Kiến trúc & Tương tác (Shape Object Architecture)

Để tương thích với hệ thống bảng vẽ (`InfiniteCanvasContainer` / `DrawingCanvas`) và công cụ Chọn (`SelectionManager`), các hình học không được vẽ trực tiếp thành điểm (Stroke) mà sẽ là các đối tượng UI (`UIElement` hoặc `Shape` của WPF).

### 2.1. Cấu trúc Đối tượng (`BoardShapeBase`)
- Các hình sẽ kế thừa từ một UserControl/Class chung `BoardShapeBase`.
- Gói gọn bên trong là `Path` hoặc các đối tượng `Shape` (WPF) để dễ dàng thay đổi màu sắc.
- Hỗ trợ các thuộc tính: `FillColor` (màu nền), `StrokeColor` (màu viền), `StrokeThickness` (độ dày viền).

### 2.2. Tích hợp vào `SelectionManager`
- `SelectionManager` hiện tại đang quản lý `InkCanvas.GetSelectedStrokes()`.
- Để quản lý hình học, `SelectionManager` cần được nâng cấp (hoặc tạo `ObjectSelectionManager`) để có thể Click chọn `BoardShapeBase`.
- Khi một hình được chọn, bao quanh nó sẽ xuất hiện **Bounding Box** với 8 điểm Neo (Handles) để **Resize** (Thu phóng) và 1 điểm neo để **Rotate** (Xoay).
- Hình học hỗ trợ di chuyển (Drag-to-move) giống như Stroke Selection.

### 2.3. Menu Context của Hình học
Khi chọn hình học, nhấn nút `(⋯)` sẽ mở menu cho phép:
- Thay đổi màu Nét (Stroke Color).
- Thay đổi màu Nền (Fill Color) - Hỗ trợ màu trong suốt (Transparent).
- Thay đổi độ dày nét.
- Xóa hình, Nhân bản (Copy/Paste).

## 3. Trạng thái thực hiện

- [ ] Cấu trúc `BoardShapeBase`
- [ ] UI Popup Menu Hình học
- [ ] Nâng cấp `SelectionManager` hỗ trợ UIElement
- [ ] Tính năng Resize/Rotate cho Hình học
- [ ] Bộ dữ liệu hình 2D
- [ ] Bộ dữ liệu hình 3D
- [ ] Undo/Redo cho Object
