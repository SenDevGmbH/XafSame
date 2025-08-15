using Microsoft.Build.Logging.StructuredLogger;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SenDev.XafSame;

class ReferenceAssembliesCollector
{
    private const string targetFrameworkPropertyName = "TargetFramework";
    private const string targetFrameworkIdentifierName = "TargetFrameworkIdentifier";
    private Folder evaluatedPropertiesFolder;
    private readonly Build build;

    public ReferenceAssembliesCollector(string projectPath, ILogger logger, Build build)
    {
        ProjectPath = projectPath;
        Logger = logger;
        ProjectFileName = Path.GetFileName(projectPath);
        this.build = build;

        evaluatedPropertiesFolder = FindProjectEvaluationNode<Folder>(build.EvaluationFolder, "Properties") ??
            throw new InvalidOperationException("Evaluated properties not found.");
    }

    public string ProjectFileName { get; }
    public string ProjectPath { get; }
    private ILogger Logger { get; }

    public IEnumerable<string> GetReferencedAssemblies(out string? outputAssembly)
    {
        string? targetFileName = GetTargetFileName();

        var outputPath = GetPropertyValue(evaluatedPropertiesFolder, "OutputPath");
        string? projectDirectory = Path.GetDirectoryName(ProjectPath) ?? throw new InvalidOperationException("Invalid project directory.");
        outputAssembly = Path.Combine(projectDirectory, outputPath ?? string.Empty, targetFileName);
        
        var references = new List<string>();

        foreach (var referenceFile in GetResolvedAssemblyReferences(build, ProjectFileName)
            .Concat(GetResolveLockFileReferences(build, ProjectFileName))
            .Concat(GetResolvePackageAssetsReferences(build))
            .Concat(GetOutputFiles(Path.GetDirectoryName(outputAssembly))))
        {
            if (!references.Any(r => string.Equals(Path.GetFileName(r), Path.GetFileName(referenceFile), StringComparison.OrdinalIgnoreCase)))
            {
                references.Add(referenceFile);
            }
        }

        return references.Select(GetRuntimeAssemblyPath);
    }

    private static IEnumerable<string> GetOutputFiles(string? outputPath)
    {
        if (outputPath == null)
        {
            return [];
        }
        return Directory.GetFiles(outputPath, "*.dll", SearchOption.TopDirectoryOnly);
    }

    private string GetTargetFileName() => GetTargetFileName(evaluatedPropertiesFolder);


    internal static Build BuildProject(string projectPath, ILogger logger)
    {
        logger.LogInfo($"Building project: {projectPath}");

        var binlogPath = projectPath + ".xafsame.binlog";

        if (File.Exists(binlogPath))
        {
            var existingBuild = BinaryLog.ReadBuild(binlogPath);
            if (existingBuild.FirstError == null)
            {
                var targetFileName = new ReferenceAssembliesCollector(projectPath, logger, existingBuild).GetTargetFileName();
                if (existingBuild.EndTime > File.GetLastWriteTime(targetFileName))
                {
                    logger.LogInfo($"Project {projectPath} is already built successfully. Using existing binary log.");
                    return existingBuild;
                }
            }
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "dotnet",

            // Using DisableScopedCssBundling=true to avoid compilation error:
            // error MSB4018: The "DiscoverPrecompressedAssets" task failed unexpectedly.
            Arguments = $"build \"{projectPath}\" --configuration Debug  /bl:{binlogPath}",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(projectPath),
        };

        var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Process cannot be created.");
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException("Build error");

        var build = BinaryLog.ReadBuild(binlogPath);
        logger.LogInfo($"Project {projectPath} built successfully.");
        return build;
    }

    private static string GetTargetFileName(Folder propertiesFolder)
    {
        var targetFileName = GetPropertyValue(propertiesFolder, "TargetPath")
            ?? GetPropertyValue(propertiesFolder, "TargetFileName")
            ?? GetPropertyValue(propertiesFolder, "AssemblyName");

        if (string.IsNullOrEmpty(targetFileName))
            throw new InvalidOperationException("Target file name cannot be found.");

        if (!string.Equals(Path.GetExtension(targetFileName), ".dll"))
            targetFileName += ".dll";
        return targetFileName;
    }

    private IEnumerable<string> GetResolveLockFileReferences(Build build, string projectFileName)
    {
        Target? target = FindProjectNode<Target>(build, projectFileName, "ResolveLockFileReferences");

        if (target == null) return Enumerable.Empty<string>();

        const string targetOutputsName = "ResolvedCompileFileDefinitionsToAdd";
        var outputs = target.Children.OfType<AddItem>().FirstOrDefault(c => c.Name == targetOutputsName);
        if (outputs == null)
        {
            throw new InvalidOperationException($"{targetOutputsName} element not found in {target.Name} target.");
        }

        return outputs.Children.OfType<Item>().Select(i => i.Name);
    }

    private IEnumerable<string> GetResolvePackageAssetsReferences(Build build)
    {
        Target? target = FindProjectNode<Target>(build, ProjectFileName, "ResolvePackageAssets");

        if (target == null) return Enumerable.Empty<string>();
        var compileFileDefinitions = target.FindChildrenRecursive<TaskParameterItem>(n => n.Name == "ResolvedCompileFileDefinitions").FirstOrDefault();
        if (compileFileDefinitions != null)
            return compileFileDefinitions.Children.OfType<Item>().Select(i => i.Name);
        else
            return Enumerable.Empty<string>();
    }
    private T? FindProjectNode<T>(NamedNode parent, string projectFileName, string nodeName) where T : NamedNode
    {
        return parent.FindChildrenRecursive<T>(t => t.Name == nodeName && GetProject(t)?.Name == projectFileName && IsTargetFrameworkValid(GetTargetFramework(GetProject(t)))).FirstOrDefault();
    }
    private T? FindProjectEvaluationNode<T>(NamedNode parent, string nodeName) where T : NamedNode
    {
        return parent.FindChildrenRecursive<T>(t => t.Name == nodeName && GetProjectEvaluation(t)?.Name == ProjectFileName && IsTargetFrameworkValid(GetTargetFramework(GetProjectEvaluation(t)))).FirstOrDefault();
    }


    private static ProjectEvaluation GetProjectEvaluation(NamedNode namedNode) => namedNode.GetNearestParent<ProjectEvaluation>();
    private static Project GetProject(NamedNode namedNode) => namedNode.GetNearestParent<Project>();

    private static string GetTargetFramework(ProjectEvaluation project)
    {
        var properties = project.FindFirstChild<Folder>(f => f.Name == "Properties");
        var targetFramework = GetPropertyValue(properties, targetFrameworkPropertyName);

        if (string.IsNullOrWhiteSpace(targetFramework))
        {
            var targetFrameworkIdentifier = GetPropertyValue(properties, targetFrameworkIdentifierName);
            return ConvertTargetFrameworkIdentifierToFrameworkMoniker(targetFrameworkIdentifier);
        }
        return targetFramework;
    }

    private static string ConvertTargetFrameworkIdentifierToFrameworkMoniker(string? targetFrameworkIdentifier)
    {
        if (targetFrameworkIdentifier == ".NETFramework")
            return "net48";
        else
            return string.Empty;
    }

    private string GetTargetFramework(Project project)
    {
        if (!project.GlobalProperties.TryGetValue(targetFrameworkPropertyName, out var targetFramework))
        {
            targetFramework = GetEvaluatedPropertyValue(targetFrameworkPropertyName);
            if (string.IsNullOrWhiteSpace(targetFramework))
            {
                var targetFrameworkIdentifier = GetEvaluatedPropertyValue(targetFrameworkIdentifierName);
                return ConvertTargetFrameworkIdentifierToFrameworkMoniker(targetFrameworkIdentifier);
            }
        }

        return targetFramework;
    }

    private IEnumerable<string> GetResolvedAssemblyReferences(Build build, string projectFileName)
    {
        var target = FindProjectNode<Target>(build, projectFileName, "ResolveAssemblyReferences");
        if (target == null) return Enumerable.Empty<string>();

        const string targetOutputsName = "TargetOutputs";
        var outputs = target.Children.OfType<Folder>().FirstOrDefault(c => c.Name == targetOutputsName);
        if (outputs == null)
        {
            return [];
        }

        return outputs.Children.OfType<Item>().Select(i => i.Name);
    }

    

    private string? GetEvaluatedPropertyValue(string propertyName)
    {
        return evaluatedPropertiesFolder.FindFirstChild<Property>(p => p.Name == propertyName)?.Value;
    }

    private static string? GetPropertyValue(Folder propertiesFolder, string propertyName)
    {
        return propertiesFolder.FindFirstChild<Property>(p => p.Name == propertyName)?.Value;
    }


    private static string ConvertRefToLibPath(string referencePath)
    {
        if (string.IsNullOrWhiteSpace(referencePath))
            return referencePath;

        var parts = referencePath.Split(Path.DirectorySeparatorChar);

        int refIndex = Array.IndexOf(parts, "ref");
        if (refIndex == -1 || refIndex < 2)
            return referencePath;

        string versionCandidate = parts[refIndex - 1];

        if (!Version.TryParse(versionCandidate, out _))
            return referencePath;

        parts[refIndex] = "lib";
        string libPath = string.Join(Path.DirectorySeparatorChar.ToString(), parts);

        return File.Exists(libPath) ? libPath : referencePath;
    }

    private static string GetRuntimeAssemblyPath(string referenceAssemblyPath)
    {
        referenceAssemblyPath = ConvertRefToLibPath(referenceAssemblyPath);

        if (string.IsNullOrWhiteSpace(referenceAssemblyPath))
            throw new ArgumentException("Path cannot be null or empty.", nameof(referenceAssemblyPath));

        var segments = referenceAssemblyPath.Split(Path.DirectorySeparatorChar);

        var packsIndex = Array.FindIndex(segments, s => s.Equals("packs", StringComparison.OrdinalIgnoreCase));
        if (packsIndex < 0 || packsIndex + 3 >= segments.Length)
            return referenceAssemblyPath; // Not a valid packs path or insufficient segments

        var packNameSegment = segments[packsIndex + 1];
        if (!packNameSegment.EndsWith(".Ref", StringComparison.OrdinalIgnoreCase))
            return referenceAssemblyPath; // Not a valid reference pack name

        var packName = packNameSegment.Substring(0, packNameSegment.Length - 4); // Remove ".Ref"
        var version = segments[packsIndex + 2];
        var dllName = segments.Last();

        segments[packsIndex] = "shared";
        segments[packsIndex + 1] = packName;
        segments[packsIndex + 2] = version;
        Array.Resize(ref segments, segments.Length - 3);
        var runtimeAssemblyPath = CombinePathSegments(segments.Append(dllName).ToArray());

        return runtimeAssemblyPath;
    }

    private static string CombinePathSegments(string[] pathParts)
    {
        int driveLetterIndex = 0;

        if (pathParts[driveLetterIndex].EndsWith(":"))
            pathParts[driveLetterIndex] += "\\";
        var expressAppWinPath = Path.Combine(pathParts);
        return expressAppWinPath;
    }

    public static string? GuessLocationOfDevExpressWinAssembly(IEnumerable<string> referenceFiles, string devExpressVersion, string dllName, string packageName)
    {
        // Find the path to the DevExpress.ExpressApp assembly
        var expressAppPath = referenceFiles.FirstOrDefault(path =>
            Path.GetFileName(path).Equals($"DevExpress.ExpressApp.v{devExpressVersion}.dll", StringComparison.OrdinalIgnoreCase));


        if (expressAppPath == null)
        {
            return null;
        }

        var pathParts = expressAppPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (pathParts.Length > 6)
        {
            if (Version.TryParse(pathParts[pathParts.Length - 4], out _) && string.Equals(pathParts[pathParts.Length - 5], "devexpress.expressapp", StringComparison.OrdinalIgnoreCase))
            {
                // If the DevExpress.ExpressApp assembly is found, return its path
                int fileIndex = pathParts.Length - 1;
                int frameworkIndex = pathParts.Length - 2;
                int packageNameIndex = pathParts.Length - 5;

                pathParts[fileIndex] = dllName;
                pathParts[packageNameIndex] = packageName;

                if (pathParts[frameworkIndex].StartsWith("net", StringComparison.OrdinalIgnoreCase)
                    && !pathParts[frameworkIndex].StartsWith("net4", StringComparison.OrdinalIgnoreCase))
                    pathParts[frameworkIndex] += "-windows";

                string expressAppWinPath = CombinePathSegments(pathParts);
                if (File.Exists(expressAppWinPath)) return expressAppWinPath;
            }
        }

        return null;
    }


    private static int GetTargetFrameworkMajorVersion(string frameworkMoniker)
    {
        if (frameworkMoniker.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        {
            var versionPart = frameworkMoniker[3].ToString();
            if (int.TryParse(versionPart, out int majorVersion))
            {
                return majorVersion;
            }
        }
        return 0;
    }

    private static bool IsTargetFrameworkValid(string frameworkMoniker)
    {
        var majorVersion = GetTargetFrameworkMajorVersion(frameworkMoniker);
#if NET48
        return majorVersion == 4;
#else
        return majorVersion > 4;
#endif
    }
}