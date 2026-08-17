using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace TouchBoard.Managers
{
    public class HistoryManager
    {
        private readonly InkCanvas _inkCanvas;
        
        // Stacks to hold the serialized states (ISF format) of the strokes
        private Stack<byte[]> _undoStack = new Stack<byte[]>();
        private Stack<byte[]> _redoStack = new Stack<byte[]>();

        private bool _isRestoring = false;
        private System.Windows.Threading.DispatcherTimer _debounceTimer;

        public event System.Action? StateChanged;

        public HistoryManager(InkCanvas inkCanvas)
        {
            _inkCanvas = inkCanvas;
            
            _debounceTimer = new System.Windows.Threading.DispatcherTimer();
            _debounceTimer.Interval = System.TimeSpan.FromMilliseconds(400);
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                SaveState();
            };

            // Listen to drawing events with debounce
            _inkCanvas.StrokeCollected += (s, e) => DebounceSaveState();
            _inkCanvas.StrokeErased += (s, e) => DebounceSaveState();
            // EraseByPoint usually triggers StrokesReplaced internally, but we can also listen to StrokesChanged just in case
            _inkCanvas.Strokes.StrokesChanged += (s, e) => DebounceSaveState();
            _inkCanvas.SelectionMoved += (s, e) => DebounceSaveState();
            _inkCanvas.SelectionResized += (s, e) => DebounceSaveState();
        }

        public bool CanUndo => _undoStack.Count > 1; 
        public bool CanRedo => _redoStack.Count > 0;

        public void SetStacks(Stack<byte[]> undoStack, Stack<byte[]> redoStack)
        {
            _undoStack = undoStack;
            _redoStack = redoStack;
            StateChanged?.Invoke();
        }

        public void SaveState()
        {
            if (_isRestoring) return;
            _debounceTimer.Stop();

            using (var ms = new System.IO.MemoryStream())
            {
                _inkCanvas.Strokes.Save(ms);
                _undoStack.Push(ms.ToArray());
            }

            _redoStack.Clear();
            StateChanged?.Invoke();
        }

        private void DebounceSaveState()
        {
            if (_isRestoring) return;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            _debounceTimer.Stop(); // Cancel pending saves

            _isRestoring = true;

            try
            {
                _redoStack.Push(_undoStack.Pop());

                byte[] previousState = _undoStack.Peek();
                RestoreState(previousState);
            }
            finally
            {
                _isRestoring = false;
                StateChanged?.Invoke();
            }
        }

        public void Redo()
        {
            if (!CanRedo) return;
            _debounceTimer.Stop(); // Cancel pending saves

            _isRestoring = true;

            try
            {
                byte[] nextState = _redoStack.Pop();
                _undoStack.Push(nextState);

                RestoreState(nextState);
            }
            finally
            {
                _isRestoring = false;
                StateChanged?.Invoke();
            }
        }

        private void RestoreState(byte[] isfData)
        {
            using (var ms = new System.IO.MemoryStream(isfData))
            {
                _inkCanvas.Strokes = new StrokeCollection(ms);
            }
        }
    }
}
