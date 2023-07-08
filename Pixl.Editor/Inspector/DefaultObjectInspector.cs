using ImGuiNET;
using System.Linq;
using System.Reflection;

namespace Pixl.Editor;

internal unsafe sealed class DefaultObjectInspector : ObjectInspector
{
    private readonly Type _type;
    private readonly FieldInfo[] _fields;
    private readonly ObjectInspector[] _inspectors;

    public DefaultObjectInspector(Type type)
    {
        _type = type;
        _fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.IsPublic && GetFieldInspector(x) != null)
            .ToArray();
        _inspectors = _fields.Select(x => GetFieldInspector(x)!)
            .ToArray();
    }

    public static ObjectInspector? GetFieldInspector(FieldInfo field)
    {
         return ValueInspectors.GetInspector(field);
    }

    public override object SubmitUI(Editor editor, string label, object value)
    {
        ImGui.SeparatorText(label);
        for (int i = 0; i < _fields.Length; i++)
        {
            var field = _fields[i];
            var inspector = _inspectors[i];

            var fieldValue = field.GetValue(value);
            if (fieldValue != null)
            {
                fieldValue = inspector.SubmitUI(editor, field.Name, fieldValue);
                field.SetValue(value, fieldValue);
            }
        }
        return value;
    }
}