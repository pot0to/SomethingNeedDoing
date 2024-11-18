using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel;
using SomethingNeedDoing.Misc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Interface.Excel;
public sealed class ExcelSheetDisplay
{
    public ExcelSheetDisplay() { }

    private List<int> _curSourceList = [];
    private readonly List<int> _curFilteredRows = [];
    private string _curSearchFilter = "";
    private float? itemHeight;
    public void Draw(ExcelSheet<RawRow> sheet)
    {
        if (sheet == null) return;
        _curSourceList = new(sheet.Count);

        var filterDirty = ImGui.InputTextWithHint($"###{nameof(ExcelSheetDisplay)}filter", "Search...", ref _curSearchFilter, 256);

        if (filterDirty)
            Task.Run(ApplyFilterAsync);

        if (sheet.Columns.Count > 100)
        {
            ImGui.TextUnformatted($"Unable to display large sheets. Column Count: {sheet.Columns.Count}");
            return;
        }

        var height = ImGui.GetContentRegionAvail().Y;
        using var table = ImRaii.Table($"{nameof(ExcelSheetDisplay)}", sheet.Columns.Count + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.SizingFixedFit, new(0, height));
        if (!table) return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableHeadersRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TableHeader("RowId");

        for (var i = 0; i < sheet.Columns.Count; i++)
        {
            using var id = ImRaii.PushId(i);
            ImGui.TableSetColumnIndex(i + 1);
            ImGui.TableHeader($"{i}: {sheet.Columns[i].Type}");
        }

        ImGui.TableSetColumnIndex(0);
        itemHeight = ImGui.GetTextLineHeightWithSpacing();
        var clipper = new ListClipper(sheet.Count, itemHeight: itemHeight ?? 0);
        foreach (var r in clipper.Rows)
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{sheet.GetRow((uint)r).RowId}");
            for (var c = 0; c < sheet.Columns.Count; c++)
            {
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
#pragma warning disable SeStringRenderer
                ImGuiHelpers.CompileSeStringWrapped(ReadCell(sheet, r, c));
#pragma warning restore SeStringRenderer
            }
        }
    }

    private string ReadCell(ExcelSheet<RawRow> sheet, int r, int c)
    {
        //switch (sheet.Columns[0].Type)
        //{
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool0:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool1:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool2:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool3:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool4:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool5:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool6:
        //    case Lumina.Data.Structs.Excel.ExcelColumnDataType.PackedBool7:
        //        var x = sheet.GetRow((uint)r).ReadPackedBoolColumn(c, 1);
        //        break;
        //}
        return sheet.GetRow((uint)r).ReadColumn(c).ToString() ?? string.Empty;
    }

    private async Task ApplyFilterAsync()
    {
        //    _curFilteredRows.Clear();
        //    var terms = _curSearchFilter.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //    async Task<bool> match(string name) => await Task.FromResult(terms.All(name.Contains));
        //    _curFilteredRows.AddRange(await FilterAsync(_curSourceList, (sheet) => match(_sheets[sheet].ToLowerInvariant())));
    }

    private async Task<T[]> FilterAsync<T>(IEnumerable<T> sourceEnumerable, Func<T, Task<bool>> predicateAsync)
        => (await Task.WhenAll(
            sourceEnumerable.Select(v => predicateAsync(v).ContinueWith(
                task => new { Predicate = task.Result, Value = v }))))
        .Where(a => a.Predicate).Select(a => a.Value).ToArray();
}
