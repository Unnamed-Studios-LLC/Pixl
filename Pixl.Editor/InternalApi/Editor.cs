global using System;
using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Pixl.Mac.Editor")]
[assembly: InternalsVisibleTo("Pixl.Win.Editor")]

namespace Pixl.Editor;

internal sealed class Editor : App
{
    private readonly Gui _gui;
    private readonly RenderTexture _gameRenderTexture;
    private readonly List<IEditorUI> _windows = new();
    private readonly StartupModal _startupModal;
    private readonly TaskModal _taskModal;
    private readonly ErrorModal _errorModal;
    private readonly MemoryLogger _memoryLogger = new(20000, 1000);
    private bool _gameFocused;
    private bool _firstUI = true;
    private EditorModal? _variableModal;

    public Editor(Window window, Graphics graphics, Resources resources, Values values, Logger logger, Files files) : base(window, graphics, resources, values, logger, files)
    {
        var gameWindow = new GameWindow();
        var gameValues = new Values(new ProjectValuesStore(this));
        var gameFiles = new Files(string.Empty, files.FileBrowser, typeof(Game).Assembly);
        Game = new Game(_memoryLogger, gameWindow, graphics, resources, gameValues, gameFiles);
        GameWindow = gameWindow;
        EditorValues = new EditorValues(values);

        _startupModal = new StartupModal(this);
        _taskModal = new TaskModal(this);
        _errorModal = new ErrorModal();

        _gui = new Gui(window, resources);
        _gameRenderTexture = new RenderTexture(Int2.Max(Int2.One, gameWindow.Size), SampleMode.Point, ColorFormat.Rgba32);
        Game.TargetRenderTexture = _gameRenderTexture;
        gameWindow.RenderTexture = _gameRenderTexture;

        var properties = new PropertiesWindow(this);

        _windows.Add(gameWindow);
        _windows.Add(new SceneWindow(Game.Scene, resources, graphics));
        _windows.Add(new EntitiesWindow(Game.Scene));
        _windows.Add(new SystemsWindow(Game.Scene, properties));
        _windows.Add(new HierarchyWindow(Game.Scene, properties));
        _windows.Add(new ProjectWindow());
        _windows.Add(new ConsoleWindow(_memoryLogger));
        _windows.Add(properties);
        _windows.Add(new TestWindow());
    }

    public EditorValues EditorValues { get; }
    public Project? Project { get; private set; }
    public Game Game { get; }
    public GameWindow GameWindow { get; }

    public override bool ProcessEvents(Span<WindowEvent> events)
    {
        _gui.Update(Time.Delta, events);
        UpdateGame(events); // propagate events to game window

        return base.ProcessEvents(events);
    }

    public void PushError(string? message, string? details = null) => PushError(new EditorException(message, details));
    public void PushError(Exception error) => _errorModal.Push(error);

    public void PushTask(EditorTask task) => _taskModal.PushTask(task);

    public override void Render()
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

    public void ShowModal(EditorModal modal)
    {
        _variableModal = modal;
        modal.Open = true;
    }

    public override void Start()
    {
        base.Start();

        _gui.Start(Resources);

        Resources.Add(_gui);
        Resources.Add(_gameRenderTexture);
    }

    public void StartProject(string projectDirectory)
    {
        EditorTask editorTask = new("Loading Project", true)
        {
            State = "Reading project files...",
            Progress = 0
        };

        async Task task()
        {
            await Task.Yield();

            editorTask.Progress = 1;
            editorTask.State = "Loading project...";

            await Task.Yield();

            if (!Directory.Exists(projectDirectory))
            {
                throw new EditorException("Unable to load project, directory does not exist!", $"Directory: {projectDirectory}");
            }

            Project = new Project(projectDirectory);
            Game.Stop();
            Game.Start();
            Game.Scene.AddDefaultSystems();
            Window.Title = $"{Project.ProjectName} - Pixl Editor";

            EditorValues.AddRecentProject(projectDirectory);
        }

        editorTask.SetTask(task());
        PushTask(editorTask);
    }

    public override void Stop()
    {
        if (Project != null)
        {
            Game.Stop();
        }

        Resources.Remove(_gameRenderTexture);
        Resources.Remove(_gui);

        _gui.Stop(Resources);

        base.Stop();
    }

    public void StopProject()
    {
        Game.Stop();
        Project = null;
        Window.Title = "Pixl Editor";
    }

    public override void Update()
    {
        Game.Run();
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
        var dockCenter = ImGuiInternal.DockBuilderSplitNode(id, ImGuiDir.Left, 0.75f, ref dummy, ref id);
        var dockRight = ImGuiInternal.DockBuilderSplitNode(id, ImGuiDir.Right, 0.25f, ref dummy, ref id);
        var dockBottom = ImGuiInternal.DockBuilderSplitNode(dockCenter, ImGuiDir.Down, 0.25f, ref dummy, ref dockCenter);
        var dockLeft = ImGuiInternal.DockBuilderSplitNode(dockCenter, ImGuiDir.Left, 0.25f, ref dummy, ref dockCenter);
        var dockLeftDown = ImGuiInternal.DockBuilderSplitNode(dockLeft, ImGuiDir.Down, 0.3f, ref dummy, ref dockLeft);

        void dockWindow<T>(uint dockId)
        {
            var window = _windows.FirstOrDefault(x => x is T);
            if (window is null) return;
            window.Open = true;
            ImGuiInternal.DockBuilderDockWindow(window.Name, dockId);
        }

        dockWindow<GameWindow>(dockCenter);
        dockWindow<SceneWindow>(dockCenter);
        dockWindow<EntitiesWindow>(dockLeft);
        dockWindow<HierarchyWindow>(dockLeft);
        dockWindow<SystemsWindow>(dockLeftDown);
        dockWindow<PropertiesWindow>(dockRight);
        dockWindow<ConsoleWindow>(dockBottom);
        dockWindow<ProjectWindow>(dockBottom);

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

    private void SubmitUI()
    {
        MainMenuBar();
        MainDockspace();
        SubmitWindows();
        SubmitModals();

        _firstUI = false;
    }

    private void SubmitModals()
    {
        static void submitModal(EditorModal modal, Action? next)
        {
            if (!modal.SubmitUI(next)) next?.Invoke();
        }

        _startupModal.Open = Project == null;
        Action? next = null;
        foreach (var modal in GetModals())
        {
            var nextVal = next;
            next = () => submitModal(modal, nextVal);
        }

        next?.Invoke();
    }

    private IEnumerable<EditorModal> GetModals()
    {
        yield return _errorModal;
        yield return _taskModal;
        if (_variableModal != null) yield return _variableModal;
        yield return _startupModal;
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
}