---
name: Refactor TouchBoard
description: Skill dùng để refactor file MainWindow.xaml.cs khổng lồ thành kiến trúc modular trong dự án WPF.
---

# Refactor TouchBoard

Kỹ năng này hướng dẫn Agent cách chia nhỏ một file Code-behind WPF khổng lồ (như `MainWindow.xaml.cs`) thành các lớp Manager nhỏ, chuyên biệt theo tính năng.

## Mục tiêu
- **Giảm số dòng code** trong `MainWindow.xaml.cs`.
- Tách biệt logic theo từng nhóm: Tool, Color, Stroke, Selection, Canvas, Shortcut, History.
- Giữ nguyên toàn bộ hành vi của ứng dụng (không làm hỏng tính năng).

## Các bước thực hiện (Best Practices cho WPF)
1. Tạo thư mục `Managers/` trong thư mục gốc dự án WPF.
2. Tách cấu hình Resource Dictionary của XAML (nếu có) ra file `Styles/XYZ.xaml` riêng để dọn dẹp `MainWindow.xaml`.
3. Tạo các lớp `XManager` (ví dụ: `ToolManager`, `ColorManager`) trong thư mục `Managers/`.
4. Mỗi Manager sẽ nhận tham số `MainWindow window` trong hàm khởi tạo (`Constructor`) để có thể truy cập các controls nội bộ (`internal fields`) của cửa sổ.
5. Di dời logic xử lý từ `MainWindow.xaml.cs` sang Manager tương ứng.
6. Tại `MainWindow.xaml.cs`, khởi tạo các managers và map (gán) các event handlers từ các controls UI sang hàm của manager đó.

**Lưu ý khi refactor WPF:** Các elements được định danh bằng thuộc tính `x:Name="..."` trong tệp `xaml` sẽ được trình biên dịch tạo thành các trường `internal` trong lớp code-behind. Do đó, các Manager khi nhận tham chiếu `MainWindow` hoàn toàn có thể thay đổi trạng thái của các elements này (ví dụ: `_window.BtnPenMode.Style = ...`).

Hãy tuân thủ kế hoạch này mỗi khi xử lý các project WPF phức tạp không sử dụng MVVM.
