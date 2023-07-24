using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pixl.Win;

internal sealed class WinFileBrowser : FileBrowser
{
    private readonly IWin32Window _window;

    public WinFileBrowser(IWin32Window window)
    {
        _window = window;
    }

    public override string? Open(FileBrowserRequest request)
    {
        string? result = null;
        void dialog()
        {
            using var dialog = new OpenFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
            dialog.Filter = GetFilter(in request);
            dialog.FilterIndex = 0;
            dialog.Multiselect = false;

            if (dialog.ShowDialog(_window) != DialogResult.OK) return;
            result = dialog.FileName;
        }
        DialogThread(dialog);
        return result;
    }

    public override Task<string?> OpenAsync(FileBrowserRequest request)
    {
        var completionSource = new TaskCompletionSource<string?>();
        string? result = null;
        void dialog()
        {
            using var dialog = new OpenFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
            dialog.Filter = GetFilter(in request);
            dialog.FilterIndex = 0;
            dialog.Multiselect = false;

            if (dialog.ShowDialog(_window) != DialogResult.OK)
            {
                completionSource.SetResult(null);
                return;
            }

            result = dialog.FileName;
            completionSource.SetResult(dialog.FileName);
        }
        DialogThreadAsync(dialog);
        return completionSource.Task;
    }

    public override string? OpenFolder(FileBrowserRequest request)
    {
        string? result = null;
        void dialog()
        {
            using var dialog = new FolderBrowserDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog(_window) != DialogResult.OK) return;
            result = dialog.SelectedPath;
        }
        DialogThread(dialog);
        return result;
    }

    public override Task<string?> OpenFolderAsync(FileBrowserRequest request)
    {
        var completionSource = new TaskCompletionSource<string?>();
        void dialog()
        {
            using var dialog = new FolderBrowserDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog(_window) != DialogResult.OK)
            {
                completionSource.SetResult(null);
                return;
            }
            completionSource.SetResult(dialog.SelectedPath);
        }
        DialogThreadAsync(dialog);
        return completionSource.Task;
    }

    public override IEnumerable<string>? OpenMultiple(FileBrowserRequest request)
    {
        using var dialog = new OpenFileDialog();
        dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
        dialog.Filter = GetFilter(in request);
        dialog.FilterIndex = 0;
        dialog.Multiselect = true;

        if (dialog.ShowDialog(_window) != DialogResult.OK) return null;
        var results = dialog.FileNames;
        if (results?.Length == 0) return null;
        return results;
    }

    public override string? Save(FileBrowserRequest request)
    {
        string? result = null;
        void dialog()
        {
            using var dialog = new SaveFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
            dialog.Filter = GetFilter(in request);
            dialog.FilterIndex = 0;

            if (dialog.ShowDialog(_window) != DialogResult.OK) return;
            result = dialog.FileName;
        }
        DialogThread(dialog);
        return result;
    }

    public override Task<string?> SaveAsync(FileBrowserRequest request)
    {
        var completionSource = new TaskCompletionSource<string?>();
        string? result = null;
        void dialog()
        {
            using var dialog = new SaveFileDialog();
            dialog.InitialDirectory = string.IsNullOrEmpty(request.Directory) ? string.Empty : request.Directory;
            dialog.Filter = GetFilter(in request);
            dialog.FilterIndex = 0;

            if (dialog.ShowDialog(_window) != DialogResult.OK)
            {
                completionSource.SetResult(null);
                return;
            }

            result = dialog.FileName;
            completionSource.SetResult(dialog.FileName);
        }
        DialogThreadAsync(dialog);
        return completionSource.Task;
    }

    private static void DialogThread(ThreadStart threadStart)
    {
        var thread = new Thread(threadStart);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    private static void DialogThreadAsync(ThreadStart threadStart)
    {
        var thread = new Thread(threadStart);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private static string GetFilter(in FileBrowserRequest request)
    {
        if (request.Extensions == null ||
            request.Extensions.Length == 0) return "All files|*.*";

        var builder = new StringBuilder();
        foreach (ref var extension in request.Extensions.AsSpan())
        {
            if (builder.Length != 0) builder.Append('|');
            builder.Append(extension.Name);
            builder.Append('|');
            builder.Append('*');
            builder.Append(extension.Extension);
        }
        return builder.ToString();
    }
}
