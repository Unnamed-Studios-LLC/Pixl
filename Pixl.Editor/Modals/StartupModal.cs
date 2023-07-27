using ImGuiNET;
using System.IO;
using System.Numerics;

namespace Pixl.Editor;

internal sealed class StartupModal : EditorModal
{
    private readonly Editor _editor;

    public StartupModal(Editor editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public override string Name => "Startup";
    protected override bool CanClose => false;
    protected override bool ShouldOpen => _editor.Project == null;

    public override void SubmitUI()
    {
        ImGui.Text("Select a project to start editing.");
        ImGui.NewLine();

        ImGui.SeparatorText("Recent Projects");

        if (ImGui.BeginChild("Recent Projects", new Vector2(500, 300), false, ImGuiWindowFlags.None))
        {
            var index = -1;
            var removeIndex = -1;
            foreach (var projectFilePath in _editor.EditorValues.RecentProjects)
            {
                index++;

                var flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
                var nodeOpen = ImGui.TreeNodeEx(Path.GetFileName(projectFilePath), flags);
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable("Remove")) removeIndex = index;
                    ImGui.EndPopup();
                }
                if (ImGui.IsItemClicked())
                {
                    _editor.OpenProject(projectFilePath);
                }
                ImGui.SameLine();
                ImGui.TextDisabled(projectFilePath);
            }

            if (removeIndex >= 0)
            {
                _editor.EditorValues.RemoveRecentProject(removeIndex);
            }

            ImGui.EndChild();
        }

        if (ImGui.Button("New Project..."))
        {
            _editor.ShowModal(new CreateProjectModal(_editor));
        }

        ImGui.SameLine();
        if (ImGui.Button("Open Project..."))
        {
            var openRequest = new FileBrowserRequest(string.Empty, string.Empty);
            var openFilePath = _editor.Files.FileBrowser.OpenFolder(openRequest);
            if (openFilePath != null)
            {
                _editor.OpenProject(openFilePath);
            }
        }
    }
}
