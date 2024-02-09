using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using NLua;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar;
using SomethingNeedDoing.Grammar.Commands;
using SomethingNeedDoing.Misc.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SomethingNeedDoing.Misc;

/// <summary>
/// A macro node queued for interaction.
/// </summary>
internal partial class ActiveMacro : IDisposable
{
    private Lua? lua;
    private LuaFunction? luaGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveMacro"/> class.
    /// </summary>
    /// <param name="node">The node to run.</param>
    public ActiveMacro(MacroNode node)
    {
        this.Node = node;

        if (node.IsLua)
        {
            this.Steps = [];
            return;
        }

        var contents = ModifyMacroForCraftLoop(node.Contents, node.CraftingLoop, node.CraftLoopCount);
        this.Steps = MacroParser.Parse(contents).ToList();
    }

    /// <summary>
    /// Gets the underlying node.
    /// </summary>
    public MacroNode Node { get; private set; }

    /// <summary>
    /// Gets the command steps.
    /// </summary>
    public List<MacroCommand> Steps { get; private set; }

    /// <summary>
    /// Gets the current step number.
    /// </summary>
    public int StepIndex { get; private set; }

    /// <summary>
    /// Modify a macro for craft looping.
    /// </summary>
    /// <param name="contents">Contents of a macroNode.</param>
    /// <param name="craftLoop">A value indicating whether craftLooping is enabled.</param>
    /// <param name="craftCount">Amount to craftLoop.</param>
    /// <returns>The modified macro.</returns>
    public static string ModifyMacroForCraftLoop(string contents, bool craftLoop, int craftCount)
    {
        if (!craftLoop)
            return contents;

        if (Service.Configuration.UseCraftLoopTemplate)
        {
            var template = Service.Configuration.CraftLoopTemplate;

            if (craftCount == 0)
                return contents;

            if (craftCount == -1)
                craftCount = 999_999;

            return !template.Contains("{{macro}}")
                ? throw new MacroCommandError("CraftLoop template does not contain the {{macro}} placeholder")
                : template
                .Replace("{{macro}}", contents)
                .Replace("{{count}}", craftCount.ToString());
        }

        var maxwait = Service.Configuration.CraftLoopMaxWait;
        var maxwaitMod = maxwait > 0 ? $" <maxwait.{maxwait}>" : string.Empty;

        var echo = Service.Configuration.CraftLoopEcho;
        var echoMod = echo ? $" <echo>" : string.Empty;

        var craftGateStep = Service.Configuration.CraftLoopFromRecipeNote
            ? $"/craft {craftCount}{echoMod}"
            : $"/gate {craftCount - 1}{echoMod}";

        var clickSteps = string.Join("\n", new string[]
        {
            $@"/waitaddon ""RecipeNote""{maxwaitMod}",
            $@"/click ""synthesize""",
            $@"/waitaddon ""Synthesis""{maxwaitMod}",
        });

        var loopStep = $"/loop{echoMod}";

        var sb = new StringBuilder();

        if (Service.Configuration.CraftLoopFromRecipeNote)
        {
            if (craftCount == -1)
            {
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
                sb.AppendLine(loopStep);
            }
            else if (craftCount == 0)
            {
                sb.AppendLine(contents);
            }
            else if (craftCount == 1)
            {
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
            }
            else
            {
                sb.AppendLine(craftGateStep);
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
                sb.AppendLine(loopStep);
            }
        }
        else
        {
            if (craftCount == -1)
            {
                sb.AppendLine(contents);
                sb.AppendLine(clickSteps);
                sb.AppendLine(loopStep);
            }
            else if (craftCount is 0 or 1)
            {
                sb.AppendLine(contents);
            }
            else
            {
                sb.AppendLine(contents);
                sb.AppendLine(craftGateStep);
                sb.AppendLine(clickSteps);
                sb.AppendLine(loopStep);
            }
        }

        return sb.ToString().Trim();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.luaGenerator?.Dispose();
        this.lua?.Dispose();
    }

    /// <summary>
    /// Go to the next step.
    /// </summary>
    public void NextStep() => this.StepIndex++;

    /// <summary>
    /// Loop.
    /// </summary>
    public void Loop()
    {
        if (this.Node.IsLua)
            throw new MacroCommandError("Loop is not supported for Lua scripts");

        this.StepIndex = -1;
    }

    /// <summary>
    /// Get the current step.
    /// </summary>
    /// <returns>A command.</returns>
    public MacroCommand? GetCurrentStep()
    {
        if (this.Node.IsLua)
        {
            if (this.lua == null)
                this.InitLuaScript();

            var results = this.luaGenerator!.Call();
            if (results.Length == 0)
                return null;

            if (results[0] is not string text)
                throw new MacroCommandError("Lua macro yielded a non-string");

            var command = MacroParser.ParseLine(text);

            if (command != null)
                this.Steps.Add(command);

            return command;
        }

        return this.StepIndex < 0 || this.StepIndex >= this.Steps.Count ? null : this.Steps[this.StepIndex];
    }

    private void InitLuaScript()
    {
        var script = this.Node.Contents
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .Select(line => $"  {line}")
            .Join('\n');

        //var imports = new List<string>
        //{
        //    "require \"Dalamud\"",
        //    "require \"Dalamud.Plugin\"",
        //    "require \"Dalamud.Logging.PluginLog\"",
        //    "require \"Lumina\"",
        //    "require \"Lumina.Excel.GeneratedSheets\"",
        //};

        //var services = typeof(IDalamudPlugin).Assembly.GetTypes()
        //    .Where(t => t.GetCustomAttribute(typeof(PluginInterfaceAttribute)) != null)
        //    .Where(t => t.Namespace != null)
        //    .Select(t => $"require \"{t.Namespace!}.{t.Name}\"");

        //imports.AddRange(services);

        //script = string.Join("\n", imports) + "\n" + script;
        //script = $"{string.Join($"\n", $"{nameof(RefreshGlobals)}()")}\n{script}";

        static void RegisterClassMethods(Lua lua, object obj)
        {
            var type = obj.GetType();
            var isStatic = type.IsAbstract && type.IsSealed;
            var flags = BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            var methods = type.GetMethods(flags);
            foreach (var method in methods)
            {
                Service.Log.Debug($"Adding Lua method: {method.Name}");
                lua.RegisterFunction(method.Name, obj, method);
            }
        }

        this.lua = new Lua();
        this.lua.State.Encoding = Encoding.UTF8;
        this.lua.LoadCLRPackage();

        #region special methods
        RegisterClassMethods(this.lua, ActionCommands.Instance);
        RegisterClassMethods(this.lua, AddonCommands.Instance);
        RegisterClassMethods(this.lua, CharacterStateCommands.Instance);
        RegisterClassMethods(this.lua, CraftingCommands.Instance);
        RegisterClassMethods(this.lua, EntityStateCommands.Instance);
        RegisterClassMethods(this.lua, InventoryCommands.Instance);
        RegisterClassMethods(this.lua, IpcCommands.Instance);
        RegisterClassMethods(this.lua, QuestCommands.Instance);
        RegisterClassMethods(this.lua, SystemCommands.Instance);
        RegisterClassMethods(this.lua, WorldStateCommands.Instance);
        #endregion

        script = string.Format(EntrypointTemplate, script);

        #region globals
        this.lua["Interface"] = Svc.PluginInterface;
        this.lua["IClientState"] = Svc.ClientState;
        this.lua["IGameGui"] = Svc.GameGui;
        this.lua["IDataManager"] = Svc.Data;
        this.lua["IBuddyList"] = Svc.Buddies;
        this.lua["IChatGui"] = Svc.Chat;
        this.lua["ICommandManager"] = Svc.Commands;
        this.lua["ICondition"] = Svc.Condition;
        this.lua["IFateTable"] = Svc.Fates;
        this.lua["IFlyTextGui"] = Svc.FlyText;
        this.lua["IFramework"] = Svc.Framework;
        this.lua["IGameNetwork"] = Svc.GameNetwork;
        this.lua["IJobGauges"] = Svc.Gauges;
        this.lua["IKeyState"] = Svc.KeyState;
        this.lua["ILibcFunction"] = Svc.LibcFunction;
        this.lua["IObjectTable"] = Svc.Objects;
        this.lua["IPartyFinderGui"] = Svc.PfGui;
        this.lua["IPartyList"] = Svc.Party;
        this.lua["ISigScanner"] = Svc.SigScanner;
        this.lua["ITargetManager"] = Svc.Targets;
        this.lua["IToastGui"] = Svc.Toasts;
        this.lua["IGameConfig"] = Svc.GameConfig;
        this.lua["IGameLifecycle"] = Svc.GameLifecycle;
        this.lua["IGamepadState"] = Svc.GamepadState;
        this.lua["IDtrBar"] = Svc.DtrBar;
        this.lua["IDutyState"] = Svc.DutyState;
        this.lua["IGameInteropProvider"] = Svc.Hook;
        this.lua["ITextureProvider"] = Svc.Texture;
        this.lua["IPluginLog"] = Svc.Log;
        this.lua["IAddonLifecycle"] = Svc.AddonLifecycle;
        this.lua["IAetheryteList"] = Svc.AetheryteList;
        this.lua["IAddonEventManager"] = Svc.AddonEventManager;
        this.lua["ITextureSubstitution"] = Svc.TextureSubstitution;
        this.lua["ITitleScreenMenu"] = Svc.TitleScreenMenu;
        unsafe
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.lua["ActionManager"] = (ActionManager)Marshal.PtrToStructure((nint)ActionManager.Instance(), typeof(ActionManager));
            this.lua["AgentMap"] = (AgentMap)Marshal.PtrToStructure((nint)AgentMap.Instance(), typeof(AgentMap));
            this.lua["EnvManager"] = (EnvManager)Marshal.PtrToStructure((nint)EnvManager.Instance(), typeof(EnvManager));
            this.lua["EventFramework"] = (EventFramework)Marshal.PtrToStructure((nint)EventFramework.Instance(), typeof(EventFramework));
            this.lua["FateManager"] = (FateManager)Marshal.PtrToStructure((nint)FateManager.Instance(), typeof(FateManager));
            this.lua["Framework"] = (Framework)Marshal.PtrToStructure((nint)Framework.Instance(), typeof(Framework));
            this.lua["InventoryManager"] = (InventoryManager)Marshal.PtrToStructure((nint)InventoryManager.Instance(), typeof(InventoryManager));
            this.lua["PlayerState"] = (PlayerState)Marshal.PtrToStructure((nint)PlayerState.Instance(), typeof(PlayerState));
            this.lua["QuestManager"] = (QuestManager)Marshal.PtrToStructure((nint)QuestManager.Instance(), typeof(QuestManager));
            this.lua["RouletteController"] = (RouletteController)Marshal.PtrToStructure((nint)RouletteController.Instance(), typeof(RouletteController));
            this.lua["UIState"] = (UIState)Marshal.PtrToStructure((nint)UIState.Instance(), typeof(UIState));
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        #endregion

        this.lua.DoString(FStringSnippet);
        var results = this.lua.DoString(script);

        if (results.Length == 0 || results[0] is not LuaFunction coro)
            throw new MacroCommandError("Could not get Lua entrypoint.");

        this.luaGenerator = coro;
    }
}

/// <summary>
/// Lua code snippets.
/// </summary>
internal partial class ActiveMacro
{
    private const string EntrypointTemplate = @"
yield = coroutine.yield
--
function entrypoint()
{0}
end
--
return coroutine.wrap(entrypoint)";

    private const string FStringSnippet = @"
function f(str)
   local outer_env = _ENV
   return (str:gsub(""%b{}"", function(block)
      local code = block:match(""{(.*)}"")
      local exp_env = {}
      setmetatable(exp_env, { __index = function(_, k)
         local stack_level = 5
         while debug.getinfo(stack_level, """") ~= nil do
            local i = 1
            repeat
               local name, value = debug.getlocal(stack_level, i)
               if name == k then
                  return value
               end
               i = i + 1
            until name == nil
            stack_level = stack_level + 1
         end
         return rawget(outer_env, k)
      end })
      local fn, err = load(""return ""..code, ""expression `""..code..""`"", ""t"", exp_env)
      if fn then
         return tostring(fn())
      else
         error(err, 0)
      end
   end))
end";
}