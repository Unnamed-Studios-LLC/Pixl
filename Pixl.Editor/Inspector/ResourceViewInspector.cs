using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace Pixl.Editor;

internal unsafe sealed class ResourceViewInspector : ObjectInspector<ResourceView>
{
    private readonly Type _type;
    private readonly string _hint;

    public ResourceViewInspector(IEnumerable<Attribute> attributes)
    {
        _type = attributes.OfType<ResourceTypeAttribute>().FirstOrDefault()?.ResourceType ?? typeof(Resource);
        _hint = $"({_type.Name})";
    }

    protected override void OnSubmitUI(Editor editor, string label, ref ResourceView value)
    {
        var resource = value.Get();
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
                var resourceId = *(uint*)payload.Data;
                value = resourceId;
            }
            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine(0, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, 0);
        ImGui.Text(label);
        ImGui.PopStyleVar();
    }
}
