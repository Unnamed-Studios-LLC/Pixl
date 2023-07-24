using System.Threading.Tasks;

namespace Pixl;

internal abstract class FileBrowser
{
    public abstract string? Open(FileBrowserRequest request);
    public virtual async Task<string?> OpenAsync(FileBrowserRequest request) => await Task.Run(() => Open(request)).ConfigureAwait(false);

    public abstract string? OpenFolder(FileBrowserRequest request);
    public virtual async Task<string?> OpenFolderAsync(FileBrowserRequest request) => await Task.Run(() => OpenFolder(request)).ConfigureAwait(false);

    public abstract IEnumerable<string>? OpenMultiple(FileBrowserRequest request);
    public virtual async Task<IEnumerable<string>?> OpenMultipleAsync(FileBrowserRequest request) => await Task.Run(() => OpenMultiple(request)).ConfigureAwait(false);

    public abstract string? Save(FileBrowserRequest request);
    public virtual async Task<string?> SaveAsync(FileBrowserRequest request) => await Task.Run(() => Save(request)).ConfigureAwait(false);
}
