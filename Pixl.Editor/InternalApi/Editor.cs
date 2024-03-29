﻿global using System;
using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

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
    private EditorTask? _reloadTask;

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

        var propertiesWindow = new PropertiesWindow(this);
        var sceneWindow = new SceneWindow(Game.Scene, propertiesWindow, this);
        SceneWindow = sceneWindow;

        _windows.Add(gameWindow);
        _windows.Add(new ExplorerWindow(Game.Scene, resources, graphics));
        _windows.Add(sceneWindow);
        _windows.Add(new ProjectWindow());
        _windows.Add(new ConsoleWindow(_memoryLogger));
        _windows.Add(propertiesWindow);
        _windows.Add(new TestWindow());
    }

    public EditorValues EditorValues { get; }
    public Project? Project { get; private set; }
    public Game Game { get; }
    public GameWindow GameWindow { get; }
    public SceneWindow SceneWindow { get; }

    public void CloseCurrentProject()
    {
        Game.Stop();
        Project = null;
        Window.Title = "Pixl Editor";
    }

    public void OpenProject(string projectDirectory)
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
            Game.Scene.AddSystem<CameraSystem>();
            Game.Scene.AddSystem<TransformSystem>();
            Game.Scene.AddSystem<ParentSystem>();

            Window.Title = $"{Project.ProjectName} - Pixl Editor";

            EditorValues.AddRecentProject(projectDirectory);
        }

        editorTask.SetTask(task());
        PushTask(editorTask);
    }

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

    public override void Update()
    {
        if (_reloadTask != null &&
            (_reloadTask.Task == null || _reloadTask.Task.IsCompleted))
        {
            _reloadTask = null;
        }

        if (Project != null)
        {
            if (_reloadTask == null &&
                Project.Source.ReloadAvailable)
            {
                // serialize scene
                _reloadTask = new EditorTask("Reloading Assembly", false)
                {
                    Progress = 0,
                    State = "Loading assembly dll..."
                };
                _reloadTask.SetTask(ReloadAsync(Project, _reloadTask));
                PushTask(_reloadTask);
            }
        }
        Game.Run();
    }

    private IEnumerable<EditorModal> GetModals()
    {
        yield return _startupModal;
        if (_variableModal != null) yield return _variableModal;
        yield return _taskModal;
        yield return _errorModal;
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

        void dockWindow<T>(uint dockId)
        {
            var window = _windows.FirstOrDefault(x => x is T);
            if (window is null) return;
            window.Open = true;
            ImGuiInternal.DockBuilderDockWindow(window.Name, dockId);
        }

        dockWindow<GameWindow>(dockCenter);
        dockWindow<ExplorerWindow>(dockCenter);
        dockWindow<SceneWindow>(dockLeft);
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

            if (ImGui.MenuItem("Open Scene", "Ctrl + O"))
            {
                SceneWindow.OpenScene();
            }

            if (ImGui.MenuItem("Save", "Ctrl + S"))
            {
                Save();
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

        if (ImGui.BeginMenu("Project"))
        {
            if (ImGui.MenuItem("Close Project"))
            {
                CloseCurrentProject();
            }

            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }

    private async Task ReloadAsync(Project project, EditorTask editorTask)
    {
        // serialize
        var documentList = new List<YamlDocument>();
        Game.Scene.GetDocuments(documentList);
        Game.Scene.Clear();

        try
        {
            await project.Source.ReloadUserAssemblyAsync(editorTask);
        }
        catch (Exception e)
        {
            PushError(e);
        }

        // deserialize
        Game.Scene.LoadDocuments(documentList);
    }

    private void Save()
    {
        foreach (var window in _windows)
        {
            if (window is not EditorWindow editorWindow) continue;
            editorWindow.Save();
        }
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
        int open = 0;
        foreach (var modal in GetModals())
        {
            if (!modal.SubmitModal()) continue;
            open++;
        }

        for (int i = 0; i < open; i++) ImGui.EndPopup();
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