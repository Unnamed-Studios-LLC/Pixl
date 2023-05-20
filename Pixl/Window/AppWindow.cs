using Veldrid;

namespace Pixl;

internal abstract class AppWindow
{
    /// <summary>
    /// The size of the main Window
    /// </summary>
    public abstract Int2 Size { get; set; }

    /// <summary>
    /// The position of the mouse relative to the bottom-left corner of the window
    /// </summary>
    public abstract Int2 MousePosition { get; }

    /// <summary>
    /// The position on the window relative to the bottom-left corner of the display
    /// </summary>
    //public abstract Int2 Position { get; }

    /// <summary>
    /// The style of the Window
    /// </summary>
    public abstract WindowStyle Style { get; set; }
    public abstract SwapchainSource SwapchainSource { get; }

    /// <summary>
    /// The title of the Window
    /// </summary>
    public abstract string Title { get; set; }

    /// <summary>
    /// Dequeues received events
    /// </summary>
    /// <returns></returns>
    public abstract Span<WindowEvent> DequeueEvents();
}
