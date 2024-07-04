using NLua.Exceptions;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Commands;
using SomethingNeedDoing.Misc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Managers;

internal partial class MacroManager : IDisposable
{
    private readonly Stack<ActiveMacro> macroStack = new();
    private CancellationTokenSource eventLoopTokenSource = new();
    private readonly ManualResetEvent loggedInWaiter = new(false);
    private readonly ManualResetEvent pausedWaiter = new(true);

    public MacroManager()
    {
        Svc.ClientState.Login += this.OnLogin;

        // If we're already logged in, toggle the waiter.
        if (Svc.ClientState.LocalPlayer != null)
            this.loggedInWaiter.Set();

        // Start the loop.
        Task.Factory.StartNew(this.EventLoop, TaskCreationOptions.LongRunning);
    }

    public LoopState State { get; private set; } = LoopState.Waiting;

    public bool PauseAtLoop { get; private set; } = false;

    public bool StopAtLoop { get; private set; } = false;

    public void Dispose()
    {
        Svc.ClientState.Login -= this.OnLogin;

        this.eventLoopTokenSource.Cancel();
        this.eventLoopTokenSource.Dispose();

        this.loggedInWaiter.Dispose();
        this.pausedWaiter.Dispose();
    }

    private void OnLogin()
    {
        this.loggedInWaiter.Set();
        this.State = LoopState.Waiting;
    }

    private async void EventLoop()
    {
        var token = this.eventLoopTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            try
            {
                // Check if the logged in waiter is set
                if (!this.loggedInWaiter.WaitOne(0))
                {
                    this.State = LoopState.NotLoggedIn;
                    this.macroStack.Clear();
                }

                // Wait to be logged in
                this.loggedInWaiter.WaitOne();

                // Check if the paused waiter has been set
                if (!this.pausedWaiter.WaitOne(0))
                {
                    this.State = this.macroStack.Count == 0
                        ? LoopState.Waiting
                        : LoopState.Paused;
                }

                // Wait for the un-pause button
                this.pausedWaiter.WaitOne();

                // Grab from the stack, or go back to being paused
                if (!this.macroStack.TryPeek(out var macro))
                {
                    this.pausedWaiter.Reset();
                    continue;
                }

                this.State = LoopState.Running;
                if (await this.ProcessMacro(macro, token))
                {
                    this.macroStack.Pop().Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                Svc.Log.Verbose("Event loop has been cancelled");
                this.State = LoopState.Stopped;
                break;
            }
            catch (ObjectDisposedException)
            {
                Svc.Log.Verbose("Event loop has been disposed");
                this.State = LoopState.Stopped;
                break;
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "Unhandled exception occurred");
                Service.ChatManager.PrintError("Peon has died unexpectedly.");
                this.macroStack.Clear();
                this.PlayErrorSound();
            }
        }
    }

    private async Task<bool> ProcessMacro(ActiveMacro macro, CancellationToken token, int attempt = 0)
    {
        MacroCommand? step = null;

        try
        {
            step = macro.GetCurrentStep();

            if (step == null)
                return true;

            await step.Execute(macro, token);
        }
        catch (GateComplete)
        {
            return true;
        }
        catch (MacroPause ex)
        {
            Service.ChatManager.PrintColor($"{ex.Message}", ex.Color);
            this.pausedWaiter.Reset();
            this.PlayErrorSound();
            return false;
        }
        catch (MacroActionTimeoutError ex)
        {
            var maxRetries = Service.Configuration.MaxTimeoutRetries;
            var message = $"Failure while running {step} (step {macro.StepIndex + 1}): {ex.Message}";
            if (attempt < maxRetries)
            {
                message += $", retrying ({attempt}/{maxRetries})";
                Service.ChatManager.PrintError(message);
                attempt++;
                return await this.ProcessMacro(macro, token, attempt);
            }
            else
            {
                Service.ChatManager.PrintError(message);
                this.pausedWaiter.Reset();
                this.PlayErrorSound();
                return false;
            }
        }
        catch (LuaScriptException ex)
        {
            Service.ChatManager.PrintError($"Failure while running script: {ex.Message}");
            this.pausedWaiter.Reset();
            this.PlayErrorSound();
            return false;
        }
        catch (MacroCommandError ex)
        {
            Service.ChatManager.PrintError($"Failure while running {step} (step {macro.StepIndex + 1}): {ex.Message}");
            this.pausedWaiter.Reset();
            this.PlayErrorSound();
            return false;
        }

        macro.NextStep();

        return false;
    }

    private void PlayErrorSound()
    {
        if (!Service.Configuration.NoisyErrors)
            return;

        var count = Service.Configuration.BeepCount;
        var frequency = Service.Configuration.BeepFrequency;
        var duration = Service.Configuration.BeepDuration;

        for (var i = 0; i < count; i++)
            Console.Beep(frequency, duration);
    }
}

internal sealed partial class MacroManager
{
    public (string Name, int StepIndex)[] MacroStatus
        => this.macroStack
            .ToArray() // Collection was modified after the enumerator was instantiated.
            .Select(macro => (macro.Node.Name, macro.StepIndex + 1))
            .ToArray();

    public void EnqueueMacro(MacroNode node)
    {
        this.macroStack.Push(new ActiveMacro(node));
        this.pausedWaiter.Set();
    }

    public void Pause(bool pauseAtLoop = false)
    {
        if (pauseAtLoop)
        {
            this.PauseAtLoop ^= true;
            this.StopAtLoop = false;
        }
        else
        {
            this.PauseAtLoop = false;
            this.StopAtLoop = false;
            this.pausedWaiter.Reset();
            Service.ChatManager.Clear();
        }
    }

    public void LoopCheckForPause()
    {
        if (this.PauseAtLoop)
            this.Pause(false);
    }

    public void Resume() => this.pausedWaiter.Set();

    public void Stop(bool stopAtLoop = false)
    {
        if (stopAtLoop)
        {
            this.PauseAtLoop = false;
            this.StopAtLoop ^= true;
        }
        else
        {
            this.PauseAtLoop = false;
            this.StopAtLoop = false;

            this.eventLoopTokenSource.TryReset();

            this.pausedWaiter.Set();
            this.macroStack.Clear();
            Service.ChatManager.Clear();
        }
    }

    public void LoopCheckForStop()
    {
        if (this.StopAtLoop)
            this.Stop(false);
    }

    public void NextStep()
    {
        if (this.macroStack.TryPeek(out var macro))
            macro.NextStep();
    }

    public string[] CurrentMacroContent() => this.macroStack.TryPeek(out var result) ? result.Steps.Select(s => s.ToString()).ToArray() : Array.Empty<string>();

    public int CurrentMacroStep() => this.macroStack.TryPeek(out var result) ? result.StepIndex : 0;
}
