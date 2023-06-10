using ImGuiNET;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Pixl.Editor;

internal sealed class ConsoleWindow : IEditorWindow
{
    public readonly MemoryLogger _memoryLogger;
    private string _searchText = string.Empty;
    private uint? _selectedLogId = null;
    private float _childRatio = 0.65f;
    private bool _open = true;

    public ConsoleWindow(MemoryLogger memoryLogger)
    {
        _memoryLogger = memoryLogger;
    }

    public string Name => "Console";
    public bool Open
    {
        get => _open;
        set => _open = value;
    }

    public void SubmitUI()
    {
        if (!ImGui.Begin(Name, ref _open))
        {
            return;
        }

        var windowSize = ImGui.GetWindowSize();

        ImGui.InputText("Search", ref _searchText, 100);

        ImGui.SameLine();
        if (ImGui.SmallButton("Clear"))
        {
            _memoryLogger.Clear();
        }

        ImGui.SameLine();
        var gotoBottom = ImGui.SmallButton("Goto Bottom");

        ImGui.Separator();

        LogEntry? selectedLog = default;
        var viewSize = ImGui.GetContentRegionAvail().Y - 5;
        if (ImGui.BeginChild("Logs Region", new Vector2(0, viewSize * _childRatio), false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            // submit log entries
            _memoryLogger.Read(logs =>
            {
                var first = true;
                var span = CollectionsMarshal.AsSpan(logs);
                foreach (ref var log in span)
                {
                    if (!first) ImGui.Separator();
                    else first = false;

                    ImGui.Selectable(log.Message, _selectedLogId == log.Id, ImGuiSelectableFlags.None, new Vector2(0, 16));
                    if (ImGui.IsItemActive())
                    {
                        _selectedLogId = log.Id;
                    }

                    if (log.Id == _selectedLogId) selectedLog = log;
                }
            });

            // auto-scroll if at the bottom
            if (gotoBottom || ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1.0f);
            }

            ImGui.EndChild();
        }

        if (ImGui.BeginChild("Details Region", new Vector2(0, viewSize * (1 - _childRatio)), false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.InvisibleButton("Split Adjuster", new Vector2(-1, 4));
            ImGui.PopStyleVar();
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
            }

            if (ImGui.IsItemActive() &&
                viewSize > 0)
            {
                var delta = ImGui.GetIO().MouseDelta.Y;
                var perc = delta / viewSize;
                _childRatio = Math.Clamp(_childRatio + perc, 0.01f, 0.99f);
            }
            ImGui.Separator();
            if (selectedLog != null)
            {
                ImGui.TextWrapped(selectedLog.Value.Message);
                ImGui.TextWrapped(selectedLog.Value.Stacktrace);
            }
            else
            {
                ImGui.TextDisabled("(Select an item to view it's details)");
            }
            ImGui.EndChild();
        }

        ImGui.End();
    }
}
