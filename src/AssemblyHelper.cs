using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace SenDev.XafSame;

public class AssemblyHelper
{

    private readonly List<string> assemblyPaths;
    private readonly string mainAssemblyPath;

    public AssemblyHelper(string mainAssemblyPath, IEnumerable<string> assemblyPaths)
    {
        this.assemblyPaths = assemblyPaths.ToList();
        this.mainAssemblyPath = mainAssemblyPath;
    }


    public void AddAssemblyPath(string assemblyPath)
    {
        if (assemblyPaths.Contains(assemblyPath))
            return;
        assemblyPaths.Add(assemblyPath);
    }

    public string? ResolveAssemlby(AssemblyName assemblyName, out bool isProjectOutputAssembly)
    {
        var assemblyPath = assemblyPaths.FirstOrDefault(rf => string.Equals(Path.GetFileNameWithoutExtension(rf), assemblyName.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Path.GetExtension(rf), ".dll", StringComparison.OrdinalIgnoreCase));

        isProjectOutputAssembly = assemblyPath != null && Path.GetDirectoryName(assemblyPath) == Path.GetDirectoryName(mainAssemblyPath);
        return assemblyPath;
    }


    public static string GetAssemblyFileName(AssemblyName assemblyName, string directory)
    {
        var name = assemblyName.Name;
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Assembly name cannot be null or empty.", nameof(assemblyName));

        return GetAssemblyFileName(directory, name);
    }

    public static string GetAssemblyFileName(string directory, string assemblyName)
    {
        return Path.Combine(directory, assemblyName + ".dll");
    }


    public static bool ShouldIgnoreAssembly(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);
        if (!HasMetaData(peReader))
            return true;

        var reader = peReader.GetMetadataReader();

        if (!TryGetAssemblyDefinitionSafe(reader, out var assemblyDefinition))
            return true;

        foreach (var handle in assemblyDefinition.GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            var ctorHandle = attribute.Constructor;

            if (ctorHandle.Kind != HandleKind.MemberReference)
                continue;
            MemberReferenceHandle memberReferenceHandle = (MemberReferenceHandle)ctorHandle;
            string attributeName = reader.GetString(reader.GetTypeReference((TypeReferenceHandle)reader.GetMemberReference(memberReferenceHandle).Parent).Name);

            if (attributeName == nameof(ReferenceAssemblyAttribute))
            {
                return true;
            }

            if (attributeName == nameof(TargetFrameworkAttribute))
            {
                var value = attribute.DecodeValue(new SimpleTypeProvider());

                if (value.FixedArguments.Length == 1)
                {
                    var frameworkName = value.FixedArguments[0].Value as string ?? string.Empty;
                    if (!frameworkName.StartsWith(".NETCoreApp", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                }
            }
        }

        return false;
    }

    private static bool TryGetAssemblyDefinitionSafe(MetadataReader reader, out AssemblyDefinition assemblyDefinition)
    {
        try
        {
            assemblyDefinition = reader.GetAssemblyDefinition();
            return true;
        }
        catch (InvalidOperationException)
        {
            assemblyDefinition = default;
            return false;
        }
    }

    private static bool HasMetaData(PEReader peReader)
    {
        try
        {
            if (!peReader.HasMetadata)
                return false;
        }
        catch (BadImageFormatException)
        {
            return false;
        }

        return true;
    }
}
