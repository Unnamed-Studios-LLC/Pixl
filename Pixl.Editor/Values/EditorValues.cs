using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pixl.Editor;

internal sealed class EditorValues
{
    private readonly Values _values;
    private readonly Dictionary<string, object?> _cache = new();

    public EditorValues(Values values)
    {
        _values = values;
    }

    public string[] RecentProjects
    {
        get => GetParsed(Array.Empty<string>(), x => string.IsNullOrWhiteSpace(x) ? Array.Empty<string>() : x.Split('|'))!;
        set => SetParsed(value, x => string.Join('|', value));
    }

    public void AddRecentProject(string projectDirectory)
    {
        var list = RecentProjects.ToList();
        int currentIndex;
        while ((currentIndex = list.FindIndex(x => x.Equals(projectDirectory, StringComparison.OrdinalIgnoreCase))) != -1)
        {
            list.RemoveAt(currentIndex);
        }
        while (list.Count >= 10) list.RemoveAt(list.Count - 1);
        list.Insert(0, projectDirectory);
        RecentProjects = list.ToArray();
        _values.Save();
    }

    public void RemoveRecentProject(int index)
    {
        var list = RecentProjects.ToList();
        list.RemoveAt(index);
        RecentProjects = list.ToArray();
        _values.Save();
    }

    private T? GetParsed<T>(T @default, Func<string, T> parser, [CallerMemberName] string? key = null)
    {
        if (parser is null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_cache.TryGetValue(key, out var parsed)) return parsed == null ? default : (T)parsed;

        var @string = _values.GetString(key, null);
        var value = @string == null ? @default : parser(@string);
        _cache[key] = value;
        return value;
    }

    private void SetParsed<T>(T value, Func<T, string> parser, [CallerMemberName] string? key = null)
    {
        if (parser is null)
        {
            throw new ArgumentNullException(nameof(parser));
        }

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        _values.SetString(key, parser(value));
        _cache[key] = value;
    }
}
