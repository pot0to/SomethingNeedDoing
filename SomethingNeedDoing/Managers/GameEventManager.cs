using Dalamud.Game.ClientState.Conditions;
using System;
using System.Threading;

namespace SomethingNeedDoing.Managers;

internal class GameEventManager : IDisposable
{
    public GameEventManager() => Svc.Condition.ConditionChange += this.Condition_ConditionChange;

    /// <summary>
    /// Gets a waiter that is released when an action or crafting action is received through the Event Framework.
    /// </summary>
    public ManualResetEvent DataAvailableWaiter { get; } = new(false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Svc.Condition.ConditionChange -= this.Condition_ConditionChange;
        this.DataAvailableWaiter.Dispose();
    }

    private void Condition_ConditionChange(ConditionFlag flag, bool value)
    {
        if (flag == ConditionFlag.Crafting40 && !value)
            DataAvailableWaiter.Set();
    }
}
