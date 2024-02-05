using System;
using System.Collections.Generic;

namespace SomethingNeedDoing.Misc;

/// <summary>
/// Extension methods.
/// </summary>
internal static class Extensions
{
    /// <inheritdoc cref="string.Join(char, string?[])"/>
    public static string Join(this IEnumerable<string> values, char separator)
        => string.Join(separator, values);

    /// <inheritdoc cref="string.Join(string, string?[])"/>
    public static string Join(this IEnumerable<string> values, string separator)
        => string.Join(separator, values);

    public static string GetLast(this string source, int tail_length) => tail_length >= source.Length ? source : source[^tail_length..];

    public static int ToUnixTimestamp(this DateTime value) => (int)Math.Truncate(value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
}
