namespace Pixl.Editor;

internal interface IEditorUI
{
    string Name { get; }
    bool Open { get; set; }
    void SubmitUI();
}
