using ECommons.DalamudServices;
using ECommons.GameFunctions;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

/// <summary>
/// The /target command.
/// </summary>
internal class TargetEnemyCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/targetenemy$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly int targetIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="TargetEnemyCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="wait">Wait value.</param>
    private TargetEnemyCommand(string text, WaitModifier wait, IndexModifier index) : base(text, wait, index)
    {
        this.targetIndex = index.ObjectId;
        Service.Log.Info("making new command");
    }

    /// <summary>
    /// Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static TargetEnemyCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);
        Service.Log.Info("parsing");
        return new TargetEnemyCommand(text, waitModifier, indexModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        var target = Service.ObjectTable.OrderBy(DistanceToObject).FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead);
        Service.Log.Info("executing");

        if (target == default && Service.Configuration.StopMacroIfTargetNotFound)
            throw new MacroCommandError("Could not find target");

        if (target != default)
            Service.TargetManager.Target = target;

        await this.PerformWait(token);
    }

    private float DistanceToObject(Dalamud.Game.ClientState.Objects.Types.IGameObject o) => Vector3.DistanceSquared(o.Position, Svc.ClientState.LocalPlayer!.Position);
}
