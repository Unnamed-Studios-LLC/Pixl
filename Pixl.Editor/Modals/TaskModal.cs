using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace Pixl.Editor;

internal sealed class TaskModal : EditorModal
{
    private readonly Queue<EditorTask> _tasks = new();
    private readonly Editor _editor;

    public TaskModal(Editor editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public override string Name => "Task...";
    protected override bool ShouldOpen
    {
        get
        {
            UpdateTasks();
            return _tasks.Count > 0;
        }
    }

    public void PushTask(EditorTask task)
    {
        lock (_tasks)
        {
            _tasks.Enqueue(task);
        }
    }

    protected override void OnUI()
    {
        EditorTask? firstTask = null;
        lock (_tasks)
        {
            if (!_tasks.TryPeek(out firstTask)) return;
        }

        ImGui.Text(firstTask.Name);
        if (!string.IsNullOrWhiteSpace(firstTask.State)) ImGui.TextDisabled(firstTask.State);
        else ImGui.NewLine();

        if (firstTask.Progress != null) ImGui.ProgressBar(MathF.Max(0, MathF.Min(1, firstTask.Progress.Value)), new Vector2(300, 0));
        if (firstTask.CancellationToken != null &&
            !firstTask.CancellationToken.IsCancellationRequested &&
            ImGui.Button("Cancel"))
        {
            firstTask.CancellationToken?.Cancel();
        }
    }

    private bool IsTaskCompleted(out EditorTask? task)
    {
        lock (_tasks)
        {
            if (!_tasks.TryPeek(out task) ||
                (task.Task != null && !task.Task.IsCompleted)) return false;
            _tasks.Dequeue();
            return true;
        }
    }

    private void UpdateTasks()
    {
        while (IsTaskCompleted(out var task))
        {
            if (task == null) continue;
            if (task.Task != null &&
                task.Task.Exception != null)
            {
                _editor.PushError(task.Task.Exception.GetBaseException());
                return;
            }
            task.Complete();
        }
    }
}
