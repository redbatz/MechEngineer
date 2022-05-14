﻿using System.Linq;
using BattleTech;
using CustomComponents;
using MechEngineer.Features.OverrideTonnage;
using MechEngineer.Helper;
using MechEngineer.Misc;

namespace MechEngineer.Features.CustomCapacities;

internal class CustomCapacitiesFeature : Feature<CustomCapacitiesSettings>, IValidateMech
{
    internal static readonly CustomCapacitiesFeature Shared = new();

    internal override CustomCapacitiesSettings Settings => Control.Settings.CustomCapacities;

    protected override void SetupFeatureLoaded()
    {
        var ccValidation = new CCValidationAdapter(this);
        Validator.RegisterMechValidator(ccValidation.ValidateMech, ccValidation.ValidateMechCanBeFielded);
    }

    internal static void CalculateCarryWeight(MechDef mechDef, out float totalCapacity, out float totalUsage)
    {
        CalculateCarryWeight(mechDef, out totalCapacity, out totalUsage, out _, out _);
    }

    public void ValidateMech(MechDef mechDef, Errors errors)
    {
        ValidateCarryWeight(mechDef, errors);
    }

    // Carry Capacity - TacOps p.92
    // HandHeld Weapons - TacOps p.316
    private void ValidateCarryWeight(MechDef mechDef, Errors errors)
    {
        CalculateCarryWeight(mechDef, out var capacity, out var usage, out var left, out var right);

        if (PrecisionUtils.SmallerThan(capacity, usage))
        {
            errors.Add(MechValidationType.Overweight, Settings.ErrorOverweight);
        }
        else if ((left == MinHandReq.Two && right != MinHandReq.None) || (right == MinHandReq.Two && left != MinHandReq.None))
        {
            errors.Add(MechValidationType.Overweight, Settings.ErrorOneFreeHand);
        }
    }

    private static void CalculateCarryWeight(MechDef mechDef, out float totalCapacity, out float totalUsage, out MinHandReq left, out MinHandReq right)
    {
        var capacitySum = 0f;
        var usageSum = 0f;

        var globalCapacityFactor = mechDef.Inventory
            .Select(x => x.GetComponent<CarryCapacityFactorCustom>())
            .Where(x => x != null)
            .Select(x => x.Value)
            .Aggregate(1f, (previous, value) => previous * value);

        MinHandReq CheckArm(ChassisLocations location)
        {
            var capacity = GetCarryCapacityOnLocation(mechDef, location, globalCapacityFactor);
            var usage = GetCarryUsageOnLocation(mechDef, location);

            capacitySum += capacity;
            usageSum += usage;

            if (PrecisionUtils.SmallerThan(capacity, usage))
            {
                return MinHandReq.Two;
            }
            if (PrecisionUtils.SmallerThan(0, usage))
            {
                return MinHandReq.One;
            }
            return MinHandReq.None;
        }
        left = CheckArm(ChassisLocations.LeftArm);
        right = CheckArm(ChassisLocations.RightArm);

        capacitySum += GetLiftCapacity(mechDef, globalCapacityFactor);
        usageSum += GetLiftUsage(mechDef);

        totalCapacity = capacitySum;
        totalUsage = usageSum;
    }

    private enum MinHandReq
    {
        None,
        One,
        Two
    }

    private static float GetCarryCapacityOnLocation(MechDef mechDef, ChassisLocations location, float globalCapacityFactor)
    {
        var baseCapacity = mechDef.Inventory
            .Where(x => x.MountedLocation == location)
            .Select(x => x.GetComponent<CarryCapacityOnArmChassisFactorCustom>())
            .Where(x => x != null)
            .Select(x => x.Value)
            .Aggregate(0f, (previous, value) => previous + mechDef.Chassis.Tonnage * value);

        if (PrecisionUtils.Equals(baseCapacity, 0))
        {
            return 0;
        }

        return baseCapacity * globalCapacityFactor;
    }

    private static float GetCarryUsageOnLocation(MechDef mechDef, ChassisLocations location)
    {
        return mechDef.Inventory
            .Where(x => x.MountedLocation == location)
            .Select(x => x.GetComponent<CarryUsageCustom>())
            .Where(x => x != null)
            .Select(x => x.Value)
            .Aggregate(0f, (previous, value) => previous + value);
    }

    private static float GetLiftCapacity(MechDef mechDef, float globalCapacityFactor)
    {
        var baseCapacity = mechDef.Inventory
            .Select(x => x.GetComponent<LiftCapacityOnMechChassisFactorCustom>())
            .Where(x => x != null)
            .Select(x => x.Value)
            .Aggregate(0f, (previous, value) => previous + mechDef.Chassis.Tonnage * value);

        return baseCapacity * globalCapacityFactor;
    }

    private static float GetLiftUsage(MechDef mechDef)
    {
        return mechDef.Inventory
            .Select(x => x.GetComponent<LiftUsageCustom>())
            .Where(x => x != null)
            .Select(x => x.Value)
            .Aggregate(0f, (previous, value) => previous + value);
    }
}
