using AutoRetainerAPI;
using SomethingNeedDoing.IPC;
using SomethingNeedDoing.Misc;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace SomethingNeedDoing.Macros.Lua;

public class Ipc
{
    internal static Ipc Instance { get; } = new();
    private readonly Dropbox dropbox;
    private readonly LifestreamIPC lifestream;
    private readonly Questionable questionable;
    private readonly RSR rsr;
    private readonly Artisan artisan;
    private readonly AutoRetainer autoretainer;
    private readonly ARDiscard ardiscard;
    private readonly VislandIPC visland;

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    private readonly AutoRetainerApi _autoRetainerApi;

    internal Ipc()
    {
        _autoRetainerApi = new();
        NavmeshIPC.Init();
        DeliverooIPC.Init();
        PandorasBoxIPC.Init();
        lifestream = new();
        dropbox = new();
        questionable = new Questionable();
        rsr = new();
        artisan = new();
        autoretainer = new();
        ardiscard = new();
        visland = new();
        AutoDutyIPC.Init();
    }

    internal void Dispose()
    {
        _autoRetainerApi.Dispose();
    }

    #region PandorasBox
    public bool? PandoraGetFeatureEnabled(string feature) => PandorasBoxIPC.GetFeatureEnabled?.InvokeFunc(feature);
    public bool? PandoraGetFeatureConfigEnabled(string feature, string config) => PandorasBoxIPC.GetConfigEnabled?.InvokeFunc(feature, config);
    public void PandoraSetFeatureState(string feature, bool state) => PandorasBoxIPC.SetFeatureEnabled?.InvokeAction(feature, state);
    public void PandoraSetFeatureConfigState(string feature, string config, bool state) => PandorasBoxIPC.SetConfigEnabled?.InvokeAction(feature, config, state);
    public void PandoraPauseFeature(string feature, int ms) => PandorasBoxIPC.PauseFeature?.InvokeAction(feature, ms);
    #endregion

    #region AutoHook
    public unsafe void SetAutoHookState(bool state) => AutoHookIPC.SetPluginState(state);
    public unsafe void SetAutoHookAutoGigState(bool state) => AutoHookIPC.SetAutoGigState(state);
    public unsafe void SetAutoHookAutoGigSize(int size) => AutoHookIPC.SetAutoGigSize(size);
    public unsafe void SetAutoHookAutoGigSpeed(int speed) => AutoHookIPC.SetAutoGigSpeed(speed);
    public unsafe void SetAutoHookPreset(string preset) => AutoHookIPC.SetPreset(preset);
    public unsafe void UseAutoHookAnonymousPreset(string preset) => AutoHookIPC.CreateAndSelectAnonymousPreset(preset);
    public unsafe void DeletedSelectedAutoHookPreset() => AutoHookIPC.DeleteSelectedPreset();
    public unsafe void DeleteAllAutoHookAnonymousPresets() => AutoHookIPC.DeleteAllAnonymousPresets();
    #endregion

    #region Deliveroo
    public bool DeliverooIsTurnInRunning() => DeliverooIPC.IsTurnInRunning!.InvokeFunc();
    #endregion

    #region Questionable
    public bool QuestionableIsRunning() => questionable.IsRunning();
    public string QuestionableGetCurrentQuestId() => questionable.GetCurrentQuestId();
    #endregion

    #region visland
    public bool IsVislandRouteRunning() => visland.IsRouteRunning();
    public bool VislandIsRoutePaused() => visland.IsRoutePaused();
    public void VislandSetRoutePaused(bool state) => visland.SetRoutePaused(state);
    public void VislandStopRoute() => visland.StopRoute();
    public void VislandStartRoute(string route, bool once) => visland.StartRoute(route, once);
    #endregion

    #region navmesh
    public bool NavIsReady() => NavmeshIPC.NavIsReady();
    public float NavBuildProgress() => NavmeshIPC.NavBuildProgress();
    public void NavReload() => NavmeshIPC.NavReload();
    public void NavRebuild() => NavmeshIPC.NavRebuild();
    public void NavPathfind(float x, float y, float z, bool fly = false) => NavmeshIPC.NavPathfind(Svc.ClientState.LocalPlayer!.Position, new Vector3(x, y, z), fly);
    public bool NavIsAutoLoad() => NavmeshIPC.NavIsAutoLoad();
    public void NavSetAutoLoad(bool state) => NavmeshIPC.NavSetAutoLoad(state);
    public float? QueryMeshNearestPointX(float x, float y, float z, float halfExtentXZ, float halfExtentY) => NavmeshIPC.QueryMeshNearestPoint(new Vector3(x, y, z), halfExtentXZ, halfExtentY)?.X;
    public float? QueryMeshNearestPointY(float x, float y, float z, float halfExtentXZ, float halfExtentY) => NavmeshIPC.QueryMeshNearestPoint(new Vector3(x, y, z), halfExtentXZ, halfExtentY)?.Y;
    public float? QueryMeshNearestPointZ(float x, float y, float z, float halfExtentXZ, float halfExtentY) => NavmeshIPC.QueryMeshNearestPoint(new Vector3(x, y, z), halfExtentXZ, halfExtentY)?.Z;
    public float? QueryMeshPointOnFloorX(float x, float y, float z, bool allowUnlandable, float halfExtentXZ) => NavmeshIPC.QueryMeshPointOnFloor(new Vector3(x, y, z), allowUnlandable, halfExtentXZ)?.X;
    public float? QueryMeshPointOnFloorY(float x, float y, float z, bool allowUnlandable, float halfExtentXZ) => NavmeshIPC.QueryMeshPointOnFloor(new Vector3(x, y, z), allowUnlandable, halfExtentXZ)?.Y;
    public float? QueryMeshPointOnFloorZ(float x, float y, float z, bool allowUnlandable, float halfExtentXZ) => NavmeshIPC.QueryMeshPointOnFloor(new Vector3(x, y, z), allowUnlandable, halfExtentXZ)?.Z;
    public void PathMoveTo(float x, float y, float z, bool fly = false) => NavmeshIPC.PathMoveTo([new Vector3(x, y, z)], fly);
    public void PathStop() => NavmeshIPC.PathStop();
    public bool PathIsRunning() => NavmeshIPC.PathIsRunning();
    public int PathNumWaypoints() => NavmeshIPC.PathNumWaypoints();
    public bool PathGetMovementAllowed() => NavmeshIPC.PathGetMovementAllowed();
    public void PathSetMovementAllowed(bool state) => NavmeshIPC.PathSetMovementAllowed(state);
    public bool PathGetAlignCamera() => NavmeshIPC.PathGetAlignCamera();
    public void PathSetAlignCamera(bool state) => NavmeshIPC.PathSetAlignCamera(state);
    public float PathGetTolerance() => NavmeshIPC.PathGetTolerance();
    public void PathSetTolerance(float t) => NavmeshIPC.PathSetTolerance(t);
    public void PathfindAndMoveTo(float x, float y, float z, bool fly = false) => NavmeshIPC.PathfindAndMoveTo(new Vector3(x, y, z), fly);
    public bool PathfindInProgress() => NavmeshIPC.PathfindInProgress();
    #endregion

    #region AutoRetainer
    public unsafe void ARSetSuppressed(bool state) => _autoRetainerApi.Suppressed = state;

    public unsafe List<string> ARGetRegisteredCharacters()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Select(c => $"{_autoRetainerApi.GetOfflineCharacterData(c).Name}@{_autoRetainerApi.GetOfflineCharacterData(c).World}").ToList();

    public unsafe List<string> ARGetRegisteredEnabledCharacters()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled)
        .Select(c => $"{_autoRetainerApi.GetOfflineCharacterData(c).Name}@{_autoRetainerApi.GetOfflineCharacterData(c).World}").ToList();

    public unsafe List<string> ARGetRegisteredRetainers()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Select(c => _autoRetainerApi.GetOfflineCharacterData(c).RetainerData.Select(r => r.Name)).SelectMany(names => names).ToList();

    public unsafe List<string> ARGetRegisteredEnabledRetainers()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled)
        .Select(c => _autoRetainerApi.GetOfflineCharacterData(c).RetainerData
        .Where(r => r.HasVenture).Select(r => r.Name)).SelectMany(names => names).ToList();

    public unsafe bool ARAnyWaitingToBeProcessed(bool allCharacters = false) => ARRetainersWaitingToBeProcessed(allCharacters) || ARSubsWaitingToBeProcessed(allCharacters);

    public unsafe bool ARRetainersWaitingToBeProcessed(bool allCharacters = false)
    {
        return !allCharacters
            ? _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).RetainerData.AsParallel().Any(x => x.HasVenture && x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp())
            : GetAllEnabledCharacters().Any(character => _autoRetainerApi.GetOfflineCharacterData(character).RetainerData.Any(x => x.HasVenture && x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp()));
    }

    public unsafe bool ARSubsWaitingToBeProcessed(bool allCharacters = false)
    {
        return !allCharacters
            ? _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).OfflineSubmarineData.AsParallel().Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp())
            : GetAllEnabledCharacters().Any(c => _autoRetainerApi.GetOfflineCharacterData(c).OfflineSubmarineData.Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp()));
    }

    public void ARFinishCharacterPostProcess() => _autoRetainerApi.FinishCharacterPostProcess();
    public List<ulong> ARGetCharacterCIDs() => _autoRetainerApi.GetRegisteredCharacters();
    public AutoRetainerAPI.Configuration.OfflineCharacterData ARGetCharacterData(ulong cid) => _autoRetainerApi.GetOfflineCharacterData(cid);

    public bool ARGetMultiModeEnabled() => autoretainer.GetMultiModeEnabled();
    public void ARSetMultiModeEnabled(bool value) => autoretainer.SetMultiModeEnabled(value);
    public bool ARIsBusy() => autoretainer.IsBusy();
    public int ARGetInventoryFreeSlotCount() => autoretainer.GetInventoryFreeSlotCount();
    public Dictionary<ulong, HashSet<string>> ARGetEnabledRetainers() => autoretainer.GetEnabledRetainers();
    public bool ARAreAnyRetainersAvailableForCurrentChara() => autoretainer.AreAnyRetainersAvailableForCurrentChara();
    public void ARAbortAllTasks() => autoretainer.AbortAllTasks();
    public void ARDisableAllFunctions() => autoretainer.DisableAllFunctions();
    public void AREnableMultiMode() => autoretainer.EnableMultiMode();
    public void AREnqueueHET() => autoretainer.EnqueueHET(() => { });
    public bool ARCanAutoLogin() => autoretainer.CanAutoLogin();
    public bool ARRelog(string charaNameWithWorld) => autoretainer.Relog(charaNameWithWorld);
    public bool ARGetOptionRetainerSense() => autoretainer.GetOptionRetainerSense();
    public void ARSetOptionRetainerSense(bool value) => autoretainer.SetOptionRetainerSense(value);
    public int ARGetOptionRetainerSenseThreshold() => autoretainer.GetOptionRetainerSenseThreshold();
    public void ARSetOptionRetainerSenseThreshold(int value) => autoretainer.SetOptionRetainerSenseThreshold(value);
    public long? ARGetClosestRetainerVentureSecondsRemaining(ulong cid) => autoretainer.GetClosestRetainerVentureSecondsRemaining(cid);
    public void AREnqueueInitiation() => autoretainer.EnqueueInitiation();
    public (uint ShopDataID, uint ExchangeDataID, Vector3 Position)? ARGetGCInfo() => autoretainer.GetGCInfo();

    private unsafe ParallelQuery<ulong> GetAllEnabledCharacters() => _autoRetainerApi.GetRegisteredCharacters().AsParallel().Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled);
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

    #region Dropbox
    public void DropboxStart() => dropbox.BeginTradingQueue();
    public void DropboxStop() => dropbox.Stop();
    public bool DropboxIsBusy() => dropbox.IsBusy();
    public int DropboxGetItemQuantity(uint id, bool hq) => dropbox.GetItemQuantity(id, hq);
    public void DropboxSetItemQuantity(uint id, bool hq, int quantity) => dropbox.SetItemQuantity(id, hq, quantity);
    #endregion

    #region Lifestream
    public bool LifestreamAethernetTeleport(string aethernetName) => lifestream.AethernetTeleport(aethernetName);
    public bool LifestreamTeleport(uint destination, byte subIndex) => lifestream.Teleport(destination, subIndex);
    public bool LifestreamTeleportToHome() => lifestream.TeleportToHome();
    public bool LifestreamTeleportToFC() => lifestream.TeleportToFC();
    public bool LifestreamTeleportToApartment() => lifestream.TeleportToApartment();
    public bool LifestreamIsBusy() => lifestream.IsBusy();
    public void LifestreamExecuteCommand(string command) => lifestream.ExecuteCommand(command);
    public void LifestreamAbort() => lifestream.Abort();
    #endregion

    #region RSR
    public void RSRAddPriorityNameID(uint nameId) => rsr.AddPriorityNameID(nameId);
    public void RSRRemovePriorityNameID(uint nameId) => rsr.RemovePriorityNameID(nameId);
    public void RSRAddBlacklistNameID(uint nameId) => rsr.AddBlacklistNameID(nameId);
    public void RSRRemoveBlacklistNameID(uint nameId) => rsr.RemoveBlacklistNameID(nameId);
    public void RSRChangeOperatingMode(byte stateCommand) => rsr.ChangeOperatingMode((RSR.StateCommandType)stateCommand);
    public void RSRTriggerSpecialState(byte specialCommand) => rsr.TriggerSpecialState((RSR.SpecialCommandType)specialCommand);
    #endregion

    #region Artisan
    public bool ArtisanGetEnduranceStatus() => artisan.GetEnduranceStatus();
    public void ArtisanSetEnduranceStatus(bool state) => artisan.SetEnduranceStatus(state);
    public bool ArtisanIsListRunning() => artisan.IsListRunning();
    public bool ArtisanIsListPaused() => artisan.IsListPaused();
    public void ArtisanSetListPause(bool state) => artisan.SetListPause(state);
    public bool ArtisanGetStopRequest() => artisan.GetStopRequest();
    public void ArtisanSetStopRequest(bool state) => artisan.SetStopRequest(state);
    public void ArtisanCraftItem(ushort recipeID, int amount) => artisan.CraftItem(recipeID, amount);
    #endregion

    #region ARDiscard
    public List<uint> ARDiscardGetItemsToDiscard() => [.. ardiscard.GetItemsToDiscard()];
    #endregion

    #region AutoDuty
    public void ADListConfig() => AutoDutyIPC.ListConfig?.InvokeAction();
    public string? ADGetConfig(string config) => AutoDutyIPC.GetConfig?.InvokeFunc(config);
    public void ADSetConfig(string config, string setting) => AutoDutyIPC.SetConfig?.InvokeAction(config, setting);
    public void ADRun(uint territoryType, int loops = 0, bool bareMode = false) => AutoDutyIPC.Run?.InvokeAction(territoryType, loops, bareMode);
    public void ADStart(bool startFromZero = true) => AutoDutyIPC.Start?.InvokeAction(startFromZero);
    public void ADStop() => AutoDutyIPC.Stop?.InvokeAction();
    public bool? ADIsNavigating() => AutoDutyIPC.IsNavigating?.InvokeFunc();
    public bool? ADIsLooping() => AutoDutyIPC.IsLooping?.InvokeFunc();
    public bool? ADIsStopped() => AutoDutyIPC.IsStopped?.InvokeFunc();
    public bool? ADContentHasPath(uint territoryType) => AutoDutyIPC.ContentHasPath?.InvokeFunc(territoryType);
    #endregion
}
