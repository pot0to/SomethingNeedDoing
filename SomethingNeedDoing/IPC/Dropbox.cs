using ECommons.EzIpcManager;
using System;

namespace SomethingNeedDoing.IPC;

#nullable disable
public class Dropbox
{
    public static string Name = "Dropbox";
    public Dropbox() => EzIPC.Init(this, Name);

    //bool IsBusy();
    //void Stop();
    //void BeginTradingQueue();
    //int GetItemQuantity(uint id, bool hq);
    //void SetItemQuantity(uint id, bool hq, int quantity);

    [EzIPC] public readonly Func<bool> IsBusy;
    [EzIPC] public readonly Func<uint, bool, int> GetItemQuantity; // id, hq
    [EzIPC] public readonly Action<uint, bool, int> SetItemQuantity; // id, hq, quantity

    [EzIPC] public readonly Action BeginTradingQueue;
    [EzIPC] public readonly Action Stop;
}