using System;

namespace SomethingNeedDoing.Exceptions;

/// <summary>
/// Error thrown when the syntax of a macro does not parse correctly.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroSyntaxError"/> class.
/// </remarks>
/// <param name="command">The command that failed parsing.</param>
internal class MacroSyntaxError(string command, string guidance = "") : InvalidOperationException($"Syntax error: {command}{(guidance != string.Empty ? $"\n{guidance}" : string.Empty)}")
{
}
