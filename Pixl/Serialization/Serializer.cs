using System.Globalization;
using System.Reflection.Metadata;
using YamlDotNet.Core;
using YamlDotNet.Core.Tokens;
using YamlDotNet.RepresentationModel;

namespace Pixl;

internal static class Serializer
{
    private static readonly YamlScalarNode s_nullScalar = new("null")
    {
        Style = ScalarStyle.Plain
    };

    public static YamlDocument? GetDocument(string label, object value, string tag)
    {
        var type = value.GetType();
        var node = GetNode(type, value);
        if (node == null) return null;
        var labelNode = new YamlScalarNode(label)
        {
            Tag = new TagName(tag)
        };
        var wrappedObject = new YamlMappingNode(labelNode, node);
        return new YamlDocument(wrappedObject);
    }

    public static YamlDocument? GetDocument(object value)
    {
        var type = value.GetType();
        var node = GetNode(type, value);
        if (node == null) return null;
        return new YamlDocument(node);
    }

    public static object? GetObject(YamlDocument document)
    {
        if (document.RootNode is not YamlMappingNode mappingNode ||
            mappingNode.Children.Count == 0) return null;
        var (key, value) = mappingNode.Children.First();

        if (key is not YamlScalarNode keyScalar ||
            keyScalar.Value == null) return null;

        var rootMetaData = MetaData.Get(keyScalar.Value);
        if (rootMetaData == null) return null;

        return GetObject(rootMetaData, value);
    }

    private static YamlNode? GetNode(Type type, object? value)
    {
        var scalar = GetScalar(type, value);
        if (scalar != null) return scalar;

        if (!MetaData.TryGet(type, out var metaData) ||
            metaData == null ||
            value == null) return null;

        return new YamlMappingNode(GetFields(metaData, value));
    }

    private static IEnumerable<KeyValuePair<YamlNode, YamlNode>> GetFields(TypeMetaData metaData, object value)
    {
        foreach (var field in metaData.Fields)
        {
            if (!field.TryGetValue(value, out var fieldValue)) continue;
            var node = GetNode(field.Type, fieldValue);
            if (node == null) continue;
            yield return new KeyValuePair<YamlNode, YamlNode>(field.Name, node);
        }
    }

    private static object? GetObject(Type type, YamlNode valueNode)
    {
        if (valueNode is YamlScalarNode scalarNode)
        {
            return GetScalar(type, scalarNode);
        }

        if (!MetaData.TryGet(type, out var metaData) ||
            metaData == null) return null;

        var value = GetObject(metaData, valueNode);
        return value;
    }

    private static object? GetObject(TypeMetaData metaData, YamlNode node)
    {
        if (node is not YamlMappingNode mappingNode) return null;
        var value = metaData.CreateInstance();
        SetFields(metaData, ref value, mappingNode.Children);
        return value;
    }

    private static YamlScalarNode? GetScalar(Type type, object? value)
    {
        // explicit null
        if (value == null) return s_nullScalar;

        // write scalar
        var typeCode = Type.GetTypeCode(type);
        var invariantCulture = CultureInfo.InvariantCulture;
        YamlScalarNode? node = typeCode switch
        {
            TypeCode.Char => new YamlScalarNode(((char)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Byte => new YamlScalarNode(((byte)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.SByte => new YamlScalarNode(((sbyte)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Int16 => new YamlScalarNode(((short)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Int32 => new YamlScalarNode(((int)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Int64 => new YamlScalarNode(((long)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.UInt16 => new YamlScalarNode(((ushort)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.UInt32 => new YamlScalarNode(((uint)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.UInt64 => new YamlScalarNode(((ulong)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Decimal => new YamlScalarNode(((decimal)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Single => new YamlScalarNode(((float)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.Double => new YamlScalarNode(((double)value).ToString(invariantCulture)) { Style = ScalarStyle.Plain },
            TypeCode.String => new YamlScalarNode(value.ToString()) { Style = ScalarStyle.DoubleQuoted },
            TypeCode.Boolean => new YamlScalarNode((bool)value ? "yes" : "no") { Style = ScalarStyle.Plain },
            _ => null
        };

        return node;
    }

    private static object? GetScalar(Type type, YamlScalarNode? node)
    {
        // parsing error
        if (node == null ||
            node.Value == null) return null;

        // explicit null
        if (node.Style == ScalarStyle.Plain &&
            node.Value != null &&
            node.Value.Equals(s_nullScalar.Value, StringComparison.Ordinal)) return null;

        static bool parseBool(string boolString)
        {
            return boolString.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                boolString.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                boolString.Equals("on", StringComparison.InvariantCultureIgnoreCase);
        }

        // parse scalar
        var typeCode = Type.GetTypeCode(type);
        var invariantCulture = CultureInfo.InvariantCulture;
        object? value = typeCode switch
        {
            TypeCode.Char => char.TryParse(node.Value, out var r) ? r : (char)0,
            TypeCode.Byte => byte.TryParse(node.Value, out var r) ? r : (byte)0,
            TypeCode.Int16 => short.TryParse(node.Value, out var r) ? r : (short)0,
            TypeCode.Int32 => int.TryParse(node.Value, out var r) ? r : 0,
            TypeCode.Int64 => long.TryParse(node.Value, out var r) ? r : 0L,
            TypeCode.UInt16 => ushort.TryParse(node.Value, out var r) ? r : (ushort)0,
            TypeCode.UInt32 => uint.TryParse(node.Value, out var r) ? r : 0,
            TypeCode.UInt64 => ulong.TryParse(node.Value, out var r) ? r : 0UL,
            TypeCode.Decimal => decimal.TryParse(node.Value, invariantCulture, out var r) ? r : 0M,
            TypeCode.Single => float.TryParse(node.Value, invariantCulture, out var r) ? r : 0F,
            TypeCode.Double => double.TryParse(node.Value, invariantCulture, out var r) ? r : 0D,
            TypeCode.String => node.Value,
            TypeCode.Boolean => parseBool(node.Value!),
            _ => null
        };

        return value;
    }

    private static void SetFields(TypeMetaData metaData, ref object value, IEnumerable<KeyValuePair<YamlNode, YamlNode>> nodes)
    {
        foreach (var (keyNode, valueNode) in nodes)
        {
            if (keyNode is not YamlScalarNode keyScalar ||
                keyScalar.Value == null ||
                !metaData.TryGetField(keyScalar.Value, out var field) ||
                field == null) continue;

            var fieldValue = GetObject(field.Type, valueNode);
            field.TrySetValue(ref value, fieldValue);
        }
    }
}
