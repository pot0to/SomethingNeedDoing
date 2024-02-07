namespace SomethingNeedDoing.IPC;

internal class AutoHookIPC
{
    public static void SetPluginState(bool state) => Service.Interface.GetIpcSubscriber<bool, object>("AutoHook.SetPluginState").InvokeAction(state);

    public static void SetAutoGigState(bool state) => Service.Interface.GetIpcSubscriber<bool, object>("AutoHook.SetAutoGigState").InvokeAction(state);

    public static void SetAutoGigSize(int size) => Service.Interface.GetIpcSubscriber<int, object>("AutoHook.SetAutoGigSize").InvokeAction(size);

    public static void SetAutoGigSpeed(int speed) => Service.Interface.GetIpcSubscriber<int, object>("AutoHook.SetAutoGigSpeed").InvokeAction(speed);

    public static void SetPreset(string preset) => Service.Interface.GetIpcSubscriber<string, object>("AutoHook.SetPreset").InvokeAction(preset);

    public static void CreateAndSelectAnonymousPreset(string preset) => Service.Interface.GetIpcSubscriber<string, object>("AutoHook.CreateAndSelectAnonymousPreset").InvokeAction(preset);

    public static void DeleteSelectedPreset() => Service.Interface.GetIpcSubscriber<object>("AutoHook.DeleteSelectedPreset").InvokeAction();

    public static void DeleteAllAnonymousPresets() => Service.Interface.GetIpcSubscriber<object>("AutoHook.DeleteAllAnonymousPresets").InvokeAction();
}
