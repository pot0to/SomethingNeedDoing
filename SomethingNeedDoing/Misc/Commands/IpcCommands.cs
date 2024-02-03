using AutoRetainerAPI;
using ECommons.DalamudServices;
using SomethingNeedDoing.IPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SomethingNeedDoing.Misc.Commands;

public class IpcCommands
{
    internal static IpcCommands Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (MethodInfo method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    private readonly AutoRetainerApi _autoRetainerApi;

    internal IpcCommands()
    {
        _autoRetainerApi = new();
        VislandIPC.Init();
        DeliverooIPC.Init();
        PandorasBoxIPC.Init();
    }

    public void Dispose()
    {
        _autoRetainerApi.Dispose();
        VislandIPC.Dispose();
        DeliverooIPC.Dispose();
        PandorasBoxIPC.Dispose();
    }

    #region PandorasBox
    public bool? PandoraGetFeatureEnabled(string feature) => PandorasBoxIPC.GetFeatureEnabled.InvokeFunc(feature);
    public bool? PandoraGetFeatureConfigEnabled(string feature, string config) => PandorasBoxIPC.GetConfigEnabled.InvokeFunc(feature, config);
    public void PandoraSetFeatureState(string feature, bool state) => PandorasBoxIPC.SetFeatureEnabled.InvokeFunc(feature, state);
    public void PandoraSetFeatureConfigState(string feature, string config, bool state) => PandorasBoxIPC.SetConfigEnabled.InvokeFunc(feature, config, state);
    public void PandoraPauseFeature(string feature, int ms) => PandorasBoxIPC.PauseFeature.InvokeFunc(feature, ms);
    #endregion

    #region AutoHook
    public unsafe void SetAutoHookState(bool state) => AutoHookIPC.SetPluginState(state);
    public unsafe void SetAutoHookAutoGigState(bool state) => AutoHookIPC.SetAutoGigState(state);
    public unsafe void SetAutoHookAutoGigSize(int size) => AutoHookIPC.SetAutoGigSize(size);
    public unsafe void SetAutoHookAutoGigSpeed(int speed) => AutoHookIPC.SetAutoGigSpeed(speed);
    public unsafe void SetAutoHookPreset(string preset) => AutoHookIPC.SetPreset(preset);
    public unsafe void UseAutoHookAnonymousPreset(string preset) => AutoHookIPC.CreateAndSelectAnonymousPreset(preset);
    public unsafe void DeletedSelectedAutoHookPreset() => AutoHookIPC.DeleteSelectedPreset();
    #endregion

    #region Deliveroo
    public unsafe bool DeliverooIsTurnInRunning() => DeliverooIPC.IsTurnInRunning!.InvokeFunc();
    #endregion

    #region visland
    public unsafe bool IsVislandRouteRunning() => VislandIPC.IsRouteRunning!.InvokeFunc();
    #endregion

    #region AutoRetainer
    public unsafe List<string> ARGetRegisteredCharacters() =>
        _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Select(c => $"{_autoRetainerApi.GetOfflineCharacterData(c).Name}@{_autoRetainerApi.GetOfflineCharacterData(c).World}").ToList();

    public unsafe List<string> ARGetRegisteredEnabledCharacters() =>
        _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled)
        .Select(c => $"{_autoRetainerApi.GetOfflineCharacterData(c).Name}@{_autoRetainerApi.GetOfflineCharacterData(c).World}").ToList();

    public unsafe bool ARAnyWaitingToBeProcessed(bool allCharacters = false) =>
        allCharacters ?
        ARRetainersWaitingToBeProcessed(allCharacters) || ARSubsWaitingToBeProcessed(allCharacters) :
        ARRetainersWaitingToBeProcessed() || ARSubsWaitingToBeProcessed();

    public unsafe bool ARRetainersWaitingToBeProcessed(bool allCharacters = false)
    {
        if (!allCharacters)
            return _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).RetainerData.AsParallel().Any(x => x.HasVenture && x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp());
        else
            return _autoRetainerApi.GetRegisteredCharacters().AsParallel().Any(character => _autoRetainerApi.GetOfflineCharacterData(character).RetainerData.Any(x => x.HasVenture && x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp()));
    }

    public unsafe bool ARSubsWaitingToBeProcessed(bool allCharacters = false)
    {
        if (!allCharacters)
            return _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).OfflineSubmarineData.AsParallel().Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp());
        else
            return _autoRetainerApi.GetRegisteredCharacters().AsParallel().Any(character => _autoRetainerApi.GetOfflineCharacterData(character).OfflineSubmarineData.Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp()));
    }
    #endregion

    #region YesAlready
    public void PauseYesAlready()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data) && !data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Disabling YesAlready");
            data.Add(nameof(SomethingNeedDoing));
        }
    }

    public void RestoreYesAlready()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data) && data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Restoring YesAlready");
            data.Remove(nameof(SomethingNeedDoing));
        }
    }
    #endregion

    #region TextAdvance
    internal static void PauseTextAdvance()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("TextAdvance.StopRequests", out var data) && !data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Disabling TextAdvance");
            data.Add(nameof(SomethingNeedDoing));
        }
    }

    internal static void RestoreTextAdvance()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("TextAdvance.StopRequests", out var data) && data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Restoring TextAdvance");
            data.Remove(nameof(SomethingNeedDoing));
        }
    }
    #endregion
}
