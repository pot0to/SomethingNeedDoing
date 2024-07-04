using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using SomethingNeedDoing.Misc.Commands;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

internal class RequireCommand : MacroCommand
{
    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new(@"^/require\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint[] statusIDs;
    private readonly int maxWait;

    private RequireCommand(string text, string statusName, WaitModifier wait, MaxWaitModifier maxWait) : base(text, wait)
    {
        statusName = statusName.ToLowerInvariant();
        var sheet = Svc.Data.GetExcelSheet<Sheets.Status>()!;
        statusIDs = sheet
            .Where(row => row.Name.RawString.Equals(statusName, System.StringComparison.InvariantCultureIgnoreCase))
            .Select(row => row.RowId)
            .ToArray()!;

        this.maxWait = maxWait.Wait == 0
            ? StatusCheckMaxWait
            : maxWait.Wait;
    }

    public static RequireCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new RequireCommand(text, nameValue, waitModifier, maxWaitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        bool IsStatusPresent() => CharacterStateCommands.Instance.HasStatusId(statusIDs);

        var hasStatus = await LinearWait(StatusCheckInterval, maxWait, IsStatusPresent, token);

        if (!hasStatus)
            throw new MacroCommandError("Status effect not found");

        await PerformWait(token);
    }
}
