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
            MacroFileSystem = new(this);
        }
        catch (Exception ex)
        {
            ex.Log();
        }
    }

    public void Dispose() => Safe(() => MacroFileSystem?.Save());
}
