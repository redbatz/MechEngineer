﻿using BattleTech;
using MechEngineer.Features.AutoFix;
using MechEngineer.Features.OverrideTonnage;
using UnityEngine;

namespace MechEngineer.Features.ArmorMaximizer;

public static class ArmorUtils
{
    //Takes TONNAGE_PER_ARMOR_POINT and multiplies it by the ArmorFactor provided by equipped items.
    public static float TonPerPoint(this MechDef mechDef)
    {
        float tonPerPoint = UnityGameInstance.BattleTechGame.MechStatisticsConstants.TONNAGE_PER_ARMOR_POINT;
        float armorFactor = WeightsUtils.CalculateArmorFactor(mechDef);
        float adjustedTonPerPoint = tonPerPoint * armorFactor;
        return adjustedTonPerPoint;
    }
    //The weight of the armor that is currently equipped on the Mech.
    public static float CalcArmorWeight(this MechDef mechDef)
    {
        float armorPoints = mechDef.MechDefAssignedArmor;
        float tonPerPoint = mechDef.TonPerPoint();
        float armorWeight = armorPoints * tonPerPoint;
        return armorWeight;
    }
    //Armor weight + free tonnage.  This is the amount of possible armor in tons that can be assigned.
    public static float UsableWeight(this MechDef mechDef)
    {
        float weight = CalcArmorWeight(mechDef);
        weight += WeightsUtils.CalculateFreeTonnage(mechDef);
        if (weight <= 0)
        {
            return 0;
        }
        return weight;
    }
    //Max total armor points that the Mech can have based on CBT rules as per MechEngineer.
    public static float MaxArmorPoints(this MechDef mechDef)
    {
        float headValue = mechDef.Chassis.Head.InternalStructure*3;
        float headMax = mechDef.Chassis.Head.MaxArmor;
        if (headValue > headMax)
        {
            headValue = headMax;
        }
        float maxPoints = headValue +
                          mechDef.Chassis.CenterTorso.InternalStructure * 2 +
                          mechDef.Chassis.LeftTorso.InternalStructure * 2 +
                          mechDef.Chassis.RightTorso.InternalStructure * 2 +
                          mechDef.Chassis.LeftArm.InternalStructure * 2 +
                          mechDef.Chassis.RightArm.InternalStructure * 2 +
                          mechDef.Chassis.LeftLeg.InternalStructure * 2 +
                          mechDef.Chassis.RightLeg.InternalStructure * 2;
        return maxPoints;
    }
    //Calculates available armor points based usable weight.
    public static float AvailableAP(this MechDef mechDef)
    {
        float maxAP = mechDef.MaxArmorPoints();
        float availableAP = mechDef.UsableWeight();
        availableAP /= mechDef.TonPerPoint();
        availableAP = Mathf.Floor(availableAP);
        if (availableAP > mechDef.MaxArmorPoints())
        {
            return maxAP;
        }
        return availableAP;
    }
    //Percentage equal to available armor points divided by max armor points.
    public static float ArmorMultiplier(this MechDef mechDef)
    {
        float headPoints = mechDef.Head.AssignedArmor;
        float availablePoints = mechDef.AvailableAP();
        float maxArmor = mechDef.MaxArmorPoints();
        if(ArmorMaximizerFeature.Shared.Settings.HeadPointsUnChanged)
        {
            maxArmor -= headPoints;
            availablePoints -= headPoints;
        }
        float multiplier = availablePoints / maxArmor;
        return multiplier;
    }
    //Max AP by location
    public static float CalcMaxAPbyLocation(this MechDef mechDef, LocationLoadoutDef location, LocationDef locationDef)
    {
        float maxAP = locationDef.InternalStructure * 2;
        if (location == mechDef.Head)
        {
            maxAP = locationDef.InternalStructure * 3;
            float maxArmor = mechDef.Chassis.Head.MaxArmor;
            if(maxArmor < maxAP)
            {
                maxAP = maxArmor;
            }
        }
        return maxAP;
    }
    public static float AssignAPbyLocation(this MechDef mechDef, LocationLoadoutDef location, LocationDef locationDef)
    {
        float maxAP = locationDef.InternalStructure * 2;
        float availableAP = mechDef.CalcMaxAPbyLocation(location, locationDef);
        availableAP *= mechDef.ArmorMultiplier();
        availableAP = Mathf.Floor(availableAP);

        if (location == mechDef.Head)
        {
            maxAP = locationDef.MaxArmor;
            if (ArmorMaximizerFeature.Shared.Settings.HeadPointsUnChanged)
            {
                availableAP = location.AssignedArmor;
                if(availableAP > maxAP)
                {
                    availableAP = maxAP;
                }
            }
        }
        if(availableAP > maxAP)
        {
            availableAP = maxAP;
        }
        return availableAP;
    }

    public static float CurrentArmorPoints(this MechDef mechDef)
    {
        float currentArmorPoints = mechDef.MechDefAssignedArmor;
        return currentArmorPoints;

    }
    public static bool CanMaxArmor(this MechDef mechDef)
    {
        float buffer = 15.0f;
        float adjustedTPP = mechDef.TonPerPoint();
        float headArmor = mechDef.Head.AssignedArmor;
        float minFree = (headArmor + buffer) * adjustedTPP;
        if (UsableWeight(mechDef) < minFree)
        {
            return false;
        }
        return true;
    }
    public static bool IsDivisible(float x, float y)
    {
        if (x < 0) x *= -1f;
        if (y < 0) y *= -1f;
        return (x % y) == 0.0f;
    }
    public static float RoundDown(float x, float y)
    {
        x /= y;
        x = Mathf.Floor(x);
        x *= y;
        return x;
    }
    public static float RoundUp(float x, float y)
    {
        x /= y;
        x = Mathf.Ceil(x);
        x *= y;
        return x;
    }
}