using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Pixl.Win.Editor")]

namespace Pixl.Editor;

internal sealed class Editor
{
    private readonly Gui _gui;
    private long _time;
    private long _deltaTime;
    private long _startTime;

    public Editor(Resources resources, Graphics graphics, AppWindow window)
    {
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        Window = window ?? throw new ArgumentNullException(nameof(window));

        _gui = new Gui(window, null);
    }

    public Resources Resources { get; }
    public Graphics Graphics { get; }
    public AppWindow Window { get; }

    public void Start()
    {
        _startTime = Stopwatch.GetTimestamp();
        Resources.Add(_gui);
    }

    public void Update()
    {
        UpdateTime();
        ProcessEvents();
        Render();
    }

    private void ProcessEvents()
    {
        var deltaTimeF = _deltaTime / (float)Stopwatch.Frequency;
        _gui.Update(deltaTimeF, Window.DequeueEvents());
    }

    private void Render()
    {
        if (!Graphics.Setup) return;

        // sync size
        Graphics.UpdateWindowSize(Window.Size);

        var commands = Graphics.Commands;
        var frameBuffer = Graphics.SwapchainFramebuffer;
        _gui.Render(Graphics, commands, frameBuffer);
        Graphics.SwapBuffers();
    }

    private void UpdateTime()
    {
        var currentTime = Stopwatch.GetTimestamp() - _startTime;
        _deltaTime = currentTime - _time;
        _time = currentTime;
    }
}