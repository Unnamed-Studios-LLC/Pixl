using Pixl.Editor;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Pixl;

internal sealed class ProjectValuesStore : ValuesStore
{
    private const string FileName = "values.json";
    private readonly Editor.Editor _editor;

    public ProjectValuesStore(Editor.Editor editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public Project Project => _editor.Project ?? throw new Exception("Project is null!");

    public override void Read(Dictionary<string, StoredValue> all)
    {
        var directory = Project.CacheDirectory;
        var filePath = Path.Combine(directory, FileName);
        if (!File.Exists(filePath)) return;

        var file = File.ReadAllText(filePath);
        var values = JsonSerializer.Deserialize<Dictionary<string, StoredValue>>(file);
        if (values != null)
        {
            foreach (var (key, value) in values)
            {
                all[key] = value;
            }
        }
    }

    public override void Write(Dictionary<string, StoredValue> all, Dictionary<string, StoredValue> edited)
    {
        var directory = Project.CacheDirectory;
        var filePath = Path.Combine(directory, FileName);
        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(all);
        File.WriteAllText(filePath, json);
    }
}
