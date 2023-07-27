using ImGuiNET;
using System.IO;

namespace Pixl.Editor;

internal sealed class CreateProjectModal : EditorModal
{
    private readonly Editor _editor;
    private string _projectName = "New Project";
    private string _rootDirectory = string.Empty;
    private string _destinationPath = string.Empty;

    public CreateProjectModal(Editor editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public override string Name => "Create Project";

    public override void SubmitUI()
    {
        ImGui.Text("Project Name");
        if (ImGui.InputTextWithHint(string.Empty, "My Project", ref _projectName, 64))
        {
            UpdateDestination();
        }

        ImGui.NewLine();
        ImGui.Text("Location");
        if (string.IsNullOrEmpty(_rootDirectory)) ImGui.TextDisabled("(Select a root directory)");
        else ImGui.TextDisabled(_rootDirectory);

        if (ImGui.Button("Browse..."))
        {
            var saveRequest = new FileBrowserRequest("Root Directory", string.Empty);
            var selectedFolder = _editor.Files.FileBrowser.OpenFolder(saveRequest);
            if (selectedFolder != null &&
                Directory.Exists(selectedFolder))
            {
                _rootDirectory = selectedFolder;
                UpdateDestination();
            }
        }

        ImGui.NewLine();
        if (!string.IsNullOrEmpty(_destinationPath))
        {
            ImGui.Text("Project folder will be created at:");
            ImGui.TextDisabled(_destinationPath);
        }
        else
        {
            ImGui.NewLine();
            ImGui.NewLine();
        }

        ImGui.NewLine();
        ImGui.NewLine();

        if (ImGui.Button("Create"))
        {
            var editorTask = new EditorTask("Creating Project", true);
            editorTask.SetTask(ProjectBuilder.CreateAsync(_destinationPath, editorTask), _editor.OpenProject);
            _editor.PushTask(editorTask);
            Open = false;
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            Open = false;
        }
    }

    private void UpdateDestination()
    {
        if (string.IsNullOrEmpty(_rootDirectory) ||
            string.IsNullOrEmpty(_projectName))
        {
            _destinationPath = string.Empty;
            return;
        }

        _destinationPath = Path.Combine(_rootDirectory, _projectName);
    }
}
