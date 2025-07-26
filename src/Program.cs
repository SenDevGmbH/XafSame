using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;



#if !NET48
using System.Runtime.Loader;
#endif
using System.Windows.Forms;

namespace SenDev.XafSame;
static class Program
{
    private const string DevExpressExpressAppWinPattern = "DevExpress.ExpressApp.Win.v{0}";
    private const string DevExpressExpressAppPattern = "DevExpress.ExpressApp.v{0}";
    private static readonly List<string> referenceFiles = new List<string>();
    private static string? devExpressVersion;
    private static AssemblyHelper? assemblyHelper;
    private static ILogger? logger;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
#if !NET48
        ApplicationConfiguration.Initialize();
#endif
        Form? modelEditorForm = null;
        var splashFrom = new SplashForm(() => modelEditorForm = CreateModelEditorForm(args[0]));
        logger = splashFrom.Logger;

        if (splashFrom.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        Application.Run(modelEditorForm!);
    }

    private static void StartModelEditor(string modelDifferencesFileName)
    {
        Form form = CreateModelEditorForm(modelDifferencesFileName);
        Application.Run(form);
    }

    private static Form CreateModelEditorForm(string modelDifferencesFileName)
    {
        var projectPath = FindProjectFile(modelDifferencesFileName);
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new InvalidOperationException("Project file not found.");


        logger!.LogInfo($"Project file found: {projectPath}");

        var build = ReferenceAssembliesCollector.BuildProject(projectPath, logger);
        var referenceAssembliesCollector = new ReferenceAssembliesCollector(projectPath, logger, build);

        referenceFiles.AddRange(referenceAssembliesCollector.GetReferencedAssemblies(out var outputDllPath));

        if (string.IsNullOrWhiteSpace(outputDllPath))
            throw new InvalidOperationException("Output dll path cannot be determined from project file.");

        DateTime latestDllTime = DateTime.MinValue;

        assemblyHelper = new AssemblyHelper(outputDllPath, referenceFiles);

        devExpressVersion = GetDevExpressVersion(outputDllPath);

        if (devExpressVersion == null)
            throw new InvalidOperationException("Cannot determine DevExpress version");

        logger!.LogInfo($"DevExpress version: {devExpressVersion}");

        AddWindowsAssembliesLocations();
        InitializeAssemblyResolution();


        logger.LogInfo("Creating instance of WinApplication...");
        Activator.CreateInstance(DevExpressExpressAppWinAssemblyName, "DevExpress.ExpressApp.Win.WinApplication");

        var controller = LoadModel(outputDllPath, modelDifferencesFileName);
        object settings = CreateObject<object>(DevExpressExpressAppAssemblyName, "DevExpress.ExpressApp.Utils.NullSettingsStorage");
        Form form = CreateObject<Form>(DevExpressExpressAppWinAssemblyName, "DevExpress.ExpressApp.Win.Core.ModelEditor.ModelEditorForm",
            controller, settings);
        return form;
    }

    private static void AddWindowsAssembliesLocations()
    {
        AddWindowsAssemblyLocation($"DevExpress.ExpressApp.Win.v{devExpressVersion}.dll", "devexpress.expressapp.win");
        AddWindowsAssemblyLocation($"DevExpress.XtraBars.v{devExpressVersion}.dll", "devexpress.win.navigation");
        AddWindowsAssemblyLocation($"DevExpress.XtraEditors.v{devExpressVersion}.dll", "devexpress.win.navigation");
        AddWindowsAssemblyLocation($"DevExpress.Utils.v{devExpressVersion}.dll", "devexpress.utils");
        AddWindowsAssemblyLocation($"DevExpress.Data.Desktop.v{devExpressVersion}.dll", "devexpress.data.desktop");
        AddWindowsAssemblyLocation($"DevExpress.XtraTreeList.v{devExpressVersion}.dll", "devexpress.win.treelist");
        AddWindowsAssemblyLocation($"DevExpress.XtraRichEdit.v{devExpressVersion}.dll", "devexpress.win.richedit");
        AddWindowsAssemblyLocation($"DevExpress.XtraVerticalGrid.v{devExpressVersion}.dll", "devexpress.win.verticalgrid");
        AddWindowsAssemblyLocation($"DevExpress.XtraLayout.v{devExpressVersion}.dll", "devexpress.win.navigation");
        AddWindowsAssemblyLocation($"DevExpress.XtraNavBar.v{devExpressVersion}.dll", "devexpress.win");
        AddWindowsAssemblyLocation($"DevExpress.XtraGrid.v{devExpressVersion}.dll", "devexpress.win.grid");
        AddWindowsAssemblyLocation($"DevExpress.DataAccess.v{devExpressVersion}.UI.dll", "devexpress.dataaccess.ui");
    }

    private static void AddWindowsAssemblyLocation(string dllName, string packageName)
    {
        var expressAppWinLocation = ReferenceAssembliesCollector.GuessLocationOfDevExpressWinAssembly(referenceFiles, devExpressVersion!, dllName, packageName);
        if (expressAppWinLocation != null)
        {
            referenceFiles.Add(expressAppWinLocation);
            assemblyHelper!.AddAssemblyPath(expressAppWinLocation);
        }
    }





    public static string DevExpressExpressAppWinAssemblyName =>
        string.Format(CultureInfo.InvariantCulture, DevExpressExpressAppWinPattern, devExpressVersion);

    public static string DevExpressExpressAppAssemblyName =>
        string.Format(CultureInfo.InvariantCulture, DevExpressExpressAppPattern, devExpressVersion);

    private static string? GetDevExpressVersion(string assemblyPath)
    {
        var regex = new Regex(@"DevExpress.*v(?<version>\d\d.\d)");
        var assembly = ReflectionOnlyLoadFrom(assemblyPath);
        var referencedAssemblies = assembly.GetReferencedAssemblies();
        foreach (var referencedAssembly in referencedAssemblies)
        {
            if (!string.IsNullOrWhiteSpace(referencedAssembly.Name))
            {
                var match = regex.Match(referencedAssembly.Name);
                if (match.Success)
                {
                    return match.Groups["version"].Value;
                }
            }
        }

        return null;
    }

    private static Assembly ReflectionOnlyLoadFrom(string assemblyPath)
    {
#if NET48
        return Assembly.ReflectionOnlyLoadFrom(assemblyPath);
#else
        return Assembly.LoadFrom(assemblyPath);
#endif
    }

    private static T CreateObject<T>(string assemblyName, string typeName, params object?[] args) where T : class
    {
        var type = GetType(assemblyName, typeName);
        var obj = (T?)Activator.CreateInstance(type, args);

        if (obj == null)
            throw new InvalidOperationException($"Cannot create instance of {typeName}");

        return obj;
    }

    private static Type GetType(string assemblyName, string typeName)
    {
        var assembly = Assembly.Load(assemblyName);
        var type = assembly.GetType(typeName);
        if (type == null)
            throw new InvalidOperationException($"Type {typeName} not found in assembly {assemblyName}");

        return type;
    }


    private static Assembly? FindAssembly(AssemblyName assemblyName)
    {
        var assemblyFileName = referenceFiles.FirstOrDefault(rf => string.Equals(Path.GetFileNameWithoutExtension(rf), assemblyName.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Path.GetExtension(rf), ".dll", StringComparison.OrdinalIgnoreCase));
        if (assemblyFileName == null)
            return null;

        return Assembly.LoadFrom(assemblyFileName);
    }

    private static string? FindProjectFile(string modelFilePath)
    {
        var directory = Path.GetDirectoryName(modelFilePath);
        if (directory == null) return null;

        var files = Directory.GetFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly);
        return files.FirstOrDefault();
    }





    private static object LoadModel(string moduleDllPath, string modelPath)
    {
        Assembly assembly = LoadAssembly(moduleDllPath);
        var moduleType = assembly.GetTypes().FirstOrDefault(t => t.BaseType?.Name == "ModuleBase");
        if (moduleType == null)
            throw new InvalidOperationException("Module not found.");

        //Loading DevExpress.ExpressApp.Xpo assembly, since AssemblyHelper.TryGetType searches already loaded assemblies in the current AppDomain.
        //If the assembly is not loaded, module not find exported types automatically.
        AppDomain.CurrentDomain.Load($"DevExpress.ExpressApp.Xpo.v{devExpressVersion}");
        dynamic designerModuleFactory = CreateObject<object>(DevExpressExpressAppAssemblyName, "DevExpress.ExpressApp.Utils.DesignerModelFactory");

        dynamic module = Activator.CreateInstance(moduleType) ?? throw new InvalidOperationException("Module is null");
        var typesInfo = GetStaticPropertyValue(GetType(DevExpressExpressAppAssemblyName, "DevExpress.ExpressApp.XafTypesInfo"), "Instance");
        Type initializerType = GetType(DevExpressExpressAppAssemblyName, "DevExpress.ExpressApp.Design.DefaultTypesInfoInitializer");

        Func<Type, IEnumerable<Type>> findTypesFunc = baseType => assembly.GetTypes().Where(baseType.IsAssignableFrom);
        Func<string, string, object?> findEntityContextTypesFunc = (assemblyName, typeName) =>
                         InvokeStaticMethod(initializerType, "CreateTypesInfoInitializer", Path.GetDirectoryName(moduleDllPath),
                             assemblyName, typeName);

        logger!.LogInfo("Initializing types info...");
        InvokeStaticMethod(initializerType, "Initialize",
            typesInfo,
            findTypesFunc,
            findEntityContextTypesFunc, null);

        logger.LogInfo("Creating modules manager...");
        var modulesManager = designerModuleFactory.CreateModulesManager(module, Path.GetDirectoryName(moduleDllPath));

        dynamic fileModelStore = CreateObject<object>(DevExpressExpressAppAssemblyName, "DevExpress.ExpressApp.FileModelStore",
            Path.GetDirectoryName(modelPath), Path.GetFileNameWithoutExtension(modelPath));
        module.DiffsStore = fileModelStore;

        logger.LogInfo("Creating application model...");
        var applicationModel = designerModuleFactory.CreateApplicationModel(module, modulesManager, fileModelStore);

        logger.LogInfo("Initializing ModelEditorViewController...");
        dynamic modelEditorViewController = CreateObject<object>(DevExpressExpressAppWinAssemblyName,
            "DevExpress.ExpressApp.Win.Core.ModelEditor.ModelEditorViewController", applicationModel, fileModelStore);
        var moduleDiffStoreInfoType = GetType(DevExpressExpressAppWinAssemblyName, "DevExpress.ExpressApp.Win.Core.ModelEditor.ModuleDiffStoreInfo");
        var moduleDiffStoreInfo = Activator.CreateInstance(moduleDiffStoreInfoType, moduleType, fileModelStore, module.Name);
        var storesListType = typeof(List<>).MakeGenericType(moduleDiffStoreInfoType);

        dynamic? storesList = Activator.CreateInstance(storesListType);
        if (storesList == null)
            throw new InvalidOperationException("Cannot create instance of List<modueDiffStoreInfoType>");

        storesList.Add(moduleDiffStoreInfo);
        modelEditorViewController.SetModuleDiffStore(storesList);

        return modelEditorViewController;

    }


    private static object? GetStaticPropertyValue(Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        if (property == null)
            throw new ArgumentException($"Property {propertyName} not found", nameof(propertyName));

        return property.GetValue(null);
    }

    private static object? InvokeStaticMethod(Type type, string methodName, params object?[] args)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null)
            throw new ArgumentException($"Method {methodName} not found", nameof(methodName));

        return method.Invoke(null, args);

    }

    private static string GetAssemblyFileName(string directory, string? assemblyName)
    {
        return Path.Combine(directory, assemblyName + ".dll");
    }


#if !NET48
    private static Assembly LoadAssembly(string assemblyPath)
    {
        return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
    }


    private static void InitializeAssemblyResolution()
    {
        AssemblyLoadContext.Default.Resolving += LoadContext_Resolving;
    }

    private static Assembly? LoadContext_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        ArgumentNullException.ThrowIfNull(assemblyHelper, nameof(assemblyHelper));

        var path = assemblyHelper.ResolveAssemlby(assemblyName, out var isProjectOutputAssembly);
        if (!string.IsNullOrWhiteSpace(path))
        {
            if (isProjectOutputAssembly)
            {
                var memoryStream = new MemoryStream();  
                using(var fileStream = File.OpenRead(path))
                {
                    fileStream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return context.LoadFromStream(memoryStream);
            }
            return context.LoadFromAssemblyPath(path);
        }

        return null;
    }



#else
    private static Assembly LoadAssembly(string moduleDllPath)
    {
        InitializeAssemblyResolution();
        return Assembly.LoadFile(moduleDllPath);
    }

    private static void InitializeAssemblyResolution()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            AssemblyName assemblyName = new AssemblyName(e.Name);
            return FindAssembly(assemblyName);
        };
    }

#endif
}
