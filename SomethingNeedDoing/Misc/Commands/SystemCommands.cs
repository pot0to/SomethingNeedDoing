using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SomethingNeedDoing.Misc.Commands;

public class SystemCommands
{
    internal static SystemCommands Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public string GetClipboard() => ImGui.GetClipboardText();

    public void SetClipboard(string text) => ImGui.SetClipboardText(text);

    public unsafe void CrashTheGame() => Framework.Instance()->UIModule = (UIModule*)0;
}
