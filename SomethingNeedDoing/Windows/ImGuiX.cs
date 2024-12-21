using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.ComponentModel;
using System;
using System.Numerics;
using System.Reflection;

namespace SomethingNeedDoing.Interface;

internal static class ImGuiX
{
    public static string EnumString(Enum v)
    {
        var name = v.ToString();
        return v.GetType().GetField(name)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
    }

    public static bool Enum<T>(string label, ref T v) where T : Enum
    {
        var res = false;
        ImGui.SetNextItemWidth(200);
        using var combo = ImRaii.Combo(label, EnumString(v));
        if (!combo) return false;
        foreach (var opt in System.Enum.GetValues(v.GetType()))
        {
            if (ImGui.Selectable(EnumString((Enum)opt), opt.Equals(v)))
            {
                v = (T)opt;
                res = true;
            }
        }
        return res;
    }
    /// <summary>
    /// An icon button.
    /// </summary>
    /// <param name="icon">Icon value.</param>
    /// <param name="tooltip">Simple tooltip.</param>
    /// <returns>Result from ImGui.Button.</returns>
    public static bool IconButton(FontAwesomeIcon icon, string tooltip)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
        ImGui.PopFont();

        if (tooltip != null)
            TextTooltip(tooltip);

        return result;
    }

    /// <summary>
    /// Show a simple text tooltip if hovered.
    /// </summary>
    /// <param name="text">Text to display.</param>
    public static void TextTooltip(string text)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(text);
            ImGui.EndTooltip();
        }
    }

    /// <summary>
    /// Get the current RGBA color for the given widget.
    /// </summary>
    /// <param name="col">The type of color to fetch.</param>
    /// <returns>A RGBA vec4.</returns>
    public static Vector4 GetStyleColorVec4(ImGuiCol col)
    {
        unsafe
        {
            return *ImGui.GetStyleColorVec4(ImGuiCol.Button);
        }
    }
}
