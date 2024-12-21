using AutoRetainerAPI;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.EzEventManager;
using ECommons.Schedulers;
using ECommons.SimpleGui;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Macros;
using SomethingNeedDoing.Macros.Lua;
using SomethingNeedDoing.Managers;
using SomethingNeedDoing.Misc;
using System.IO;

namespace SomethingNeedDoing;

public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "Something Need Doing (Expanded Edition)";
    public static string Prefix => "SND";
    public const string Command = "/somethingneeddoing";
    public static string[] Aliases => ["/snd", "/pcraft"];

    internal static Plugin P = null!;
    internal static Config C => P.Config;
    internal static MacroFileSystem FS => P.OtterGuiHandler.MacroFileSystem;
    internal Config Config = null!;
    internal Config _legacyConf = null!;
    internal OtterGuiHandler OtterGuiHandler = null!;
    internal AutoRetainerApi _autoRetainerApi = null!;
    private FileSystemWatcher _watcher = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        pluginInterface.Create<Service>();
        Service.Plugin = this;
        ECommonsMain.Init(pluginInterface, this, Module.ObjectFunctions, Module.DalamudReflector);

        _ = new TickScheduler(() =>
        {
            _legacyConf = Config.Load(Svc.PluginInterface.ConfigDirectory); // must be done before EzConfig migration
            if (_legacyConf != default)
            {
                var migration = new V2();
                migration.Migrate(ref _legacyConf);
            }

            EzConfig.DefaultSerializationFactory = new ConfigFactory();
            EzConfig.Migrate<Config>();
            Service.Configuration = Config = EzConfig.Init<Config>();

            Service.ChatManager = new ChatManager();
            Service.GameEventManager = new GameEventManager();
            Service.MacroManager = new MacroManager();
            OtterGuiHandler = new();
            _autoRetainerApi = new();

            EzConfigGui.Init(new Windows.Macros().Draw);
            EzConfigGui.WindowSystem.AddWindow(new HelpWindow());
            EzConfigGui.WindowSystem.AddWindow(new ExcelWindow());

            EzCmd.Add(Command, OnChatCommand, "Open a window to edit various settings.");
            Aliases.ToList().ForEach(a => EzCmd.Add(a, OnChatCommand, $"{Command} Alias"));

            _autoRetainerApi.OnCharacterPostprocessStep += CheckCharacterPostProcess;
            _autoRetainerApi.OnCharacterReadyToPostProcess += DoCharacterPostProcess;
            _ = new EzFrameworkUpdate(CheckForMacroCompletion);
            _watcher = new FileSystemWatcher
            {
                Path = Service.Configuration.RootFolderPath,
                IncludeSubdirectories = true,
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
            _watcher.EnableRaisingEvents = true;
        });
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) => FS.BuildFileSystem();

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
        var macro = C.Files.FirstOrDefault(x => x.UseInARPostProcess);
        if (macro != default)
        {
            RunningPostProcess = true;
            Service.MacroManager.EnqueueMacro(macro);
        }
        else
        {
            RunningPostProcess = false;
            _autoRetainerApi.FinishCharacterPostProcess();
        }
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
        _watcher.Changed -= OnFileChanged;
        _watcher.Created -= OnFileChanged;
        _watcher.Deleted -= OnFileChanged;
        _watcher.Renamed -= OnFileChanged;
        _watcher.Dispose();
        FS.Dispose();

        _autoRetainerApi.OnCharacterPostprocessStep -= CheckCharacterPostProcess;
        _autoRetainerApi.OnCharacterReadyToPostProcess -= DoCharacterPostProcess;

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
            EzConfigGui.GetWindow<HelpWindow>()!.Toggle();
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
            Service.Configuration.SetProperty(args[0], args[1]);
            return;
        }
    }
}
