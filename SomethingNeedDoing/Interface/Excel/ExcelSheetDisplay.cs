namespace SomethingNeedDoing.Interface.Excel;
public sealed class ExcelSheetDisplay
{
    public ExcelSheetDisplay() { }

    //private float? itemHeight;
    //public void Draw(ExcelSheet<RawRow> sheet)
    //{
    //    if (sheet == null) return;
    //    var listingsHeight = ImGui.GetContentRegionAvail().Y;
    //    using var listings = ImRaii.Table($"{nameof(ExcelSheetDisplay)}", sheet.Columns.Count, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY, new(0, listingsHeight));
    //    if (!listings) return;
    //    ImGui.TableSetupScrollFreeze(0, 1);
    //    ImGui.TableHeadersRow();
    //    ImGui.TableSetColumnIndex(0);
    //    ImGui.TableHeader("Row");

    //    for (var i = 0; i < sheet.Columns.Count; i++)
    //    {
    //        using var id = ImRaii.PushId(i);
    //        ImGui.TableSetColumnIndex(i + 1);
    //        ImGui.TableHeader(colName);

    //        if (ImGui.IsItemHovered())
    //        {
    //            ImGui.BeginTooltip();
    //            ImGui.TextUnformatted(offsetStr);
    //            ImGui.EndTooltip();
    //        }
    //    }

    //    var clipper = new ListClipper(sheet.Count, itemHeight: itemHeight ?? 0);
    //}
}
