using Dalamud.Plugin.Ipc;

namespace SomethingNeedDoing.IPC;

internal static class VislandIPC
{
    private const string IsRouteRunningStr = "visland.IsRouteRunning";

    internal static ICallGateSubscriber<bool>? IsRouteRunning;

    internal static void Init() => IsRouteRunning = Svc.PluginInterface.GetIpcSubscriber<bool>(IsRouteRunningStr);
}