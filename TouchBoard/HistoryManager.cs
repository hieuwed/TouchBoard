using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;

namespace TouchBoard
{
    public class HistoryManager
    {
        private readonly InkCanvas _inkCanvas;
        
        // Stacks to hold the serialized states (ISF format) of the strokes
        // Using ISF byte array is highly efficient and captures all stroke properties (color, size, position)
        private readonly Stack<byte[]> _undoStack = new Stack<byte[]>();
        private readonly Stack<byte[]> _redoStack = new Stack<byte[]>();

        // Flag to prevent capturing state while we are in the middle of undoing/redoing
        private bool _isRestoring = false;

        public event System.Action? StateChanged;

        public HistoryManager(InkCanvas inkCanvas)
        {
            _inkCanvas = inkCanvas;
            
            // Capture initial empty state
            SaveState();
            
            // Listen to drawing events
            _inkCanvas.StrokeCollected += (s, e) => SaveState();
            _inkCanvas.StrokeErased += (s, e) => SaveState();
            _inkCanvas.SelectionMoved += (s, e) => SaveState();
            _inkCanvas.SelectionResized += (s, e) => SaveState();
        }

        public bool CanUndo => _undoStack.Count > 1; // Need at least 2 states (current and previous)
        public bool CanRedo => _redoStack.Count > 0;

        public void SaveState()
        {
            if (_isRestoring) return;

            // Serialize current strokes to a memory stream (ISF format)
            using (var ms = new System.IO.MemoryStream())
            {
                _inkCanvas.Strokes.Save(ms);
                _undoStack.Push(ms.ToArray());
            }

            // Clear redo stack because a new action was taken
            _redoStack.Clear();
            StateChanged?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            _isRestoring = true;

            try
            {
                // Pop the current state and push it to redo
                _redoStack.Push(_undoStack.Pop());

                // Peek the previous state and restore it
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

            _isRestoring = true;

            try
            {
                // Pop the state from redo and push it to undo
                byte[] nextState = _redoStack.Pop();
                _undoStack.Push(nextState);

                // Restore this state
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
