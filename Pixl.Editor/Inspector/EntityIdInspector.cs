using ImGuiNET;

namespace Pixl.Editor;

internal unsafe sealed class EntityIdInspector : ObjectInspector<Entity>
{
    private string? _nameCache;

    protected override void OnSubmitUI(Editor editor, string label, ref Entity entity)
    {
        if (!editor.Game.Scene.Entities.EntityExists(entity.Id))
        {
            entity = 0;
            _nameCache = string.Empty;
        }
        else
        {
            var name = editor.Game.Scene.Entities.GetNameOrPrefixed(entity.Id);
            var nameSpan = name.AsSpan();
            if (_nameCache == null || !_nameCache.AsSpan().SequenceEqual(nameSpan))
            {
                _nameCache = nameSpan.ToString();
            }
        }

        ImGui.InputTextWithHint(string.Empty, "(Entity)", ref _nameCache, 256, ImGuiInputTextFlags.ReadOnly);

        if (entity != 0 &&
            ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Remove")) entity = 0;
            ImGui.EndPopup();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("EntityId");
            if (payload.NativePtr != null)
            {
                var entityId = *(uint*)payload.Data;
                entity = entityId;
            }
            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine(0, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, 0);
        ImGui.Text(label);
        ImGui.PopStyleVar();
    }
}