# Kế hoạch Thiết kế lại Giao diện (UI Redesign) - Phong cách Umind / Ipro

Dựa trên nghiên cứu về các phần mềm bảng tương tác hiện đại như **UMind (Riotouch)** và **IPro/Prowise**, giao diện của chúng thường tập trung vào trải nghiệm "không giới hạn" (infinite canvas) với các thanh công cụ nổi (floating toolbars) tinh tế, thay vì một dải menu cố định chiếm diện tích như các ứng dụng desktop truyền thống.

## Đề xuất Thay đổi Giao diện (UI Proposed Changes)

### 1. Chuyển đổi Thanh công cụ (Toolbar) sang dạng Floating Dock
- **Hiện tại:** Thanh công cụ là một `Border` cố định nằm ngang ở mép trên cùng của màn hình.
- **Thay đổi:** 
  - Gộp chung Toolbar và InkCanvas vào cùng một `Grid` để Toolbar **nổi (float)** đè lên trên Canvas.
  - Vị trí: Đặt ở **chính giữa cạnh dưới** màn hình (hoặc hai bên lề), giống như dock của smartphone/tablet, giúp giáo viên dễ dàng thao tác bằng tay khi đứng trước màn hình lớn.
  - Thiết kế: Chuyển sang dạng hình viên thuốc (pill-shape) bo góc tròn (`CornerRadius="24"`), màu nền bán trong suốt (semi-transparent) với hiệu ứng bóng đổ (DropShadow) để tạo cảm giác hiện đại (Glassmorphism).

### 2. Sử dụng Font Icon chuyên nghiệp
- **Hiện tại:** Đang sử dụng các Emoji (✏️, 👆, 🧽) làm biểu tượng. Trông hơi không chuyên nghiệp.
- **Thay đổi:** Sử dụng font chữ biểu tượng hệ thống của Windows như `Segoe Fluent Icons` (Win 11) hoặc `Segoe MDL2 Assets` (Win 10). Các biểu tượng sẽ là dạng nét thanh (outline/monoline), đồng nhất, chuyên nghiệp như các app UWP/Fluent Design.

### 3. Tối ưu hóa Nhóm Công cụ (Grouping)
- Thiết kế lại các nhóm nút bấm để gọn gàng hơn.
- Nhóm Bút/Tẩy, Nhóm Màu Sắc/Độ dày nét, Nhóm Hoàn tác/Thao tác bảng.
- Căn chỉnh lại `ComboBox` chọn nền bảng cho đồng bộ với thiết kế nổi.

---

## Các File Bị Ảnh Hưởng (Modified Files)

### `TouchBoard/MainWindow.xaml`
- Thay đổi cấu trúc `<Grid>` chính. Bỏ `RowDefinitions`.
- Đưa `InkCanvas` ra làm lớp nền dưới cùng.
- Đặt `ToolbarBorder` vào một `Border` nổi, `VerticalAlignment="Bottom"`, `HorizontalAlignment="Center"`, `Margin="0,0,0,32"`.
- Thêm `DropShadowEffect` cho thanh công cụ.
- Đổi các Emoji `<TextBlock>` sang các biểu tượng mã Unicode của `Segoe Fluent Icons`.

### `TouchBoard/Styles/ToolbarStyles.xaml`
- Cập nhật lại `ToolButtonStyle` và các style liên quan để loại bỏ viền vuông vức, chuyển sang thiết kế phẳng, bo góc mềm mại hơn, màu sắc hài hòa với nền bán trong suốt.

---

> [!IMPORTANT]
> ## Cần Xác Nhận (User Review Required)
> 
> 1. **Vị trí thanh công cụ:** Bạn muốn thanh công cụ nổi nằm ở **dưới cùng ở giữa (Bottom Center)** (phổ biến nhất trên màn hình tương tác) hay nằm dọc ở bên trái/phải màn hình?
> 2. **Biểu tượng (Icons):** Tôi sẽ sử dụng Font hệ thống `Segoe Fluent Icons` của Windows để thay thế Emoji. Bạn có đồng ý với thay đổi này không?

Sau khi bạn xác nhận, tôi sẽ tiến hành cập nhật lại file XAML và Styles ngay lập tức.
