using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TouchBoard.Managers;

namespace TouchBoard
{
    /// <summary>
    /// Touch Board - Interactive Whiteboard Application
    /// Two modes: Pen (Ink) and Select (Move / Resize / Delete)
    /// Optimized for touchscreen interactive displays.
    /// </summary>
    public partial class MainWindow : Window
    {
        private ToolManager _toolManager = null!;
        private ColorManager _colorManager = null!;
        private StrokeWidthManager _strokeWidthManager = null!;
        private SelectionManager _selectionManager = null!;
        private CanvasManager _canvasManager = null!;
        private ShortcutManager _shortcutManager = null!;
        private HistoryManager _historyManager = null!;
        private BackgroundManager _backgroundManager = null!;
        private MultiTouchManager _multiTouchManager = null!;
        private PageManager _pageManager = null!;
        private NavigationManager _navigationManager = null!;

        public NavigationManager NavigationManager => _navigationManager;
        public ToolManager ToolManager => _toolManager;

        public MainWindow()
        {
            InitializeComponent();
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            _historyManager = new HistoryManager(DrawingCanvas);
            _historyManager.StateChanged += UpdateUndoRedoButtons;

            _toolManager = new ToolManager(this);
            _strokeWidthManager = new StrokeWidthManager(this, _toolManager, _historyManager);
            _colorManager = new ColorManager(this, _toolManager, _historyManager, _strokeWidthManager);
            _selectionManager = new SelectionManager(this, _toolManager, _historyManager, _colorManager, _strokeWidthManager);
            _canvasManager = new CanvasManager(this, _historyManager);
            _backgroundManager = new BackgroundManager(this, _toolManager, _colorManager);
            _shortcutManager = new ShortcutManager(this, _toolManager, _selectionManager, _historyManager, _canvasManager);
            
            _multiTouchManager = new MultiTouchManager(this, _toolManager, _historyManager);
            
            // Khởi tạo PageManager và NavigationManager
            _pageManager = new PageManager(this, _backgroundManager, _historyManager);
            _pageManager.PageChanged += OnPageChanged;
            _pageManager.PagesListChanged += OnPagesListChanged;
            
            _navigationManager = new NavigationManager(this);

            // Cập nhật giao diện trang ban đầu
            OnPagesListChanged();
            OnPageChanged();
            _colorManager.ApplyDrawingAttributes();
            UpdateUndoRedoButtons();
        }

        private void UpdateUndoRedoButtons()
        {
            BtnUndo.IsEnabled = _historyManager.CanUndo;
            BtnUndo.Opacity = _historyManager.CanUndo ? 1.0 : 0.4;

            BtnRedo.IsEnabled = _historyManager.CanRedo;
            BtnRedo.Opacity = _historyManager.CanRedo ? 1.0 : 0.4;
        }

        // ═══════════════════════════════════════════════════
        // EVENT HANDLERS DELEGATED TO MANAGERS
        // ═══════════════════════════════════════════════════

        // Mode Switching
        private void BtnPenMode_Click(object sender, RoutedEventArgs e)
        {
            if (_toolManager.CurrentMode == ToolMode.Pen)
            {
                PenSettingsPopup.IsOpen = !PenSettingsPopup.IsOpen;
            }
            else
            {
                _toolManager.SwitchToMode(ToolMode.Pen);
                PenSettingsPopup.IsOpen = false;
            }
        }
        private void BtnSelectMode_Click(object sender, RoutedEventArgs e) => _toolManager.SwitchToMode(ToolMode.Select);
        private ToolMode _lastEraserMode = ToolMode.EraserStroke;

        private void BtnEraserMode_Click(object sender, RoutedEventArgs e)
        {
            if (_toolManager.CurrentMode == ToolMode.EraserStroke || _toolManager.CurrentMode == ToolMode.EraserPoint)
            {
                EraserSettingsPopup.IsOpen = !EraserSettingsPopup.IsOpen;
            }
            else
            {
                _toolManager.SwitchToMode(_lastEraserMode); // Remember last mode
                EraserSettingsPopup.IsOpen = false;
            }
        }

        private void BtnEraserType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string eraserType)
            {
                if (eraserType == "Stroke")
                {
                    _lastEraserMode = ToolMode.EraserStroke;
                    _toolManager.SwitchToMode(ToolMode.EraserStroke);
                    BtnEraserTypeStroke.Style = (Style)FindResource("ActiveToolButtonStyle");
                    BtnEraserTypePoint.Style = (Style)FindResource("ToolButtonStyle");
                    PanelEraserSize.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _lastEraserMode = ToolMode.EraserPoint;
                    _toolManager.SwitchToMode(ToolMode.EraserPoint);
                    BtnEraserTypeStroke.Style = (Style)FindResource("ToolButtonStyle");
                    BtnEraserTypePoint.Style = (Style)FindResource("ActiveToolButtonStyle");
                    PanelEraserSize.Visibility = Visibility.Visible;
                }
            }
        }

        private void SliderEraserSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DrawingCanvas != null)
            {
                DrawingCanvas.EraserShape = new System.Windows.Ink.EllipseStylusShape(e.NewValue, e.NewValue);
            }
        }

        // Color & Stroke
        private void BtnColor_Click(object sender, RoutedEventArgs e) 
        {
            _colorManager.HandleColorClick(sender);
            PenSettingsPopup.IsOpen = false;
        }
        private void SliderStrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_strokeWidthManager != null && _colorManager != null)
            {
                _strokeWidthManager.HandleStrokeWidthChanged(e.NewValue, _colorManager.ApplyDrawingAttributes);
            }
        }

        private bool _isCustomColorForSelection = false;

        private void BtnCustomColor_Click(object sender, RoutedEventArgs e)
        {
            _isCustomColorForSelection = false;
            CustomColorPopup.PlacementTarget = BtnColorCustom;
            CustomColorPopup.IsOpen = true;
            UpdateCustomColorPreview();
        }

        private void BtnPenType_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string penType)
            {
                if (penType == "Highlighter")
                {
                    DrawingCanvas.DefaultDrawingAttributes.IsHighlighter = true;
                    BtnPenNormal.Style = (Style)FindResource("ToolButtonStyle");
                    BtnPenHighlighter.Style = (Style)FindResource("ActiveToolButtonStyle");
                }
                else
                {
                    DrawingCanvas.DefaultDrawingAttributes.IsHighlighter = false;
                    BtnPenNormal.Style = (Style)FindResource("ActiveToolButtonStyle");
                    BtnPenHighlighter.Style = (Style)FindResource("ToolButtonStyle");
                }
                PenSettingsPopup.IsOpen = false;
            }
        }

        // Actions
        private void BtnDeleteSelected_Click(object sender, RoutedEventArgs e) => _selectionManager.DeleteSelectedStrokes();
        private void BtnClearAll_Click(object sender, RoutedEventArgs e) { _canvasManager.ClearAll(); HideSelectionContext(); }
        private void BtnUndo_Click(object sender, RoutedEventArgs e) { _historyManager.Undo(); HideSelectionContext(); }
        private void BtnRedo_Click(object sender, RoutedEventArgs e) { _historyManager.Redo(); HideSelectionContext(); }
        private void BtnFullscreen_Click(object sender, RoutedEventArgs e) => _canvasManager.ToggleFullscreen();

        // Insert Menu Actions
        private void BtnInsertMode_Click(object sender, RoutedEventArgs e)
        {
            InsertPopup.IsOpen = !InsertPopup.IsOpen;
        }

        private void BtnInsertImage_Click(object sender, RoutedEventArgs e)
        {
            InsertPopup.IsOpen = false;
            MessageBox.Show("Tính năng chèn ảnh đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnInsertRuler_Click(object sender, RoutedEventArgs e)
        {
            BtnUnderConstruction_Click(sender, e);
        }

        private void BtnInsertSetSquare_Click(object sender, RoutedEventArgs e)
        {
            BtnUnderConstruction_Click(sender, e);
        }

        private void BtnInsertProtractor_Click(object sender, RoutedEventArgs e)
        {
            BtnUnderConstruction_Click(sender, e);
        }

        private void BtnInsertCompass_Click(object sender, RoutedEventArgs e)
        {
            BtnUnderConstruction_Click(sender, e);
        }

        private void BtnUnderConstruction_Click(object sender, RoutedEventArgs e)
        {
            InsertPopup.IsOpen = false;
            MessageBox.Show("Tính năng này đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HideSelectionContext()
        {
            SelectionMenuButton.Visibility = System.Windows.Visibility.Collapsed;
            SelectionPopup.IsOpen = false;
        }

        // Canvas Background
        // ==========================================
        // PAGES MANAGEMENT UI EVENT HANDLERS
        // ==========================================
        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_pageManager.CurrentPageIndex > 0)
                _pageManager.SwitchToPage(_pageManager.CurrentPageIndex - 1);
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_pageManager.CurrentPageIndex < _pageManager.Pages.Count - 1)
                _pageManager.SwitchToPage(_pageManager.CurrentPageIndex + 1);
        }

        private void BtnPages_Click(object sender, RoutedEventArgs e)
        {
            PagesPopup.IsOpen = !PagesPopup.IsOpen;
            if (PagesPopup.IsOpen)
            {
                PanelAddPageTypes.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnAddPage_Click(object sender, RoutedEventArgs e)
        {
            PanelAddPageTypes.Visibility = PanelAddPageTypes.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        // Track selected pattern & theme for new page creation
        private TouchBoard.Managers.BackgroundPattern _newPagePattern = TouchBoard.Managers.BackgroundPattern.Plain;
        private TouchBoard.Managers.BackgroundTheme _newPageTheme = TouchBoard.Managers.BackgroundTheme.Light;

        private void BtnNewPattern_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string patStr && System.Enum.TryParse(patStr, out TouchBoard.Managers.BackgroundPattern pat))
            {
                _newPagePattern = pat;
                // Update button styles
                BtnNewPatternPlain.Style = (Style)FindResource("ToolButtonStyle");
                BtnNewPatternGrid.Style = (Style)FindResource("ToolButtonStyle");
                BtnNewPatternRuled.Style = (Style)FindResource("ToolButtonStyle");
                btn.Style = (Style)FindResource("ActiveToolButtonStyle");
            }
        }

        private void BtnNewTheme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string thStr && System.Enum.TryParse(thStr, out TouchBoard.Managers.BackgroundTheme th))
            {
                _newPageTheme = th;
                BtnNewThemeDark.Style = (Style)FindResource("ToolButtonStyle");
                BtnNewThemeLight.Style = (Style)FindResource("ToolButtonStyle");
                BtnNewThemeBlackboard.Style = (Style)FindResource("ToolButtonStyle");
                btn.Style = (Style)FindResource("ActiveToolButtonStyle");
            }
        }

        private void BtnCreateNewPage_Click(object sender, RoutedEventArgs e)
        {
            _pageManager.AddPage(_newPagePattern, _newPageTheme);
            PanelAddPageTypes.Visibility = Visibility.Collapsed;
            PagesPopup.IsOpen = false;
        }

        private void BtnDeletePage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is System.Guid pageId)
            {
                for (int i = 0; i < _pageManager.Pages.Count; i++)
                {
                    if (_pageManager.Pages[i].Id == pageId)
                    {
                        _pageManager.DeletePage(i);
                        break;
                    }
                }
            }
        }

        private void LstPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstPages.SelectedIndex >= 0 && LstPages.SelectedIndex != _pageManager.CurrentPageIndex)
            {
                _pageManager.SwitchToPage(LstPages.SelectedIndex);
            }
        }

        private void OnPageChanged()
        {
            LstPages.SelectedIndex = _pageManager.CurrentPageIndex;
            UpdateUndoRedoButtons();
            
            // Re-apply the current tool mode because PageManager clears selection (which internally sets EditingMode to Select)
            if (_toolManager != null)
            {
                _toolManager.SwitchToMode(_toolManager.CurrentMode);
            }
        }

        private void OnPagesListChanged()
        {
            LstPages.ItemsSource = null;
            LstPages.ItemsSource = _pageManager.Pages;
        }

        // ==========================================
        // LstPages DRAG & DROP (Canva-style)
        // ==========================================
        private Point _dragStartPoint;
        private System.Guid _changeBgTargetPageId;

        private void LstPages_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void LstPages_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var listBoxItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
                    if (listBoxItem != null && FindVisualParent<Button>((DependencyObject)e.OriginalSource) == null)
                    {
                        DragDrop.DoDragDrop(listBoxItem, listBoxItem.DataContext, DragDropEffects.Move);
                    }
                }
            }
        }

        private void LstPages_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (listBoxItem != null && FindVisualParent<Button>((DependencyObject)e.OriginalSource) == null)
            {
                PagesPopup.IsOpen = false;
            }
        }

        private void LstPages_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;

            // Ẩn tất cả indicators trước
            ClearAllInsertIndicators();

            // Tìm ListBoxItem đang hover
            var targetItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null) return;

            // Xác định chuột ở nửa trái hay nửa phải
            Point pos = e.GetPosition(targetItem);
            double halfWidth = targetItem.ActualWidth / 2;

            // Tìm Grid con bên trong DataTemplate
            var contentPresenter = FindVisualChild<System.Windows.Controls.ContentPresenter>(targetItem);
            if (contentPresenter == null) return;
            var grid = VisualTreeHelper.GetChild(contentPresenter, 0) as Grid;
            if (grid == null) return;

            if (pos.X < halfWidth)
            {
                // Hiển thị vạch bên trái
                var leftIndicator = FindChildByName<Border>(grid, "LeftInsertIndicator");
                if (leftIndicator != null) leftIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                // Hiển thị vạch bên phải
                var rightIndicator = FindChildByName<Border>(grid, "RightInsertIndicator");
                if (rightIndicator != null) rightIndicator.Visibility = Visibility.Visible;
            }
        }

        private void LstPages_DragLeave(object sender, DragEventArgs e)
        {
            ClearAllInsertIndicators();
        }

        private void LstPages_Drop(object sender, DragEventArgs e)
        {
            ClearAllInsertIndicators();

            if (!e.Data.GetDataPresent(typeof(TouchBoard.Models.PageModel))) return;

            var droppedData = (TouchBoard.Models.PageModel)e.Data.GetData(typeof(TouchBoard.Models.PageModel));
            var targetItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null || !(targetItem.DataContext is TouchBoard.Models.PageModel targetData)) return;

            int oldIndex = _pageManager.Pages.IndexOf(droppedData);
            int targetIndex = _pageManager.Pages.IndexOf(targetData);
            if (oldIndex == -1 || targetIndex == -1 || oldIndex == targetIndex) return;

            // Xác định chèn trước hay sau dựa trên vị trí chuột
            Point pos = e.GetPosition(targetItem);
            double halfWidth = targetItem.ActualWidth / 2;
            int newIndex = pos.X < halfWidth ? targetIndex : targetIndex;

            // Nếu kéo từ trái sang phải, chèn vào vị trí sau target
            if (pos.X >= halfWidth && oldIndex < targetIndex)
                newIndex = targetIndex;
            else if (pos.X >= halfWidth && oldIndex > targetIndex)
                newIndex = targetIndex + 1;
            else if (pos.X < halfWidth && oldIndex > targetIndex)
                newIndex = targetIndex;
            else if (pos.X < halfWidth && oldIndex < targetIndex)
                newIndex = targetIndex - 1;

            if (newIndex < 0) newIndex = 0;
            if (newIndex >= _pageManager.Pages.Count) newIndex = _pageManager.Pages.Count - 1;
            if (oldIndex != newIndex)
            {
                _pageManager.MovePage(oldIndex, newIndex);
            }
        }

        private void ClearAllInsertIndicators()
        {
            for (int i = 0; i < LstPages.Items.Count; i++)
            {
                var container = LstPages.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (container == null) continue;
                var cp = FindVisualChild<System.Windows.Controls.ContentPresenter>(container);
                if (cp == null) continue;
                var grid = VisualTreeHelper.GetChild(cp, 0) as Grid;
                if (grid == null) continue;

                var left = FindChildByName<Border>(grid, "LeftInsertIndicator");
                var right = FindChildByName<Border>(grid, "RightInsertIndicator");
                if (left != null) left.Visibility = Visibility.Collapsed;
                if (right != null) right.Visibility = Visibility.Collapsed;
            }
        }

        // ==========================================
        // CHANGE PAGE BACKGROUND (Panel-based)
        // ==========================================
        private TouchBoard.Managers.BackgroundPattern _editPattern;
        private TouchBoard.Managers.BackgroundTheme _editTheme;

        private void BtnChangePageBg_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is System.Guid pageId)
            {
                _changeBgTargetPageId = pageId;

                // Tìm trang đang chỉnh để highlight đúng nút
                var page = _pageManager.Pages.FirstOrDefault(p => p.Id == pageId);
                if (page == null) return;

                _editPattern = page.Pattern;
                _editTheme = page.Theme;

                // Highlight nút Pattern
                BtnEditPatternPlain.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditPatternGrid.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditPatternRuled.Style = (Style)FindResource("ToolButtonStyle");
                switch (_editPattern)
                {
                    case Managers.BackgroundPattern.Plain: BtnEditPatternPlain.Style = (Style)FindResource("ActiveToolButtonStyle"); break;
                    case Managers.BackgroundPattern.Grid: BtnEditPatternGrid.Style = (Style)FindResource("ActiveToolButtonStyle"); break;
                    case Managers.BackgroundPattern.Ruled: BtnEditPatternRuled.Style = (Style)FindResource("ActiveToolButtonStyle"); break;
                }

                // Highlight nút Theme
                BtnEditThemeDark.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditThemeLight.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditThemeBlackboard.Style = (Style)FindResource("ToolButtonStyle");
                switch (_editTheme)
                {
                    case Managers.BackgroundTheme.Dark: BtnEditThemeDark.Style = (Style)FindResource("ActiveToolButtonStyle"); break;
                    case Managers.BackgroundTheme.Light: BtnEditThemeLight.Style = (Style)FindResource("ActiveToolButtonStyle"); break;
                    case Managers.BackgroundTheme.Blackboard: BtnEditThemeBlackboard.Style = (Style)FindResource("ActiveToolButtonStyle"); break;
                }

                TxtChangeBgTitle.Text = $"Đổi nền — {page.Title}";
                
                // Đóng popup danh sách trang để tránh xung đột focus/hit-test
                PagesPopup.IsOpen = false;
                
                // Mở popup đổi nền ở giữa màn hình chính
                ChangeBgPopup.PlacementTarget = this;
                ChangeBgPopup.IsOpen = true;
            }
        }

        private void BtnEditPattern_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string patStr && System.Enum.TryParse(patStr, out TouchBoard.Managers.BackgroundPattern pat))
            {
                _editPattern = pat;
                BtnEditPatternPlain.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditPatternGrid.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditPatternRuled.Style = (Style)FindResource("ToolButtonStyle");
                btn.Style = (Style)FindResource("ActiveToolButtonStyle");

                // Áp dụng ngay
                _pageManager.ChangePagePattern(_changeBgTargetPageId, pat);
            }
        }

        private void BtnEditTheme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string thStr && System.Enum.TryParse(thStr, out TouchBoard.Managers.BackgroundTheme th))
            {
                _editTheme = th;
                BtnEditThemeDark.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditThemeLight.Style = (Style)FindResource("ToolButtonStyle");
                BtnEditThemeBlackboard.Style = (Style)FindResource("ToolButtonStyle");
                btn.Style = (Style)FindResource("ActiveToolButtonStyle");

                // Áp dụng ngay
                _pageManager.ChangePageTheme(_changeBgTargetPageId, th);
            }
        }

        private void BtnApplyChangeBg_Click(object sender, RoutedEventArgs e)
        {
            ChangeBgPopup.IsOpen = false;
        }

        private void BtnCloseChangeBg_Click(object sender, RoutedEventArgs e)
        {
            ChangeBgPopup.IsOpen = false;
        }

        // ==========================================
        // ==========================================
        // TOUCH DRAG & DROP (for touch screens)
        // ==========================================
        private Point _touchDragStartPoint;
        private int _touchDragDeviceId = -1;
        private bool _touchDragInProgress = false;
        private DispatcherTimer? _longPressTimer;
        private ListBoxItem? _longPressedItem;

        private void LstPages_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _touchDragStartPoint = e.GetTouchPoint(null).Position;
            _touchDragDeviceId = e.TouchDevice.Id;
            _touchDragInProgress = false;

            var listBoxItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (listBoxItem != null && FindVisualParent<Button>((DependencyObject)e.OriginalSource) == null)
            {
                _longPressedItem = listBoxItem;
                _longPressTimer?.Stop();
                _longPressTimer = new DispatcherTimer { Interval = System.TimeSpan.FromMilliseconds(400) };
                _longPressTimer.Tick += LongPressTimer_Tick;
                _longPressTimer.Start();
            }
        }

        private void LongPressTimer_Tick(object? sender, System.EventArgs e)
        {
            _longPressTimer?.Stop();
            if (_longPressedItem != null)
            {
                _touchDragInProgress = true; // Sẵn sàng để drag
                
                // Hiệu ứng "nhấc lên"
                var scaleTrans = new ScaleTransform(1.0, 1.0);
                _longPressedItem.RenderTransform = scaleTrans;
                _longPressedItem.RenderTransformOrigin = new Point(0.5, 0.5);

                var anim = new DoubleAnimation(1.0, 0.95, System.TimeSpan.FromMilliseconds(150));
                scaleTrans.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                scaleTrans.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }

        private void LstPages_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (e.TouchDevice.Id != _touchDragDeviceId) return;

            var position = e.GetTouchPoint(null).Position;
            
            if (!_touchDragInProgress)
            {
                // Nếu chưa long press mà đã di chuyển xa -> Hủy long press (để scroll ngang)
                if (System.Math.Abs(position.X - _touchDragStartPoint.X) > 15 ||
                    System.Math.Abs(position.Y - _touchDragStartPoint.Y) > 15)
                {
                    _longPressTimer?.Stop();
                    _longPressedItem = null;
                }
            }
            else
            {
                // Đã long press xong, bắt đầu kéo thả
                if (System.Math.Abs(position.X - _touchDragStartPoint.X) > 15 ||
                    System.Math.Abs(position.Y - _touchDragStartPoint.Y) > 15)
                {
                    if (_longPressedItem != null)
                    {
                        e.TouchDevice.Capture(null);
                        e.Handled = true;
                        
                        DragDrop.DoDragDrop(_longPressedItem, _longPressedItem.DataContext, DragDropEffects.Move);
                        
                        // Hủy hiệu ứng nhấc lên sau khi kết thúc drag
                        _longPressedItem.RenderTransform = null;
                        _longPressedItem = null;
                        _touchDragInProgress = false;
                    }
                }
            }
        }

        private void LstPages_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (e.TouchDevice.Id == _touchDragDeviceId)
            {
                _longPressTimer?.Stop();
                
                if (_longPressedItem != null)
                {
                    _longPressedItem.RenderTransform = null; // Xóa hiệu ứng nếu chưa kịp drag
                }

                if (!_touchDragInProgress)
                {
                    // Nếu không kéo, click để đóng popup
                    var listBoxItem = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
                    if (listBoxItem != null && FindVisualParent<Button>((DependencyObject)e.OriginalSource) == null)
                    {
                        PagesPopup.IsOpen = false;
                    }
                }
                
                _touchDragDeviceId = -1;
                _touchDragInProgress = false;
                _longPressedItem = null;
            }
        }

        // ==========================================
        // VISUAL TREE HELPERS
        // ==========================================
        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindVisualParent<T>(parentObject);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result) return result;
                var found = FindVisualChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }

        private static T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name) return fe;
                var found = FindChildByName<T>(child, name);
                if (found != null) return found;
            }
            return null;
        }

        // Keyboard Shortcuts
        private void Window_KeyDown(object sender, KeyEventArgs e) => _shortcutManager.HandleKeyDown(e);

        // ═══════════════════════════════════════════════════
        // WINDOW DRAG (Click anywhere on background to move)
        // ═══════════════════════════════════════════════════
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // ═══════════════════════════════════════════════════
        // SELECTION CONTEXT MENU (⋯)
        // ═══════════════════════════════════════════════════

        private void SelectionMenuButton_Click(object sender, RoutedEventArgs e)
        {
            _selectionManager.ToggleContextMenu();
        }

        private void PopupColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string colorHex)
                _selectionManager.ChangeSelectionColor(colorHex);
        }

        private void SliderSelectionStrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_selectionManager != null)
            {
                _selectionManager.ChangeSelectionStrokeWidth(e.NewValue);
            }
        }

        private void PopupCustomColor_Click(object sender, RoutedEventArgs e)
        {
            _isCustomColorForSelection = true;
            CustomColorPopup.PlacementTarget = sender as UIElement;
            CustomColorPopup.IsOpen = true;
            UpdateCustomColorPreview();
        }

        // ==========================================
        // CUSTOM COLOR POPUP LOGIC
        // ==========================================

        private void UpdateCustomColorPreview()
        {
            if (SliderR == null || SliderG == null || SliderB == null || CustomColorPreview == null) return;
            var color = Color.FromRgb((byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
            CustomColorPreview.Background = new SolidColorBrush(color);
        }

        private void RgbSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateCustomColorPreview();
        }

        private void CustomPaletteColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Background is SolidColorBrush brush)
            {
                SliderR.Value = brush.Color.R;
                SliderG.Value = brush.Color.G;
                SliderB.Value = brush.Color.B;
                UpdateCustomColorPreview();
            }
        }

        private void BtnApplyCustomColor_Click(object sender, RoutedEventArgs e)
        {
            if (CustomColorPreview.Background is SolidColorBrush brush)
            {
                string hex = brush.Color.ToString();

                if (_isCustomColorForSelection)
                {
                    _selectionManager.ChangeSelectionColor(hex);
                }
                else
                {
                    BtnColorCustom.Background = brush;
                    BtnColorCustom.Tag = hex;
                    _colorManager.HandleColorClick(BtnColorCustom);
                    PenSettingsPopup.IsOpen = false;
                }
            }
            CustomColorPopup.IsOpen = false;
        }

        private void PopupCopy_Click(object sender, RoutedEventArgs e)
        {
            _selectionManager.CopySelection();
            SelectionPopup.IsOpen = false;
        }

        private void PopupDelete_Click(object sender, RoutedEventArgs e)
        {
            _selectionManager.DeleteSelectedStrokes();
        }
    }

    public class BackgroundTypeToBrushConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TouchBoard.Managers.BackgroundTheme theme)
            {
                var color = TouchBoard.Managers.BackgroundManager.GetThemeBgColor(theme);
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}