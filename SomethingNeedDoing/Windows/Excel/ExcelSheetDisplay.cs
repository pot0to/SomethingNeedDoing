using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Interface.Excel;
public sealed class ExcelSheetDisplay
{
    public ExcelSheetDisplay() { }

    private ExcelSheet<RawRow>? _sheet;
    private SubrowExcelSheet<RawSubrow>? _subsheet;
    private List<int> _curSourceList = [];
    private SortedSet<RawRow>? _curFilteredRows;
    private string _curSearchFilter = "";
    private CancellationTokenSource? _filterCts;
    private float? itemHeight;

    public void Draw(ExcelSheet<RawRow> sheet)
    {
        if (sheet == null) return;
        _sheet = sheet;
        _curSourceList = new(sheet.Count);

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * ImGuiHelpers.GlobalScale - ImGui.GetStyle().ItemSpacing.X);
        var filterDirty = ImGui.InputTextWithHint($"###{nameof(ExcelSheetDisplay)}filter", "Search...", ref _curSearchFilter, 256);

        if (filterDirty)
        {
            _filterCts?.Cancel();
            _filterCts = new();
            Task.Run(() => ApplyFilterAsync(sheet, _filterCts));
        }

        if (sheet.Columns.Count > 100)
        {
            ImGui.TextUnformatted($"Unable to display large sheets. Column Count: {sheet.Columns.Count}");
            return;
        }

        var height = ImGui.GetContentRegionAvail().Y;
        using var table = ImRaii.Table($"{nameof(ExcelSheetDisplay)}", sheet.Columns.Count + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.NoSavedSettings, new(0, height));
        if (!table) return;

        ImGui.TableSetupScrollFreeze(1, 1);
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
        ImGuiClip.ClippedDraw(_curFilteredRows?.ToArray() ?? [.. _sheet], DrawRow, itemHeight ?? 0);
    }

    public void Draw(SubrowExcelSheet<RawSubrow> sheet)
    {
        if (sheet == null) return;
        _subsheet = sheet;

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * ImGuiHelpers.GlobalScale - ImGui.GetStyle().ItemSpacing.X);
        var filterDirty = ImGui.InputTextWithHint($"###{nameof(ExcelSheetDisplay)}filter", "Search...", ref _curSearchFilter, 256);

        if (filterDirty)
        {
            _filterCts?.Cancel();
            _filterCts = new();
            Task.Run(() => ApplyFilterAsync(sheet, _filterCts));
        }

        ImGui.TextUnformatted($"dev didn't get this far implementing subrow sheets");
    }

    private void DrawRow(RawRow row)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(row.RowId.ToString());
        for (var c = 0; c < _sheet!.Columns.Count; c++)
        {
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            //#pragma warning disable SeStringRenderer
            //            ImGuiHelpers.CompileSeStringWrapped(ReadCell(_sheet, (int)row.RowId, c));
            //#pragma warning restore SeStringRenderer
            ImGui.TextUnformatted(ReadCell(_sheet, (int)row.RowId, c));
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

    private async Task ApplyFilterAsync(ExcelSheet<RawRow> sheet, CancellationTokenSource _filterCts)
    {
        if (string.IsNullOrWhiteSpace(_curSearchFilter) || _filterCts.IsCancellationRequested)
        {
            _curFilteredRows = null;
            return;
        }

        _curFilteredRows = new(Comparer<RawRow>.Create((x, y) => x.RowId.CompareTo(y.RowId)));
        for (var i = 0; i < sheet.Columns.Count; i++)
        {
            for (var r = 0; r < sheet.Count; r++)
            {
                if (sheet.GetRow((uint)r).ReadColumn(i).ToString() is { } text)
                {
                    if (text.Contains(_curSearchFilter, StringComparison.OrdinalIgnoreCase) && !_filterCts.IsCancellationRequested)
                    {
                        _curFilteredRows.Add(sheet.GetRow((uint)r));
                    }
                }
            }
        }
    }

    private async Task ApplyFilterAsync(SubrowExcelSheet<RawSubrow> sheet, CancellationTokenSource _filterCts) { }
}
