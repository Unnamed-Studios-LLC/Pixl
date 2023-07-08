using System.IO;
using System.Reflection;

namespace Pixl.Editor;

internal sealed class Project
{
    public Project(string projectDirectory)
    {
        ProjectDirectory = projectDirectory ?? throw new ArgumentNullException(nameof(projectDirectory));
        Source = new Source(ProjectDirectory, ProjectName);
    }

    public string AssetsDirectory => Path.Combine(ProjectDirectory, "Assets");
    public string CacheDirectory => Path.Combine(ProjectDirectory, ".cache");
    public string ProjectDirectory { get; }
    public string ProjectName => Path.GetFileName(ProjectDirectory) ?? "Unknown Project";

    public Source Source { get; }
}
