using Dalamud.Plugin.Ipc;

namespace SomethingNeedDoing.IPC;

internal static class VislandIPC
{
    private const string IsRouteRunningStr = "visland.IsRouteRunning";

    internal static ICallGateSubscriber<bool>? IsRouteRunning;

    internal static void Init()
    {
        IsRouteRunning = Service.Interface.GetIpcSubscriber<bool>(IsRouteRunningStr);
    }

    internal static void Dispose()
    {
        IsRouteRunning = null;
    }
}