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
        Svc.ClientState.Login += OnLogin;

        // If we're already logged in, toggle the waiter.
        if (Svc.ClientState.LocalPlayer != null)
            loggedInWaiter.Set();

        // Start the loop.
        Task.Factory.StartNew(EventLoop, TaskCreationOptions.LongRunning);
    }

    public LoopState State { get; private set; } = LoopState.Waiting;

    public bool PauseAtLoop { get; private set; } = false;

    public bool StopAtLoop { get; private set; } = false;

    public void Dispose()
    {
        Svc.ClientState.Login -= OnLogin;

        eventLoopTokenSource.Cancel();
        eventLoopTokenSource.Dispose();

        loggedInWaiter.Dispose();
        pausedWaiter.Dispose();
    }

    private void OnLogin()
    {
        loggedInWaiter.Set();
        State = LoopState.Waiting;
    }

    private async void EventLoop()
    {
        var token = eventLoopTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            try
            {
                // Check if the logged in waiter is set
                if (!loggedInWaiter.WaitOne(0))
                {
                    State = LoopState.NotLoggedIn;
                    macroStack.Clear();
                }

                // Wait to be logged in
                loggedInWaiter.WaitOne();

                // Check if the paused waiter has been set
                if (!pausedWaiter.WaitOne(0))
                {
                    State = macroStack.Count == 0
                        ? LoopState.Waiting
                        : LoopState.Paused;
                }

                // Wait for the un-pause button
                pausedWaiter.WaitOne();

                // Grab from the stack, or go back to being paused
                if (!macroStack.TryPeek(out var macro))
                {
                    pausedWaiter.Reset();
                    continue;
                }

                State = LoopState.Running;
                if (await ProcessMacro(macro, token))
                {
                    macroStack.Pop().Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                Svc.Log.Verbose("Event loop has been cancelled");
                State = LoopState.Stopped;
                break;
            }
            catch (ObjectDisposedException)
            {
                Svc.Log.Verbose("Event loop has been disposed");
                State = LoopState.Stopped;
                break;
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "Unhandled exception occurred");
                Service.ChatManager.PrintError("Peon has died unexpectedly.");
                macroStack.Clear();
                PlayErrorSound();
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
            pausedWaiter.Reset();
            PlayErrorSound();
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
                return await ProcessMacro(macro, token, attempt);
            }
            else
            {
                Service.ChatManager.PrintError(message);
                pausedWaiter.Reset();
                PlayErrorSound();
                return false;
            }
        }
        catch (LuaScriptException ex)
        {
            Service.ChatManager.PrintError($"Failure while running script: {ex.Message}");
            pausedWaiter.Reset();
            PlayErrorSound();
            return false;
        }
        catch (MacroCommandError ex)
        {
            Service.ChatManager.PrintError($"Failure while running {step} (step {macro.StepIndex + 1}): {ex.Message}");
            pausedWaiter.Reset();
            PlayErrorSound();
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
        => macroStack
            .ToArray() // Collection was modified after the enumerator was instantiated.
            .Select(macro => (macro.Node.Name, macro.StepIndex + 1))
            .ToArray();

    public void EnqueueMacro(MacroNode node)
    {
        macroStack.Push(new ActiveMacro(node));
        pausedWaiter.Set();
    }

    public void Pause(bool pauseAtLoop = false)
    {
        if (pauseAtLoop)
        {
            PauseAtLoop ^= true;
            StopAtLoop = false;
        }
        else
        {
            PauseAtLoop = false;
            StopAtLoop = false;
            pausedWaiter.Reset();
            Service.ChatManager.Clear();
        }
    }

    public void LoopCheckForPause()
    {
        if (PauseAtLoop)
            Pause(false);
    }

    public void Resume() => pausedWaiter.Set();

    public void Stop(bool stopAtLoop = false)
    {
        if (stopAtLoop)
        {
            PauseAtLoop = false;
            StopAtLoop ^= true;
        }
        else
        {
            PauseAtLoop = false;
            StopAtLoop = false;

            eventLoopTokenSource.TryReset();

            pausedWaiter.Set();
            macroStack.Clear();
            Service.ChatManager.Clear();
        }
    }

    public void LoopCheckForStop()
    {
        if (StopAtLoop)
            Stop(false);
    }

    public void NextStep()
    {
        if (macroStack.TryPeek(out var macro))
            macro.NextStep();
    }

    public string[] CurrentMacroContent() => macroStack.TryPeek(out var result) ? result.Steps.Select(s => s.ToString()).ToArray() : Array.Empty<string>();

    public int CurrentMacroStep() => macroStack.TryPeek(out var result) ? result.StepIndex : 0;
}
