namespace Pixl;

public struct FileBrowserRequest
{
    public FileBrowserRequest(string defaultName, string directory) : this(defaultName, directory, Array.Empty<FileExtension>()) { }
    public FileBrowserRequest(string defaultName, string directory, params string[] extensions) : this(defaultName, directory, extensions.Select(x => new FileExtension(x, x)).ToArray()) { }
    public FileBrowserRequest(string defaultName, string directory, params FileExtension[] extensions)
    {
        DefaultName = defaultName;
        Directory = directory;
        Extensions = extensions;
    }

    public string DefaultName { get; set; }
    public string Directory { get; set; }
    public FileExtension[] Extensions { get; set; }
}
