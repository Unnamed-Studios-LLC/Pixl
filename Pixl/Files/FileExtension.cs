namespace Pixl;

public readonly struct FileExtension
{
    public FileExtension(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }

    public string Name { get; }
    public string Extension { get; }
}
