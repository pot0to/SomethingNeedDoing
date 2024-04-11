using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SomethingNeedDoing.Misc.Commands;

public class AddonCommands
{
    internal static AddonCommands Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public unsafe void OpenRouletteDuty(byte contentRouletteID) => AgentContentsFinder.Instance()->OpenRouletteDuty(contentRouletteID);
    public unsafe void OpenRegularDuty(uint cfcID) => AgentContentsFinder.Instance()->OpenRegularDuty(cfcID);
    public void SetDFLanguageJ(bool state) => Svc.GameConfig.UiConfig.Set("ContentsFinderUseLangTypeJA", state);
    public void SetDFLanguageE(bool state) => Svc.GameConfig.UiConfig.Set("ContentsFinderUseLangTypeEN", state);
    public void SetDFLanguageD(bool state) => Svc.GameConfig.UiConfig.Set("ContentsFinderUseLangTypeDE", state);
    public void SetDFLanguageF(bool state) => Svc.GameConfig.UiConfig.Set("ContentsFinderUseLangTypeFR", state);
    public void SetDFJoinInProgress(bool state) => Svc.GameConfig.UiConfig.Set("ContentsFinderSupplyEnable", state);
    public unsafe void SetDFUnrestricted(bool state) => ContentsFinder.Instance()->IsUnrestrictedParty = state;
    public unsafe void SetDFLevelSync(bool state) => ContentsFinder.Instance()->IsLevelSync = state;
    public unsafe void SetDFMinILvl(bool state) => ContentsFinder.Instance()->IsMinimalIL = state;
    public unsafe void SetDFSilenceEcho(bool state) => ContentsFinder.Instance()->IsSilenceEcho = state;
    public unsafe void SetDFExplorerMode(bool state) => ContentsFinder.Instance()->IsExplorerMode = state;
    public unsafe void SetDFLimitedLeveling(bool state) => ContentsFinder.Instance()->IsLimitedLevelingRoulette = state;

    public unsafe int GetDiademAetherGaugeBarCount() => GenericHelpers.TryGetAddonByName<AtkUnitBase>("HWDAetherGauge", out var addon) ? addon->AtkValues[1].Int / 200 : 0;

    public unsafe int GetDDPassageProgress()
    {
        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("DeepDungeonMap", out var addon))
        {
            var key = addon->GetNodeById(7)->ChildNode->PrevSiblingNode;
            var image = key->GetAsAtkComponentNode()->Component->UldManager.NodeList[1]->GetAsAtkImageNode();
            return image->PartId * 10;
        }

        return 0;
    }

    public unsafe bool IsAddonVisible(string addonName)
    {
        var ptr = Service.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            return false;

        var addon = (AtkUnitBase*)ptr;
        return addon->IsVisible;
    }

    public unsafe bool IsNodeVisible(string addonName, int node, int child1 = -1, int child2 = -1)
    {
        var ptr = Service.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            return false;

        var addon = (AtkUnitBase*)ptr;

        return child2 != -1
            ? addon->UldManager.NodeList[node]->ChildNode[child1].ChildNode[child2].IsVisible
            : child1 != -1
                ? addon->UldManager.NodeList[node]->ChildNode[child1].IsVisible
                : addon->UldManager.NodeList[node]->IsVisible;
    }

    public unsafe bool IsAddonReady(string addonName)
    {
        var ptr = Service.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            return false;

        var addon = (AtkUnitBase*)ptr;
        return addon->UldManager.LoadedState == AtkLoadState.Loaded;
    }

    public unsafe string GetToastNodeText(int index, params int[] nodeNumbers)
    {
        var ptr = (AtkUnitBase*)Service.GameGui.GetAddonByName("_WideText", index);
        if (ptr == null) return string.Empty;
        if (ptr->UldManager.NodeList == null || ptr->UldManager.NodeListCount < 4) return string.Empty;

        var uld = ptr->UldManager;
        AtkResNode* node = null;
        var debugString = string.Empty;

        for (var i = 0; i < nodeNumbers.Length; i++)
        {
            var nodeNumber = nodeNumbers[i];

            var count = uld.NodeListCount;
            if (nodeNumber < 0 || nodeNumber >= count)
                throw new MacroCommandError($"Addon node number must be between 0 and {count} for the _WideText addon");

            node = uld.NodeList[nodeNumber];
            debugString += $"[{nodeNumber}]";

            if (node == null)
                throw new MacroCommandError($"_WideText addon node{debugString} is null");

            // More nodes to traverse
            if (i < nodeNumbers.Length - 1)
            {
                if ((int)node->Type < 1000)
                    throw new MacroCommandError($"_WideText addon node{debugString} is not a component");

                uld = ((AtkComponentNode*)node)->Component->UldManager;
            }
        }

        var textNode = (AtkTextNode*)node;
        return textNode->NodeText.ToString();
    }

    public unsafe string GetNodeText(string addonName, params int[] nodeNumbers)
    {
        if (nodeNumbers.Length == 0)
            throw new MacroCommandError("At least one node number is required");

        var ptr = Service.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            throw new MacroCommandError($"Could not find {addonName} addon");

        var addon = (AtkUnitBase*)ptr;
        var uld = addon->UldManager;

        AtkResNode* node = null;
        var debugString = string.Empty;
        for (var i = 0; i < nodeNumbers.Length; i++)
        {
            var nodeNumber = nodeNumbers[i];

            var count = uld.NodeListCount;
            if (nodeNumber < 0 || nodeNumber >= count)
                throw new MacroCommandError($"Addon node number must be between 0 and {count} for the {addonName} addon");

            node = uld.NodeList[nodeNumber];
            debugString += $"[{nodeNumber}]";

            if (node == null)
                throw new MacroCommandError($"{addonName} addon node {debugString} is null");

            // More nodes to traverse
            if (i < nodeNumbers.Length - 1)
            {
                if ((int)node->Type < 1000)
                    throw new MacroCommandError($"{addonName} addon node {debugString} is not a component");

                uld = ((AtkComponentNode*)node)->Component->UldManager;
            }
        }

        if (node->Type is not NodeType.Text or NodeType.Counter)
            throw new MacroCommandError($"{addonName} addon node {debugString} is not a text node");

        if (node->Type == NodeType.Counter)
            return ((AtkCounterNode*)node)->NodeText.ToString();

        var textNode = (AtkTextNode*)node;
        return textNode->NodeText.ToString();
    }

    public unsafe string GetSelectStringText(int index)
    {
        var ptr = Service.GameGui.GetAddonByName("SelectString", 1);
        if (ptr == nint.Zero)
            throw new MacroCommandError("Could not find SelectString addon");

        var addon = (AddonSelectString*)ptr;
        var popup = &addon->PopupMenu.PopupMenu;

        var count = popup->EntryCount;
        Service.Log.Debug($"index={index} // Count={count} // {index < 0 || index > count}");
        if (index < 0 || index > count)
            throw new MacroCommandError("Index out of range");

        var textPtr = popup->EntryNames[index];
        return textPtr == null
            ? throw new MacroCommandError("Text pointer was null")
            : Marshal.PtrToStringUTF8((nint)textPtr) ?? string.Empty;
    }

    public unsafe string GetSelectIconStringText(int index)
    {
        var ptr = Service.GameGui.GetAddonByName("SelectIconString", 1);
        if (ptr == nint.Zero)
            throw new MacroCommandError("Could not find SelectIconString addon");

        var addon = (AddonSelectIconString*)ptr;
        var popup = &addon->PopupMenu.PopupMenu;

        var count = popup->EntryCount;
        if (index < 0 || index > count)
            throw new MacroCommandError("Index out of range");

        var textPtr = popup->EntryNames[index];
        return textPtr == null
            ? throw new MacroCommandError("Text pointer was null")
            : Marshal.PtrToStringUTF8((nint)textPtr) ?? string.Empty;
    }

    public unsafe int GetNodeListCount(string addonName) => GenericHelpers.TryGetAddonByName<AtkUnitBase>(addonName, out var addon) ? addon->UldManager.NodeListCount : 0;
}
