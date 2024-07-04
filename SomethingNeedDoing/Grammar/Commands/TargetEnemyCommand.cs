using ECommons.GameFunctions;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

internal class TargetEnemyCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/targetenemy$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly int targetIndex;

    private TargetEnemyCommand(string text, WaitModifier wait, IndexModifier index) : base(text, wait, index)
    {
        targetIndex = index.ObjectId;
        Svc.Log.Info("making new command");
    }

    public static TargetEnemyCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);
        Svc.Log.Info("parsing");
        return new TargetEnemyCommand(text, waitModifier, indexModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        var target = Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead);
        Svc.Log.Info("executing");

        if (target == default && Service.Configuration.StopMacroIfTargetNotFound)
            throw new MacroCommandError("Could not find target");

        if (target != default)
            Svc.Targets.Target = target;

        await PerformWait(token);
    }

    private float DistanceToObject(Dalamud.Game.ClientState.Objects.Types.IGameObject o) => Vector3.DistanceSquared(o.Position, Svc.ClientState.LocalPlayer!.Position);
}
