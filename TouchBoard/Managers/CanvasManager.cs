using System.Windows;

namespace TouchBoard.Managers
{
    public class CanvasManager
    {
        private readonly MainWindow _window;
        private readonly HistoryManager _historyManager;
        private bool _isFullscreen = false;

        public CanvasManager(MainWindow window, HistoryManager historyManager)
        {
            _window = window;
            _historyManager = historyManager;
        }

        public void ClearAll()
        {
            if (_window.DrawingCanvas.Strokes.Count == 0 && _window.DrawingCanvas.Children.Count == 0)
                return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa toàn bộ bảng?",
                "Xác nhận Xóa Sạch",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _window.DrawingCanvas.Strokes.Clear();
                _window.DrawingCanvas.Children.Clear();
                _historyManager.SaveState();
            }
        }

        public void ToggleFullscreen()
        {
            _isFullscreen = !_isFullscreen;

            if (_isFullscreen)
            {
                _window.WindowStyle = WindowStyle.None;
                _window.WindowState = WindowState.Maximized;
                _window.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                _window.WindowStyle = WindowStyle.SingleBorderWindow;
                _window.WindowState = WindowState.Maximized;
                _window.ResizeMode = ResizeMode.CanResize;
            }
        }

        public bool IsFullscreen => _isFullscreen;
    }
}
