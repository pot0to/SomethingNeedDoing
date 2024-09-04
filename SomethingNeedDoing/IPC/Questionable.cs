using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomethingNeedDoing.IPC;
#nullable disable
public class Questionable
{
    public const string Name = "Questionable";
    public Questionable() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

    [EzIPC] public Func<bool> IsRunning;
    [EzIPC] public Func<string> GetCurrentQuestId;
}
