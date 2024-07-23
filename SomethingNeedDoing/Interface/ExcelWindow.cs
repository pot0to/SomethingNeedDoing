using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using SomethingNeedDoing.Excel;
using SomethingNeedDoing.Misc;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ExcelModule = SomethingNeedDoing.Excel.ExcelModule;

namespace SomethingNeedDoing.Interface;

public class ExcelWindow : Window
{
    public static new readonly string WindowName = "SND Excel Browser";
    private readonly ExcelModule module;
    private RawExcelSheet? selectedSheet;
    private Dictionary<uint, (uint, uint?)> rowMapping = []; // Working around a Lumina bug to map index to row

    private float sidebarWidth = 300f;
    private bool fullTextSearch;

    private string sidebarFilter = string.Empty;
    private string contentFilter = string.Empty;
    private List<string>? filteredSheets;
    private List<uint>? filteredRows;

    private CancellationTokenSource? scriptToken;
    private CancellationTokenSource? sidebarToken;
    private string? scriptError;

    private (string, int?)? queuedOpen;

    private int? highlightRow;
    private int? tempScroll;
    private int paintTicksLeft = -1;
    private float? itemHeight = 0;

    public ExcelWindow() : base(WindowName)
    {
        module = new();
        Size = new(960, 540);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Reload()
    {
        // Re-fetch the sheet for language changes
        if (selectedSheet is null) return;
        selectedSheet = module.GetSheet(selectedSheet!.Name);
    }

    public void OpenSheet(string sheetName, int? scrollTo = null)
    {
        queuedOpen = (sheetName, scrollTo);
    }

    private void ProcessQueuedOpen()
    {
        var (sheetName, scrollTo) = queuedOpen!.Value;
        queuedOpen = null;

        highlightRow = scrollTo;
        tempScroll = scrollTo;

        var sheet = module.GetSheet(sheetName);
        if (sheet is null)
        {
            Svc.Log.Warning("Tried to open sheet that doesn't exist: {SheetName}", sheetName);
            return;
        }

        Svc.Log.Debug("Opening sheet: {SheetName} {ScrollTo}", sheetName, scrollTo);
        selectedSheet = sheet;
        filteredRows = null;
        itemHeight = 0;

        rowMapping = SetupRows(sheet);
        ResolveContentFilter();
    }

    // TODO deduplicate this code from fs module
    public override void Draw()
    {
        if (queuedOpen is not null) ProcessQueuedOpen();

        DrawSidebar();

        ImGui.BeginGroup();

        if (selectedSheet is not null)
        {
            var width = ImGui.GetContentRegionAvail().X;
            DrawContentFilter(width);
            DrawSheet(width);
        }

        ImGui.EndGroup();
    }

    private void DrawSidebar()
    {
        var temp = ImGui.GetCursorPosY();
        DrawSidebarFilter(sidebarWidth);

        var cra = ImGui.GetContentRegionAvail();
        ImGui.BeginChild("##ExcelModule_Sidebar", cra with { X = sidebarWidth }, true);

        var sheets = filteredSheets?.ToArray() ?? module.Sheets;
        foreach (var sheet in sheets)
        {
            if (ImGui.Selectable(sheet, sheet == selectedSheet?.Name))
            {
                OpenSheet(sheet);
            }

            if (ImGui.BeginPopupContextItem($"##ExcelModule_Sidebar_{sheet}"))
            {
                if (ImGui.Selectable("Open in new window"))
                {
                    module.OpenNewWindow(sheet);
                }

                ImGui.EndPopup();
            }
        }

        ImGui.EndChild();

        ImGui.SameLine();
        ImGui.SetCursorPosY(temp);

        ImGui.Button("##splitter", new Vector2(5, -1));
        //UiUtils.HorizontalSplitter(ref sidebarWidth);

        ImGui.SameLine();
        ImGui.SetCursorPosY(temp);
    }

    private void DrawSidebarFilter(float width)
    {
        ImGui.SetNextItemWidth(width);

        var shouldOrange = fullTextSearch;
        using var colour = ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(1f, 0.5f, 0f, 0.5f), shouldOrange);

        var flags = fullTextSearch ? ImGuiInputTextFlags.EnterReturnsTrue : ImGuiInputTextFlags.None;
        if (ImGui.InputText("##ExcelFilter", ref sidebarFilter, 1024, flags))
        {
            ResolveSidebarFilter();
        }

        if (ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
            {
                var filterMode = fullTextSearch ? "Full text search" : "Name search";
                ImGui.TextUnformatted(
                    $"Current filter mode: {filterMode}\n"
                    + "Right click to change the filter mode.");

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    fullTextSearch = !fullTextSearch;
            }
        }
    }

    private void DrawContentFilter(float width)
    {
        ImGui.SetNextItemWidth(width);

        var shouldRed = scriptError is not null;
        if (shouldRed) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0f, 0f, 1f));
        var shouldOrange = scriptToken is not null && !shouldRed;
        if (shouldOrange) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.5f, 0f, 1f));

        var flags = contentFilter.StartsWith("$")
                        ? ImGuiInputTextFlags.EnterReturnsTrue
                        : ImGuiInputTextFlags.None;
        if (ImGui.InputText("##ExcelContentFilter", ref contentFilter, 1024, flags))
        {
            ResolveContentFilter();
        }

        // Disable filter on right click
        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            contentFilter = string.Empty;
            ResolveContentFilter();
        }

        if (shouldRed)
        {
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(scriptError ?? "Unknown error");
                ImGui.EndTooltip();
            }
        }

        if (shouldOrange)
        {
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(
                    "A script is currently running on each row. This may impact performance.\n"
                    + "To stop the script, empty or right click the input box.");
                ImGui.EndTooltip();
            }
        }
    }

    private void DrawSheet(float width)
    {
        ImGui.SetNextItemWidth(width);

        // Wait for the sheet definition request to finish before drawing the sheet
        // This does *not* mean sheets with no definitions will be skipped
        if (!module.SheetDefinitions.TryGetValue(selectedSheet!.Name, out var sheetDefinition))
        {
            return;
        }

        var rowCount = selectedSheet.RowCount;
        var colCount = selectedSheet.ColumnCount;
        colCount = Math.Min(colCount, 2048 - 1); // I think this is an ImGui limitation?

        var flags = ImGuiTableFlags.Borders
                    | ImGuiTableFlags.NoSavedSettings
                    | ImGuiTableFlags.RowBg
                    | ImGuiTableFlags.Resizable
                    | ImGuiTableFlags.ScrollX
                    | ImGuiTableFlags.ScrollY;

        // +1 here for the row ID column
        if (!ImGui.BeginTable("##ExcelTable", (int)(colCount + 1), flags))
        {
            return;
        }

        ImGui.TableSetupScrollFreeze(1, 1);

        ImGui.TableHeadersRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TableHeader("Row");

        var colMappings = new int[colCount];
        if (Service.Configuration.SortByOffsets)
        {
            var colOffsets = new Dictionary<int, uint>();

            for (var i = 0; i < colCount; i++)
            {
                var col = selectedSheet.Columns[i];
                colOffsets[i] = col.Offset;
            }

            colOffsets = colOffsets
                .OrderBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            for (var i = 0; i < colCount; i++) colMappings[i] = colOffsets.ElementAt(i).Key;
        }
        else
        {
            for (var i = 0; i < colCount; i++) colMappings[i] = i;
        }

        for (var i = 0; i < colCount; i++)
        {
            var colId = colMappings[i];
            var colName = sheetDefinition?.GetNameForColumn(colId) ?? colId.ToString();

            var col = selectedSheet.Columns[colId];
            var offset = col.Offset;
            var offsetStr = $"Offset: {offset} (0x{offset:X})\nIndex: {colId}\nData type: {col.Type}";

            if (Service.Configuration.AlwaysShowOffsets) colName += "\n" + offsetStr;

            ImGui.TableSetColumnIndex(i + 1);
            ImGui.TableHeader(colName);

            if (ImGui.IsItemHovered() && !Service.Configuration.AlwaysShowOffsets)
            {
                using (ImRaii.Tooltip())
                    ImGui.TextUnformatted(offsetStr);
            }
        }

        var actualRowCount = filteredRows?.Count ?? (int)rowCount;
        var clipper = new ListClipper(actualRowCount, itemHeight: itemHeight ?? 0);

        // Sheets can have non-linear row IDs, so we use the index the row appears in the sheet instead of the row ID
        var newHeight = 0f;
        foreach (var i in clipper.Rows)
        {
            var rowId = i;
            if (filteredRows is not null)
            {
                rowId = (int)filteredRows[i];
            }

            var row = GetRow(selectedSheet, rowMapping, (uint)rowId);
            if (row is null)
            {
                ImGui.TableNextRow();
                continue;
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            using var highlighted = ImRaii.PushColor(ImGuiCol.TableRowBg, new Vector4(1f, 0.5f, 0f, 0.5f), highlightRow == rowId && Service.Configuration.HighlightLinks);
            using var highlightedAlt = ImRaii.PushColor(ImGuiCol.TableRowBgAlt, new Vector4(1f, 0.5f, 0f, 0.5f), highlightRow == rowId && Service.Configuration.HighlightLinks);

            var str = row.RowId.ToString();
            if (row.SubRowId != 0) str += $".{row.SubRowId}";
            ImGui.TextUnformatted(str);
            if (ImGui.BeginPopupContextItem($"##ExcelModule_Row_{rowId}"))
            {
                if (ImGui.Selectable("Copy row ID"))
                {
                    ImGui.SetClipboardText(str);
                }

                ImGui.EndPopup();
            }

            ImGui.TableNextColumn();

            for (var col = 0; col < colCount; col++)
            {
                var obj = row.ReadColumnRaw(colMappings[col]);
                var prev = ImGui.GetCursorPosY();

                if (obj != null)
                {
                    var converter = sheetDefinition?.GetConverterForColumn(colMappings[col]);

                    module.DrawEntry(
                        this,
                        selectedSheet,
                        rowId,
                        colMappings[col],
                        obj,
                        converter
                    );
                }

                var next = ImGui.GetCursorPosY();
                if (itemHeight is not null)
                {
                    var spacing = ImGui.GetStyle().ItemSpacing.Y;
                    var height = next - prev;
                    var needed = itemHeight.Value - (height + spacing);
                    if (needed > 0)
                    {
                        ImGui.Dummy(new Vector2(0, needed));
                    }

                    if (height > newHeight) newHeight = height;
                }

                if (col < colCount - 1) ImGui.TableNextColumn();
            }
        }

        if (itemHeight is not null && newHeight > itemHeight)
        {
            itemHeight = newHeight;
        }

        // I don't know why I need to do this but I really don't care, it's 12 AM and I want sleep
        // seems to crash if you scroll immediately, seems to do nothing if you scroll too little
        // stupid tick hack works for now lol
        if (tempScroll is not null & paintTicksLeft == -1)
        {
            paintTicksLeft = 5;
        }
        else if (paintTicksLeft <= 0)
        {
            tempScroll = null;
            paintTicksLeft = -1;
        }
        else if (tempScroll is not null)
        {
            ImGui.SetScrollY(tempScroll.Value * clipper.ItemsHeight);
            paintTicksLeft--;
        }

        clipper.End();
        ImGui.EndTable();
    }

    // Mapping index to row/subrow ID, since they are not linear
    private Dictionary<uint, (uint, uint?)> SetupRows(RawExcelSheet sheet)
    {
        var rowMapping = new Dictionary<uint, (uint, uint?)>();

        var currentRow = 0u;
        foreach (var page in sheet.DataPages)
        {
            foreach (var row in page.File.RowData.Values)
            {
                var parser = new RowParser(sheet, page.File);
                parser.SeekToRow(row.RowId);

                if (sheet.Header.Variant == ExcelVariant.Subrows)
                {
                    for (uint i = 0; i < parser.RowCount; i++)
                    {
                        rowMapping[currentRow] = (row.RowId, i);
                        currentRow++;
                    }
                }
                else
                {
                    rowMapping[currentRow] = (row.RowId, null);
                    currentRow++;
                }
            }
        }

        return rowMapping;
    }

    // Building a new RowParser every time is probably not the best idea, but doesn't seem to impact performance that hard
    private RowParser? GetRow(
        RawExcelSheet sheet,
        Dictionary<uint, (uint, uint?)> rowMapping,
        uint index
    )
    {
        var (row, subrow) = rowMapping[index];
        var page = sheet.DataPages.FirstOrDefault(x => x.File.RowData.ContainsKey(row));
        if (page is null) return null;

        var parser = new RowParser(sheet, page.File);
        if (subrow is not null)
        {
            parser.SeekToRow(row, subrow.Value);
        }
        else
        {
            parser.SeekToRow(row);
        }

        return parser;
    }

    private void ResolveContentFilter()
    {
        Svc.Log.Debug("Resolving content filter...");

        // clean up scripts
        if (scriptToken is not null && !scriptToken.IsCancellationRequested)
        {
            scriptToken.Cancel();
            scriptToken.Dispose();
            scriptToken = null;
        }

        scriptError = null;

        if (string.IsNullOrEmpty(contentFilter))
        {
            filteredRows = null;
            return;
        }

        if (selectedSheet is null)
        {
            filteredRows = null;
            return;
        }

        filteredRows = [];
        if (contentFilter.StartsWith("$"))
        {
            var script = contentFilter[1..];
            ContentFilterScript(script);
        }
        else
        {
            ContentFilterSimple(contentFilter);
        }

        itemHeight = 0;
        Svc.Log.Debug("Filter resolved!");
    }

    private void ContentFilterSimple(string filter)
    {
        var colCount = selectedSheet!.ColumnCount;
        for (var i = 0u; i < selectedSheet.RowCount; i++)
        {
            var row = GetRow(selectedSheet, rowMapping, i);
            if (row is null) continue;

            var rowStr = row.RowId.ToString();
            if (row.SubRowId != 0) rowStr += $".{row.SubRowId}";
            if (rowStr.ToLower().Contains(filter.ToLower()))
            {
                filteredRows!.Add(i);
                continue;
            }

            for (var col = 0; col < colCount; col++)
            {
                var obj = row.ReadColumnRaw(col);
                if (obj is null) continue;
                var str = module.DisplayObject(obj);

                if (str.ToLower().Contains(filter.ToLower()))
                {
                    filteredRows!.Add(i);
                    break;
                }
            }
        }
    }

    private void ContentFilterScript(string script)
    {
        scriptError = null;

        // picked a random type for this, doesn't really matter
        var luminaTypes = Assembly.GetAssembly(typeof(Sheets.Addon))?.GetTypes();
        var sheets = luminaTypes?
            .Where(t => t.GetCustomAttributes(typeof(SheetAttribute), false).Length > 0)
            .ToDictionary(t => ((SheetAttribute)t.GetCustomAttributes(typeof(SheetAttribute), false)[0]).Name);

        Type? sheetRow = null;
        if (sheets?.TryGetValue(selectedSheet!.Name, out var sheetType) == true)
        {
            sheetRow = sheetType;
        }

        // GameData.GetExcelSheet<T>();
        var getExcelSheet = typeof(GameData).GetMethod("GetExcelSheet", Type.EmptyTypes);
        var genericMethod = sheetRow is not null ? getExcelSheet?.MakeGenericMethod(sheetRow) : null;
        var sheetInstance = genericMethod?.Invoke(Svc.Data, []);

        var ct = new CancellationTokenSource();
        Task.Run(async () =>
        {
            try
            {
                var globalsType = sheetRow != null
                                      ? typeof(ExcelScriptingGlobal<>).MakeGenericType(sheetRow)
                                      : null;
                var expr = CSharpScript.Create<bool>(script, globalsType: globalsType);
                expr.Compile(ct.Token);

                for (var i = 0u; i < selectedSheet!.RowCount; i++)
                {
                    if (ct.IsCancellationRequested)
                    {
                        Svc.Log.Debug("Filter script cancelled - aborting");
                        return;
                    }

                    var row = GetRow(selectedSheet, rowMapping, i);
                    if (row is null) continue;

                    async void SimpleEval()
                    {
                        try
                        {
                            var res = await expr.RunAsync(cancellationToken: ct.Token);
                            if (res.ReturnValue) filteredRows?.Add(i);
                        }
                        catch (Exception e)
                        {
                            scriptError = e.Message;
                        }
                    }

                    if (sheetRow is null)
                    {
                        SimpleEval();
                    }
                    else
                    {
                        object? instance;
                        if (row.SubRowId == 0)
                        {
                            // sheet.GetRow(row.RowId);
                            var getRow = sheetInstance?.GetType().GetMethod("GetRow", [typeof(uint)]);
                            instance = getRow?.Invoke(sheetInstance, [row.RowId]);
                        }
                        else
                        {
                            // sheet.GetRow(row.RowId, row.SubRowId);
                            var getRow = sheetInstance?.GetType()
                                .GetMethod("GetRow", [typeof(uint), typeof(uint)]);
                            instance = getRow?.Invoke(sheetInstance, [row.RowId, row.SubRowId]);
                        }

                        // new ExcelScriptingGlobal<ExcelRow>(sheet, row);
                        var excelScriptingGlobal = typeof(ExcelScriptingGlobal<>).MakeGenericType(sheetRow);
                        var globals = Activator.CreateInstance(excelScriptingGlobal, sheetInstance, instance);
                        if (globals is null)
                        {
                            SimpleEval();
                        }
                        else
                        {
                            try
                            {
                                var res = await expr.RunAsync(globals, ct.Token);
                                if (res.ReturnValue) filteredRows?.Add(i);
                            }
                            catch (Exception e)
                            {
                                scriptError = e.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Svc.Log.Error(e, "Filter script failed");
                scriptError = e.Message;
            }

            Svc.Log.Debug("Filter script finished");
            scriptToken = null;
        }, ct.Token);

        scriptToken = ct;
    }

    private void ResolveSidebarFilter()
    {
        Svc.Log.Debug("Resolving sidebar filter...");

        if (sidebarToken is not null && !sidebarToken.IsCancellationRequested)
        {
            sidebarToken.Cancel();
            sidebarToken.Dispose();
            sidebarToken = null;
        }

        if (string.IsNullOrEmpty(sidebarFilter))
        {
            filteredSheets = null;
            return;
        }

        var filter = sidebarFilter.ToLower();
        if (fullTextSearch)
        {
            filteredSheets = [];

            var ct = new CancellationTokenSource();
            Task.Run(() =>
            {
                foreach (var sheetName in module.Sheets)
                {
                    var sheet = module.GetSheet(sheetName, true);
                    if (sheet is null) continue;

                    if (filteredSheets.Contains(sheetName)) continue;

                    var rowMapping = SetupRows(sheet);
                    var colCount = sheet.ColumnCount;

                    var found = false;
                    foreach (var rowId in rowMapping.Keys)
                    {
                        if (found) break;

                        var row = GetRow(sheet, rowMapping, rowId);
                        if (row is null) continue;

                        for (var col = 0; col < colCount; col++)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                Svc.Log.Debug("Sidebar filter cancelled - aborting");
                                return;
                            }

                            var obj = row.ReadColumnRaw(col);
                            if (obj is null) continue;
                            var str = module.DisplayObject(obj);

                            if (str.ToLower().Contains(filter))
                            {
                                filteredSheets.Add(sheetName);
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }, ct.Token);
        }
        else
        {
            filteredSheets = module.Sheets
                .Where(x => x.ToLower().Contains(filter))
                .ToList();
        }
    }
}
