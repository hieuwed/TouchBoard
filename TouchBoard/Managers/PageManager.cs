using System;
using System.Collections.ObjectModel;
using System.Windows;
using TouchBoard.Models;

namespace TouchBoard.Managers
{
    public class PageManager
    {
        private readonly MainWindow _window;
        private readonly BackgroundManager _backgroundManager;
        private readonly HistoryManager _historyManager;

        public ObservableCollection<PageModel> Pages { get; private set; }
        public int CurrentPageIndex { get; private set; }

        public event Action? PageChanged;
        public event Action? PagesListChanged;

        public PageManager(MainWindow window, BackgroundManager backgroundManager, HistoryManager historyManager)
        {
            _window = window;
            _backgroundManager = backgroundManager;
            _historyManager = historyManager;

            Pages = new ObservableCollection<PageModel>();
            
            // Khởi tạo trang đầu tiên
            AddPage(BackgroundPattern.Plain, BackgroundTheme.Dark, false);
            SwitchToPage(0);
        }

        public PageModel CurrentPage => Pages[CurrentPageIndex];

        public void AddPage(BackgroundPattern pattern, BackgroundTheme theme, bool switchToNew = true)
        {
            var newPage = new PageModel
            {
                Title = $"Trang {Pages.Count + 1}",
                Pattern = pattern,
                Theme = theme
            };
            
            // Lưu trạng thái trống đầu tiên vào lịch sử
            using (var ms = new System.IO.MemoryStream())
            {
                newPage.Strokes.Save(ms);
                newPage.UndoStack.Push(ms.ToArray());
            }

            Pages.Add(newPage);
            PagesListChanged?.Invoke();

            if (switchToNew)
            {
                SwitchToPage(Pages.Count - 1);
            }
        }

        public void SwitchToPage(int index)
        {
            if (index < 0 || index >= Pages.Count) return;

            // Xóa vùng chọn trước khi đổi trang để ẩn menu 3 chấm
            _window.DrawingCanvas.Select(new System.Windows.Ink.StrokeCollection());

            // Lưu dữ liệu của trang hiện tại (nếu đang ở một trang hợp lệ)
            if (Pages.Count > 0 && CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
            {
                Pages[CurrentPageIndex].Strokes = _window.DrawingCanvas.Strokes.Clone();
            }

            CurrentPageIndex = index;
            var newPage = Pages[index];

            // Tạm thời vô hiệu hóa việc ghi đè history trong khi switch
            _historyManager.SetStacks(newPage.UndoStack, newPage.RedoStack);
            
            _window.DrawingCanvas.Strokes = newPage.Strokes.Clone();
            _backgroundManager.SetBackground(newPage.Pattern, newPage.Theme);
            
            PageChanged?.Invoke();
        }

        public void DeletePage(int index)
        {
            if (Pages.Count <= 1)
            {
                MessageBox.Show("Không thể xóa trang cuối cùng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (index < 0 || index >= Pages.Count) return;

            Pages.RemoveAt(index);
            UpdatePageTitles();

            if (CurrentPageIndex >= Pages.Count)
            {
                SwitchToPage(Pages.Count - 1);
            }
            else if (CurrentPageIndex == index)
            {
                SwitchToPage(CurrentPageIndex);
            }
            else if (CurrentPageIndex > index)
            {
                CurrentPageIndex--;
            }

            PagesListChanged?.Invoke();
        }

        public void MovePage(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= Pages.Count || newIndex < 0 || newIndex >= Pages.Count) return;

            var page = Pages[oldIndex];
            Pages.RemoveAt(oldIndex);
            Pages.Insert(newIndex, page);
            
            UpdatePageTitles();

            // Cập nhật lại CurrentPageIndex
            if (CurrentPageIndex == oldIndex)
            {
                CurrentPageIndex = newIndex;
            }
            else if (CurrentPageIndex > oldIndex && CurrentPageIndex <= newIndex)
            {
                CurrentPageIndex--;
            }
            else if (CurrentPageIndex < oldIndex && CurrentPageIndex >= newIndex)
            {
                CurrentPageIndex++;
            }

            PagesListChanged?.Invoke();
        }

        /// <summary>
        /// Đổi Pattern (loại kẻ) của một trang.
        /// </summary>
        public void ChangePagePattern(Guid pageId, BackgroundPattern newPattern)
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                if (Pages[i].Id == pageId)
                {
                    Pages[i].Pattern = newPattern;
                    if (i == CurrentPageIndex)
                    {
                        _backgroundManager.SetBackground(newPattern, Pages[i].Theme);
                    }
                    PagesListChanged?.Invoke();
                    break;
                }
            }
        }

        /// <summary>
        /// Đổi Theme (màu nền) của một trang.
        /// </summary>
        public void ChangePageTheme(Guid pageId, BackgroundTheme newTheme)
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                if (Pages[i].Id == pageId)
                {
                    Pages[i].Theme = newTheme;
                    if (i == CurrentPageIndex)
                    {
                        _backgroundManager.SetBackground(Pages[i].Pattern, newTheme);
                    }
                    PagesListChanged?.Invoke();
                    break;
                }
            }
        }

        private void UpdatePageTitles()
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].Title = $"Trang {i + 1}";
            }
        }
    }
}
