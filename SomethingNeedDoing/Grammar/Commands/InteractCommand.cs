using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

internal class InteractCommand : MacroCommand
{
    public static string[] Commands => ["interact"];
    public static string Description => "Checks your inventory and armoury for an item and tries to equip it.";
    public static string[] Examples => ["/equipitem 40280"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private InteractCommand(string text, WaitModifier wait, IndexModifier index) : base(text, wait, index) { }

    public static InteractCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);
        return new InteractCommand(text, waitModifier, indexModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        var target = Svc.Targets.Target;

        if (target != default)
            unsafe { TargetSystem.Instance()->InteractWithObject(target.Struct(), false); }

        await PerformWait(token);
    }
}
