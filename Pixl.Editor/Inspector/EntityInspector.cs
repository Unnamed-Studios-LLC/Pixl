using EntitiesDb;
using ImGuiNET;
using System.Linq;

namespace Pixl.Editor;

internal sealed class EntityInspector : ObjectInspector<uint>
{
    private string? _idLabel;
    private string? _tempName;
    private bool _editingName;
    private string _nameEdit = string.Empty;
    private EntityArchetype? _archetype;
    private (int, ObjectInspector)[] _inspectors = Array.Empty<(int, ObjectInspector)>();

    protected override void OnSubmitUI(Editor editor, string label, ref uint value)
    {
        var entities = editor.Game.Scene.Entities;

        if (value == 0 || !entities.EntityExists(value))
        {
            _archetype = null;
            _inspectors = Array.Empty<(int, ObjectInspector)>();
            return;
        }

        var archetype = entities.GetArchetype(value);
        if (archetype != _archetype)
        {
            _inspectors = archetype.GetIds().Select(x => (x, (ObjectInspector)new DefaultObjectInspector(entities.GetComponentType(x).Type))).ToArray();
            _archetype = archetype;
        }

        var hasDisabled = entities.HasComponent<Disabled>(value);
        var disabled = hasDisabled;
        if (ImGui.Checkbox("Disabled", ref disabled) &&
            disabled != hasDisabled)
        {
            if (disabled) entities.DisableEntity(value);
            else entities.EnableEntity(value);
        }

        _idLabel ??= value.ToString();
        ImGui.Text("Id:");
        ImGui.SameLine();
        ImGui.TextDisabled(_idLabel);

        var nameSystem = editor.Game.Scene.GetSystem<NameSystem>();
        if (nameSystem != null &&
            entities.HasComponent<Named>(value))
        {
            var name = nameSystem.GetName(value);
            if (name == null)
            {
                _tempName ??= $"Entity {value}";
                name = _tempName;
            }

            var didReturn = _editingName ?
                    ImGui.InputText("Name", ref _nameEdit, 256, ImGuiInputTextFlags.EnterReturnsTrue) :
                    ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue);

            var editingName = ImGui.IsItemActive();
            if (_editingName != editingName)
            {
                if (editingName) _nameEdit = name;
                _editingName = editingName;
            }

            if (didReturn)
            {
                nameSystem.SetName(value, _nameEdit);
                _editingName = false;
            }
            ImGui.NewLine();
        }

        var namedType = entities.GetComponentType<Named>();
        foreach (var (typeId, inspector) in _inspectors)
        {
            var componentType = entities.GetComponentType(typeId);
            if (nameSystem != null && componentType.Id == namedType.Id) continue;

            var componentValue = entities.GetComponent(value, typeId);
            componentValue = inspector.SubmitUI(editor, componentType.Type.Name, componentValue);
            if (componentValue != null) entities.SetComponent(value, typeId, componentValue);
            ImGui.NewLine();
        }

        if (ImGui.BeginPopupContextWindow())
        {
            if (ImGui.BeginMenu("Add Component"))
            {
                foreach (var metaData in MetaData.All)
                {
                    if (!metaData.IsComponent ||
                        metaData.HasComponent(entities, value)) continue;

                    if (ImGui.MenuItem(metaData.Name))
                    {
                        metaData.AddComponent(entities, value, metaData.CreateInstance());
                    }
                }
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }
}
