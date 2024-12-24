using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using ECommons.SimpleGui;
using ImGuiNET;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Macros;
using SomethingNeedDoing.Misc;
using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace SomethingNeedDoing.Windows;
public class MacrosUI : Window
{
    public MacrosUI() : base($"Something Need Doing {Service.Plugin.GetType().Assembly.GetName().Version}###SomethingNeedDoing")
    {
        Size = new Vector2(525, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
        RespectCloseHotkey = false;
        //LockButton = new()
        //{
        //    Click = OnLockButtonClick,
        //    Icon = Service.Configuration.LockWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
        //    IconOffset = new(3, 2),
        //    ShowTooltip = () => ImGui.SetTooltip("Lock window position and size"),
        //};
        //TitleBarButtons.Add(LockButton);
    }

    private static MacroFile? Selected => FS.Selector.Selected;

    public override bool DrawConditions() => !FS.Building;

    public override void Draw()
    {
        FS.Selector.Draw(200f);
        ImGui.SameLine();
        using var group = ImRaii.Group();
        DrawStateHeader();
        DrawRunningMacro();
        DrawSelected();
    }

    private static void DrawStateHeader()
    {
        ImGui.TextUnformatted("Macro Queue");

        var state = Service.MacroManager.State;

        var stateName = state switch
        {
            LoopState.NotLoggedIn => "Not Logged In",
            LoopState.Running when Service.MacroManager.PauseAtLoop => "Pausing Soon",
            LoopState.Running when Service.MacroManager.StopAtLoop => "Stopping Soon",
            _ => Enum.GetName(state),
        };

        var buttonCol = ImGuiX.GetStyleColorVec4(ImGuiCol.Button);
        using (var _ = ImRaii.PushColor(ImGuiCol.ButtonActive, buttonCol).Push(ImGuiCol.ButtonHovered, buttonCol))
            ImGui.Button($"{stateName}##LoopState", new Vector2(100, 0));

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, "Help"))
            EzConfigGui.GetWindow<HelpWindow>()!.Toggle();
        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FileExcel, "Excel Browser"))
            EzConfigGui.GetWindow<ExcelWindow>()!.Toggle();

        if (Service.MacroManager.State == LoopState.NotLoggedIn) { /* Nothing to do */ }
        else if (Service.MacroManager.State == LoopState.Stopped) { /* Nothing to do */ }
        else if (Service.MacroManager.State == LoopState.Waiting) { /* Nothing to do */ }
        else if (Service.MacroManager.State == LoopState.Paused)
        {
            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Resume"))
                Service.MacroManager.Resume();

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.StepForward, "Step"))
                Service.MacroManager.NextStep();

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Clear"))
                Service.MacroManager.Stop();
        }
        else if (Service.MacroManager.State == LoopState.Running)
        {
            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Pause, "Pause (hold control to pause at next /loop)"))
                Service.MacroManager.Pause(ImGui.GetIO().KeyCtrl);

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Stop, "Stop (hold control to stop at next /loop)"))
                Service.MacroManager.Stop(ImGui.GetIO().KeyCtrl);
        }
    }

    public static void DrawRunningMacro()
    {
        ImGui.PushItemWidth(-1);

        var style = ImGui.GetStyle();
        var runningHeight = ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 3 + style.FramePadding.Y * 2 + style.ItemSpacing.Y * 2;
        if (ImGui.BeginListBox("##running-macros", new Vector2(-1, runningHeight)))
        {
            var macroStatus = Service.MacroManager.MacroStatus;
            for (var i = 0; i < macroStatus.Length; i++)
            {
                var (name, stepIndex) = macroStatus[i];
                var text = name;
                if (i == 0 || stepIndex > 1)
                    text += $" (step {stepIndex})";
                ImGui.Selectable($"{text}##{Guid.NewGuid()}", i == 0);
            }

            ImGui.EndListBox();
        }

        var contentHeight = ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 5 + style.FramePadding.Y * 2 + style.ItemSpacing.Y * 4;
        var macroContent = Service.MacroManager.CurrentMacroContent();
        if (ImGui.BeginListBox("##current-macro", new Vector2(-1, contentHeight)))
        {
            var stepIndex = Service.MacroManager.CurrentMacroStep();
            if (stepIndex == -1)
                ImGui.Selectable("Looping", true);
            else
            {
                for (var i = stepIndex; i < macroContent.Length; i++)
                {
                    var step = macroContent[i];
                    var isCurrentStep = i == stepIndex;
                    ImGui.Selectable(step, isCurrentStep);
                }
            }

            ImGui.EndListBox();
        }

        ImGui.PopItemWidth();
    }

    public static void DrawSelected()
    {
        using var child = ImRaii.Child("##Panel", -Vector2.One, true);
        if (!child || Selected == null) return;

        ImGui.TextUnformatted("Macro Editor");
        ImGui.TextUnformatted($"{Selected.Name}\n{Selected.Path}\n{Selected.RelativePath}\n{Selected.RelativePath.Replace(Path.DirectorySeparatorChar, '/')}");
        using var disabled = ImRaii.Disabled(Service.MacroManager.State == LoopState.Running);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
            Selected.Run();

        ImGui.SameLine();
        var lang = Selected.Language;
        if (ImGuiX.Enum("Language", ref lang))
            Selected.ChangeExtension(lang);

        if (Selected.Language == Language.Native)
        {
            var sb = new StringBuilder("Toggle CraftLoop");
            var craftLoopEnabled = Selected.CraftingLoop;

            if (craftLoopEnabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.HealerGreen);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.HealerGreen);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.ParsedGreen);

                sb.AppendLine(" (0=disabled, -1=infinite)");
                sb.AppendLine($"When enabled, your macro is modified as follows:");
                sb.AppendLine(
                    ActiveMacro.ModifyMacroForCraftLoop("[YourMacro]", true, Selected.CraftLoopCount)
                    .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                    .Select(line => $"- {line}")
                    .Aggregate(string.Empty, (s1, s2) => $"{s1}\n{s2}"));
            }

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Sync, sb.ToString()))
            {
                Selected.CraftingLoop ^= true;
                Service.Configuration.Save();
            }

            if (craftLoopEnabled)
                ImGui.PopStyleColor(3);

            if (Selected.CraftingLoop)
            {
                ImGui.SameLine();
                ImGui.PushItemWidth(50);

                var v_min = -1;
                var v_max = 999;
                var loops = Selected.CraftLoopCount;
                if (ImGui.InputInt("##CraftLoopCount", ref loops, 0) || MouseWheelInput(ref loops))
                {
                    if (loops < v_min)
                        loops = v_min;

                    if (loops > v_max)
                        loops = v_max;

                    Selected.CraftLoopCount = loops;
                    Service.Configuration.Save();
                }

                ImGui.PopItemWidth();
            }
        }

        ImGui.SameLine();
        using (var colour = ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.HealerGreen, Selected.UseInARPostProcess)
            .Push(ImGuiCol.ButtonHovered, ImGuiColors.HealerGreen, Selected.UseInARPostProcess)
            .Push(ImGuiCol.ButtonActive, ImGuiColors.ParsedGreen, Selected.UseInARPostProcess))
            if (ImGuiX.IconButton(FontAwesomeIcon.Faucet, "use in ar post process"))
                Selected.SetAsPostProcessMacro(!Selected.UseInARPostProcess);

        ImGui.SameLine();
        var buttonSize = ImGuiHelpers.GetButtonSize(FontAwesomeIcon.FileImport.ToIconString());
        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X - ImGui.GetStyle().WindowPadding.X);
        if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import from clipboard"))
        {
            var text = Utils.ConvertClipboardToSafeString();

            //if (Utils.IsLuaCode(text))
            //    Selected.Language = Language.Lua;

            Selected.Write(text);
        }

        ImGui.SetNextItemWidth(-1);
        var useMono = !Service.Configuration.DisableMonospaced;
        using var font = ImRaii.PushFont(UiBuilder.MonoFont, useMono);

        if (Selected.Exists)
        {
            var contents = Selected.Contents;
            if (ImGui.InputTextMultiline($"##{Selected.Name}-editor", ref contents, 100_000, new Vector2(-1, -1)))
                Selected.Write(contents);
        }
    }

    private static bool MouseWheelInput(ref int iv)
    {
        if (ImGui.IsItemHovered())
        {
            var mouseDelta = (int)ImGui.GetIO().MouseWheel;  // -1, 0, 1
            if (mouseDelta != 0)
            {
                iv += mouseDelta;
                return true;
            }
        }

        return false;
    }
}
