using AutoRetainerAPI;
using ECommons;
using ECommons.DalamudServices;
using SomethingNeedDoing.IPC;
using System;
using System.Linq;

namespace SomethingNeedDoing.Misc;

/// <summary>
/// Miscellaneous functions that commands/scripts can use.
/// </summary>
public class IpcCommands
{
    private readonly AutoRetainerApi _autoRetainerApi;

    internal static IpcCommands Instance { get; } = new();

    internal IpcCommands()
    {
        _autoRetainerApi = new();
        DeliverooIPC.Init();
    }

    public void Dispose()
    {
        _autoRetainerApi.Dispose();
        DeliverooIPC.Dispose();
    }

    public unsafe bool DeliverooIsTurnInRunning() => DeliverooIPC.IsTurnInRunning!.InvokeFunc();

    public unsafe bool ARAnyWaitingToBeProcessed() =>
        _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).RetainerData.Any(x => x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp())
        || _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).OfflineSubmarineData.Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp());

    public unsafe bool ARRetainersWaitingToBeProcessed() =>
        _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).RetainerData.Any(x => x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp());
    public unsafe bool ARSubsWaitingToBeProcessed() =>
        _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).OfflineSubmarineData.Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp());
}
