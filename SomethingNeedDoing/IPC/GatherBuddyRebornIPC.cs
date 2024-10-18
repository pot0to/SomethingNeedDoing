using Dalamud.Plugin.Ipc;

namespace SomethingNeedDoing.IPC;

internal static class GatherBuddyRebornIPC
{
    private const string GBRVersionStr = "GatherBuddyReborn.Version";
    private const string IsGBRAutoGatherEnabledStr = "GatherBuddyReborn.AutoGatherEnabled";

    internal static ICallGateSubscriber<int>? GBRVersion;
    internal static ICallGateSubscriber<bool>? IsGBRAutoGatherEnabled;

    internal static void Init()
    {
        GBRVersion = Svc.PluginInterface.GetIpcSubscriber<int>(GBRVersionStr);
        IsGBRAutoGatherEnabled = Svc.PluginInterface.GetIpcSubscriber<bool>(IsGBRAutoGatherEnabledStr);
    }
}
