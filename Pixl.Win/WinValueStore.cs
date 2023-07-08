using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixl.Win;

internal sealed class WinValueStore : ValuesStore
{
    public readonly string _companyName;
    public readonly string _productName;

    public WinValueStore(string companyName, string productName)
    {
        _companyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        _productName = productName ?? throw new ArgumentNullException(nameof(productName));
    }

    private string SubKeyName => $"Software\\{_companyName}\\{_productName}";

    public override void Read(Dictionary<string, StoredValue> all)
    {
        var registryKey = OpenKey();
        if (registryKey == null) return;

        foreach (var name in registryKey.GetValueNames())
        {
            var kind = registryKey.GetValueKind(name);
            if (kind != RegistryValueKind.Binary ||
                registryKey.GetValue(name) is not byte[] registryValue) continue;

            var value = Read(registryValue);
            if (value == null) continue;
            all[name] = value.Value;
        }
    }

    public override void Write(Dictionary<string, StoredValue> all, Dictionary<string, StoredValue> edited)
    {
        var registryKey = OpenKey() ?? CreateKey();
        if (registryKey == null) return;

        foreach (var (key, value) in edited)
        {
            var length = LengthOf(value);
            var binary = new byte[length + 1];
            binary[0] = (byte)value.Type;
            Write(value, binary.AsSpan(1));
            registryKey.SetValue(key, binary, RegistryValueKind.Binary);
        }
    }


    private RegistryKey? CreateKey() => Registry.CurrentUser.CreateSubKey(SubKeyName);
    private RegistryKey? OpenKey() => Registry.CurrentUser.OpenSubKey(SubKeyName, true);

    private static StoredValue? Read(byte[] binary)
    {
        if (binary.Length < 1) return null;
        var type = (ValueType)binary[0];
        if (!Enum.IsDefined(type)) return null;
        var @object = type switch
        {
            ValueType.Float => ReadFloat(binary.AsSpan(1)),
            ValueType.Int => ReadInt(binary.AsSpan(1)),
            _ => ReadString(binary.AsSpan(1))
        };
        if (@object == null) return null;
        return new StoredValue(type, @object);
    }

    private static object? ReadFloat(ReadOnlySpan<byte> binary)
    {
        if (binary.Length != 4) return null;
        var value = BitConverter.ToSingle(binary);
        return value;
    }

    private static object? ReadInt(ReadOnlySpan<byte> binary)
    {
        if (binary.Length != 4) return null;
        var value = BitConverter.ToInt32(binary);
        return value;
    }

    private static object? ReadString(ReadOnlySpan<byte> binary)
    {
        var value = Encoding.UTF8.GetString(binary);
        return value;
    }

    private static int LengthOf(StoredValue value)
    {
        return value.Type switch
        {
            ValueType.Float => LengthOfFloat(),
            ValueType.Int => LengthOfInt(),
            _ => LengthOfString(value.Value.ToString() ?? string.Empty)
        };
    }

    private static int LengthOfFloat() => sizeof(float);
    private static int LengthOfInt() => sizeof(int);
    private static int LengthOfString(string value) => Encoding.UTF8.GetByteCount(value);

    private static void Write(StoredValue value, Span<byte> binary)
    {
        switch (value.Type)
        {
            case ValueType.Float:
                WriteFloat((float)value.Value, binary);
                break;
            case ValueType.Int:
                WriteInt((int)value.Value, binary);
                break;
            default:
                WriteString(value.Value.ToString() ?? string.Empty, binary);
                break;
        }
    }

    private static void WriteFloat(float value, Span<byte> binary)
    {
        BitConverter.TryWriteBytes(binary, value);
    }

    private static void WriteInt(int value, Span<byte> binary)
    {
        BitConverter.TryWriteBytes(binary, value);
    }

    private static void WriteString(string value, Span<byte> binary)
    {
        Encoding.UTF8.GetBytes(value, binary);
    }
}
