using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixl.Editor;

internal static class ProjectBuilder
{
    public static bool IsValidDestination(string? filePath)
    {
        if (filePath == null) return false;
        if (Directory.Exists(filePath))
        {
            var hasFiles = Directory.EnumerateFiles(filePath, "*.*", SearchOption.AllDirectories).Any();
            if (hasFiles) return false;
            var hasFolders = Directory.EnumerateDirectories(filePath, "*.*", SearchOption.AllDirectories).Any();
            if (hasFolders) return false;
        }
        return true;
    }

    public static async Task<string> CreateAsync(string destinationPath, EditorTask editorTask)
    {
        editorTask.State = "Validating destination...";
        if (!IsValidDestination(destinationPath))
        {
            throw new EditorException("Unable to create project, destination folder is not empty!", $"Destination Folder: {destinationPath}");
        }

        editorTask.State = "Creating directory...";
        try
        {
            Directory.CreateDirectory(destinationPath);
        }
        catch (Exception e)
        {
            throw new EditorException("Unable to create project directory.", e.Message, e);
        }

        editorTask.State = "Creating project files...";
        try
        {
            var project = await Task.Run(() => new Project(destinationPath));

            // create directories
            Directory.CreateDirectory(project.AssetsDirectory);
        }
        catch (Exception e)
        {
            Directory.Delete(destinationPath, true);
            throw new EditorException("Unable to create project files.", e.Message, e);
        }

        return destinationPath;
    }
}
