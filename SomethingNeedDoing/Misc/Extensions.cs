using System;
using System.Collections.Generic;
using System.Numerics;

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

    public static uint ToUintColour(this Vector4 color) => (uint)((int)(color.W * 255) << 24 | (int)(color.X * 255) << 16 | (int)(color.Y * 255) << 8 | (int)(color.Z * 255));
}
