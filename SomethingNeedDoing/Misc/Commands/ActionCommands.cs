using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SomethingNeedDoing.Misc.Commands
{
    internal class ActionCommands
    {
        internal static ActionCommands Instance { get; } = new();

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

        private readonly AbandonDuty abandonDuty = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 43 28 B1 01"));

        private delegate void AbandonDuty(bool a1);

        public void LeaveDuty() => this.abandonDuty(false);

        public unsafe void TeleportToGCTown()
        {
            var gc = UIState.Instance()->PlayerState.GrandCompany;
            var aetheryte = gc switch
            {
                0 => 0u,
                1 => 8u,
                2 => 2u,
                3 => 9u,
                _ => 0u
            };
            Telepo.Instance()->Teleport(aetheryte, 0);
        }
    }
}
