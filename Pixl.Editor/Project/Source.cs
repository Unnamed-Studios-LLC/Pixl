﻿using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pixl.Editor;

internal sealed class Source
{
    private readonly string _projectDirectory;
    private readonly string _projectName;
    private DateTime? _loadedAssemblyVersion;

    public Source(string projectDirectory, string projectName)
    {
        _projectDirectory = projectDirectory ?? throw new ArgumentNullException(nameof(projectDirectory));
        _projectName = projectName ?? throw new ArgumentNullException(nameof(projectName));

        if (!AssemblyProjectExists()) CreateAssemblyProject();
        if (!SolutionExists()) CreateSolution();
    }

    public string AssemblyOutputDirectory => Path.Combine(AssemblyProjectDirectory, "bin/Debug/net7.0");
    public string AssemblyOutputDll => Path.Combine(AssemblyOutputDirectory, "Assembly.dll");
    public string AssemblyOutputPdb => Path.Combine(AssemblyOutputDirectory, "Assembly.pdb");
    public string AssemblyProjectDirectory => Path.Combine(_projectDirectory, "Assembly");
    public string AssemblyProjectFilePath => Path.Combine(AssemblyProjectDirectory, "Assembly.csproj");
    public string SolutionFilePath => Path.Combine(_projectDirectory, $"{_projectName}.sln");
    public Assembly? UserAssembly { get; private set; }
    public bool ReloadAvailable => GetReloadAvailable();

    public async Task ReloadUserAssemblyAsync(EditorTask editorTask)
    {
        var previousAssembly = UserAssembly;
        var previousVersion = _loadedAssemblyVersion;
        try
        {
            editorTask.Progress = 0;
            editorTask.State = "Reading assembly dll...";

            var fileVersion = File.GetLastWriteTimeUtc(AssemblyOutputDll);
            var dllBytes = await File.ReadAllBytesAsync(AssemblyOutputDll);

            editorTask.Progress = 0.33f;
            editorTask.State = "Reading assembly pdb...";

            var pdbBytes = File.Exists(AssemblyOutputPdb) ? await File.ReadAllBytesAsync(AssemblyOutputPdb) : null;

            editorTask.Progress = 0.66f;
            editorTask.State = "Loading assembly...";

            var assembly = pdbBytes == null ? Assembly.Load(dllBytes) : Assembly.Load(dllBytes, pdbBytes);
            UserAssembly = assembly;
            _loadedAssemblyVersion = fileVersion;

            editorTask.Progress = 0.95f;
            editorTask.State = "Initializing assembly...";

            MetaData.ClearRecordedMetaData();
            MetaData.RecordMetaData = true;
            foreach (var module in assembly.GetModules())
            {
                RuntimeHelpers.RunModuleConstructor(module.ModuleHandle);
            }
        }
        catch (Exception e)
        {
            UserAssembly = previousAssembly;
            _loadedAssemblyVersion = previousVersion;
            throw;
        }
        finally
        {
            MetaData.RecordMetaData = false;
        }
    }

    private bool AssemblyProjectExists()
    {
        if (!File.Exists(AssemblyProjectFilePath)) return false;
        try
        {
            var projectCollection = new ProjectCollection();
            var rootElement = ProjectRootElement.Open(AssemblyProjectFilePath, projectCollection);
            return true;
        }
        catch
        {
            File.Delete(AssemblyProjectFilePath);
            return false;
        }
    }

    private bool GetReloadAvailable()
    {
        if (!File.Exists(AssemblyOutputDll))
        {
            // TODO write error
            return false;
        }

        if (_loadedAssemblyVersion != null)
        {
            var fileVersion = File.GetLastWriteTimeUtc(AssemblyOutputDll);
            if (fileVersion == _loadedAssemblyVersion) return false;
        }

        return true;
    }

    private void CreateAssemblyProject()
    {
        static void addDllReference(ProjectItemGroupElement itemGroup, string name, string dllFileName)
        {
            var dllFolder = AppDomain.CurrentDomain.BaseDirectory;
            var dllLocation = Path.Combine(dllFolder, dllFileName);
            itemGroup.AddItem("Reference", name, new[] { new KeyValuePair<string, string>("HintPath", dllLocation) });
        }

        static void addAnalyzerReference(ProjectItemGroupElement itemGroup, string name, string dllFileName)
        {
            var dllFolder = AppDomain.CurrentDomain.BaseDirectory;
            var dllLocation = Path.Combine(dllFolder, dllFileName);
            itemGroup.AddItem("Analyzer", dllLocation);
        }

        var options = NewProjectFileOptions.None;
        var projectCollection = new ProjectCollection();
        var rootElement = ProjectRootElement.Create(AssemblyProjectFilePath, projectCollection, options);
        rootElement.Sdk = "Microsoft.NET.Sdk";

        var propertyGroup = rootElement.AddPropertyGroup();
        propertyGroup.AddProperty("TargetFramework", "net7.0");
        propertyGroup.AddProperty("OutputType", "Library");

        var itemGroup = rootElement.AddItemGroup();
        addDllReference(itemGroup, "Pixl", "Pixl.dll");
        addDllReference(itemGroup, "EntitiesDb", "EntitiesDb.dll");
        addAnalyzerReference(itemGroup, "Pixl.SourceGenerators", "Pixl.SourceGenerators.dll");

        rootElement.Save();
    }

    private void CreateSolution()
    {
        Directory.CreateDirectory(AssemblyProjectDirectory);

        var projectGuid = Guid.NewGuid();
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var solution = $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.5.33627.172
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{{guid1}}}"") = ""Assembly"", ""Assembly\Assembly.csproj"", ""{{{projectGuid}}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{{projectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{projectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{projectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{projectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {{{guid2}}}
	EndGlobalSection
EndGlobal
";

        File.WriteAllText(SolutionFilePath, solution);
    }

    private bool SolutionExists()
    {
        if (!File.Exists(SolutionFilePath)) return false;
        return true;
    }
}
