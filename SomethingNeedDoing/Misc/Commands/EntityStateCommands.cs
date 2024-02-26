using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace SomethingNeedDoing.Misc.Commands;

internal class EntityStateCommands
{
    internal static EntityStateCommands Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public float GetObjectRawXPos(string name) => Svc.Objects.FirstOrDefault(x => x.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Position.X ?? 0;
    public float GetObjectRawYPos(string name) => Svc.Objects.FirstOrDefault(x => x.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Position.Y ?? 0;
    public float GetObjectRawZPos(string name) => Svc.Objects.FirstOrDefault(x => x.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Position.Z ?? 0;

    public float GetDistanceToPoint(float x, float y, float z) => Vector3.Distance(Svc.ClientState.LocalPlayer!.Position, new Vector3(x, y, z));
    public float GetDistanceToTarget() => Vector3.Distance(Svc.ClientState.LocalPlayer!.Position, Svc.Targets.Target?.Position ?? Svc.ClientState.LocalPlayer!.Position);
    public float GetDistanceToObject(string name) => Vector3.Distance(Svc.ClientState.LocalPlayer!.Position, Svc.Objects.FirstOrDefault(x => x.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Position ?? Vector3.Zero);

    #region Target
    public string GetTargetName() => Svc.Targets.Target?.Name.TextValue ?? "";
    public float GetTargetRawXPos() => Svc.Targets.Target?.Position.X ?? 0;
    public float GetTargetRawYPos() => Svc.Targets.Target?.Position.Y ?? 0;
    public float GetTargetRawZPos() => Svc.Targets.Target?.Position.Z ?? 0;
    public unsafe bool IsTargetCasting() => ((Character*)Svc.Targets.Target?.Address!)->IsCasting;
    public unsafe uint GetTargetActionID() => ((Character*)Svc.Targets.Target?.Address!)->GetCastInfo()->ActionID;
    public unsafe uint GetTargetUsedActionID() => ((Character*)Svc.Targets.Target?.Address!)->GetCastInfo()->UsedActionId;
    public float GetTargetHP() => (Svc.Targets.Target as Dalamud.Game.ClientState.Objects.Types.Character)?.CurrentHp ?? 0;
    public float GetTargetMaxHP() => (Svc.Targets.Target as Dalamud.Game.ClientState.Objects.Types.Character)?.MaxHp ?? 0;
    public float GetTargetHPP() => GetTargetHP() / GetTargetMaxHP() * 100;

    public byte? GetTargetObjectKind() => (byte?)Svc.Targets.Target?.ObjectKind;
    public byte? GetTargetSubKind() => Svc.Targets.Target?.SubKind;

    public unsafe void TargetClosestEnemy(float distance = 0) => Svc.Targets.Target = Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead && (distance == 0 || DistanceToObject(o) <= distance));
    #endregion

    #region Focus Target
    public string GetFocusTargetName() => Svc.Targets.FocusTarget?.Name.TextValue ?? "";
    public float GetFocusTargetRawXPos() => Svc.Targets.FocusTarget?.Position.X ?? 0;
    public float GetFocusTargetRawYPos() => Svc.Targets.FocusTarget?.Position.Y ?? 0;
    public float GetFocusTargetRawZPos() => Svc.Targets.FocusTarget?.Position.Z ?? 0;
    public unsafe bool IsFocusTargetCasting() => ((Character*)Svc.Targets.FocusTarget?.Address!)->IsCasting;
    public unsafe uint GetFocusTargetActionID() => ((Character*)Svc.Targets.FocusTarget?.Address!)->GetCastInfo()->ActionID;
    public unsafe uint GetFocusTargetUsedActionID() => ((Character*)Svc.Targets.FocusTarget?.Address!)->GetCastInfo()->UsedActionId;
    public float GetFocusTargetHP() => (Svc.Targets.FocusTarget as Dalamud.Game.ClientState.Objects.Types.Character)?.CurrentHp ?? 0;
    public float GetFocusTargetMaxHP() => (Svc.Targets.FocusTarget as Dalamud.Game.ClientState.Objects.Types.Character)?.MaxHp ?? 0;
    public float GetFocusTargetHPP() => GetFocusTargetHP() / GetFocusTargetMaxHP() * 100;
    #endregion

    private float DistanceToObject(Dalamud.Game.ClientState.Objects.Types.GameObject o) => Vector3.DistanceSquared(o.Position, Svc.ClientState.LocalPlayer!.Position);
}
