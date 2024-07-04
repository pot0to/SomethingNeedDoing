using ECommons.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SomethingNeedDoing.Managers;
public class CSharpManager
{
    public static void RunSnippet(string code)
    {
        string fullCode = @"
            using System;
            using ECommons.DalamudServices;
            public class UserCodeExecutor
            {
                public static void Execute()
                {
                    " + code + @"
                }
            }";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fullCode);

        var compilation = CSharpCompilation.Create("UserCode")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(GetProjectReferences())
            .AddSyntaxTrees(syntaxTree);

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);

        if (!result.Success)
            foreach (Diagnostic diag in result.Diagnostics)
                Svc.Log.Info(diag.ToString());
        else
        {
            ms.Seek(0, SeekOrigin.Begin);
            if (DalamudReflector.TryGetDalamudPlugin(SomethingNeedDoingPlugin.Name, out var plugin, out AssemblyLoadContext alc))
            {
                Assembly assembly = alc.LoadFromStream(ms);

                Type type = assembly.GetType("UserCodeExecutor")!;
                MethodInfo executeMethod = type.GetMethod("Execute")!;

                executeMethod.Invoke(null, null);
                alc.Unload();
            }
        }
    }

    private static List<MetadataReference> GetProjectReferences()
    {
        var references = new List<MetadataReference>();
        var assemblyFileInfo = Svc.PluginInterface.AssemblyLocation;

        if (assemblyFileInfo == null || !assemblyFileInfo.Exists)
            Svc.Log.Info("Error: Assembly file info is null or does not exist");
        else
        {
            var assemblyLocation = assemblyFileInfo.Directory!.FullName;
            var relativeAssemblyPath = Path.Combine(assemblyLocation, "ECommons.dll");

            if (string.IsNullOrEmpty(relativeAssemblyPath))
                Svc.Log.Info("Error: Relative assembly path is empty or null");
            else
                references.Add(MetadataReference.CreateFromFile(relativeAssemblyPath));
        }

        var dalamudLibPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "addon", "Hooks", "dev");
        references.Add(MetadataReference.CreateFromFile(Path.Combine(dalamudLibPath, "Dalamud.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(dalamudLibPath, "Dalamud.Common.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(dalamudLibPath, "FFXIVClientStructs.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(dalamudLibPath, "Lumina.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(dalamudLibPath, "Lumina.Excel.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(dalamudLibPath, "ImGui.NET.dll")));

        return references;
    }
}
