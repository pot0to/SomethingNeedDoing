using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Grammar.Modifiers;

/// <summary>
/// The party &lt;index&gt; modifier.
/// </summary>
internal class PartyIndexModifier : MacroModifier
{
    private static readonly Regex Regex = new(@"(?<modifier><(?<index>\d+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private PartyIndexModifier(int index) => PartyIndex = index;

    /// <summary>
    /// Gets the objectIndex of the specified Target.
    /// </summary>
    public int PartyIndex { get; }

    /// <summary>
    /// Parse the text as a modifier.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="command">A parsed modifier.</param>
    /// <returns>A value indicating whether the modifier matched.</returns>
    public static bool TryParse(ref string text, out PartyIndexModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (!success)
        {
            command = new PartyIndexModifier(0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var indexGroup = match.Groups["index"];
        var indexValue = indexGroup.Value;
        var index = int.Parse(indexValue, CultureInfo.InvariantCulture);

        command = new PartyIndexModifier(index);
        return true;
    }
}
