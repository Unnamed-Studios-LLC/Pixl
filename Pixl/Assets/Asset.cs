namespace Pixl;

internal sealed class Asset
{
    public Guid Guid { get; set; }
    public FileHandle Handle { get; set; }
    public long Version { get; set; }
    public AssetImporter? Importer { get; set; }
    public Resource[]? Resources { get; set; }
}