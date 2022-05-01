﻿using System;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using MechEngineer.Features.ArmorStructureRatio;
using MechEngineer.Features.DynamicSlots;
using MechEngineer.Features.MechLabSlots;
using UnityEngine;
using MechEngineer.Features.OverrideTonnage;
using MechEngineer.Helper;

namespace MechEngineer.Features.ArmorMaximizer;

internal static class ArmorMaximizerHandler
{
    internal static void OnMaxArmor(MechLabPanel mechLabPanel, MechLabMechInfoWidget infoWidget)
    {
        var settings = ArmorMaximizerFeature.Shared.Settings;
        APState state = new(mechLabPanel.activeMechDef);
        if (MechDefBuilder.Locations.Any(location => mechLabPanel.GetLocationWidget(location).IsDestroyed))
        {
            return;
        }

        var locationStates = state.Locations.Values.ToArray();
        var stepSize = ArmorStructureRatioFeature.ArmorPerStep;
        var changes = false;

        while (state.Remaining >= stepSize)
        {
            Array.Sort(locationStates, (x, y) =>
            {
                var cmp = -(x.PriorityPrimary - y.PriorityPrimary);
                if (cmp != 0)
                {
                    return cmp;
                }

                return -(x.PrioritySecondary - y.PrioritySecondary);
            });

            var locationState = locationStates[0];
            {
                var location = locationState.Location;
                Control.Logger.Trace?.Log($"OnMaxArmor location={location.GetShortString()} state.Remaining={state.Remaining} stepSize={stepSize}");

                // with priority queues, we would just not re-queued any location that is Full
                if (locationState.IsFull)
                {
                    break;
                }

                locationState.Assigned += stepSize;
                state.Remaining -= stepSize;
                changes = true;

                Control.Logger.Trace?.Log($"OnMaxArmor location={location.GetShortString()} locationState={locationState}");
            }
        }

        if (!changes)
        {
            return;
        }

        void SetArmor(ChassisLocations location)
        {
            var widget = mechLabPanel.GetLocationWidget(location);
            var locationState = state.Locations[location];
            if ((location & ChassisLocations.Torso) != ChassisLocations.None)
            {
                var front = PrecisionUtils.RoundDownToInt(locationState.Assigned * settings.TorsoFrontBackRatio);
                if (PrecisionUtils.SmallerThan(5f, locationState.Assigned))
                {
                    front = (int)PrecisionUtils.RoundDown(front, ArmorStructureRatioFeature.ArmorPerStep);
                }
                var rear = locationState.Assigned - front;
                widget.SetArmor(false, front);
                widget.SetArmor(true, rear);
                Control.Logger.Trace?.Log($"SetArmor Assigned={locationState.Assigned} Max={locationState.Max} front={front} rear={rear}");
            }
            else
            {
                widget.SetArmor(false, locationState.Assigned);
                Control.Logger.Trace?.Log($"SetArmor Assigned={locationState.Assigned} Max={locationState.Max}");
            }
        }

        foreach (var location in MechDefBuilder.Locations)
        {
            SetArmor(location);
        }

        infoWidget.RefreshInfo();
        mechLabPanel.FlagAsModified();
        mechLabPanel.ValidateLoadout(false);
    }

    internal static void HandleArmorUpdate(MechLabLocationWidget widget, bool isRearArmor, float direction)
    {
        var shiftModifierPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var precision = shiftModifierPressed ? 1 : ArmorStructureRatioFeature.ArmorPerStep;
        var stepDirection = direction < 0 ? -1 : 1;
        var current = isRearArmor ? widget.currentRearArmor : widget.currentArmor;

        var updated = stepDirection > 0
            ? PrecisionUtils.RoundUp(current + 1, precision)
            : PrecisionUtils.RoundDown(current - 1, precision);

        if (stepDirection > 0)
        {
            var max = MaxForFrontOrRearArmor(widget, isRearArmor);
            updated = Mathf.Min(updated, max);
        }

        Control.Logger.Trace?.Log($"HandleArmorUpdate stepDirection={stepDirection} current={current} precision={precision} updated={updated} isRearArmor={isRearArmor}");
        widget.SetArmor(isRearArmor, updated);
    }

    internal static void OnRefreshArmor(MechLabLocationWidget widget)
    {
        void RefreshArmorBar(LanceStat armorBar, bool isRearArmor) {
            armorBar.SetTextColor(UIColor.White, UIColor.White);

            void SetButtonColor(string buttonId, UIColor uiColor)
            {
                var button = armorBar.transform.GetChild(buttonId);
                // the plus icon is actually made of two minus icons
                var icons = button.GetChild("startButtonFill").GetChildren();
                foreach (var icon in icons)
                {
                    var colorRefTracker = icon.GetComponent<UIColorRefTracker>();
                    colorRefTracker.SetUIColor(uiColor);
                }
            }

            const UIColor limitReachedColor = UIColor.MedGray;
            {
                var max = widget.useRearArmor ? MaxForFrontOrRearArmor(widget, isRearArmor) : widget.maxArmor;
                var maxReached = PrecisionUtils.SmallerOrEqualsTo(max, isRearArmor ? widget.currentRearArmor : widget.currentArmor);
                SetButtonColor("bttn_plus", maxReached ? limitReachedColor : UIColor.White);
            }
            {
                var minReached = PrecisionUtils.SmallerOrEqualsTo(isRearArmor ? widget.currentRearArmor : widget.currentArmor, 0);
                SetButtonColor("bttn_minus", minReached ? limitReachedColor : UIColor.White);
            }
        }

        RefreshArmorBar(widget.armorBar, false);
        if (widget.useRearArmor)
        {
            RefreshArmorBar(widget.rearArmorBar, true);
        }
    }

    private static int MaxForFrontOrRearArmor(MechLabLocationWidget widget, bool isRearArmor)
    {
        var max = ArmorStructureRatioFeature.GetMaximumArmorPoints(widget.chassisLocationDef);
        max -= isRearArmor
            ? PrecisionUtils.RoundUpToInt(widget.currentArmor)
            : PrecisionUtils.RoundUpToInt(widget.currentRearArmor);
        return max;
    }
}