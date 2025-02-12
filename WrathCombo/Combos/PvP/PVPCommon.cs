﻿using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;
using WrathCombo.Combos.PvE;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;

namespace WrathCombo.Combos.PvP
{
    internal static class PvPCommon
    {
        public const uint
            Teleport = 5,
            Return = 6,
            StandardElixir = 29055,
            Recuperate = 29711,
            Purify = 29056,
            Guard = 29054,
            Sprint = 29057;

        internal class Config
        {
            public const string
                EmergencyHealThreshold = "EmergencyHealThreshold",
                EmergencyGuardThreshold = "EmergencyGuardThreshold",
                QuickPurifyStatuses = "QuickPurifyStatuses";
        }

        internal class Debuffs
        {
            public const ushort
                Silence = 1347,
                Bind = 1345,
                Stun = 1343,
                HalfAsleep = 3022,
                Sleep = 1348,
                DeepFreeze = 3219,
                Heavy = 1344,
                Unguarded = 3021,
                MiracleOfNature = 3085;
        }

        internal class Buffs
        {
            public const ushort
                Sprint = 1342,
                Guard = 3054;
        }

        /// <summary> Checks if the target is immune to damage. Optionally, include buffs that provide significant damage reduction. </summary>
        /// <param name="includeReductions"> Includes buffs that provide significant damage reduction. </param>
        /// <param name="optionalTarget"> Optional target to check. </param>
        public static bool TargetImmuneToDamage(bool includeReductions = true, IGameObject? optionalTarget = null)
        {
            var t = optionalTarget ?? CustomComboFunctions.CurrentTarget;
            if (t is null || !CustomComboFunctions.InPvP()) return false;

            bool targetHasReductions = CustomComboFunctions.TargetHasEffectAny(Buffs.Guard, t) || CustomComboFunctions.TargetHasEffectAny(VPRPvP.Buffs.HardenedScales, t);
            bool targetHasImmunities = CustomComboFunctions.TargetHasEffectAny(DRKPvP.Buffs.UndeadRedemption, t) || CustomComboFunctions.TargetHasEffectAny(PLDPvP.Buffs.HallowedGround, t);

            return includeReductions
                ? targetHasReductions || targetHasImmunities
                : targetHasImmunities;
        }

        // Lists of Excluded skills 
        internal static readonly List<uint>
            MovmentSkills = [WARPvP.Onslaught, VPRPvP.Slither, NINPvP.Shukuchi, DNCPvP.EnAvant, MNKPvP.ThunderClap, RDMPvP.CorpsACorps, RDMPvP.Displacement, SGEPvP.Icarus, RPRPvP.HellsIngress, RPRPvP.Regress, BRDPvP.RepellingShot, BLMPvP.AetherialManipulation, DRGPvP.ElusiveJump, GNBPvP.RoughDivide],
            GlobalSkills = [Teleport, Guard, Recuperate, Purify, StandardElixir, Sprint];

        internal class GlobalEmergencyHeals : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PvP_EmergencyHeals;

            protected override uint Invoke(uint actionID)
            {
                if ((HasEffect(Buffs.Guard) || JustUsed(Guard)) && IsEnabled(CustomComboPreset.PvP_MashCancel))
                {
                    if (actionID == Guard) return Guard;
                    return All.SavageBlade;
                }

                if (Execute() &&
                     InPvP() &&
                    !GlobalSkills.Contains(actionID) &&
                    !MovmentSkills.Contains(actionID))
                    return OriginalHook(Recuperate);

                return actionID;
            }

            public static bool Execute()
            {
                var jobMaxHp = LocalPlayer.MaxHp;
                var threshold = PluginConfiguration.GetCustomIntValue(Config.EmergencyHealThreshold);
                var maxHPThreshold = jobMaxHp - 15000;
                var remainingPercentage = (float)LocalPlayer.CurrentHp / (float)maxHPThreshold;


                if (HasEffect(3180)) return false; //DRG LB buff
                if (HasEffectAny(1420)) return false; //Rival Wings Mounted
                if (HasEffect(4096)) return false; //VPR Snakesbane
                if (HasEffect(DRKPvP.Buffs.UndeadRedemption)) return false;
                if (LocalPlayer.CurrentMp < 2500) return false;
                if (remainingPercentage * 100 > threshold) return false;

                return true;

            }
        }

        internal class GlobalEmergencyGuard : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PvP_EmergencyGuard;

            protected override uint Invoke(uint actionID)
            {
                if ((HasEffect(Buffs.Guard) || JustUsed(Guard)) && IsEnabled(CustomComboPreset.PvP_MashCancel))
                {
                    if (actionID == Guard)
                    {
                        if (IsEnabled(CustomComboPreset.PvP_MashCancelRecup) && !JustUsed(Guard, 2f) && LocalPlayer.CurrentMp >= 2500 && LocalPlayer.CurrentHp <= LocalPlayer.MaxHp - 15000) 
                            return Recuperate;
                        return Guard;
                    }
                    return All.SavageBlade;
                }

                if (Execute() &&
                    InPvP() &&
                    !GlobalSkills.Contains(actionID) &&
                    !MovmentSkills.Contains(actionID))
                    return All.SavageBlade;

                return actionID;
            }

            public static bool Execute()
            {
                var jobMaxHp = LocalPlayer.MaxHp;
                var threshold = PluginConfiguration.GetCustomIntValue(Config.EmergencyGuardThreshold);
                var remainingPercentage = (float)LocalPlayer.CurrentHp / (float)jobMaxHp;

                if (HasEffect(3180)) return false; //DRG LB buff
                if (HasEffect(4096)) return false; //VPR Snakesbane
                if (HasEffectAny(1420)) return false; //Rival Wings Mounted
                if (HasEffect(DRKPvP.Buffs.UndeadRedemption)) return false;
                if (HasEffectAny(Debuffs.Unguarded) || HasEffect(WARPvP.Buffs.InnerRelease)) return false;
                if (GetCooldown(Guard).IsCooldown) return false;
                if (remainingPercentage * 100 > threshold) return false;

                return true;

            }
        }

        internal class QuickPurify : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PvP_QuickPurify;

            protected override uint Invoke(uint actionID)
            {
                if ((HasEffect(Buffs.Guard) || JustUsed(Guard)) && IsEnabled(CustomComboPreset.PvP_MashCancel))
                {
                    if (actionID == Guard) return Guard;
                    return All.SavageBlade;
                }

                if (Execute() &&
                    InPvP() &&
                    !GlobalSkills.Contains(actionID))
                    return OriginalHook(Purify);

                return actionID;
            }

            public static bool Execute()
            {
                var selectedStatuses = PluginConfiguration.GetCustomBoolArrayValue(Config.QuickPurifyStatuses);

                if (HasEffect(3180)) return false; //DRG LB buff
                if (HasEffect(4096)) return false; //VPR Snakesbane
                if (HasEffectAny(1420)) return false; //Rival Wings Mounted

                if (selectedStatuses.Length == 0) return false;
                if (GetCooldown(Purify).IsCooldown) return false;
                if (HasEffectAny(Debuffs.Stun) && selectedStatuses[0]) return true;
                if (HasEffectAny(Debuffs.DeepFreeze) && selectedStatuses[1]) return true;
                if (HasEffectAny(Debuffs.HalfAsleep) && selectedStatuses[2]) return true;
                if (HasEffectAny(Debuffs.Sleep) && selectedStatuses[3]) return true;
                if (HasEffectAny(Debuffs.Bind) && selectedStatuses[4]) return true;
                if (HasEffectAny(Debuffs.Heavy) && selectedStatuses[5]) return true;
                if (HasEffectAny(Debuffs.Silence) && selectedStatuses[6]) return true;
                if (HasEffectAny(Debuffs.MiracleOfNature) && selectedStatuses[7]) return true;

                return false;

            }
        }

    }

}
