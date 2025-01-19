using OtterGui.Log;
using System;

namespace SomethingNeedDoing.Macros;
#nullable disable
public sealed class OtterGuiHandler : IDisposable
{
    public MacroFileSystem MacroFileSystem;
    public Logger Logger;
    public OtterGuiHandler()
    {
        try
        {
            Logger = new();
            if (C.UseMacroFileSystem)
                CreateMacroFileSystem();
        }
        catch (Exception ex)
        {
            ex.Log();
        }
    }

    public void CreateMacroFileSystem() => MacroFileSystem = new(this);
    public void Dispose() => Safe(() => MacroFileSystem?.Save());
}
