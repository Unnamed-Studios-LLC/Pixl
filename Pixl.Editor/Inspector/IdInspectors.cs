using ImGuiNET;

namespace Pixl.Editor;

internal unsafe sealed class EntityIdInspector : ObjectInspector<uint>
{
    private string? _nameCache;

    protected override void OnSubmitUI(Editor editor, string label, ref uint value)
    {
        if (!editor.Game.Scene.Entities.EntityExists(value)) value = 0;

        var name = editor.Game.Scene.Entities.GetNameOrPrefixed(value);
        var nameSpan = name.AsSpan();
        if (_nameCache == null || !_nameCache.AsSpan().SequenceEqual(nameSpan))
        {
            _nameCache = nameSpan.ToString();
        }

        ImGui.InputTextWithHint(string.Empty, "(Entity)", ref _nameCache, 256, ImGuiInputTextFlags.ReadOnly);

        if (value != 0 &&
            ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove")) value = 0;
            ImGui.EndPopup();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("EntityId");
            if (payload.NativePtr != null)
            {
                var entityId = *(uint*)payload.Data;
                value = entityId;
            }
            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine(0, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, 0);
        ImGui.Text(label);
        ImGui.PopStyleVar();
    }
}

internal unsafe sealed class ResourceIdInspector : ObjectInspector<uint>
{
    private readonly Type _type;
    private readonly string _hint;

    public ResourceIdInspector(Type type)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
        _hint = $"({_type.Name})";
    }

    protected override void OnSubmitUI(Editor editor, string label, ref uint value)
    {
        if (!editor.Game.Scene.Entities.EntityExists(value)) value = 0;

        var resource = editor.Resources.TryGet(value, out var r) ? r : null;
        var name = resource?.Name ?? string.Empty;
        ImGui.InputTextWithHint(string.Empty, _hint, ref name, 256, ImGuiInputTextFlags.ReadOnly);

        if (value != 0 &&
            ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove")) value = 0;
            ImGui.EndPopup();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("ResourceId");
            if (payload.NativePtr != null)
            {

            }
            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine(0, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, 0);
        ImGui.Text(label);
        ImGui.PopStyleVar();
    }
}

