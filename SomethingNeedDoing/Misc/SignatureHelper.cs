namespace SomethingNeedDoing.Misc;

public static class SignatureHelper
{
    public static void Initialise(object which, bool log = false) => Service.Hook.InitializeFromAttributes(which);
}