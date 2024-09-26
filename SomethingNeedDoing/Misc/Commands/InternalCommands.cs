using System;
using System.Collections.Generic;
using System.Reflection;

namespace SomethingNeedDoing.Misc.Commands;

public class InternalCommands
{
    internal static InternalCommands Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name is not nameof(ListAllFunctions) or nameof(InternalGetMacroText) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public string? InternalGetMacroText(string name)
    {
        return Service.Configuration
            .GetAllNodes()
            .OfType<MacroNode>()
            .FirstOrDefault(node =>
                string.Equals(node.Name.Trim(), name.Trim(), StringComparison.InvariantCultureIgnoreCase))?
            .Contents
            .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
            .Select(line => $"  {line}")
            .Join('\n');
    }

    public void SetSNDProperty(string key, string value) => Service.Configuration.SetProperty(key, value);
    public object? GetSNDProperty(string key) => Service.Configuration.GetProperty(key);
    public bool IsPauseLoopSet() => Service.MacroManager.PauseAtLoop;
    public bool IsStopLoopSet() => Service.MacroManager.StopAtLoop;
    public string GetActiveMacroName() => Service.MacroManager.ActiveMacroName;
}
