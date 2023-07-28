using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Pixl;

public struct Node
{
    private Dictionary<string, YamlNode>? _dictionary;
    private readonly SerializationMode _mode;

    internal Node(YamlMappingNode mappingNode)
    {
        _mode = SerializationMode.Read;

        foreach (var (key, value) in mappingNode)
        {
            if (key is not YamlScalarNode scalarNode ||
                scalarNode.Value == null) continue;
            _dictionary ??= new();
            _dictionary[scalarNode.Value] = value;
        }
    }

    public string? Value(string key, string? value)
    {
        if (_mode == SerializationMode.Write)
        {
            _dictionary ??= new();
            if (value is null) _dictionary[key] = Serializer.NullScalar;
            else _dictionary.Add(key, new YamlScalarNode(value));
            return value;
        }

        if (_dictionary == null ||
            !_dictionary.TryGetValue(key, out var node) ||
            node is not YamlScalarNode scalarNode)
        {
            return null;
        }

        return scalarNode.Value;
    }

    internal YamlMappingNode GetMappingNode()
    {
        var node = new YamlMappingNode();
        if (_dictionary != null)
        {
            foreach (var (key, value) in _dictionary)
            {
                node.Add(key, value);
            }
        }
        return node;
    }
}
