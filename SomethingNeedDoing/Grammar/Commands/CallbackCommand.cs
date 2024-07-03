using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ECommons.GenericHelpers;

namespace SomethingNeedDoing.Grammar.Commands;

internal class CallbackCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/callback\s+(?<addon>\b\w+\b)\s+(?<updateState>true|false)\s+(?<values>(true|false|\b\w+\b|-?\d+|""[^""]+"")(\s+(true|false|\b\w+\b|-?\d+|""[^""]+""))*)\s*$", RegexOptions.Compiled);
    private readonly unsafe AtkUnitBase* addon;
    private readonly bool updateState;
    private readonly List<object> valueArgs = [];

    private unsafe CallbackCommand(AtkUnitBase* addon, bool updateState, List<object> valueArgs, WaitModifier wait) : base("", wait)
    {
        this.addon = addon;
        this.updateState = updateState;
        this.valueArgs = valueArgs;
    }

    public unsafe static CallbackCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var addonGroup = match.Groups["addon"];
        var boolGroup = match.Groups["updateState"];
        var valueGroup = match.Groups["values"];

        if (!addonGroup.Success)
            throw new MacroSyntaxError(text, "Invalid addon {addonGroup.Value}. Please follow \"/callback <addon> <bool> <atkValues>\"");
        if (!boolGroup.Success)
            throw new MacroSyntaxError(text, "Invalid bool. Please follow \"/callback <addon> <bool> <atkValues>\"");
        if (!valueGroup.Success)
            throw new MacroSyntaxError(text, "Invalid values. Please follow \"/callback <addon> <bool> <atkValues>\"");

        var rawValues = valueGroup.Value.Split(' ');
        var valueArgs = new List<object>();

        var current = "";
        var inQuotes = false;

        for (var i = 0; i < rawValues.Length; i++)
        {
            if (!inQuotes)
            {
                if (rawValues[i].StartsWith('\"'))
                {
                    inQuotes = true;
                    current = rawValues[i].TrimStart('"');
                }
                else
                {
                    if (int.TryParse(rawValues[i], out var iValue)) valueArgs.Add(iValue);
                    else if (uint.TryParse(rawValues[i].TrimEnd('U', 'u'), out var uValue)) valueArgs.Add(uValue);
                    else if (bool.TryParse(rawValues[i], out var bValue)) valueArgs.Add(bValue);
                    else valueArgs.Add(rawValues[i]);
                }
            }
            else
            {
                if (rawValues[i].EndsWith("\""))
                {
                    inQuotes = false;
                    current += " " + rawValues[i].TrimEnd('"');
                    valueArgs.Add(current);
                    current = "";
                }
                else
                {
                    current += " " + rawValues[i];
                }
            }
        }

        if (!string.IsNullOrEmpty(current))
            throw new MacroSyntaxError(text, "Unclosed quotes.");
        if (TryGetAddonByName<AtkUnitBase>(addonGroup.Value, out var addonArg))
            return new CallbackCommand(addonArg, bool.Parse(boolGroup.Value), valueArgs, waitModifier);
        else
            throw new MacroSyntaxError(text, "Invalid addon.");
    }

    public async override Task Execute(ActiveMacro macro, CancellationToken token)
    {
        unsafe
        {
            Callback.Fire(addon, updateState, valueArgs.ToArray());
        }
        await this.PerformWait(token);
    }
}
