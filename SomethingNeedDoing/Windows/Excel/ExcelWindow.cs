using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel;
using SomethingNeedDoing.Interface.Excel;

namespace SomethingNeedDoing.Interface;

public class ExcelWindow : Window
{
    public static new readonly string WindowName = "SND Excel Browser";

    private readonly ExcelSheetList _sheetList = new();
    private readonly ExcelSheetDisplay _sheetDisplay = new();

    public ExcelWindow() : base(WindowName)
    {
        Size = new(960, 540);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        using (var c = ImRaii.Child($"{nameof(ExcelSheetList)}", new(267 * ImGui.GetIO().FontGlobalScale, 0), true))
        {
            if (c)
                _sheetList.Draw();
        }
        ImGui.SameLine();

        using var ch = ImRaii.Child($"{nameof(ExcelSheetDisplay)}");
        if (ch)
        {
            var sheet = Svc.Data.GetExcelSheet<RawRow>(null, _sheetList._sheets[_sheetList.SelectedItem]);
            if (_sheetList.SelectedItem != 0)
                _sheetDisplay.Draw(sheet);
        }
    }
}
