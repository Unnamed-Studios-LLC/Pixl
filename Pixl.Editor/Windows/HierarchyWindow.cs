using ImGuiNET;
using System.Numerics;

namespace Pixl.Editor;

internal sealed class HierarchyWindow : IEditorWindow
{
    private readonly Scene _scene;
    private uint _selectedEntityId;
    private NameSystem? _nameSystem;
    private TransformSystem? _transformSystem;
    private float _childRatio = 0.3f;
    private bool _open = true;

    public HierarchyWindow(Scene scene)
    {
        _scene = scene;
    }

    public string Name => "Hierarchy";
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

        _nameSystem = _scene.GetSystem<NameSystem>();
        _transformSystem = _scene.GetSystem<TransformSystem>();
        var viewSize = ImGui.GetContentRegionAvail().Y - 5;
        if (ImGui.BeginChild("Canvas Region", new Vector2(0, viewSize * _childRatio), false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.TextDisabled("Canvas");
            ImGui.PopStyleVar();
            ImGui.Separator();

            _scene.Entities.ForEach((uint id, ref CanvasTransform transform) =>
            {
                if (transform.SyncedParentId != 0)
                {
                    return; // this object will get rendered with its hierarchy
                }

                SubmitEntity(id);
            });

            ImGui.EndChild();
        }

        if (ImGui.BeginChild("World Region", new Vector2(0, viewSize * (1 - _childRatio)), false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            var pos = ImGui.GetCursorPos();
            ImGui.TextDisabled("World");
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.SetCursorPos(pos);
            ImGui.InvisibleButton("Resize", new Vector2(-1, 12));
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

            _scene.Entities.ForEach((uint id, ref Transform transform) =>
            {
                if (transform.SyncedParentId != 0)
                {
                    return; // this object will get rendered with its hierarchy
                }

                SubmitEntity(id);
            });

            ImGui.EndChild();
        }

        ImGui.End();
    }

    private void SubmitEntity(uint id)
    {
        var name = _nameSystem?.GetName(id) ?? "Name Error";
        var children = _transformSystem?.GetChildren(id) ?? Array.Empty<uint>();

        var flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (_selectedEntityId == id)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (children.Count == 0)
        {
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        var nodeOpen = ImGui.TreeNodeEx(name, flags);
        if (ImGui.IsItemClicked())
        {
            _selectedEntityId = id;
        }

        if (!nodeOpen) return;
        foreach (var child in children)
        {
            SubmitEntity(child);
        }
    }
}
