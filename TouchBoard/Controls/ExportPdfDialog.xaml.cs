using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TouchBoard.Controls
{
    public partial class ExportPdfDialog : Window
    {
        private readonly int _totalPages;
        private readonly int _currentPageIndex;
        private readonly HashSet<int> _selectedPages = new();

        public int[] SelectedPageIndices => _selectedPages.OrderBy(p => p).ToArray();

        // =====================================================
        // Constructor
        // =====================================================
        public ExportPdfDialog(int totalPages, int currentPageIndex)
        {
            InitializeComponent();
            _totalPages = totalPages;
            _currentPageIndex = currentPageIndex;

            Loaded += (s, e) =>
            {
                BuildPageCards();
                // Mặc định: chọn tất cả trang
                SelectAll();
            };
        }

        // =====================================================
        // Xây dựng lưới thẻ trang
        // =====================================================
        private void BuildPageCards()
        {
            PageCardsPanel.Children.Clear();

            for (int i = 0; i < _totalPages; i++)
            {
                int pageIndex = i;
                bool isCurrent = (i == _currentPageIndex);

                // Dùng Button thay vì Border để touch tap được nhận ngay
                // (Button xử lý touch event natively, không cần giữ)
                var btn = new Button
                {
                    Width = 68,
                    Height = 80,
                    Margin = new Thickness(4),
                    Cursor = Cursors.Hand,
                    Tag = pageIndex,
                    Background = new SolidColorBrush(Color.FromRgb(0xF1, 0xF2, 0xF6)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)),
                    BorderThickness = new Thickness(1.5),
                    Template = CreateCardTemplate(i + 1, isCurrent),
                };
                btn.Click += (s, e) => TogglePage(pageIndex);
                PageCardsPanel.Children.Add(btn);
            }
        }

        private static ControlTemplate CreateCardTemplate(int pageNumber, bool isCurrent)
        {
            var template = new ControlTemplate(typeof(Button));

            // Root border
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            borderFactory.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            borderFactory.SetBinding(Border.BorderBrushProperty,
                new System.Windows.Data.Binding("BorderBrush")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            borderFactory.SetBinding(Border.BorderThicknessProperty,
                new System.Windows.Data.Binding("BorderThickness")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });

            // Stack panel
            var stackFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Icon
            var iconFactory = new FrameworkElementFactory(typeof(TextBlock));
            iconFactory.SetValue(TextBlock.TextProperty, "\uE8A5");
            iconFactory.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe MDL2 Assets"));
            iconFactory.SetValue(TextBlock.FontSizeProperty, 22.0);
            iconFactory.SetValue(TextBlock.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)));
            iconFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            iconFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 8, 0, 4));
            iconFactory.Name = "PageIcon";
            stackFactory.AppendChild(iconFactory);

            // "Hiện tại" badge
            if (isCurrent)
            {
                var badgeFactory = new FrameworkElementFactory(typeof(Border));
                badgeFactory.SetValue(Border.BackgroundProperty,
                    new SolidColorBrush(Color.FromRgb(0x2E, 0x86, 0xDE)));
                badgeFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
                badgeFactory.SetValue(Border.PaddingProperty, new Thickness(4, 1, 4, 1));
                badgeFactory.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                badgeFactory.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, 2));

                var badgeText = new FrameworkElementFactory(typeof(TextBlock));
                badgeText.SetValue(TextBlock.TextProperty, "hiện tại");
                badgeText.SetValue(TextBlock.FontSizeProperty, 9.0);
                badgeText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
                badgeText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                badgeFactory.AppendChild(badgeText);
                stackFactory.AppendChild(badgeFactory);
            }

            // Page number
            var numFactory = new FrameworkElementFactory(typeof(TextBlock));
            numFactory.SetValue(TextBlock.TextProperty, $"Trang {pageNumber}");
            numFactory.SetValue(TextBlock.FontSizeProperty, 11.0);
            numFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            numFactory.SetValue(TextBlock.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)));
            numFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            numFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 8));
            stackFactory.AppendChild(numFactory);

            borderFactory.AppendChild(stackFactory);
            template.VisualTree = borderFactory;
            return template;
        }

        // =====================================================
        // Toggle chọn trang
        // =====================================================
        private void TogglePage(int pageIndex)
        {
            if (_selectedPages.Contains(pageIndex))
                _selectedPages.Remove(pageIndex);
            else
                _selectedPages.Add(pageIndex);

            RefreshCardVisuals();
            UpdateSelectionCount();
        }

        private void RefreshCardVisuals()
        {
            foreach (Button btn in PageCardsPanel.Children)
            {
                if (btn.Tag is int idx)
                {
                    bool selected = _selectedPages.Contains(idx);
                    btn.Background = selected
                        ? new SolidColorBrush(Color.FromRgb(0xEB, 0xF4, 0xFF))
                        : new SolidColorBrush(Color.FromRgb(0xF1, 0xF2, 0xF6));
                    btn.BorderBrush = selected
                        ? new SolidColorBrush(Color.FromRgb(0x2E, 0x86, 0xDE))
                        : new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB));
                    btn.BorderThickness = selected ? new Thickness(2) : new Thickness(1.5);
                }
            }
        }

        private void UpdateSelectionCount()
        {
            int count = _selectedPages.Count;
            TxtSelectionCount.Text = count == 0
                ? "Chưa chọn trang nào"
                : $"Đã chọn {count} / {_totalPages} trang";

            TxtSelectionCount.Foreground = count == 0
                ? Brushes.Red
                : new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));
        }

        // =====================================================
        // Quick select buttons
        // =====================================================
        private void BtnSelectAll_Click(object sender, RoutedEventArgs e) => SelectAll();
        private void BtnSelectCurrent_Click(object sender, RoutedEventArgs e) => SelectCurrent();
        private void BtnSelectNone_Click(object sender, RoutedEventArgs e) => SelectNone();

        private void SelectAll()
        {
            _selectedPages.Clear();
            for (int i = 0; i < _totalPages; i++) _selectedPages.Add(i);
            RefreshCardVisuals();
            UpdateSelectionCount();
        }

        private void SelectCurrent()
        {
            _selectedPages.Clear();
            _selectedPages.Add(_currentPageIndex);
            RefreshCardVisuals();
            UpdateSelectionCount();
        }

        private void SelectNone()
        {
            _selectedPages.Clear();
            RefreshCardVisuals();
            UpdateSelectionCount();
        }

        // =====================================================
        // OK / Cancel
        // =====================================================
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPages.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một trang để xuất.",
                    "Chưa chọn trang", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
