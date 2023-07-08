using System.Threading.Tasks;

namespace Pixl;

public static class Browser
{
    public static string? Open(FileBrowserRequest request) => FileBrowser.Open(request);
    public static Task<string?> OpenAsync(FileBrowserRequest request) => FileBrowser.OpenAsync(request);
    public static string? OpenFolder(FileBrowserRequest request) => FileBrowser.OpenFolder(request);
    public static Task<string?> OpenFolderAsync(FileBrowserRequest request) => FileBrowser.OpenFolderAsync(request);
    public static IEnumerable<string>? OpenMultiple(FileBrowserRequest request) => FileBrowser.OpenMultiple(request);
    public static Task<IEnumerable<string>?> OpenMultipleAsync(FileBrowserRequest request) => FileBrowser.OpenMultipleAsync(request);
    public static string? Save(FileBrowserRequest request) => FileBrowser.Save(request);
    public static Task<string?> SaveAsync(FileBrowserRequest request) => FileBrowser.SaveAsync(request);

    private static FileBrowser FileBrowser => Game.Shared.Files.FileBrowser;
}
