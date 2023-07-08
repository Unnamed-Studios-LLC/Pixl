namespace Pixl.Editor;

internal sealed class PropertiesWindow : EditorWindow
{
    private readonly Editor _editor;
    private object? _selectedObject;
    private ObjectInspector? _selectedInspector;

    public PropertiesWindow(Editor editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public override string Name => "Properties";

    public object? SelectedObject
    {
        get => _selectedObject;
        set => SetSelectedObject(value);
    }

    protected override void OnUI()
    {
        if (_selectedObject != null &&
            _selectedInspector != null)
        {
            _selectedInspector.SubmitUI(_editor, _selectedObject.GetType().Name, _selectedObject);
        }
    }

    private void SetSelectedObject(object? value)
    {
        if (_selectedObject == value) return;
        if (value is uint)
        {
            _selectedInspector = new EntityInspector();
        }
        else _selectedInspector = value == null ? null : new DefaultObjectInspector(value.GetType());
        _selectedObject = value;
    }
}
