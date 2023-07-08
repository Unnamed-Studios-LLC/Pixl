using EntitiesDb;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace Pixl.Editor;

internal sealed class HierarchyWindow : EditorWindow
{
    private readonly Scene _scene;
    private readonly List<uint> _selectedEntities = new();
    private readonly List<int> _selectedIndices = new();
    private readonly List<uint> _entityList = new();
    private readonly List<(int, int)> _rangeSelect = new();
    private readonly EntityLayout _createLayout;
    private readonly PropertiesWindow _properties;
    private NameSystem? _nameSystem;
    private int _listLength = 0;
    private uint _createdEntityId;
    private uint _destroyEntityId;
    private uint _clickedEntityId;

    public HierarchyWindow(Scene scene, PropertiesWindow properties)
    {
        _scene = scene;
        _properties = properties;
        Open = true;
        _createLayout = EntityLayoutBuilder.Create()
            .Add<Transform>()
            .Add<Editable>()
            .Build();
        _createLayout.Set(Transform.Default);
    }

    public override string Name => "Hierarchy";

    protected override void OnUI()
    {
        for (int i = 0; i < _selectedEntities.Count; i++)
        {
            var entity = _selectedEntities[i];
            if (_scene.Entities.EntityExists(entity)) continue;
            _selectedEntities.RemoveAt(i);
            _selectedIndices.RemoveAt(i);
            i--;
        }

        _nameSystem = _scene.GetSystem<NameSystem>();

        if (ImGui.Button("Create"))
        {
            _selectedEntities.Clear();
            _selectedIndices.Clear();
            _createdEntityId = _scene.Entities.CreateEntity(_createLayout);
        }

        ImGui.SameLine();
        if (ImGui.Button("Destroy"))
        {
            foreach (var entity in _selectedEntities)
            {
                _scene.Entities.DestroyEntity(entity);
            }
            _selectedEntities.Clear();
            _selectedIndices.Clear();
        }

        ImGui.Separator();

        if (ImGui.BeginChild("Scroll Region", new Vector2(0, ImGui.GetContentRegionAvail().Y), false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            _scene.Entities.With<Editable>().ForEach((in Entity entity) =>
            {
                ref var transform = ref entity.TryGetComponent<Transform>(out var found);
                SubmitEntity(in entity);
            });
            PostEntities();
            ImGui.EndChild();
        }
    }

    private void PostEntities()
    {
        _listLength = 0;
        _createdEntityId = 0;
        RangeSelect();

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _clickedEntityId = 0;
        }
        if (_destroyEntityId != 0)
        {
            _scene.Entities.DestroyEntity(_destroyEntityId);
            foreach (var entityId in _selectedEntities)
            {
                _scene.Entities.DestroyEntity(entityId);
            }
            _destroyEntityId = 0;
            _selectedEntities.Clear();
            _selectedIndices.Clear();
        }
    }

    private void RangeSelect()
    {
        for (int i = 0; i < _rangeSelect.Count; i++)
        {
            var (start, end) = _rangeSelect[i];
            var dif = end - start;
            var direction = Math.Sign(dif);
            var count = Math.Abs(dif);
            for (int j = 0; j < count; j++)
            {
                var index = start + j * direction;
                var entityId = _entityList[index];
                if (!_selectedEntities.Contains(entityId))
                {
                    _selectedEntities.Add(entityId);
                    _selectedIndices.Add(index);
                }
            }
        }
        _rangeSelect.Clear();
    }

    private unsafe void SubmitEntity(in Entity entity)
    {
        int listIndex = _listLength++;
        if (_entityList.Count > listIndex) _entityList[listIndex] = entity.Id;
        else _entityList.Add(entity.Id);

        var selectedIndex = _selectedEntities.IndexOf(entity.Id);

        var name = _nameSystem?.GetName(entity.Id) ?? "No Name";
        var flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf;
        if (selectedIndex >= 0) flags |= ImGuiTreeNodeFlags.Selected;

        var nodeOpen = ImGui.TreeNodeEx(name, flags);
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.Selectable("Destroy"))
            {
                _destroyEntityId = entity.Id;
                if (selectedIndex < 0)
                {
                    _selectedEntities.Clear();
                    _selectedIndices.Clear();
                }
            }
            ImGui.EndPopup();
        }

        if (_createdEntityId == entity.Id)
        {
            _selectedEntities.Add(entity.Id);
            _selectedIndices.Add(listIndex);
        }
        else if (ImGui.IsItemClicked())
        {
            _clickedEntityId = entity.Id;
        }
        else if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) &&
            ImGui.IsItemHovered() &&
            _clickedEntityId == entity.Id)
        {
            var shiftDown = ImGui.GetIO().KeyShift;
            var ctrlDown = ImGui.GetIO().KeyCtrl;
            if (shiftDown &&
                _selectedIndices.Count > 0)
            {
                _rangeSelect.Add((_selectedIndices[^1], listIndex));
            }

            if (!shiftDown &&
                !ctrlDown)
            {
                _selectedEntities.Clear();
                _selectedIndices.Clear();
                selectedIndex = -1;
            }

            if (selectedIndex < 0)
            {
                _selectedEntities.Add(entity.Id);
                _selectedIndices.Add(listIndex);
                _properties.SelectedObject = entity.Id;
            }
            else if (!shiftDown)
            {
                _selectedEntities.RemoveAt(selectedIndex);
                _selectedIndices.RemoveAt(selectedIndex);
            }
        }

        if (_selectedEntities.Count != 0 &&
            ImGui.BeginDragDropSource())
        {
            var id = entity.Id;
            ImGui.SetDragDropPayload("EntityId", (nint)(&id), sizeof(uint), ImGuiCond.Once);
            for (int i = 0; i < 5 && i < _selectedEntities.Count; i++)
            {
                var selectedName = _nameSystem?.GetName(_selectedEntities[_selectedEntities.Count - i - 1]) ?? "No Name";
                if (i == 0) ImGui.Text(selectedName);
                else ImGui.TextDisabled(selectedName);
            }
            if (_selectedEntities.Count > 5) ImGui.Text($"{_selectedEntities.Count - 5} more...");
            ImGui.EndDragDropSource();
        }

        if (!nodeOpen) return;
        ImGui.TreePop();
    }
}
