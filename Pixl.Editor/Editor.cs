global using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Pixl.Mac.Editor")]
[assembly: InternalsVisibleTo("Pixl.Win.Editor")]

namespace Pixl.Editor;

internal sealed class Editor
{
    private readonly Gui _gui;
    private readonly RenderTexture _gameRenderTexture;
    private readonly List<IEditorWindow> _windows = new();
    private long _time;
    private long _deltaTime;
    private long _startTime;
    private bool _gameFocused;
    private bool _firstUI = true;

    public Editor(Resources resources, Graphics graphics, AppWindow window, Game game, GameWindow gameWindow, MemoryLogger memoryLogger)
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

        _windows.Add(gameWindow);
        _windows.Add(new ConsoleWindow(memoryLogger));
        _windows.Add(new HierarchyWindow(game.Scene));
        _windows.Add(new SystemsWindow(game.Scene));
        _windows.Add(new PropertiesWindow());
        _windows.Add(new TestWindow());
    }

    public EditorDefaultResources DefaultResources { get; }
    public Resources Resources { get; }
    public Graphics Graphics { get; }
    public AppWindow Window { get; }
    public Game Game { get; }
    public GameWindow GameWindow { get; }

    public bool Run()
    {
        var events = Window.DequeueEvents();
        Update(events);
        UpdateGame(events); // propagate events to game window
        if (!Game.Run()) return false;
        Render();
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
    }

    public void WaitForNextUpdate()
    {
        Game.WaitForNextUpdate();
    }

    private void MainDockspace()
    {
        var id = ImGui.GetID("Main DockSpace");
        var viewport = ImGui.GetMainViewport();
        ImGuiInternal.DockBuilderSetNodeSize(id, viewport.WorkSize);
        ImGuiInternal.DockBuilderSetNodePos(id, viewport.WorkPos);

        if (!_firstUI) return; // dock builder only on first UI

        ImGuiInternal.DockBuilderAddNode(id, ImGuiDockNodeFlags.NoResize);

        uint dummy = 0;
        var dock1 = ImGuiInternal.DockBuilderSplitNode(id, ImGuiDir.Left, 0.75f, ref dummy, ref id);
        var dock2 = ImGuiInternal.DockBuilderSplitNode(id, ImGuiDir.Right, 0.25f, ref dummy, ref id);
        var dock3 = ImGuiInternal.DockBuilderSplitNode(dock1, ImGuiDir.Down, 0.25f, ref dummy, ref dock1);
        var dock4 = ImGuiInternal.DockBuilderSplitNode(dock1, ImGuiDir.Left, 0.25f, ref dummy, ref dock1);
        var dock5 = ImGuiInternal.DockBuilderSplitNode(dock4, ImGuiDir.Down, 0.3f, ref dummy, ref dock4);

        ImGuiInternal.DockBuilderDockWindow("Game", dock1);
        ImGuiInternal.DockBuilderDockWindow("Hierarchy", dock4);
        ImGuiInternal.DockBuilderDockWindow("Systems", dock5);
        ImGuiInternal.DockBuilderDockWindow("Properties", dock2);
        ImGuiInternal.DockBuilderDockWindow("Console", dock3);

        ImGuiInternal.DockBuilderFinish(id);
    }

    private void MainMenuBar()
    {
        if (!ImGui.BeginMainMenuBar()) return;

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("New Scene", "Ctrl + N"))
            {

            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Window"))
        {
            foreach (var window in _windows)
            {
                if (ImGui.MenuItem(window.Name))
                {
                    window.Open = true;
                }
            }

            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }

    private void ProcessEvents(Span<WindowEvent> events)
    {
        var deltaTimeF = _deltaTime / (float)Stopwatch.Frequency;
        _gui.Update(deltaTimeF, events);
    }

    private void Render()
    {
        if (!Graphics.Setup) return;

        // submit ui
        SubmitUI();

        // sync size
        Graphics.UpdateWindowSize(Window.Size);

        var commands = Graphics.Commands;
        var frameBuffer = Graphics.SwapchainFramebuffer;
        _gui.Render(Graphics, commands, frameBuffer);
    }

    private void SubmitUI()
    {
        MainMenuBar();
        MainDockspace();
        SubmitWindows();

        _firstUI = false;
    }

    private void SubmitWindows()
    {
        foreach (var window in _windows)
        {
            if (!window.Open) continue;
            window.SubmitUI();
        }
    }

    private void UpdateGame(Span<WindowEvent> events)
    {
        static bool shouldPropagateEvent(WindowEventType type)
        {
            return type switch
            {
                WindowEventType.KeyDown or
                WindowEventType.KeyUp or
                WindowEventType.MouseMove or
                WindowEventType.Scroll or
                WindowEventType.Character or
                WindowEventType.Quit => true,
                _ => false
            };
        }

        var focused = GameWindow.Focused;
        if (focused != _gameFocused)
        {
            GameWindow.PushEvent(new WindowEvent(focused ? WindowEventType.Focused : WindowEventType.Unfocused));
            _gameFocused = focused;
        }

        if (!focused) return; // do not propagate if the game is unfocused

        foreach (ref var @event in events)
        {
            if (!shouldPropagateEvent(@event.Type)) continue;
            GameWindow.PushEvent(in @event);
        }
    }

    private void UpdateTime()
    {
        var currentTime = Stopwatch.GetTimestamp() - _startTime;
        _deltaTime = currentTime - _time;
        _time = currentTime;
    }
}