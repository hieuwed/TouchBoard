using System.Windows.Controls;
using System.Windows.Input.StylusPlugIns;

namespace TouchBoard.Controls
{
    /// <summary>
    /// Subclass của InkCanvas để expose StylusPlugIns (protected trong UIElement).
    /// Cho phép MainWindow gắn SnappingPlugIn từ bên ngoài.
    /// </summary>
    public class SnappableInkCanvas : InkCanvas
    {
        public void AddStylusPlugin(StylusPlugIn plugin)
        {
            StylusPlugIns.Add(plugin);
        }

        public void RemoveStylusPlugin(StylusPlugIn plugin)
        {
            StylusPlugIns.Remove(plugin);
        }
    }
}
