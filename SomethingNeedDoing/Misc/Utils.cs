using Dalamud.Game;
using Dalamud.Utility;
using ECommons.Logging;
using ExdSheets;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Misc;

internal class Utils
{
    private static readonly unsafe nint pronounModule = (nint)Framework.Instance()->GetUIModule()->GetPronounModule();
    private static readonly unsafe delegate* unmanaged<nint, uint, GameObject*> getGameObjectFromPronounID = (delegate* unmanaged<nint, uint, GameObject*>)Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
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
            DuoLog.Error($"Clipboard import error");
            Svc.Log.Error($"{ex.Message}\n{ex.StackTrace}");
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
        string[] luaPatterns = [
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
        ];

        foreach (var pattern in luaPatterns)
            if (Regex.IsMatch(code, pattern))
                return true;

        return false;
    }

    public static Sheet<T> GetSheet<T>(ClientLanguage? language = null) where T : struct, ISheetRow<T>
        => Service.Module.GetSheet<T>((language ?? Svc.ClientState.ClientLanguage).ToLumina());

    public static int GetRowCount<T>() where T : struct, ISheetRow<T>
        => GetSheet<T>().Count;

    public static T? GetRow<T>(uint rowId, ClientLanguage? language = null) where T : struct, ISheetRow<T>
        => GetSheet<T>(language).TryGetRow(rowId);

    public static T? GetRow<T>(uint rowId, ushort subRowId, ClientLanguage? language = null) where T : struct, ISheetRow<T>
        => GetSheet<T>(language).TryGetRow(rowId, subRowId);

    public static T? FindRow<T>(Func<T, bool> predicate) where T : struct, ISheetRow<T>
         => GetSheet<T>().FirstOrDefault(predicate);

    public static T[] FindRows<T>(Func<T, bool> predicate) where T : struct, ISheetRow<T>
        => GetSheet<T>().Where(predicate).ToArray();
}
