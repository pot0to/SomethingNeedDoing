using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;

namespace SomethingNeedDoing.IPC;

#nullable disable
public class ARDiscard
{
    public ARDiscard() => EzIPC.Init(this, "ARDiscard");

    [EzIPC] public readonly Func<IReadOnlySet<uint>> GetItemsToDiscard;
}
