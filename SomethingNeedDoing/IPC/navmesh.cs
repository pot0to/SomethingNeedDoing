using Dalamud.Plugin.Ipc;
using ECommons;
using System;
using System.Numerics;

namespace SomethingNeedDoing.IPC;
internal class NavmeshIPC
{
    internal static readonly string Name = "vnavmesh";
    private static ICallGateSubscriber<bool>? _navIsReady;
    private static ICallGateSubscriber<float>? _navBuildProgress;
    private static ICallGateSubscriber<object>? _navReload;
    private static ICallGateSubscriber<object>? _navRebuild;
    private static ICallGateSubscriber<bool>? _navIsAutoLoad;
    private static ICallGateSubscriber<bool, object>? _navSetAutoLoad;

    private static ICallGateSubscriber<Vector3, float, Vector3?>? _queryMeshNearestPoint;

    private static ICallGateSubscriber<Vector3, object>? _pathMoveTo;
    private static ICallGateSubscriber<Vector3, object>? _pathFlyTo;
    private static ICallGateSubscriber<object>? _pathStop;
    private static ICallGateSubscriber<bool>? _pathIsRunning;
    private static ICallGateSubscriber<int>? _pathNumWaypoints;
    private static ICallGateSubscriber<bool>? _pathGetMovementAllowed;
    private static ICallGateSubscriber<bool, object>? _pathSetMovementAllowed;
    private static ICallGateSubscriber<bool>? _pathGetAlignCamera;
    private static ICallGateSubscriber<bool, object>? _pathSetAlignCamera;
    private static ICallGateSubscriber<float>? _pathGetTolerance;
    private static ICallGateSubscriber<float, object>? _pathSetTolerance;

    internal static void Init()
    {
        try
        {
            _navIsReady = Service.Interface.GetIpcSubscriber<bool>($"{Name}.Nav.IsReady");
            _navBuildProgress = Service.Interface.GetIpcSubscriber<float>($"{Name}.Nav.BuildProgress");
            _navReload = Service.Interface.GetIpcSubscriber<object>($"{Name}.Nav.Reload");
            _navRebuild = Service.Interface.GetIpcSubscriber<object>($"{Name}.Nav.Rebuild");
            _navIsAutoLoad = Service.Interface.GetIpcSubscriber<bool>($"{Name}.Nav.IsAutoLoad");
            _navSetAutoLoad = Service.Interface.GetIpcSubscriber<bool, object>($"{Name}.Nav.SetAutoLoad");

            _queryMeshNearestPoint = Service.Interface.GetIpcSubscriber<Vector3, float, Vector3?>($"{Name}.Query.Mesh.NearestPoint");

            _pathMoveTo = Service.Interface.GetIpcSubscriber<Vector3, object>($"{Name}.Path.MoveTo");
            _pathFlyTo = Service.Interface.GetIpcSubscriber<Vector3, object>($"{Name}.Path.FlyTo");
            _pathStop = Service.Interface.GetIpcSubscriber<object>($"{Name}.Path.Stop");
            _pathIsRunning = Service.Interface.GetIpcSubscriber<bool>($"{Name}.Path.IsRunning");
            _pathNumWaypoints = Service.Interface.GetIpcSubscriber<int>($"{Name}.Path.NumWaypoints");
            _pathGetMovementAllowed = Service.Interface.GetIpcSubscriber<bool>($"{Name}.Path.GetMovementAllowed");
            _pathSetMovementAllowed = Service.Interface.GetIpcSubscriber<bool, object>($"{Name}.Path.SetMovementAllowed");
            _pathGetAlignCamera = Service.Interface.GetIpcSubscriber<bool>($"{Name}.Path.GetAlignCamera");
            _pathSetAlignCamera = Service.Interface.GetIpcSubscriber<bool, object>($"{Name}.Path.SetAlignCamera");
            _pathGetTolerance = Service.Interface.GetIpcSubscriber<float>($"{Name}.Path.GetTolerance");
            _pathSetTolerance = Service.Interface.GetIpcSubscriber<float, object>($"{Name}.Path.SetTolerance");
        }
        catch (Exception ex) { ex.Log(); }
    }

    internal static T? Execute<T>(Func<T> func)
    {
        try
        {
            if (func != null)
                return func();
        }
        catch (Exception ex) { ex.Log(); }

        return default;
    }

    internal static void Execute<T>(Action<T> action, T param)
    {
        try
        {
            action?.Invoke(param);
        }
        catch (Exception ex) { ex.Log(); }
    }

    internal static void Execute(Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception ex) { ex.Log(); }
    }

    internal static bool NavIsReady() => Execute(() => _navIsReady!.InvokeFunc());
    internal static float NavBuildProgress() => Execute(() => _navBuildProgress!.InvokeFunc());
    internal static void NavReload() => Execute(_navReload!.InvokeAction);
    internal static void NavRebuild() => Execute(_navRebuild!.InvokeAction);
    internal static bool NavIsAutoLoad() => Execute(() => _navIsAutoLoad!.InvokeFunc());
    internal static void NavSetAutoLoad(bool value) => Execute(_navSetAutoLoad!.InvokeAction, value);

    internal static Vector3? QueryMeshNearestPoint(Vector3 pos, float maxDistance) => Execute(() => _queryMeshNearestPoint!.InvokeFunc(pos, maxDistance));

    internal static void PathMoveTo(Vector3 pos) => Execute(_pathMoveTo!.InvokeAction, pos);
    internal static void PathFlyTo(Vector3 pos) => Execute(_pathFlyTo!.InvokeAction, pos);
    internal static void PathStop() => Execute(_pathStop!.InvokeAction);
    internal static bool PathIsRunning() => Execute(() => _pathIsRunning!.InvokeFunc());
    internal static int PathNumWaypoints() => Execute(() => _pathNumWaypoints!.InvokeFunc());
    internal static bool PathGetMovementAllowed() => Execute(() => _pathGetMovementAllowed!.InvokeFunc());
    internal static void PathSetMovementAllowed(bool value) => Execute(_pathSetMovementAllowed!.InvokeAction, value);
    internal static bool PathGetAlignCamera() => Execute(() => _pathGetAlignCamera!.InvokeFunc());
    internal static void PathSetAlignCamera(bool value) => Execute(_pathSetAlignCamera!.InvokeAction, value);
    internal static float PathGetTolerance() => Execute(() => _pathGetTolerance!.InvokeFunc());
    internal static void PathSetTolerance(float tolerance) => Execute(_pathSetTolerance!.InvokeAction, tolerance);
}

