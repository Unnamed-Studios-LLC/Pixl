global using System;
using ImGuiNET;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Pixl.Win.Editor")]

namespace Pixl.Editor;

internal sealed class Editor
{
    private readonly Gui _gui;
    private readonly RenderTexture _gameRenderTexture;
    private long _time;
    private long _deltaTime;
    private long _startTime;

    public Editor(Resources resources, Graphics graphics, AppWindow window, Game game, EditorGameWindow gameWindow)
    {
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        Window = window ?? throw new ArgumentNullException(nameof(window));
        Game = game ?? throw new ArgumentNullException(nameof(game));
        GameWindow = gameWindow;
        DefaultResources = new EditorDefaultResources();

        _gui = new Gui(window, resources, game.DefaultResources, DefaultResources);
        _gameRenderTexture = new RenderTexture(gameWindow.Size, SampleMode.Point, ColorFormat.Rgba32);
        Game.TargetRenderTexture = _gameRenderTexture;
        gameWindow.RenderTexture = _gameRenderTexture;
    }

    public EditorDefaultResources DefaultResources { get; }
    public Resources Resources { get; }
    public Graphics Graphics { get; }
    public AppWindow Window { get; }
    public Game Game { get; }
    public EditorGameWindow GameWindow { get; }

    public bool Run()
    {
        var events = Window.DequeueEvents();
        GameWindow.PushEvents(events); // propagate events to game window
        if (!Game.Run()) return false;
        Update(events);
        return true;
    }

    public void Start()
    {
        _startTime = Stopwatch.GetTimestamp();
        _gui.Start(Resources);

        DefaultResources.Add(Resources);
        Resources.Add(_gui);
        Resources.Add(_gameRenderTexture);
    }

    public void Stop()
    {
        _gui.Stop(Resources);
        Resources.Remove(_gui);
        Resources.Remove(_gameRenderTexture);
    }

    public void Update(Span<WindowEvent> events)
    {
        UpdateTime();
        ProcessEvents(events);
        SubmitUi();
        Render();
    }

    public void WaitForNextUpdate()
    {
        Game.WaitForNextUpdate();
    }

    private void ProcessEvents(Span<WindowEvent> events)
    {
        var deltaTimeF = _deltaTime / (float)Stopwatch.Frequency;
        _gui.Update(deltaTimeF, events);
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

        GameWindow.SubmitUI();
    }

    private void UpdateTime()
    {
        var currentTime = Stopwatch.GetTimestamp() - _startTime;
        _deltaTime = currentTime - _time;
        _time = currentTime;
    }
}