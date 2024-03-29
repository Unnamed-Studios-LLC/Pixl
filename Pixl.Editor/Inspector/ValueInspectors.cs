﻿using System.Collections.Generic;
using System.Reflection;

namespace Pixl.Editor;

internal static class ValueInspectors
{
    private static readonly Dictionary<Type, Func<FieldInfo, ObjectInspector>> s_inspectors = new()
    {
        [typeof(sbyte)] = field => new Int8Inspector(field),
        [typeof(short)] = field => new Int16Inspector(),
        [typeof(int)] = field => new Int32Inspector(),
        [typeof(long)] = field => new Int64Inspector(),
        [typeof(byte)] = field => new UInt8Inspector(),
        [typeof(ushort)] = field => new UInt16Inspector(),
        [typeof(uint)] = field => new UInt32Inspector(),
        [typeof(ulong)] = field => new UInt64Inspector(),
        [typeof(float)] = field => new FloatInspector(),
        [typeof(double)] = field => new DoubleInspector(),
        [typeof(bool)] = field => new BoolInspector(),

        [typeof(Color32)] = field => new Color32Inspector(),

        [typeof(Vec2)] = field => new Vec2Inspector(field),
        [typeof(Vec3)] = field => new Vec3Inspector(field),
        [typeof(Vec4)] = field => new Vec4Inspector(field),

        [typeof(Rect)] = field => new RectInspector(field),
        [typeof(RectInt)] = field => new RectIntInspector(field),

        [typeof(Entity)] = field => new EntityIdInspector(),
        [typeof(ResourceView)] = field => new ResourceViewInspector(field.GetCustomAttribute<ResourceTypeAttribute>()?.ResourceType),
    };

    public static ObjectInspector? GetInspector(FieldInfo field)
    {
        return s_inspectors.TryGetValue(field.FieldType, out var factory) ? factory(field) : null;
    }
}
