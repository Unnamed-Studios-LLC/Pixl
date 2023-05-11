using Veldrid;

namespace Pixl;

internal interface IPlayer
{
    int ExitCode { get; set; }

    string DataPath { get; }
    string AssetsPath { get; }
    string InternalAssetsPath { get; }

    Int2 WindowSize { get; set; }
    WindowStyle WindowStyle { get; set; }
    string WindowTitle { get; set; }

    GraphicsApi GraphicsApi { get; }
    SwapchainSource SwapchainSource { get; }

    Span<PlayerEvent> DequeueEvents();
    void Log(object @object);
}
