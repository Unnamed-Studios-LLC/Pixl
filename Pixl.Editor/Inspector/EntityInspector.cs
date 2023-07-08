using EntitiesDb;
using ImGuiNET;
using System.Linq;

namespace Pixl.Editor;

internal sealed class EntityInspector : ObjectInspector<uint>
{
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

        var nameSystem = editor.Game.Scene.GetSystem<NameSystem>();
        if (nameSystem != null)
        {
            var name = nameSystem.GetName(value);
            if (ImGui.InputText("Name", ref name, 256))
            {
                nameSystem.SetName(value, name);
            }
            ImGui.NewLine();
        }

        var editableType = entities.GetComponentType<Editable>();
        foreach (var (typeId, inspector) in _inspectors)
        {
            var componentType = entities.GetComponentType(typeId);
            if (componentType.Id == editableType.Id) continue;

            var componentValue = entities.GetComponent(value, typeId);
            componentValue = inspector.SubmitUI(editor, componentType.Type.Name, componentValue);
            if (componentValue != null) entities.SetComponent(value, typeId, componentValue);
            ImGui.NewLine();
        }

        if (ImGui.BeginPopupContextWindow())
        {
            if (ImGui.BeginMenu("Add"))
            {
                if (ImGui.MenuItem("Sprite"))
                {
                    entities.AddComponent(value, Sprite.Default);
                }
                if (ImGui.MenuItem("Camera"))
                {
                    entities.AddComponent<Camera>(value);
                }
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }
}
