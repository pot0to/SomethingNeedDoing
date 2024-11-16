using AutoRetainerAPI;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.SimpleGui;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Managers;
using SomethingNeedDoing.Misc;
using SomethingNeedDoing.Misc.Commands;

namespace SomethingNeedDoing;

public sealed class SomethingNeedDoingPlugin : IDalamudPlugin
{
    public static string Name => "Something Need Doing (Expanded Edition)";
    public static string Prefix => "SND";
    private const string Command = "/somethingneeddoing";
    private static string[] Aliases => ["/pcraft", "/snd"];

    private readonly AutoRetainerApi _autoRetainerApi;

    public SomethingNeedDoingPlugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.Plugin = this;
        Service.Configuration = SomethingNeedDoingConfiguration.Load(pluginInterface.ConfigDirectory);

        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        Service.ChatManager = new ChatManager();
        Service.GameEventManager = new GameEventManager();
        Service.MacroManager = new MacroManager();

        EzConfigGui.Init(new MacroWindow());
        EzConfigGui.WindowSystem.AddWindow(new HelpWindow());
        EzConfigGui.WindowSystem.AddWindow(new ExcelWindow());
        MacroWindow.Setup();

        EzCmd.Add(Command, OnChatCommand, "Open a window to edit various settings.");
        Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{Command} Alias"));

        //_ = new Watcher();
        _autoRetainerApi = new();

        _autoRetainerApi.OnCharacterPostprocessStep += CheckCharacterPostProcess;
        _autoRetainerApi.OnCharacterReadyToPostProcess += DoCharacterPostProcess;
        Svc.Framework.Update += CheckForMacroCompletion;
    }

    private void CheckCharacterPostProcess()
    {
        if (Service.Configuration.ARCharacterPostProcessExcludedCharacters.Any(x => x == Svc.ClientState.LocalContentId))
            Svc.Log.Info("Skipping post process macro for current character.");
        else
            _autoRetainerApi.RequestCharacterPostprocess();
    }

    private bool RunningPostProcess;
    private void DoCharacterPostProcess()
    {
        if (Service.Configuration.ARCharacterPostProcessMacro != null)
        {
            RunningPostProcess = true;
            Service.MacroManager.EnqueueMacro(Service.Configuration.ARCharacterPostProcessMacro);
        }
        else
        {
            RunningPostProcess = false;
            _autoRetainerApi.FinishCharacterPostProcess();
        }
    }

    private void CheckForMacroCompletion(IFramework framework)
    {
        if (!RunningPostProcess) return;
        if (Service.MacroManager.State != LoopState.Running)
        {
            RunningPostProcess = false;
            _autoRetainerApi.FinishCharacterPostProcess();
        }
    }

    public void Dispose()
    {
        _autoRetainerApi.OnCharacterPostprocessStep -= CheckCharacterPostProcess;
        _autoRetainerApi.OnCharacterReadyToPostProcess -= DoCharacterPostProcess;
        Svc.Framework.Update -= CheckForMacroCompletion;

        Service.MacroManager?.Dispose();
        Service.GameEventManager?.Dispose();
        Service.ChatManager?.Dispose();
        IpcCommands.Instance?.Dispose();
        ECommonsMain.Dispose();
    }

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            EzConfigGui.Window.IsOpen ^= true;
            return;
        }
        else if (arguments.StartsWith("run "))
        {
            arguments = arguments[4..].Trim();

            var loopCount = 0u;
            if (arguments.StartsWith("loop "))
            {
                arguments = arguments[5..].Trim();
                var nextSpace = arguments.IndexOf(' ');
                if (nextSpace == -1)
                {
                    Service.ChatManager.PrintError("Could not determine loop count");
                    return;
                }

                if (!uint.TryParse(arguments[..nextSpace], out loopCount))
                {
                    Service.ChatManager.PrintError("Could not parse loop count");
                    return;
                }

                arguments = arguments[(nextSpace + 1)..].Trim();
            }

            var macroName = arguments.Trim('"');
            var nodes = Service.Configuration.GetAllNodes()
                .OfType<MacroNode>()
                .Where(node => node.Name.Trim() == macroName)
                .ToArray();

            if (nodes.Length == 0)
            {
                Service.ChatManager.PrintError("No macros match that name");
                return;
            }

            if (nodes.Length > 1)
            {
                Service.ChatManager.PrintError("More than one macro matches that name");
                return;
            }

            var node = nodes[0];

            if (loopCount > 0)
            {
                // Clone a new node so the modification doesn't save.
                node = new MacroNode()
                {
                    Name = node.Name,
                    Contents = node.Contents,
                };

                var lines = node.Contents.Split('\r', '\n');
                for (var i = lines.Length - 1; i >= 0; i--)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("/loop"))
                    {
                        var parts = line.Split()
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToArray();

                        var echo = line.Contains("<echo>") ? "<echo>" : string.Empty;
                        lines[i] = $"/loop {loopCount} {echo}";
                        node.Contents = string.Join('\n', lines);
                        Service.ChatManager.PrintMessage($"Running macro \"{macroName}\" {loopCount} times");
                        break;
                    }
                }
            }
            else
            {
                Service.ChatManager.PrintMessage($"Running macro \"{macroName}\"");
            }

            Service.MacroManager.EnqueueMacro(node);
            return;
        }
        else if (arguments == "pause")
        {
            Service.ChatManager.PrintMessage("Pausing");
            Service.MacroManager.Pause();
            return;
        }
        else if (arguments == "pause loop")
        {
            Service.ChatManager.PrintMessage("Pausing at next /loop");
            Service.MacroManager.Pause(true);
            return;
        }
        else if (arguments == "resume")
        {
            Service.ChatManager.PrintMessage("Resuming");
            Service.MacroManager.Resume();
            return;
        }
        else if (arguments == "stop")
        {
            Service.ChatManager.PrintMessage($"Stopping");
            Service.MacroManager.Stop();
            return;
        }
        else if (arguments == "stop loop")
        {
            Service.ChatManager.PrintMessage($"Stopping at next /loop");
            Service.MacroManager.Stop(true);
            return;
        }
        else if (arguments == "help")
        {
            EzConfigGui.WindowSystem.Windows.FirstOrDefault(w => w.WindowName == HelpWindow.WindowName)!.IsOpen ^= true;
            return;
        }
        else if (arguments == "excel")
        {
            EzConfigGui.WindowSystem.Windows.FirstOrDefault(w => w.WindowName == ExcelWindow.WindowName)!.IsOpen ^= true;
            return;
        }
        else if (arguments.StartsWith("cfg"))
        {
            var args = arguments[4..].Trim().Split(" ");
            Service.Configuration.SetProperty(args[0], args[1]);
            return;
        }
    }
}
