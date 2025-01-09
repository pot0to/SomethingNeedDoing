using AutoRetainerAPI;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.EzEventManager;
using ECommons.SimpleGui;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Macros;
using SomethingNeedDoing.Macros.Lua;
using SomethingNeedDoing.Managers;
using SomethingNeedDoing.Misc;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    internal string Name => "Something Need Doing (Expanded Edition)";
    internal string Prefix => "SND";
    private const string Command = "/somethingneeddoing";
    internal string[] Aliases => ["/snd", "/pcraft"];

    internal static Plugin P = null!;
    internal static Config C => P.Config;
    internal static MacroFileSystem FS => P._ottergui.MacroFileSystem;
    private readonly Config Config = null!;
    //private readonly Config _legacyConf = null!;
    private readonly OtterGuiHandler _ottergui = null!;
    private readonly AutoRetainerApi _autoRetainerApi = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        pluginInterface.Create<Service>();
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        //_legacyConf = Config.Load(Svc.PluginInterface.ConfigDirectory); // must be done before EzConfig migration
        //if (_legacyConf != default)
        //{
        //    var migration = new V2();
        //    migration.Migrate(ref _legacyConf);
        //}

        EzConfig.DefaultSerializationFactory = new ConfigFactory();
        EzConfig.Migrate<Config>();
        Config = EzConfig.Init<Config>();

        Service.ChatManager = new ChatManager();
        Service.GameEventManager = new GameEventManager();
        Service.MacroManager = new MacroManager();
        _ottergui = new();
        _autoRetainerApi = new();

        EzConfigGui.Init(new Windows.MacrosUI().Draw);
        EzConfigGui.WindowSystem.AddWindow(new HelpUI());
        EzConfigGui.WindowSystem.AddWindow(new ExcelWindow());
        Svc.PluginInterface.UiBuilder.OpenMainUi += EzConfigGui.Window.Toggle;

        EzCmd.Add(Command, OnChatCommand, "Open a window to edit various settings.");
        Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{Command} Alias"));

        _autoRetainerApi.OnCharacterPostprocessStep += CheckCharacterPostProcess;
        _autoRetainerApi.OnCharacterReadyToPostProcess += DoCharacterPostProcess;
        _ = new EzFrameworkUpdate(CheckForMacroCompletion);
    }

    private void CheckCharacterPostProcess()
    {
        //if (C.ARCharacterPostProcessExcludedCharacters.Any(x => x == Svc.ClientState.LocalContentId))
        //    Svc.Log.Info("Skipping post process macro for current character.");
        //else
        //    _autoRetainerApi.RequestCharacterPostprocess();
    }

    private bool RunningPostProcess;
    private void DoCharacterPostProcess()
    {
        //var macro = C.Files.FirstOrDefault(x => x.UseInARPostProcess);
        //if (macro != default)
        //{
        //    RunningPostProcess = true;
        //    Service.MacroManager.EnqueueMacro(macro);
        //}
        //else
        //{
        //    RunningPostProcess = false;
        //    _autoRetainerApi.FinishCharacterPostProcess();
        //}
    }

    private void CheckForMacroCompletion()
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
        FS.Dispose();

        _autoRetainerApi.OnCharacterPostprocessStep -= CheckCharacterPostProcess;
        _autoRetainerApi.OnCharacterReadyToPostProcess -= DoCharacterPostProcess;

        Svc.PluginInterface.UiBuilder.OpenMainUi -= EzConfigGui.Window.Toggle;
        Service.MacroManager?.Dispose();
        Service.GameEventManager?.Dispose();
        Service.ChatManager?.Dispose();
        Ipc.Instance?.Dispose();
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
            if (FS.TryFindMacroByName(macroName, out var macroFile))
            {
                if (loopCount > 0)
                {
                }
                else
                {
                    Service.ChatManager.PrintMessage($"Running macro \"{macroName}\"");
                }
                Service.MacroManager.EnqueueMacro(macroFile);
                return;
            }
            else
            {
                Service.ChatManager.PrintError("No macros match that name");
                return;
            }

            //if (loopCount > 0)
            //{
            //    // Clone a new node so the modification doesn't save.
            //    node = new MacroNode()
            //    {
            //        Name = node.Name,
            //        Contents = node.Contents,
            //    };

            //    var lines = node.Contents.Split('\r', '\n');
            //    for (var i = lines.Length - 1; i >= 0; i--)
            //    {
            //        var line = lines[i].Trim();
            //        if (line.StartsWith("/loop"))
            //        {
            //            var parts = line.Split()
            //                .Where(s => !string.IsNullOrEmpty(s))
            //                .ToArray();

            //            var echo = line.Contains("<echo>") ? "<echo>" : string.Empty;
            //            lines[i] = $"/loop {loopCount} {echo}";
            //            node.Contents = string.Join('\n', lines);
            //            Service.ChatManager.PrintMessage($"Running macro \"{macroName}\" {loopCount} times");
            //            break;
            //        }
            //    }
            //}
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
            EzConfigGui.GetWindow<HelpUI>()!.Toggle();
            return;
        }
        else if (arguments == "excel")
        {
            EzConfigGui.GetWindow<ExcelWindow>()!.Toggle();
            return;
        }
        else if (arguments.StartsWith("cfg"))
        {
            var args = arguments[4..].Trim().Split(" ");
            C.SetProperty(args[0], args[1]);
            return;
        }
    }
}
