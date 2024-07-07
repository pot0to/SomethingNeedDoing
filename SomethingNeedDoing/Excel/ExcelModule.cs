using ECommons.SimpleGui;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Lumina.Text.Expressions;
using Lumina.Text.Payloads;
using SomethingNeedDoing.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace SomethingNeedDoing.Excel;

public class ExcelModule
{
    public readonly string[] Sheets;
    public readonly Dictionary<string, RawExcelSheet?> SheetsCache = [];
    public readonly Dictionary<string, SheetDefinition?> SheetDefinitions = [];

    private HttpClient _httpClient = new();

    public ExcelModule()
    {
        Sheets = [.. Svc.Data.Excel.GetSheetNames()];
    }

    public RawExcelSheet? GetSheet(string name, bool skipCache = false)
    {
        if (skipCache) return Svc.Data.Excel.GetSheetRaw(name);
        if (SheetsCache.TryGetValue(name, out var sheet)) return sheet;

        sheet = Svc.Data.Excel.GetSheetRaw(name);
        SheetsCache[name] = sheet;

        if (!SheetDefinitions.ContainsKey(name))
        {
            ResolveSheetDefinition(name);
        }

        return sheet;
    }

    public void ReloadAllSheets()
    {
        SheetsCache.Clear();
        //foreach (var window in Windows) window.Reload();
    }

    public void OpenNewWindow(string? sheet = null, int? scrollTo = null)
    {
        EzConfigGui.WindowSystem.Windows.FirstOrDefault(w => w.WindowName == ExcelWindow.WindowName)!.IsOpen = true;
    }

    private void ResolveSheetDefinition(string name)
    {
        var url = $"https://raw.githubusercontent.com/xivapi/SaintCoinach/master/SaintCoinach/Definitions/{name}.json";

        _httpClient.GetAsync(url).ContinueWith(t =>
        {
            try
            {
                var result = t.Result;
                if (result.IsSuccessStatusCode)
                {
                    var json = result.Content.ReadAsStringAsync().Result;

                    var sheetDefinition = JsonSerializer.Deserialize<SheetDefinition>(json);
                    if (sheetDefinition is null)
                    {
                        Svc.Log.Error("Failed to deserialize sheet definition");
                        return;
                    }

                    Svc.Log.Debug("Resolved sheet definition: {sheetName} -> {sheetDefinition}",
                              name,
                              sheetDefinition);

                    SheetDefinitions[name] = sheetDefinition;
                }
                else
                {
                    Svc.Log.Error("Request for sheet definition failed: {sheetName} -> {statusCode}",
                              name,
                              result.StatusCode);

                    SheetDefinitions[name] = null;
                }
            }
            catch (Exception e)
            {
                Svc.Log.Error(e, "Failed to resolve sheet definition");
            }
        });
    }

    // Abstracted here so we can show previous of what links are
    internal void DrawEntry(ExcelWindow sourceWindow, RawExcelSheet sheet, int row, int col, object data, ConverterDefinition? converter, bool insideLink = false)
    {
        switch (converter)
        {
            // Was originally 'link when link.Target != null', Rider wants me to turn it into this monstrous thing
            case LinkConverterDefinition { Target: not null } link:
                {
                    var targetRow = 0;
                    try
                    {
                        targetRow = Convert.ToInt32(data);
                    }
                    catch
                    {
                        // ignored
                    }

                    if (insideLink && ImGui.IsKeyDown(ImGuiKey.ModAlt))
                    {
                        // Draw what the link points to
                        var targetSheet = GetSheet(link.Target);
                        var targetRowObj = targetSheet?.GetRow((uint)targetRow);
                        var sheetDef = SheetDefinitions.TryGetValue(link.Target, out var definition)
                                           ? definition
                                           : null;

                        if (sheetDef is not null)
                        {
                            var targetCol = sheetDef.DefaultColumn is not null
                                                ? sheetDef.GetColumnForName(sheetDef.DefaultColumn) ?? 0
                                                : 0;
                            var targetData = targetRowObj?.ReadColumnRaw(targetCol);

                            if (targetData is not null)
                            {
                                DrawEntry(
                                    sourceWindow,
                                    targetSheet!,
                                    targetRow,
                                    targetCol,
                                    targetData,
                                    sheetDef.GetConverterForColumn(targetCol),
                                    true
                                );
                                return;
                            }
                        }
                    }

                    DrawLink(sourceWindow, link.Target, targetRow, row, col);
                    break;
                }

            case IconConverterDefinition:
                {
                    var iconId = 0u;
                    try
                    {
                        iconId = Convert.ToUInt32(data);
                    }
                    catch
                    {
                        // ignored
                    }

                    var icon = Svc.Texture.GetFromGameIcon(iconId).GetWrapOrEmpty();
                    if (icon is not null)
                    {
                        var path = icon.ImGuiHandle;
                        var handle = icon.ImGuiHandle;
                        if (handle == IntPtr.Zero) break;

                        Vector2 ScaleSize(float maxY)
                        {
                            var size = new Vector2(icon!.Width, icon.Height);
                            if (size.Y > maxY) size *= maxY / size.Y;
                            return size;
                        }

                        var lineSize = ScaleSize(Service.Configuration.LineHeightImages ? ImGui.GetTextLineHeight() * 2 : 512);
                        ImGui.Image(handle, lineSize);

                        var shouldShowMagnum = ImGui.IsKeyDown(ImGuiKey.ModAlt) && ImGui.IsItemHovered();
                        if (shouldShowMagnum)
                        {
                            var magnumSize = ScaleSize(1024);
                            ImGui.BeginTooltip();
                            ImGui.Image(handle, magnumSize);
                            ImGui.EndTooltip();
                        }

                        if (ImGui.BeginPopupContextItem($"{row}_{col}"))
                        {
                            ImGui.MenuItem("Icon", false);

                            if (ImGui.MenuItem("Copy icon ID"))
                            {
                                ImGui.SetClipboardText(iconId.ToString());
                            }

                            if (ImGui.MenuItem("Copy icon path"))
                            {
                                ImGui.SetClipboardText(Svc.Texture.GetIconPath(iconId));
                            }

                            ImGui.EndPopup();
                        }
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        ImGui.TextUnformatted($"(couldn't load icon {iconId})");
                        ImGui.EndDisabled();
                    }

                    break;
                }

            case TomestoneConverterDefinition:
                {
                    // FIXME this allocates memory like a motherfucker, cache this
                    var dataInt = Convert.ToUInt32(data);
                    var tomestone = dataInt > 0
                                        ? Svc.Data.GetExcelSheet<TomestonesItem>()!
                                            .FirstOrDefault(x => x.Tomestones.Row == dataInt)
                                        : null;

                    if (tomestone is null)
                    {
                        DrawLink(
                            sourceWindow,
                            "Item",
                            (int)dataInt,
                            row,
                            col
                        );
                    }
                    else
                    {
                        DrawLink(
                            sourceWindow,
                            "Item",
                            (int)tomestone.Item.Row,
                            row,
                            col
                        );
                    }

                    break;
                }

            case ComplexLinkConverterDefinition complex:
                {
                    var targetRow = 0;
                    try
                    {
                        targetRow = Convert.ToInt32(data);
                    }
                    catch
                    {
                        // ignored
                    }

                    var resolvedLinks = complex.ResolveComplexLink(
                        this,
                        sheet,
                        row,
                        targetRow
                    );

                    foreach (var link in resolvedLinks)
                    {
                        DrawLink(sourceWindow, link.Link, link.TargetRow, row, col);
                    }

                    break;
                }

            default:
                {
                    string? str;

                    try
                    {
                        str = data.ToString();
                        if (data is SeString seString)
                        {
                            str = DisplaySeString(seString);
                        }
                    }
                    catch
                    {
                        // Some sheets (like CustomTalkDefineClient) have broken SeString, so let's catch that
                        break;
                    }

                    if (str is null) break;

                    ImGui.TextUnformatted(str);

                    if (ImGui.BeginPopupContextItem($"{row}_{col}"))
                    {
                        var fileExists = false;
                        try
                        {
                            fileExists = Svc.Data.FileExists(str);
                        }
                        catch
                        {
                            // ignored
                        }

                        if (ImGui.MenuItem("Copy"))
                        {
                            ImGui.SetClipboardText(str);
                        }

                        ImGui.EndPopup();
                    }

                    break;
                }
        }
    }

    private void DrawLink(ExcelWindow sourceWindow, string link, int targetRow, int row, int col)
    {
        var text = $"{link}#{targetRow}" + $"##{row}_{col}";

        if (ImGui.Button(text))
        {
            sourceWindow.OpenSheet(link, targetRow);
        }

        if (ImGui.BeginPopupContextItem($"{row}_{col}"))
        {
            if (ImGui.MenuItem("Open in new window"))
            {
                OpenNewWindow(link, targetRow);
            }

            ImGui.EndPopup();
        }

        // Hack to preview a link
        var targetSheet = GetSheet(link);
        if (
            targetSheet is not null
            && SheetDefinitions.TryGetValue(link, out var sheetDef)
            && sheetDef is not null)
        {
            var targetRowObj = targetSheet.GetRow((uint)targetRow);
            var targetCol = sheetDef.DefaultColumn is not null
                                ? sheetDef.GetColumnForName(sheetDef.DefaultColumn) ?? 0
                                : 0;

            var data = targetRowObj?.ReadColumnRaw(targetCol);
            if (data is not null && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                DrawEntry(
                    sourceWindow,
                    targetSheet,
                    targetRow,
                    targetCol,
                    data,
                    sheetDef.GetConverterForColumn(targetCol),
                    true
                );
                ImGui.EndTooltip();
            }
        }
    }

    public string DisplayObject(object obj)
    {
        if (obj is SeString seString)
        {
            return DisplaySeString(seString);
        }

        return obj.ToString() ?? "";
    }

    private static void XmlRepr(StringBuilder sb, BaseExpression expr)
    {
        switch (expr)
        {
            case PlaceholderExpression ple:
                sb.Append('<').Append(ple.ExpressionType).Append(" />");
                break;
            case IntegerExpression ie:
                sb.Append('<').Append(ie.ExpressionType).Append('>');
                sb.Append(ie.Value);
                sb.Append("</").Append(ie.ExpressionType).Append('>');
                break;
            case StringExpression se:
                sb.Append('<').Append(se.ExpressionType).Append('>');
                XmlRepr(sb, se.Value);
                sb.Append("</").Append(se.ExpressionType).Append('>');
                break;
            case ParameterExpression pae:
                sb.Append('<').Append(pae.ExpressionType).Append('>');
                sb.Append("<operand>");
                XmlRepr(sb, pae.Operand);
                sb.Append("</operand>");
                sb.Append("</").Append(pae.ExpressionType).Append('>');
                break;
            case BinaryExpression pae:
                sb.Append('<').Append(pae.ExpressionType).Append('>');
                sb.Append("<operand1>");
                XmlRepr(sb, pae.Operand1);
                sb.Append("</operand1>");
                sb.Append("<operand2>");
                XmlRepr(sb, pae.Operand2);
                sb.Append("</operand2>");
                sb.Append("</").Append(pae.ExpressionType).Append('>');
                break;
        }
    }

    private static void XmlRepr(StringBuilder sb, SeString s)
    {
        foreach (var payload in s.Payloads)
        {
            if (payload is TextPayload t)
            {
                sb.Append(t.RawString);
            }
            else if (!payload.Expressions.Any())
            {
                sb.Append($"<{payload.PayloadType} />");
            }
            else
            {
                sb.Append($"<{payload.PayloadType}>");
                foreach (var expr in payload.Expressions)
                    XmlRepr(sb, expr);
                sb.Append($"<{payload.PayloadType}>");
            }
        }
    }

    public static string DisplaySeString(SeString s)
    {
        var sb = new StringBuilder();
        XmlRepr(sb, s);
        return sb.ToString();
    }
}
