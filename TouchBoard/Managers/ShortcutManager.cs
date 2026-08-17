using System.Windows.Input;

namespace TouchBoard.Managers
{
    public class ShortcutManager
    {
        private readonly MainWindow _window;
        private readonly ToolManager _toolManager;
        private readonly SelectionManager _selectionManager;
        private readonly HistoryManager _historyManager;
        private readonly CanvasManager _canvasManager;

        public ShortcutManager(MainWindow window, ToolManager toolManager, SelectionManager selectionManager, HistoryManager historyManager, CanvasManager canvasManager)
        {
            _window = window;
            _toolManager = toolManager;
            _selectionManager = selectionManager;
            _historyManager = historyManager;
            _canvasManager = canvasManager;
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.P:
                    _toolManager.SwitchToMode(ToolMode.Pen);
                    e.Handled = true;
                    break;

                case Key.S:
                    _toolManager.SwitchToMode(ToolMode.Select);
                    e.Handled = true;
                    break;

                case Key.E:
                    _toolManager.SwitchToMode(ToolMode.EraserStroke);
                    e.Handled = true;
                    break;

                case Key.R:
                    _toolManager.SwitchToMode(ToolMode.EraserPoint);
                    e.Handled = true;
                    break;

                case Key.Delete:
                    if (_toolManager.CurrentMode == ToolMode.Select)
                        _selectionManager.DeleteSelectedStrokes();
                    e.Handled = true;
                    break;

                case Key.Z:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        _historyManager.Undo();
                        HideSelectionContext();
                        e.Handled = true;
                    }
                    break;

                case Key.Y:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        _historyManager.Redo();
                        HideSelectionContext();
                        e.Handled = true;
                    }
                    break;

                case Key.F11:
                    _canvasManager.ToggleFullscreen();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    if (_canvasManager.IsFullscreen)
                        _canvasManager.ToggleFullscreen();
                    else
                        _toolManager.SwitchToMode(ToolMode.Pen);
                    e.Handled = true;
                    break;
            }
        }

        private void HideSelectionContext()
        {
            _window.SelectionMenuButton.Visibility = System.Windows.Visibility.Collapsed;
            _window.SelectionPopup.IsOpen = false;
        }
    }
}
