---
name: apply-qasmartclass-toolbar
description: Áp dụng phong cách thiết kế thanh công cụ sáng màu (Light Theme) và các Vector Icon nguyên bản từ QASmartClass sang TouchBoard.
---

# Áp dụng giao diện thanh công cụ QASmartClass

Mục tiêu của skill này là thay đổi thiết kế thanh công cụ hiện tại của TouchBoard sang phong cách phẳng, sáng màu, vuông vắn hơn giống hệt QASmartClass, đồng thời tái sử dụng các biểu tượng dạng Vector (`<Path>`).

## 1. Danh sách SVG Path Data (Vector Icons) của QASmartClass

Các biểu tượng này không phải là ảnh mà là mã lệnh vẽ, cho phép hiển thị cực kỳ sắc nét trên màn hình lớn. Cách chèn vào XAML:
```xml
<Viewbox Width="24" Height="24">
    <Canvas Width="24" Height="24">
        <Path Fill="#2F3542" Data="[ĐIỀN_MÃ_VÀO_ĐÂY]" />
    </Canvas>
</Viewbox>
```

| Tên Công Cụ | Đoạn mã SVG Path Data |
| :--- | :--- |
| Bút viết (Pen) | `M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z` |
| Cục tẩy (Eraser) | `M16.24,3.56L21.19,8.5C21.97,9.29 21.97,10.55 21.19,11.34L12,20.53C10.44,22.09 7.91,22.09 6.34,20.53L2.81,17C2.03,16.21 2.03,14.95 2.81,14.16L13.41,3.56C14.2,2.78 15.46,2.78 16.24,3.56M4.22,15.58L7.76,19.11C8.54,19.9 9.8,19.9 10.59,19.11L14.12,15.58L9.17,10.63L4.22,15.58Z` |
| Undo (Hoàn tác) | `M12.5,8C9.85,8 7.45,9 5.6,10.6L2,7V16H11L7.38,12.38C8.77,11.22 10.54,10.5 12.5,10.5C16.04,10.5 19.05,12.81 20.1,16L22.47,15.22C21.08,11.03 17.15,8 12.5,8Z` |
| Redo (Làm lại) | `M18.4,10.6C16.55,9 14.15,8 11.5,8C6.85,8 2.92,11.03 1.54,15.22L3.9,16C4.95,12.81 7.95,10.5 11.5,10.5C13.45,10.5 15.23,11.22 16.62,12.38L13,16H22V7L18.4,10.6Z` |
| Chuột (Select) | `M13.64,21.97C13.14,22.21 12.54,22 12.31,21.5L10.13,16.76L7.62,18.78C7.45,18.92 7.24,19 7,19A1,1 0 0,1 6,18V3A1,1 0 0,1 7,2C7.24,2 7.47,2.09 7.64,2.23L7.65,2.22L19.14,11.86C19.57,12.22 19.62,12.85 19.27,13.27C19.12,13.45 18.91,13.57 18.7,13.61L15.54,14.23L17.74,18.96C18,19.46 17.76,20.05 17.26,20.28L13.64,21.97Z` |
| Toàn màn hình | `M3,12V6.75L9,5.43V11.91L3,12M20,3V11.75L10,11.9V5.21L20,3M3,13L9,13.09V19.9L3,18.75V13M20,13.25V22L10,20.09V13.1L20,13.25Z` |
| Hình học (Shapes) | `M3,3H11V11H3V3M13,3H21V11H13V3M3,13H11V21H3V13M18,13L22,21H14L18,13Z` |
| Xóa bảng/Thoát | `M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z` |

## 2. Cập nhật `TouchBoard\Styles\ToolbarStyles.xaml`
- Sử dụng các màu Light Theme chuẩn: 
  - `SurfaceBrush`: `#F1F2F6` (Nền nút bấm)
  - `SurfaceHoverBrush`: `#E1E8ED`
  - `AccentBrush`: `#2E86DE` (Nền nút Active)
  - `TextPrimaryBrush`: `#2F3542` (Màu Icon SVG)
- Đổi bo góc `CornerRadius` của `ToolButtonStyle` thành `10`, bỏ hoàn toàn GlowRing và DropShadow.
- Chỉnh ComboBox về Light Theme.

## 3. Cập nhật `TouchBoard\MainWindow.xaml`
- Thay `ToolbarBorder`:
  - `Background="White"`
  - `BorderBrush="#E1E8ED" BorderThickness="1"`
  - `CornerRadius="15"`
  - `DropShadowEffect` sáng màu.
- Thay toàn bộ các nút bấm bằng `Viewbox` chứa thẻ `Path` sử dụng Data lấy từ bảng trên. Nút Active sử dụng `Fill="White"`, nút thường sử dụng `Fill="{TemplateBinding Foreground}"`.
