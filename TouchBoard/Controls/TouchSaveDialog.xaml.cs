using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace TouchBoard.Controls
{
    public enum TouchSaveDialogMode { Save, ExportPdf }

    public partial class TouchSaveDialog : Window
    {
        // =====================================================
        // P/Invoke — Bàn phím ảo Windows
        // =====================================================
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        // Các tên class của TabTip trên Windows 10/11 (khác nhau theo phiên bản)
        private static readonly string[] TabTipClasses =
        {
            "IPTip_Main_Window",
            "IPTIP_Main_Window",
            "TouchKeyboardWindow"
        };

        private static readonly string? TabTipExe = FindTabTipExe();

        private static string? FindTabTipExe()
        {
            string[] paths = {
                @"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe",
                @"C:\Program Files (x86)\Common Files\Microsoft Shared\ink\TabTip.exe"
            };
            foreach (var p in paths) if (File.Exists(p)) return p;
            return null;
        }

        // =====================================================
        // State
        // =====================================================
        private readonly TouchSaveDialogMode _mode;
        private string _selectedFolder;
        private readonly string _defaultFolder;
        private readonly string _ext;

        public string ResultFilePath { get; private set; } = string.Empty;

        // =====================================================
        // Constructor
        // =====================================================
        public TouchSaveDialog(TouchSaveDialogMode mode, string defaultName = "")
        {
            InitializeComponent();
            _mode = mode;

            if (mode == TouchSaveDialogMode.Save)
            {
                _ext = ".tbproj";
                _defaultFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TouchBoard", "Projects");
                TxtDialogTitle.Text = "Lưu dự án";
                TxtHeaderIcon.Text = "\uE74E";
                SetButtonLabels("\uE74E", "Lưu");
            }
            else
            {
                _ext = ".pdf";
                _defaultFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "TouchBoard", "Exports");
                TxtDialogTitle.Text = "Xuất file PDF";
                TxtHeaderIcon.Text = "\uE8D5";
                SetButtonLabels("\uE8D5", "Xuất PDF");
            }

            _selectedFolder = _defaultFolder;
            Directory.CreateDirectory(_selectedFolder);

            TxtFileName.Text = string.IsNullOrWhiteSpace(defaultName)
                ? $"BaiGiang_{DateTime.Now:yyyyMMdd_HHmm}"
                : defaultName;

            UpdateFolderLabel();

            Loaded += (s, e) => { TxtFileName.Focus(); TxtFileName.SelectAll(); };
        }

        // Tìm nút trong template của BtnConfirm để đổi icon/label
        private void SetButtonLabels(string icon, string label)
        {
            // Thay đổi sau khi Loaded để template đã được áp dụng
            Loaded += (s, e) =>
            {
                var iconBlock = FindVisualChild<System.Windows.Controls.TextBlock>(
                    this, "IconBlock");
                var labelBlock = FindVisualChild<System.Windows.Controls.TextBlock>(
                    this, "LabelBlock");
                if (iconBlock != null) iconBlock.Text = icon;
                if (labelBlock != null) labelBlock.Text = label;
            };
        }

        // =====================================================
        // Chọn thư mục
        // =====================================================
        private void BtnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            // OpenFolderDialog có sẵn trong .NET 8 WPF — không cần thêm package
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Chọn thư mục lưu file",
                Multiselect = false,
                InitialDirectory = _selectedFolder
            };

            if (dialog.ShowDialog(this) == true)
            {
                _selectedFolder = dialog.FolderName;
                UpdateFolderLabel();
            }
        }

        private void UpdateFolderLabel()
        {
            string name = GetSanitizedFileName();
            TxtFolderPath.Text = Path.Combine(_selectedFolder, name + _ext);
        }

        // =====================================================
        // TextBox events
        // =====================================================
        private void TxtFileName_GotFocus(object sender, RoutedEventArgs e)
        {
            // Không tự bật bàn phím — chỉ hiện khi bấm nút ⌨️ bên cạnh
        }

        private void TxtFileName_TextChanged(object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateFolderLabel();
        }

        private void TxtFileName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) Confirm();
            if (e.Key == Key.Escape) Cancel();
        }

        // =====================================================
        // Nút bàn phím — toggle
        // =====================================================
        private void BtnKeyboard_Click(object sender, RoutedEventArgs e)
        {
            ToggleKeyboard();
            // Trả focus lại textbox sau khi bấm nút
            Dispatcher.BeginInvoke(new Action(() => TxtFileName.Focus()),
                System.Windows.Threading.DispatcherPriority.Input);
        }

        // =====================================================
        // ShowKeyboard: hiện bàn phím (không toggle, dùng cho GotFocus)
        // ToggleKeyboard: bật/tắt (dùng cho nút ⌨️)
        // Win+Ctrl+O = osk.exe (On-Screen Keyboard nổi)
        // =====================================================
        private static void ShowKeyboard()
        {
            // Ưu tiên: osk.exe (floating, Windows built-in, đáng tin hơn)
            var oskProcs = Process.GetProcessesByName("osk");
            if (oskProcs.Length == 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("osk.exe") { UseShellExecute = true });
                    return;
                }
                catch { }
            }
            else
            {
                // Đưa lên foreground nếu đã chạy
                foreach (var p in oskProcs)
                {
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        ShowWindow(p.MainWindowHandle, SW_SHOW);
                        SetForegroundWindow(p.MainWindowHandle);
                        return;
                    }
                }
            }

            // Fallback: TabTip
            StartTabTip();
        }

        private static void ToggleKeyboard()
        {
            // Kiểm tra OSK đã chạy chưa
            var oskProcs = Process.GetProcessesByName("osk");
            if (oskProcs.Length > 0)
            {
                // Toggle OSK
                foreach (var p in oskProcs)
                {
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        bool visible = IsWindowVisible(p.MainWindowHandle);
                        ShowWindow(p.MainWindowHandle, visible ? SW_HIDE : SW_SHOW);
                        if (!visible) SetForegroundWindow(p.MainWindowHandle);
                        return;
                    }
                }
                // Proc chạy nhưng không có window → kill và restart
                foreach (var p in oskProcs) try { p.Kill(); } catch { }
            }

            // Chưa có OSK → bật mới
            try
            {
                Process.Start(new ProcessStartInfo("osk.exe") { UseShellExecute = true });
            }
            catch
            {
                // Fallback: TabTip
                ToggleTouchKeyboard();
            }
        }

        // Giữ lại ToggleTouchKeyboard và StartTabTip cho fallback
        private static void ToggleTouchKeyboard()
        {
            // Kiểm tra TabTip đang chạy chưa
            var procs = Process.GetProcessesByName("TabTip");

            if (procs.Length > 0)
            {
                // Đang chạy → thử show cửa sổ qua tất cả class name có thể
                bool shown = false;
                foreach (var className in TabTipClasses)
                {
                    var hwnd = FindWindow(className, null);
                    if (hwnd == IntPtr.Zero) continue;

                    if (IsWindowVisible(hwnd))
                    {
                        // Đang hiện → ẩn đi (toggle)
                        ShowWindow(hwnd, SW_HIDE);
                    }
                    else
                    {
                        ShowWindow(hwnd, SW_SHOW);
                        SetForegroundWindow(hwnd);
                    }
                    shown = true;
                    break;
                }

                // Nếu không tìm được cửa sổ (xảy ra trên Win11) → kill và restart
                if (!shown)
                {
                    foreach (var p in procs)
                        try { p.Kill(); } catch { }

                    System.Threading.Thread.Sleep(300);
                    StartTabTip();
                }
            }
            else
            {
                // Chưa chạy → khởi động
                StartTabTip();
            }
        }

        private static void StartTabTip()
        {
            if (TabTipExe != null)
            {
                try { Process.Start(new ProcessStartInfo(TabTipExe) { UseShellExecute = true }); }
                catch { }
                return;
            }
            // Fallback: On-Screen Keyboard
            try { Process.Start(new ProcessStartInfo("osk.exe") { UseShellExecute = true }); }
            catch { }
        }

        private static void HideTouchKeyboard()
        {
            foreach (var className in TabTipClasses)
            {
                var hwnd = FindWindow(className, null);
                if (hwnd != IntPtr.Zero && IsWindowVisible(hwnd))
                    ShowWindow(hwnd, SW_HIDE);
            }
        }

        // =====================================================
        // Confirm / Cancel
        // =====================================================
        private void BtnConfirm_Click(object sender, RoutedEventArgs e) => Confirm();
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Cancel();

        private void Confirm()
        {
            string name = GetSanitizedFileName();
            if (string.IsNullOrWhiteSpace(name))
            {
                FileNameBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                TxtFileName.Focus();
                return;
            }

            Directory.CreateDirectory(_selectedFolder);
            ResultFilePath = Path.Combine(_selectedFolder, name + _ext);

            if (File.Exists(ResultFilePath))
            {
                var result = MessageBox.Show(
                    $"File \"{name + _ext}\" đã tồn tại.\nBạn có muốn ghi đè không?",
                    "Xác nhận ghi đè", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }

            // Ẩn bàn phím ảo khi xong
            HideTouchKeyboard();

            DialogResult = true;
            Close();
        }

        private void Cancel()
        {
            HideTouchKeyboard();
            DialogResult = false;
            Close();
        }

        private string GetSanitizedFileName()
        {
            string name = TxtFileName.Text.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");
            // Bỏ đuôi file nếu người dùng tự gõ vào
            if (name.EndsWith(_ext, StringComparison.OrdinalIgnoreCase))
                name = name[..^_ext.Length];
            return name;
        }

        // =====================================================
        // Helper: tìm control theo tên trong visual tree
        // =====================================================
        private static T? FindVisualChild<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name) return fe;
                var result = FindVisualChild<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
