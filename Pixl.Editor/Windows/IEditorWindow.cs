namespace Pixl.Editor;

internal interface IEditorWindow
{
    string Name { get; }
    bool Open { get; set; }
    void SubmitUI();
}
