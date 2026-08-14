# Kế hoạch Phát triển Mở rộng TouchBoard

Sau khi chức năng Cảm ứng cơ bản (Viết và Chọn) đã hoạt động ổn định trên màn hình tương tác, giai đoạn này sẽ tập trung vào việc bổ sung các tính năng nâng cao đã được đề xuất trước đó, giúp ứng dụng trở thành một Whiteboard hoàn chỉnh.

---

## 1. Các Tính Năng Đề Xuất (Phases)

Tôi đã chia các tính năng thành từng nhóm để dễ quản lý. Bạn có thể chọn ưu tiên làm nhóm nào trước.

### Nhóm 1: Quản lý Lịch sử (Undo / Redo)
Đây là tính năng cực kỳ quan trọng cho các ứng dụng vẽ.
- Theo dõi các sự kiện: Thêm nét (`StrokesChanged`), Xóa nét, Di chuyển/Thay đổi kích thước (`SelectionMoved`, `SelectionResized`).
- Xây dựng ngăn xếp (Stack) Undo và Redo.
- Thêm 2 nút **Hoàn tác (Undo)** ↩️ và **Làm lại (Redo)** ↪️ lên thanh công cụ (hỗ trợ phím tắt `Ctrl+Z`, `Ctrl+Y`).

### Nhóm 2: Công cụ Tẩy (Eraser) nâng cao
Mặc dù hiện tại đã có thể "Chọn" và "Xóa" nét, nhưng người dùng thường quen với công cụ Tẩy chuyên dụng.
- **Tẩy theo điểm (`EraseByPoint`)**: Xóa chính xác vùng chạm vào (giống cục tẩy thật). Cần có thể chọn kích thước cục tẩy.
- **Tẩy theo nét (`EraseByStroke`)**: Chạm vào nét nào là xóa toàn bộ nét đó (nhanh hơn).

### Nhóm 3: Lưu và Xuất file (Save / Export)
Giúp người dùng lưu lại nội dung buổi họp/giảng dạy.
- **Lưu / Mở file chuẩn (`.isf`)**: Lưu toàn bộ nét vẽ và đối tượng dưới dạng vector để lần sau mở ra chỉnh sửa tiếp.
- **Xuất hình ảnh (`.png`, `.jpg`)**: Lưu lại trạng thái bảng hiện tại thành ảnh tĩnh để chia sẻ (kết xuất cả background).

### Nhóm 4: Nền bảng đa dạng (Canvas Backgrounds)
Cung cấp các loại giấy/nền khác nhau phục vụ nhiều mục đích.
- Nút chuyển đổi nền: Trắng trơn (mặc định hiện tại), Đen/Xanh đậm (Blackboard), Lưới ô ly (Grid), Kẻ ngang (Ruled).

### Nhóm 5: Thêm Đối tượng nâng cao (Shapes, Text, Image)
Cho phép chèn nội dung phong phú hơn thay vì chỉ vẽ tay.
- Vẽ hình khối cơ bản (Vuông, Tròn, Đường thẳng, Mũi tên).
- Chèn hộp văn bản (Text Box) để gõ chữ bằng bàn phím.
- Chèn hình ảnh (Import Image) từ máy tính vào bảng để ghi chú đè lên.

---

## 2. Kiến trúc & Cấu trúc mã

Do số lượng tính năng tăng lên, mã nguồn trong `MainWindow.xaml.cs` sẽ trở nên phức tạp nếu không cấu trúc lại. Kế hoạch refactor (tái cấu trúc) nhẹ:
- Tạo lớp `HistoryManager.cs` để chuyên quản lý logic Undo/Redo.
- Chuyển bớt các cấu hình hình ảnh (như vẽ lưới nền) sang các Resource Dictionary riêng biệt.

---

## 3. Các câu hỏi cần xác nhận từ bạn (Open Questions)

> [!IMPORTANT]
> Để tối ưu hóa quá trình phát triển, vui lòng cho biết ý kiến của bạn về các vấn đề sau:

1. **Thứ tự ưu tiên**: Trong 5 nhóm tính năng trên, bạn muốn làm nhóm nào trước tiên? (Tôi đề xuất làm **Nhóm 1 (Undo/Redo)** và **Nhóm 2 (Eraser)** trước vì chúng liên quan trực tiếp đến trải nghiệm vẽ cốt lõi).
2. **Giao diện Thanh công cụ (Toolbar)**: Khi thêm nhiều nút (Tẩy, Undo, Redo, Save, Hình học, Nền), thanh công cụ hiện tại có thể bị dài. Bạn muốn thanh công cụ nằm ngang bên dưới/trên cùng (như hiện tại), hay gom thành một menu xổ xuống, hoặc thanh công cụ dọc bên mép màn hình?
3. **Phạm vi tính năng Hình học (Nhóm 5)**: Việc hỗ trợ vẽ hình vuông/tròn bằng cách kéo thả sẽ cần viết thêm custom adorner/logic khá nhiều. Bạn có thực sự cần tính năng này ngay lập tức không, hay chỉ ưu tiên Text và Ảnh trước?

---

## 4. Kế hoạch Triển khai (Sau khi xác nhận)

1. Cập nhật `task.md` với các tính năng được bạn chọn.
2. Tạo các lớp hỗ trợ (như `HistoryManager`).
3. Cập nhật giao diện `MainWindow.xaml` để thêm các nút mới (kích thước lớn, thân thiện với màn hình cảm ứng).
4. Cài đặt logic cho từng nhóm tính năng.
5. Kiểm tra và Build lại.
