using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

/// <summary>
/// The /loop command.
/// </summary>
internal class LoopCommand : MacroCommand
{
    private const int MaxLoops = int.MaxValue;
    private static readonly Regex Regex = new(@"^/loop(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly EchoModifier echoMod;
    private readonly int startingLoops;
    private int loopsRemaining;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="loopCount">Loop count.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="echo">Echo value.</param>
    private LoopCommand(string text, int loopCount, WaitModifier wait, EchoModifier echo)
        : base(text, wait)
    {
        loopsRemaining = loopCount >= 0 ? loopCount : MaxLoops;
        startingLoops = loopsRemaining;

        if (Service.Configuration.LoopTotal && loopsRemaining != 0 && loopsRemaining != MaxLoops)
            loopsRemaining -= 1;

        echoMod = echo;
    }

    /// <summary>
    /// Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static LoopCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = EchoModifier.TryParse(ref text, out var echoModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var countGroup = match.Groups["count"];
        var countValue = countGroup.Success
            ? int.Parse(countGroup.Value, CultureInfo.InvariantCulture)
            : int.MaxValue;

        return new LoopCommand(text, countValue, waitModifier, echoModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (loopsRemaining == MaxLoops)
        {
            if (echoMod.PerformEcho || Service.Configuration.LoopEcho)
            {
                Service.ChatManager.PrintMessage("Looping");
            }
        }
        else
        {
            if (echoMod.PerformEcho || Service.Configuration.LoopEcho)
            {
                if (loopsRemaining == 0)
                {
                    Service.ChatManager.PrintMessage("No loops remaining");
                }
                else
                {
                    var noun = loopsRemaining == 1 ? "loop" : "loops";
                    Service.ChatManager.PrintMessage($"{loopsRemaining} {noun} remaining");
                }
            }

            loopsRemaining--;

            if (loopsRemaining < 0)
            {
                loopsRemaining = startingLoops;
                return;
            }
        }

        macro.Loop();
        Service.MacroManager.LoopCheckForPause();
        Service.MacroManager.LoopCheckForStop();
        await Task.Delay(10, token);
        await PerformWait(token);
    }
}
