using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Misc;

internal class MiscHelpers
{
    private static readonly unsafe nint pronounModule = (nint)Framework.Instance()->GetUiModule()->GetPronounModule();
    private static readonly unsafe delegate* unmanaged<nint, uint, GameObject*> getGameObjectFromPronounID = (delegate* unmanaged<nint, uint, GameObject*>)Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
    public static unsafe GameObject* GetGameObjectFromPronounID(uint id) => getGameObjectFromPronounID(pronounModule, id);

    public static string ConvertClipboardToSafeString()
    {
        string text;
        try
        {
            text = ImGui.GetClipboardText();
        }
        catch (NullReferenceException ex)
        {
            text = string.Empty;
            Service.ChatManager.PrintError($"[SND] Could not import from clipboard.");
            Service.Log.Error(ex, "Clipboard import error");
        }

        // Replace \r with \r\n, usually from copy/pasting from the in-game macro window
        var rex = new Regex("\r(?!\n)", RegexOptions.Compiled);
        var matches = from Match match in rex.Matches(text)
                      let index = match.Index
                      orderby index descending
                      select index;
        foreach (var index in matches)
            text = text.Remove(index, 1).Insert(index, "\r\n");

        return text;
    }

    public static bool IsLuaCode(string code)
    {
        string[] luaPatterns = {
            @"function\s+.+\(.*\)",
            @"\bend\b",
            @"local\s+\w+",
            @"\bthen\b",
            @"--.*",
            @"\bdo\b",
            @"\brepeat\b",
            @"\buntil\b",
            @"\bif\b",
            @"\belseif\b",
            @"\belse\b"
        };

        foreach (var pattern in luaPatterns)
            if (Regex.IsMatch(code, pattern))
                return true;

        return false;
    }
}
