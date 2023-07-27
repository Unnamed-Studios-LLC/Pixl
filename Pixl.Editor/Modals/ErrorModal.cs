using ImGuiNET;
using System.Collections.Generic;

namespace Pixl.Editor;

internal sealed class ErrorModal : EditorModal
{
    private readonly Stack<Exception> _errors = new();

    public override string Name => "Error...";

    protected override bool CanClose => false;
    protected override bool ShouldOpen => _errors.Count > 0;

    public void Push(Exception error)
    {
        lock (_errors)
        {
            _errors.Push(error);
        }
    }

    public override void SubmitUI()
    {
        lock (_errors)
        {
            if (!_errors.TryPeek(out var topError)) return;

            if (!string.IsNullOrEmpty(topError.Message)) ImGui.Text(topError.Message);
            if (topError is EditorException editorException)
            {
                if (!string.IsNullOrEmpty(editorException.Details)) ImGui.TextDisabled(editorException.Details);
            }

            if (!string.IsNullOrWhiteSpace(topError.StackTrace))
            {
#if DEBUG
                ImGui.TextWrapped(topError.StackTrace);
#endif
            }

            if (ImGui.Button("Close"))
            {
                _errors.Pop();
                ImGui.CloseCurrentPopup();
            }
        }
    }
}
