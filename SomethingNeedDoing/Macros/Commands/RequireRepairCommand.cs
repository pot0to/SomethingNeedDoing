using ECommons.ChatMethods;
using SomethingNeedDoing.Macros.Exceptions;
using SomethingNeedDoing.Macros.Commands.Modifiers;
using SomethingNeedDoing.Macros.Lua;
using SomethingNeedDoing.Misc;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

internal class RequireRepairCommand : MacroCommand
{
    public static string[] Commands => ["requirerepair"];
    public static string Description => "Pause if an item is at zero durability.";
    public static string[] Examples => ["/requirerepair"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private RequireRepairCommand(string text, WaitModifier wait) : base(text, wait) { }

    public static RequireRepairCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        return !match.Success ? throw new MacroSyntaxError(text) : new RequireRepairCommand(text, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (CraftingState.Instance.NeedsRepair())
            throw new MacroPause("You need to repair", UIColor.Yellow);

        await PerformWait(token);
    }
}
