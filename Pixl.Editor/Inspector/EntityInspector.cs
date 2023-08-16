using EntitiesDb;
using ImGuiNET;
using System.Collections.Generic;
using System.Text;

namespace Pixl.Editor;

internal sealed class EntityInspector : ObjectInspector<uint>
{
    private string? _idLabel;
    private bool _editingName;
    private readonly byte[] _nameBuffer = new byte[64];
    private readonly Dictionary<Type, ObjectInspector> _inspectors = new();

    private int BufferStringLength
    {
        get
        {
            int i;
            for (i = 0; i < _nameBuffer.Length && _nameBuffer[i] != 0; i++) { }
            return i;
        }
    }

    protected override void OnSubmitUI(Editor editor, string label, ref uint entityId)
    {
        var entities = editor.Game.Scene.Entities;

        if (entityId == 0 || !entities.EntityExists(entityId))
        {
            return;
        }

        // disabled
        var hasDisabled = entities.HasComponent<Disabled>(entityId);
        var disabled = hasDisabled;
        if (ImGui.Checkbox("Disabled", ref disabled) &&
            disabled != hasDisabled)
        {
            if (disabled) entities.AddComponent<Disabled>(entityId);
            else entities.RemoveComponent<Disabled>(entityId);
        }

        // id
        _idLabel ??= entityId.ToString();
        ImGui.Text("Id:");
        ImGui.SameLine();
        ImGui.TextDisabled(_idLabel);

        // name
        SubmitNameField(entities, entityId);

        // components
        foreach (var type in entities.GetComponentTypes(entityId))
        {
            var metaData = MetaData.Get(type);
            if (!_inspectors.TryGetValue(type, out var inspector))
            {
                inspector = ObjectInspector.Create(type);
                _inspectors.Add(type, inspector);
            }

            if (metaData.Bufferable)
            {
                var bufferValue = metaData.GetBuffer(entities, entityId);
                if (bufferValue != null)
                {
                    inspector.SubmitUI(editor, type.Name, bufferValue);
                }
            }
            else
            {
                var componentValue = metaData.GetComponent(entities, entityId);
                if (componentValue != null)
                {
                    componentValue = inspector.SubmitUI(editor, type.Name, componentValue);
                    if (componentValue != null)
                    {
                        metaData.SetComponent(entities, entityId, componentValue);
                    }
                }
            }
            ImGui.NewLine();
        }

        // popup window
        if (ImGui.BeginPopupContextWindow())
        {
            if (ImGui.BeginMenu("Add Component"))
            {
                foreach (var metaData in MetaData.All)
                {
                    if (!metaData.IsComponent ||
                        metaData.HasComponent(entities, entityId)) continue;

                    if (ImGui.MenuItem(metaData.Name))
                    {
                        metaData.AddComponent(entities, entityId, metaData.CreateInstance());
                    }
                }
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private void SubmitNameField(EntityDatabase entities, uint entityId)
    {
        var name = entities.GetNameOrPrefixed(entityId);

        if (!_editingName)
        {
            var nameSpan = name.AsSpan();
            Encoding.UTF8.GetBytes(nameSpan, _nameBuffer);
        }

        if (ImGui.InputText("Name", _nameBuffer, (uint)_nameBuffer.Length, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            var nameString = Encoding.UTF8.GetString(_nameBuffer.AsSpan(0, BufferStringLength));
            entities.AddComponent(entityId, new Name(nameString));
            _editingName = false;
        }
        ImGui.NewLine();
    }
}
