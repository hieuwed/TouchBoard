using System.Windows;

namespace TouchBoard;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Custom entry point to enable Pointer-based touch stack BEFORE WPF initializes.
    /// This replaces the legacy WISP (Windows Ink Services Platform) stylus stack
    /// with the modern WM_POINTER message-based stack, which properly supports
    /// external USB touchscreens and avoids the NullReferenceException in
    /// WispStylusPlugInCollection.UpdateState when using InkCanvas.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        // CRITICAL: Switch from legacy WISP stack to WM_POINTER stack.
        // - Fixes crash with InkCanvas + DisableStylusAndTouchSupport
        // - Properly recognizes external USB touchscreens
        // - Uses modern Windows pointer input messages (WM_POINTER)
        // - Must be set BEFORE any WPF object is created
        AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", true);

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
