namespace Pixl.SourceGenerators
{
    public enum ImplmentedTypes
    {
        None,
        ComponentSystem = 1,
        Component = 1 << 1,
        Vertex = 1 << 2,
        Serializable = 1 << 3
    }
}
