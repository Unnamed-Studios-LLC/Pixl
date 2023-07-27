using EntitiesDb;
using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using YamlDotNet.RepresentationModel;

namespace Pixl.Editor;

internal sealed class SceneWindow : EditorWindow
{
    private readonly Scene _scene;
    private readonly List<uint> _selectedEntities = new();
    private readonly List<int> _selectedIndices = new();
    private readonly List<uint> _entityList = new();
    private readonly List<(int, int)> _rangeSelect = new();
    private readonly EntityLayout _createTransformLayout;
    private readonly EntityLayout _createSpriteLayout;
    private readonly PropertiesWindow _properties;
    private readonly Editor _editor;
    private int _listLength = 0;
    private uint _createdEntityId;
    private uint _clickedEntityId;
    private string? _openFilePath;


    public SceneWindow(Scene scene, PropertiesWindow properties, Editor editor)
    {
        _scene = scene;
        _properties = properties;
        _editor = editor;
        Open = true;

        _createTransformLayout = new EntityLayout();
        _createTransformLayout.Add(Transform.Default);

        _createSpriteLayout = new EntityLayout();
        _createSpriteLayout.Add(Transform.Default);
        _createSpriteLayout.Add(Sprite.Default);
    }

    public void OpenScene()
    {
        var filePath = _editor.Files.FileBrowser.Open(new FileBrowserRequest("Scene", string.Empty, new FileExtension("Pixl Scene File", "pixl")));
        if (filePath == null) return;

        try
        {
            using var stream = File.OpenRead(filePath);
            using var streamReader = new StreamReader(stream);

            var yamlStream = new YamlStream();
            yamlStream.Load(streamReader);

            _scene.Clear();
            _scene.LoadDocuments(yamlStream.Documents);

            _openFilePath = filePath;
            UnsavedChanges = false;
        }
        catch (Exception e)
        {
            _editor.PushError(e);
        }
    }

    public override void Save()
    {
        var filePath = _openFilePath ?? _editor.Files.FileBrowser.Save(new FileBrowserRequest("Scene", string.Empty, new FileExtension("Pixl Scene File", "pixl")));
        if (filePath == null || _editor.Project == null) return;

        var tempPath = Path.Combine(_editor.Project.CacheDirectory, $"{Guid.NewGuid()}.temp");
        try
        {
            var directoryName = Path.GetDirectoryName(tempPath);
            if (!string.IsNullOrEmpty(directoryName)) Directory.CreateDirectory(directoryName);

            using (var stream = File.Open(tempPath, FileMode.Create))
            {
                using var streamWriter = new StreamWriter(stream);

                var documentList = new List<YamlDocument>();
                _scene.GetDocuments(documentList);
                var yamlStream = new YamlStream(documentList);
                yamlStream.Save(streamWriter, false);
            }

            File.Move(tempPath, filePath, true);

            _openFilePath = filePath;
            UnsavedChanges = false;
        }
        catch (Exception e)
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
            _editor.Logger.Log(e);
        }
    }

    public override string Name => "Scene";

    protected override void OnUI()
    {
        if (_editor.Project != null && _openFilePath == null) UnsavedChanges = true;

        for (int i = 0; i < _selectedEntities.Count; i++)
        {
            var entity = _selectedEntities[i];
            if (_scene.Entities.EntityExists(entity)) continue;
            _selectedEntities.RemoveAt(i);
            _selectedIndices.RemoveAt(i);
            i--;
        }

        if (!ImGui.BeginTabBar("Scene Tabs")) return;

        if (ImGui.BeginTabItem("Entities"))
        {
            if (ImGui.BeginChild("Scroll Region", new Vector2(0, ImGui.GetContentRegionAvail().Y), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                _scene.Entities.IncludeDisabled().ForEach(SubmitEntity);
                PostEntities();

                if (ImGui.BeginPopupContextWindow())
                {
                    if (_selectedEntities.Count > 0)
                    {
                        if (ImGui.MenuItem("Destroy"))
                        {
                            DestroySelected();
                        }

                        ImGui.Separator();
                    }

                    if (ImGui.MenuItem("Create Empty"))
                    {
                        _createdEntityId = _scene.Entities.CreateEntity();
                        _selectedEntities.Clear();
                        _selectedIndices.Clear();
                    }

                    if (ImGui.BeginMenu("Create"))
                    {
                        if (ImGui.MenuItem("Transform"))
                        {
                            _createdEntityId = _scene.Entities.CreateEntity(_createTransformLayout);
                            _selectedEntities.Clear();
                            _selectedIndices.Clear();
                        }

                        if (ImGui.MenuItem("Sprite"))
                        {
                            _createdEntityId = _scene.Entities.CreateEntity(_createSpriteLayout);
                            _selectedEntities.Clear();
                            _selectedIndices.Clear();
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndPopup();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Systems"))
        {
            if (ImGui.BeginChild("Scroll Region", new Vector2(0, ImGui.GetContentRegionAvail().Y), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                var systems = _scene.GetSystems();
                foreach (var system in systems)
                {
                    if (system == null) continue;
                    if (ImGui.Selectable(system.GetType().Name, _properties.SelectedObject == system))
                    {
                        _properties.SelectedObject = system;
                    }
                }
                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Canvas"))
        {
            if (ImGui.BeginChild("Scroll Region", new Vector2(0, ImGui.GetContentRegionAvail().Y), false, ImGuiWindowFlags.HorizontalScrollbar))
            {

                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }
    }

    private void DestroySelected()
    {
        foreach (var entityId in _selectedEntities)
        {
            _scene.Entities.DestroyEntity(entityId);
        }
        _selectedEntities.Clear();
        _selectedIndices.Clear();
    }

    private void PostEntities()
    {
        _listLength = 0;
        _createdEntityId = 0;
        RangeSelect();

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right))
        {
            _clickedEntityId = 0;
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

    private unsafe void SubmitEntity(uint entityId)
    {
        var entities = _scene.Entities;

        int listIndex = _listLength++;
        if (_entityList.Count > listIndex) _entityList[listIndex] = entityId;
        else _entityList.Add(entityId);

        var selectedIndex = _selectedEntities.IndexOf(entityId);

        // flags
        var flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf;
        if (selectedIndex >= 0) flags |= ImGuiTreeNodeFlags.Selected;

        // disabled
        var disabled = entities.HasComponent<Disabled>(entityId);
        if (disabled) ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));

        // tree node
        var name = entities.GetNameOrPrefixed(entityId, "Entity ");
        var nodeOpen = ImGui.TreeNodeEx(name.AsSpan(), flags);

        // pop disabled style
        if (disabled) ImGui.PopStyleColor();

        if (_createdEntityId == entityId)
        {
            _selectedEntities.Add(entityId);
            _selectedIndices.Add(listIndex);
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Left) || ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            _clickedEntityId = entityId;
        }
        else if ((ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)) &&
            ImGui.IsItemHovered() &&
            _clickedEntityId == entityId)
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
                _selectedEntities.Add(entityId);
                _selectedIndices.Add(listIndex);
                _properties.SelectedObject = entityId;
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
            var id = entityId;
            ImGui.SetDragDropPayload("EntityId", (nint)(&id), sizeof(uint), ImGuiCond.Once);
            for (int i = 0; i < 5 && i < _selectedEntities.Count; i++)
            {
                var listEntityId = _selectedEntities[_selectedEntities.Count - i - 1];
                var selectedName = entities.GetNameOrPrefixed(listEntityId);

                if (i == 0) ImGui.Text(selectedName.AsSpan());
                else ImGui.TextDisabled(selectedName.AsSpan());
            }
            if (_selectedEntities.Count > 5) ImGui.Text($"{_selectedEntities.Count - 5} more...");
            ImGui.EndDragDropSource();
        }

        if (!nodeOpen) return;
        ImGui.TreePop();
    }
}
