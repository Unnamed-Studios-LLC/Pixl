using ImGuiNET;

namespace Pixl.Editor;

internal abstract class ListInspector : ObjectInspector
{
    private readonly ObjectInspector _elementInspector;
    private int _selected;

    protected ListInspector(ObjectInspector elementInspector)
    {
        _elementInspector = elementInspector;
    }

    public override object? SubmitUI(Editor editor, string label, object @object)
    {
        if (ImGui.TreeNode(label))
        {
            var length = GetLength(@object);
            for (int i = 0; i < length; i++)
            {
                var element = GetElement(@object, i);
                element = _elementInspector.SubmitUI(editor, $"Element {i}", element);
                if (element != null) @object = SetElement(@object, i, element);
            }

            if (ImGui.Button("Add"))
            {
                @object = AddElement(@object, length);
            }

            ImGui.SameLine();
            ImGui.Button("Remove");
        }
        return @object;
    }

    protected abstract object AddElement(object @object, int index);
    protected abstract int GetLength(object @object);
    protected abstract object GetElement(object @object, int index);
    protected abstract object RemoveElement(object @object, int index);
    protected abstract object SetElement(object @object, int index, object value);
}
