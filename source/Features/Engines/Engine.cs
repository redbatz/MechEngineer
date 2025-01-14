﻿using System.Collections.Generic;
using System.Linq;
using BattleTech;
using CustomComponents;
using MechEngineer.Features.Engines.Helper;
using MechEngineer.Features.OverrideTonnage;
using MechEngineer.Misc;
using UnityEngine;

namespace MechEngineer.Features.Engines;

[UsedBy(User.BattleValue)]
public class Engine
{
    [UsedBy(User.BattleValue)]
    public static Engine? GetEngine(ChassisDef chassisDef, IList<MechComponentRef> componentRefs)
    {
        var result = EngineSearcher.SearchInventory(componentRefs);
        if (result.CoolingDef == null || result.CoreDef == null || result.HeatBlockDef == null)
        {
            return null;
        }

        if (chassisDef.ChassisTags.Contains(EngineFeature.settings.ProtoMechEngineTag))
        {
            return new ProtoMechEngine(result);
        }

        return new Engine(result);
    }

    internal Engine(EngineSearcher.Result result) : this(result.CoolingDef!, result.HeatBlockDef!, result.CoreDef!, result.WeightFactors, result.HeatSinks)
    {
    }

    // should be private but used during autofixer, rename EngineSearcher.Result to .Builder and apply new semantics
    internal Engine(
        CoolingDef coolingDef,
        EngineHeatBlockDef heatBlockDef,
        EngineCoreDef coreDef,
        WeightFactors weightFactors,
        List<MechComponentRef> heatSinksExternal,
        bool calculate = true)
    {
        CoolingDef = coolingDef;
        HeatBlockDef = heatBlockDef;
        CoreDef = coreDef;
        WeightFactors = weightFactors;
        HeatSinksExternal = heatSinksExternal;
        if (calculate)
        {
            CalculateStats();
        }
    }

    private CoolingDef _coolingDef = null!;
    [UsedBy(User.BattleValue)]
    public CoolingDef CoolingDef
    {
        get => _coolingDef;
        private set
        {
            _coolingDef = value;
            var id = _coolingDef.HeatSinkDefId;
            var def = UnityGameInstance.BattleTechGame.DataManager.HeatSinkDefs.Get(id);
            HeatSinkDef = def.GetComponent<EngineHeatSinkDef>();
        }
    }
    // type of internal heat sinks and compatible external heat sinks
    [UsedBy(User.BattleValue)]
    public EngineHeatSinkDef HeatSinkDef { get; set; } = null!;

    // amount of internal heat sinks
    [UsedBy(User.BattleValue)]
    public EngineHeatBlockDef HeatBlockDef { get; set; }

    [UsedBy(User.BattleValue)]
    public EngineCoreDef CoreDef { get; set; }

    [UsedBy(User.BattleValue)]
    public WeightFactors WeightFactors { get; set; }

    [UsedBy(User.BattleValue)]
    public List<MechComponentRef> HeatSinksExternal { get; set; }

    private int HeatSinkExternalCount { get; set; }
    internal void CalculateStats()
    {
        HeatSinkExternalCount = MatchingCount(HeatSinksExternal, HeatSinkDef.Def);
    }
    private static int MatchingCount(IEnumerable<MechComponentRef> heatSinks, HeatSinkDef heatSinkDef)
    {
        return heatSinks.Select(r => r.Def).Count(d => d == heatSinkDef);
    }

    [UsedBy(User.BattleValue)]
    public float EngineHeatDissipation
    {
        get
        {
            var dissipation = HeatSinkDef.Def.DissipationCapacity * (HeatSinkInternalFreeMaxCount + HeatBlockDef.HeatSinkCount);
            dissipation += CoreDef.Def.DissipationCapacity;
            dissipation += CoolingDef.Def.DissipationCapacity;
            return dissipation;
        }
    }

    #region heat sink counting

    internal int HeatSinkExternalFreeCount => Mathf.Min(HeatSinkExternalCount, HeatSinkExternalFreeMaxCount);
    internal int HeatSinkExternalAdditionalCount => HeatSinkExternalCount - HeatSinkExternalFreeCount;

    internal int HeatSinkTotalCount => HeatSinkInternalCount + HeatSinkExternalCount;
    internal int HeatSinkInternalCount => HeatSinkInternalFreeMaxCount + HeatBlockDef.HeatSinkCount;

    private int HeatSinksFreeMaxCount => EngineFeature.settings.HeatSinksMaximumFreeCount;
    private int HeatSinksInternalMaxCount => CoreDef.Rating / 25;

    internal virtual int HeatSinkInternalFreeMaxCount => Mathf.Min(HeatSinksFreeMaxCount, HeatSinksInternalMaxCount);
    internal virtual int HeatSinkInternalAdditionalMaxCount => Mathf.Max(0, HeatSinksInternalMaxCount - HeatSinksFreeMaxCount);
    internal virtual int HeatSinkExternalFreeMaxCount => HeatSinksFreeMaxCount - HeatSinkInternalFreeMaxCount;

    #endregion

    #region weights

    private float HeatSinkExternalFreeTonnage => HeatSinkExternalFreeCount * HeatSinkDef.Def.Tonnage;
    internal float GyroTonnage => PrecisionUtils.RoundUp(StandardGyroTonnage * WeightFactors.GyroFactor, WeightPrecision);
    internal float EngineTonnage => PrecisionUtils.RoundUp(StandardEngineTonnage * WeightFactors.EngineFactor, WeightPrecision);
    internal float HeatSinkTonnage => -HeatSinkExternalFreeTonnage;
    internal float TotalTonnage => HeatSinkTonnage + EngineTonnage + GyroTonnage;

    protected virtual float StandardGyroTonnage => PrecisionUtils.RoundUp(CoreDef.Rating / 100f, 1f);
    protected virtual float StandardEngineTonnage => CoreDef.Def.Tonnage - StandardGyroTonnage;

    protected virtual float WeightPrecision => OverrideTonnageFeature.settings.TonnageStandardPrecision;

    #endregion
}

internal class ProtoMechEngine : Engine
{
    internal ProtoMechEngine(EngineSearcher.Result result) : base(result)
    {
    }

    internal override int HeatSinkInternalFreeMaxCount => 0;
    internal override int HeatSinkInternalAdditionalMaxCount => 0;
    internal override int HeatSinkExternalFreeMaxCount => 0;

    protected override float StandardGyroTonnage => 0;
    protected override float StandardEngineTonnage => CoreDef.Def.Tonnage;

    protected override float WeightPrecision => OverrideTonnageFeature.settings.KilogramStandardPrecision;
}