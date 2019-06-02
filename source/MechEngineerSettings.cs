﻿using BattleTech;
using MechEngineer.Features.ArmActuators;
using MechEngineer.Features.ArmorStructureRatio;
using MechEngineer.Features.AutoFix;
using MechEngineer.Features.CompressFloatieMessages;
using MechEngineer.Features.DebugCycleCombatSounds;
using MechEngineer.Features.DebugSaveMechToFile;
using MechEngineer.Features.Engines;
using MechEngineer.Features.HardpointFix;
using MechEngineer.Features.MechLabSlots;
using MechEngineer.Features.MoveMultiplierStat;
using MechEngineer.Features.NewSaveFolder;
using MechEngineer.Features.OverrideGhostVFX;
using MechEngineer.Features.OverrideTonnage;
using MechEngineer.Features.ShutdownInjuryProtection;
using MechEngineer.Features.TurretLimitedAmmo;

namespace MechEngineer
{
    public class AddHelper
    {
        public string ComponentDefId { get; set; }
        public ChassisLocations ChassisLocation { get; set; }
        public ComponentType ComponentType { get; set; }
    }

    internal class MechEngineerSettings
    {
        #region Settings
        public NewSaveFolderFeature.Settings NewSaveFolder = new NewSaveFolderFeature.Settings();
        public OverrideGhostVFXFeature.Settings OverrideGhostVFX = new OverrideGhostVFXFeature.Settings();
        public HardpointFixFeature.Settings HardpointFix = new HardpointFixFeature.Settings();
        public ArmActuatorFeature.Settings ArmActuator = new ArmActuatorFeature.Settings();
        public ShutdownInjuryProtectionFeature.Settings ShutdownInjuryProtection = new ShutdownInjuryProtectionFeature.Settings();
        public ArmorStructureRatioFeature.Settings ArmorStructureRatio = new ArmorStructureRatioFeature.Settings();
        public DebugSaveMechToFileFeature.Settings DebugSaveMechToFile = new DebugSaveMechToFileFeature.Settings();
        public OverrideTonnageFeature.Settings OverrideTonnage = new OverrideTonnageFeature.Settings();
        public CompressFloatieMessagesFeature.Settings CompressFloatieMessages = new CompressFloatieMessagesFeature.Settings();
        public MoveMultiplierStatFeature.Settings MoveMultiplierStat = new MoveMultiplierStatFeature.Settings();
        public TurretLimitedAmmoFeature.Settings TurretLimitedAmmo = new TurretLimitedAmmoFeature.Settings();
        public DebugCycleCombatSoundsFeature.Settings DebugCycleCombatSounds = new DebugCycleCombatSoundsFeature.Settings();
        public EngineFeature.Settings Engine = new EngineFeature.Settings();
        public MechLabSlotsFeature.Settings MechLabSlots = new MechLabSlotsFeature.Settings();
        #endregion

        #region DynamicSlots
        public bool FeatureDynamicSlotsEnabled = true;
        // MWO does not allow to drop if that would mean to go overweight
        // battletech allows overweight, to stay consistent so we also allow overspace usage by default
        // set to true to switch to MWO style
        public bool DynamicSlotsValidateDropEnabled = false;
        #endregion

        #region AccuracyEffects
        #region LocationalEffects
        public bool FeatureAccuracyEffectsEnabled = true;
        #endregion
        #endregion

        #region OverrideDesciptions
        public bool FeatureOverrideDescriptionsEnabled = true;
        public string BonusDescriptionsDescriptionTemplate = "Traits:<b><color=#F79B26FF>\r\n{{elements}}</color></b>\r\n{{originalDescription}}";
        #region CriticalEffects
        public string BonusDescriptionsElementTemplate = " <indent=10%><line-indent=-5%><line-height=65%>{{element}}</line-height></line-indent></indent>\r\n";
        #endregion
        #endregion

        #region CriticalEffects
        public string CriticalEffectsDescriptionTemplate = "Critical Effects:<b><color=#F79B26FF>\r\n{{elements}}</color></b>\r\n{{originalDescription}}";
        public bool CriticalEffectsDescriptionUseName = false;
        #region LocationalEffects
        public bool FeatureCriticalEffectsEnabled = true;
        #endregion
        #endregion

        #region AutoFix

        public string[] AutoFixMechDefSkip = { }; // mech defs to skip for AutoFixMechDef*
        public string[] AutoFixUpgradeDefSkip =
        {
            "Gear_Cockpit_SLDF_Custom",
            "Gear_Cockpit_Generic_Small",
            "Gear_Cockpit_Generic_Standard",
            "Gear_Cockpit_LifeSupportA_Standard",
            "Gear_Cockpit_LifeSupportB_Standard",
            "Gear_Cockpit_SensorsA_Standard",
            "Gear_Cockpit_SensorsB_Standard",
            "Gear_Gyro_Generic_Standard",
            "emod_arm_part_shoulder",
            "emod_arm_part_upper",
            "emod_arm_part_lower",
            "emod_arm_part_hand",
            "emod_leg_hip",
            "emod_leg_upper",
            "emod_leg_lower",
            "emod_leg_foot",
        }; // upgrades to not autofix

        public bool AutoFixMechDefEngine = true; // adds missing engine and removes too many jump jets
        public string AutoFixMechDefCoolingDef = "emod_kit_shs";
        public string AutoFixMechDefHeatBlockDef = "emod_engine_cooling";
        public string AutoFixMechDefCoreDummy = "emod_engine_dummy";

        public IdentityHelper AutoFixGyroCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.CenterTorso, // optional if category is properly setup
            ComponentType = ComponentType.Upgrade, // optional if category is properly setup
            Prefix = "Gear_Gyro_", // optional if category is properly setup
            CategoryId = "Gyro", // required
            AutoAddCategoryIdIfMissing = true // adds category id to items matched by optional filters
        };
        public ValueChange<int> AutoFixGyroSlotChange = new ValueChange<int> {From = 3, By = 1};

        public IdentityHelper AutoFixCockpitCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.Head,
            ComponentType = ComponentType.Upgrade,
            Prefix = "Gear_Cockpit_",
            CategoryId = "Cockpit",
            AutoAddCategoryIdIfMissing = true
        };
        public ValueChange<float> AutoFixCockpitTonnageChange = new ValueChange<float> {From = 0, By = 3};
        public ValueChange<int> AutoFixCockpitSlotChange = new ValueChange<int> {From = 1, By = 0};

        public IdentityHelper AutoFixLegUpgradesCategorizer = new IdentityHelper
        {
            AllowedLocations = ChassisLocations.Legs,
            ComponentType = ComponentType.Upgrade,
            Prefix = null, //"Gear_Actuator_";
            CategoryId = "LegFootActuator",
            AutoAddCategoryIdIfMissing = true
        };
        public ValueChange<int> AutoFixLegUpgradesSlotChange = new ValueChange<int>  {From = 3, By = -1, FromIsMin = true, NewMin = 1};

        public string[] AutoFixChassisDefSkip = { };

        public ChassisSlotsChange[] AutoFixChassisDefSlotsChanges =
        {
            // vanilla mechs
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftTorso,
                Change = new ValueChange<int> {From = 10, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightTorso,
                Change = new ValueChange<int> {From = 10, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftLeg,
                Change = new ValueChange<int> {From = 4, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightLeg,
                Change = new ValueChange<int> {From = 4, By = 2}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.Head,
                Change = new ValueChange<int> {From = 1, By = 5}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.CenterTorso,
                Change = new ValueChange<int> {From = 4, By = 11}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftArm,
                Change = new ValueChange<int> {From = 8, By = 4}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightArm,
                Change = new ValueChange<int> {From = 8, By = 4}
            },
            // old ME values
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftLeg,
                Change = new ValueChange<int> {From = 2, By = 4}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightLeg,
                Change = new ValueChange<int> {From = 2, By = 4}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.Head,
                Change = new ValueChange<int> {From = 3, By = 3}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.LeftArm,
                Change = new ValueChange<int> {From = 11, By = 1}
            },
            new ChassisSlotsChange
            {
                Location = ChassisLocations.RightArm,
                Change = new ValueChange<int> {From = 11, By = 1}
            },
        };

        public bool AutoFixChassisDefInitialTonnage = true;
        public float AutoFixChassisDefInitialToTotalTonnageFactor = 0.1f; // 10% structure weight
        public bool AutoFixChassisDefMaxJumpjets = true;
        public int AutoFixChassisDefMaxJumpjetsCount = 8;
        public int AutoFixChassisDefMaxJumpjetsRating = 400;

        public WeaponSlotChange[] AutoFixWeaponDefSlotsChanges =
        {
            new WeaponSlotChange
            {
                Type = WeaponSubType.AC5,
                Change = new ValueChange<int> {From = 2, By = 2}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.AC10,
                Change = new ValueChange<int> {From = 3, By = 4}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.AC20,
                Change = new ValueChange<int> {From = 4, By = 6}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.Gauss,
                Change = new ValueChange<int> {From = 5, By = 2}
            },
            new WeaponSlotChange
            {
                Type = WeaponSubType.LRM20,
                Change = new ValueChange<int> {From = 4, By = 1}
            }
        };

        #endregion
    }
}
