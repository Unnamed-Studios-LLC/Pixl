global using System;
using ImGuiNET;
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

    public Editor(Resources resources, Graphics graphics, AppWindow window, Game game)
    {
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        Window = window ?? throw new ArgumentNullException(nameof(window));
        Game = game ?? throw new ArgumentNullException(nameof(game));
        DefaultResources = new EditorDefaultResources();

        _gui = new Gui(window, resources, game.DefaultResources, DefaultResources);
    }

    public EditorDefaultResources DefaultResources { get; }
    public Resources Resources { get; }
    public Graphics Graphics { get; }
    public AppWindow Window { get; }
    public Game Game { get; }

    public void Start()
    {
        _startTime = Stopwatch.GetTimestamp();
        DefaultResources.Add(Resources);
        Resources.Add(_gui);
        _gui.Start(Resources);
        Window.PushEvent(new WindowEvent(WindowEventType.Render));
    }

    public void Stop()
    {
        _gui.Stop(Resources);
        Resources.Remove(_gui);
    }

    public void Update()
    {
        UpdateTime();
        ProcessEvents();
        SubmitUi();
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
        Game.Run();
        _gui.Render(Graphics, commands, frameBuffer);
        Graphics.SwapBuffers();
    }

    private void SubmitUi()
    {
        ImGui.Text("Hello, world!");
        ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");
        ImGui.SameLine(0, -1);

        float framerate = ImGui.GetIO().Framerate;
        ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
    }

    private void UpdateTime()
    {
        var currentTime = Stopwatch.GetTimestamp() - _startTime;
        _deltaTime = currentTime - _time;
        _time = currentTime;
    }
}