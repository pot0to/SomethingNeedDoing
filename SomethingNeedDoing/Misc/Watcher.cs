using Dalamud.Memory;
using ECommons.EzHookManager;
using System;

namespace SomethingNeedDoing.Misc;
public class Watcher
{
    public Watcher() => EzSignatureHelper.Initialize(this);

    private delegate nint InitZoneDelegate(nint a1, int a2, nint a3);

    [EzHook("E8 ?? ?? ?? ?? 45 33 C0 48 8D 53 10 8B CE E8 ?? ?? ?? ?? 48 8D 4B 64")]
    private readonly EzHook<InitZoneDelegate> InitZoneHook = null!;

    private nint InitZoneDetour(nint a1, int a2, nint a3)
    {
        try
        {
            var serverId = MemoryHelper.Read<ushort>(a3);
            var zoneId = MemoryHelper.Read<ushort>(a3 + 2);
            WatchedValues.InstanceServerID = serverId;
            WatchedValues.InstanceZoneID = zoneId;
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Something went wrong with {nameof(InitZoneDetour)}, {ex}");
        }

        return InitZoneHook.Original(a1, a2, a3);
    }
}
