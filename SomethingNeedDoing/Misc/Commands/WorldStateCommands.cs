using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace SomethingNeedDoing.Misc.Commands;

public class WorldStateCommands
{
    internal static WorldStateCommands Instance { get; } = new();

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

    public int GetZoneID() => Service.ClientState.TerritoryType;

    public unsafe float GetFlagXCoord() => AgentMap.Instance()->FlagMapMarker.XFloat;
    public unsafe float GetFlagYCoord() => AgentMap.Instance()->FlagMapMarker.YFloat;
    public unsafe float GetFlagZone() => AgentMap.Instance()->FlagMapMarker.TerritoryId;
    public unsafe void SetMapFlag(uint territory, float worldX, float worldY, float worldZ) => AgentMap.Instance()->SetFlagMapMarker(territory, Svc.Data.GetExcelSheet<TerritoryType>()!.GetRow(territory)!.Map.Value!.RowId, new Vector3(worldX, worldY, worldZ));

    public unsafe byte GetActiveWeatherID() => EnvManager.Instance()->ActiveWeather;

    public unsafe long GetCurrentEorzeaTimestamp() => Framework.Instance()->ClientTime.EorzeaTime;
    public unsafe int GetCurrentEorzeaSecond() => DateTimeOffset.FromUnixTimeSeconds(Framework.Instance()->ClientTime.EorzeaTime).Second;
    public unsafe int GetCurrentEorzeaMinute() => DateTimeOffset.FromUnixTimeSeconds(Framework.Instance()->ClientTime.EorzeaTime).Minute;
    public unsafe int GetCurrentEorzeaHour() => DateTimeOffset.FromUnixTimeSeconds(Framework.Instance()->ClientTime.EorzeaTime).Hour;

    #region Fate
    public unsafe List<ushort> GetActiveFates() =>
        FateManager.Instance()->Fates.AsSpan().ToArray()
        .Where(f => f.Value is not null)
        .OrderBy(f => Vector3.Distance(Svc.ClientState.LocalPlayer!.Position, f.Value->Location))
        .Select(f => f.Value->FateId)
        .ToList();

    public unsafe ushort GetNearestFate() => FateManager.Instance()->Fates.AsSpan().ToArray()
        .Where(f => f.Value is not null)
        .OrderBy(f => Vector3.Distance(Svc.ClientState.LocalPlayer!.Position, f.Value->Location))
        .Select(f => f.Value->FateId)
        .FirstOrDefault();

    public unsafe bool IsInFate() => FateManager.Instance()->CurrentFate is not null;
    public unsafe float GetFateDuration(ushort fateID) => FateManager.Instance()->GetFateById(fateID)->Duration;
    public unsafe float GetFateHandInCount(ushort fateID) => FateManager.Instance()->GetFateById(fateID)->HandInCount;
    public unsafe float GetFateLocationX(ushort fateID) => FateManager.Instance()->GetFateById(fateID)->Location.X;
    public unsafe float GetFateLocationY(ushort fateID) => FateManager.Instance()->GetFateById(fateID)->Location.Y;
    public unsafe float GetFateLocationZ(ushort fateID) => FateManager.Instance()->GetFateById(fateID)->Location.Z;
    public unsafe float GetFateProgress(ushort fateID) => FateManager.Instance()->GetFateById(fateID)->Progress;
    #endregion

    public float DistanceBetween(float x1, float y1, float z1, float x2, float y2, float z2) => Vector3.DistanceSquared(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2));

    public unsafe float GetContentTimeLeft() => EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft;

    public unsafe int GetLastInstanceServerID() => WatchedValues.InstanceServerID;
    public unsafe int GetLastInstanceZoneID() => WatchedValues.InstanceServerID;

    #region OceanFishing
    public unsafe uint GetCurrentOceanFishingRoute() => EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentRoute;
    public byte GetCurrentOceanFishingTimeOfDay() => Svc.Data.GetExcelSheet<IKDRoute>()?.GetRow(this.GetCurrentOceanFishingRoute())?.Time[this.GetCurrentOceanFishingZone()].Value?.Unknown0 ?? 0;
    public unsafe int GetCurrentOceanFishingStatus() => (int)EventFramework.Instance()->GetInstanceContentOceanFishing()->Status;
    public unsafe uint GetCurrentOceanFishingZone() => EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentZone;
    public float GetCurrentOceanFishingZoneTimeLeft() => this.GetContentTimeLeft() - this.GetCurrentOceanFishingTimeOffset();
    public unsafe uint GetCurrentOceanFishingTimeOffset() => EventFramework.Instance()->GetInstanceContentOceanFishing()->TimeOffset;
    public unsafe uint GetCurrentOceanFishingWeatherID() => EventFramework.Instance()->GetInstanceContentOceanFishing()->WeatherId;
    public unsafe bool OceanFishingIsSpectralActive() => EventFramework.Instance()->GetInstanceContentOceanFishing()->SpectralCurrentActive;
    public unsafe uint GetCurrentOceanFishingMission1Type() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Type;
    public unsafe uint GetCurrentOceanFishingMission2Type() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Type;
    public unsafe uint GetCurrentOceanFishingMission3Type() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Type;
    public unsafe byte GetCurrentOceanFishingMission1Goal() => Svc.Data.GetExcelSheet<IKDPlayerMissionCondition>()?.GetRow(this.GetCurrentOceanFishingMission1Type())?.Unknown1 ?? 0;
    public unsafe byte GetCurrentOceanFishingMission2Goal() => Svc.Data.GetExcelSheet<IKDPlayerMissionCondition>()?.GetRow(this.GetCurrentOceanFishingMission2Type())?.Unknown1 ?? 0;
    public unsafe byte GetCurrentOceanFishingMission3Goal() => Svc.Data.GetExcelSheet<IKDPlayerMissionCondition>()?.GetRow(this.GetCurrentOceanFishingMission3Type())?.Unknown1 ?? 0;
    public unsafe string GetCurrentOceanFishingMission1Name() => Svc.Data.GetExcelSheet<IKDPlayerMissionCondition>()?.GetRow(this.GetCurrentOceanFishingMission1Type())?.Unknown0.RawString ?? "";
    public unsafe string GetCurrentOceanFishingMission2Name() => Svc.Data.GetExcelSheet<IKDPlayerMissionCondition>()?.GetRow(this.GetCurrentOceanFishingMission2Type())?.Unknown0.RawString ?? "";
    public unsafe string GetCurrentOceanFishingMission3Name() => Svc.Data.GetExcelSheet<IKDPlayerMissionCondition>()?.GetRow(this.GetCurrentOceanFishingMission3Type())?.Unknown0.RawString ?? "";
    public unsafe uint GetCurrentOceanFishingMission1Progress() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Progress;
    public unsafe uint GetCurrentOceanFishingMission2Progress() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Progress;
    public unsafe uint GetCurrentOceanFishingMission3Progress() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Progress;
    public unsafe uint GetCurrentOceanFishingPoints() => AgentModule.Instance()->GetAgentIKDFishingLog()->Points;
    public unsafe uint GetCurrentOceanFishingScore() => AgentModule.Instance()->GetAgentIKDResult()->Data->Score;
    public unsafe uint GetCurrentOceanFishingTotalScore() => AgentModule.Instance()->GetAgentIKDResult()->Data->TotalScore;
    #endregion

    #region DeepDungeons
    public float GetAccursedHoardRawX() => Svc.Objects.FirstOrDefault(x => x.DataId == DeepDungeonDataIDs.AccursedHoard)?.Position.X ?? 0;
    public float GetAccursedHoardRawY() => Svc.Objects.FirstOrDefault(x => x.DataId == DeepDungeonDataIDs.AccursedHoard)?.Position.Y ?? 0;
    public float GetAccursedHoardRawZ() => Svc.Objects.FirstOrDefault(x => x.DataId == DeepDungeonDataIDs.AccursedHoard)?.Position.Z ?? 0;
    public List<(float, float, float)> GetBronzeChestLocations() => Svc.Objects.OrderBy(DistanceToObject).Where(x => DeepDungeonDataIDs.BronzeChestIDs.Contains(x.DataId)).Select(x => (x.Position.X, x.Position.Y, x.Position.Z)).ToList();
    public List<(float, float, float)> GetSilverChestLocations() => Svc.Objects.OrderBy(DistanceToObject).Where(x => x.DataId == DeepDungeonDataIDs.SilverChest).Select(x => (x.Position.X, x.Position.Y, x.Position.Z)).ToList();
    public List<(float, float, float)> GetGoldChestLocations() => Svc.Objects.OrderBy(DistanceToObject).Where(x => x.DataId == DeepDungeonDataIDs.GoldChest).Select(x => (x.Position.X, x.Position.Y, x.Position.Z)).ToList();
    public List<(float, float, float)> GetMimicChestLocations() => Svc.Objects.OrderBy(DistanceToObject).Where(x => x.DataId == DeepDungeonDataIDs.MimicChest || DeepDungeonDataIDs.MimicIDs.Contains(x.DataId)).Select(x => (x.Position.X, x.Position.Y, x.Position.Z)).ToList();
    public (float, float, float) GetPassageLocation() => Svc.Objects.OrderBy(DistanceToObject).Where(x => DeepDungeonDataIDs.PassageIDs.Contains(x.DataId)).Select(x => (x.Position.X, x.Position.Y, x.Position.Z)).FirstOrDefault();
    public List<(float, float, float)> GetTrapLocations() => Svc.Objects.OrderBy(DistanceToObject).Where(x => DeepDungeonDataIDs.TrapIDs.ContainsKey(x.DataId)).Select(x => (x.Position.X, x.Position.Y, x.Position.Z)).ToList();
    #endregion

    public List<string> GetNearbyObjectNames(float distance = 0, byte objectKind = 0) =>
        Svc.Objects
            .OrderBy(o => Vector3.DistanceSquared(o.Position, Svc.ClientState.LocalPlayer!.Position))
            .Where(o => o.IsTargetable && (distance == 0 || Vector3.DistanceSquared(o.Position, Svc.ClientState.LocalPlayer!.Position) <= distance) && (objectKind == 0 || (byte)o.ObjectKind == objectKind))
            .Select(o => o.Name.TextValue)
            .ToList();

    private float DistanceToObject(Dalamud.Game.ClientState.Objects.Types.IGameObject o) => Vector3.DistanceSquared(o.Position, Svc.ClientState.LocalPlayer!.Position);
}
