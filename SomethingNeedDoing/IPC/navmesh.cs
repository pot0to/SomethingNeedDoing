using Dalamud.Plugin.Ipc;
using System.Numerics;

namespace SomethingNeedDoing.IPC;
internal class NavmeshIPC
{
    internal static readonly string PluginName = "vnavmesh";
    internal static ICallGateSubscriber<bool>? NavIsReady;
    internal static ICallGateSubscriber<float>? NavBuildProgress;
    internal static ICallGateSubscriber<object>? NavReload;
    internal static ICallGateSubscriber<object>? NavRebuild;
    internal static ICallGateSubscriber<bool>? NavIsAutoLoad;
    internal static ICallGateSubscriber<bool, object>? NavSetAutoLoad;
    internal static ICallGateSubscriber<Vector3, object>? PathMoveTo;
    internal static ICallGateSubscriber<Vector3, object>? PathFlyTo;
    internal static ICallGateSubscriber<object>? PathStop;
    internal static ICallGateSubscriber<bool>? PathIsRunning;
    internal static ICallGateSubscriber<int>? PathNumWaypoints;
    internal static ICallGateSubscriber<bool>? PathGetMovementAllowed;
    internal static ICallGateSubscriber<bool, object>? PathSetMovementAllowed;
    internal static ICallGateSubscriber<float>? PathGetTolerance;
    internal static ICallGateSubscriber<bool, object>? PathSetTolerance;

    internal static void Init()
    {
        NavIsReady = Service.Interface.GetIpcSubscriber<bool>($"{PluginName}.Nav.IsReady");
        NavBuildProgress = Service.Interface.GetIpcSubscriber<float>($"{PluginName}.Nav.BuildProgress");
        NavReload = Service.Interface.GetIpcSubscriber<object>($"{PluginName}.Nav.Reload");
        NavRebuild = Service.Interface.GetIpcSubscriber<object>($"{PluginName}.Nav.Rebuild");
        NavIsAutoLoad = Service.Interface.GetIpcSubscriber<bool>($"{PluginName}.Nav.IsAutoLoad");
        NavSetAutoLoad = Service.Interface.GetIpcSubscriber<bool, object>($"{PluginName}.Nav.SetAutoLoad");
        PathMoveTo = Service.Interface.GetIpcSubscriber<Vector3, object>($"{PluginName}.Path.MoveTo");
        PathFlyTo = Service.Interface.GetIpcSubscriber<Vector3, object>($"{PluginName}.Path.FlyTo");
        PathStop = Service.Interface.GetIpcSubscriber<object>($"{PluginName}.Path.Stop");
        PathIsRunning = Service.Interface.GetIpcSubscriber<bool>($"{PluginName}.Path.IsRunning");
        PathNumWaypoints = Service.Interface.GetIpcSubscriber<int>($"{PluginName}.Path.NumWaypoints");
        PathGetMovementAllowed = Service.Interface.GetIpcSubscriber<bool>($"{PluginName}.Path.GetMovementAllowed");
        PathSetMovementAllowed = Service.Interface.GetIpcSubscriber<bool, object>($"{PluginName}.Path.SetMovementAllowed");
        PathGetTolerance = Service.Interface.GetIpcSubscriber<float>($"{PluginName}.Path.GetTolerance");
        PathSetTolerance = Service.Interface.GetIpcSubscriber<bool, object>($"{PluginName}.Path.SetTolerance");
    }

    internal static void Dispose()
    {
        NavIsReady = null;
        NavBuildProgress = null;
        NavReload = null;
        NavRebuild = null;
        NavIsAutoLoad = null;
        NavSetAutoLoad = null;
        PathMoveTo = null;
        PathFlyTo = null;
        PathStop = null;
        PathIsRunning = null;
        PathNumWaypoints = null;
        PathGetMovementAllowed = null;
        PathSetMovementAllowed = null;
        PathGetTolerance = null;
        PathSetTolerance = null;
    }
}
