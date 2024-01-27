using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SomethingNeedDoing.Misc.Commands
{
    public class WorldStateCommands
    {
        internal static WorldStateCommands Instance { get; } = new();

        public List<string> ListAllFunctions()
        {
            MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var list = new List<string>();
            foreach (MethodInfo method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
            {
                var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
                list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
            }
            return list;
        }

        public int GetZoneID() => Service.ClientState.TerritoryType;

        public unsafe float GetFlagXCoord() => AgentMap.Instance()->FlagMapMarker.XFloat;
        public unsafe float GetFlagYCoord() => AgentMap.Instance()->FlagMapMarker.YFloat;

        public unsafe byte GetActiveWeatherID() => EnvManager.Instance()->ActiveWeather;
    }
}
