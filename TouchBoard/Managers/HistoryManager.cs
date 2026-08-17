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

        public event System.Action? StateChanged;

        public HistoryManager(InkCanvas inkCanvas)
        {
            _inkCanvas = inkCanvas;

            
            // Listen to drawing events
            _inkCanvas.StrokeCollected += (s, e) => SaveState();
            _inkCanvas.StrokeErased += (s, e) => SaveState();
            _inkCanvas.SelectionMoved += (s, e) => SaveState();
            _inkCanvas.SelectionResized += (s, e) => SaveState();
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

            using (var ms = new System.IO.MemoryStream())
            {
                _inkCanvas.Strokes.Save(ms);
                _undoStack.Push(ms.ToArray());
            }

            _redoStack.Clear();
            StateChanged?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo) return;

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
