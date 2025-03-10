#region Dependencies
using System.Linq;
using WrathCombo.Combos.PvE.Content;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
#endregion

namespace WrathCombo.Combos.PvE;

internal partial class GNB
{
    #region Simple Mode - Single Target
    internal class GNB_ST_Simple : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_Simple;
        protected override uint Invoke(uint actionID)
        {
            //Our Button
            if (actionID is not KeenEdge)
                return actionID;

            #region Non-Standard
            //Variant
            uint variantAction = GetVariantAction();
            if (variantAction != 0)
                return variantAction;

            //Bozja
            if (Bozja.IsInBozja)
            {
                uint bozjaAction = GetBozjaAction();
                if (bozjaAction != 0)
                    return bozjaAction;
            }

            //Interject
            if (ActionReady(All.Interject) && CanInterruptEnemy())
                return All.Interject;

            #region Mitigations
            if (Config.GNB_ST_MitsOptions != 1)
            {
                if (InCombat() && !MitUsed)
                {
                    if (ActionReady(Superbolide) && HPP < 30)
                        return Superbolide;
                    if (IsPlayerTargeted())
                    {
                        if (ActionReady(OriginalHook(Nebula)) && HPP < 60)
                            return OriginalHook(Nebula);
                        if (ActionReady(All.Rampart) && HPP < 80)
                            return All.Rampart;
                        if (ActionReady(All.Reprisal) && InActionRange(All.Reprisal) && HPP < 90)
                            return All.Reprisal;
                    }
                    if (ActionReady(Camouflage) && HPP < 70)
                        return Camouflage;
                    if (ActionReady(OriginalHook(HeartOfStone)) && HPP < 90)
                        return OriginalHook(HeartOfStone);
                    if (ActionReady(Aurora) && !(HasEffect(Buffs.Aurora) || TargetHasEffectAny(Buffs.Aurora)) && HPP < 85)
                        return Aurora;
                }
            }
            #endregion

            #endregion

            #region Standard
            if (ShouldUseLightningShot())
                return LightningShot;
            if (ShouldUseNoMercy())
                return NoMercy;
            if (JustUsed(BurstStrike, 5f) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast) && NmCD is > 1 or <= 0.1f)
                return Hypervelocity;
            if (ShouldUseContinuation())
                return OriginalHook(Continuation);
            if (ShouldUseBloodfest())
                return Bloodfest;
            if (ShouldUseZone())
                return OriginalHook(DangerZone);
            if (ShouldUseBowShock())
                return BowShock;
            if (LevelChecked(DoubleDown) && HasNM && GunStep == 0 && ComboAction is BrutalShell && Ammo == 1)
                return SolidBarrel;
            if (ShouldUseGnashingFang())
                return GnashingFang;
            if (ShouldUseDoubleDown())
                return DoubleDown;
            if (ShouldUseSonicBreak())
                return SonicBreak;
            if (ShouldUseReignOfBeasts())
                return ReignOfBeasts;
            if (ShouldUseBurstStrike())
                return BurstStrike;
            if (GunStep is 1 or 2)
                return OriginalHook(GnashingFang);
            if (GunStep is 3 or 4)
                return OriginalHook(ReignOfBeasts);
            if (ComboTimer > 0)
            {
                if (LevelChecked(BrutalShell) && ComboAction == KeenEdge)
                    return BrutalShell;
                if (LevelChecked(SolidBarrel) && ComboAction == BrutalShell)
                {
                    if (LevelChecked(Hypervelocity) &&  HasEffect(Buffs.ReadyToBlast) && NmCD is > 1 or <= 0.1f)
                        return Hypervelocity;
                    return SolidBarrel;
                }
            }
            #endregion

            return KeenEdge;
        }
    }
    #endregion

    #region Advanced Mode - Single Target
    internal class GNB_ST_Advanced : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_ST_Advanced;
        protected override uint Invoke(uint actionID)
        {
            //Our Button
            if (actionID is not KeenEdge)
                return actionID;

            #region Non-Standard
            //Variant
            uint variantAction = GetVariantAction();
            if (variantAction != 0)
                return variantAction;

            //Bozja
            if (Bozja.IsInBozja)
            {
                uint bozjaAction = GetBozjaAction();
                if (bozjaAction != 0)
                    return bozjaAction;
            }

            //Interject
            if (IsEnabled(CustomComboPreset.GNB_ST_Interrupt) && ActionReady(All.Interject) && CanInterruptEnemy())
                return All.Interject;

            #region Mitigations
            if (IsEnabled(CustomComboPreset.GNB_ST_Mitigation) && InCombat() && !MitUsed)
            {
                if (IsEnabled(CustomComboPreset.GNB_ST_Superbolide) && ActionReady(Superbolide) && HPP < Config.GNB_ST_Superbolide_Health &&
                    (Config.GNB_ST_Superbolide_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Superbolide_SubOption == 1))
                    return Superbolide;
                if (IsPlayerTargeted())
                {
                    if (IsEnabled(CustomComboPreset.GNB_ST_Nebula) && ActionReady(OriginalHook(Nebula)) && HPP < Config.GNB_ST_Nebula_Health &&
                        (Config.GNB_ST_Nebula_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Nebula_SubOption == 1))
                        return OriginalHook(Nebula);
                    if (IsEnabled(CustomComboPreset.GNB_ST_Rampart) && ActionReady(All.Rampart) && HPP < Config.GNB_ST_Rampart_Health &&
                        (Config.GNB_ST_Rampart_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Rampart_SubOption == 1))
                        return All.Rampart;
                    if (IsEnabled(CustomComboPreset.GNB_ST_Reprisal) && ActionReady(All.Reprisal) && InActionRange(All.Reprisal) && HPP < Config.GNB_ST_Reprisal_Health &&
                        (Config.GNB_ST_Reprisal_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Reprisal_SubOption == 1))
                        return All.Reprisal;
                    if (IsEnabled(CustomComboPreset.GNB_ST_ArmsLength) && ActionReady(All.ArmsLength) && HPP < Config.GNB_ST_ArmsLength_Health && !InBossEncounter())
                        return All.ArmsLength;
                }
                if (IsEnabled(CustomComboPreset.GNB_ST_Camouflage) && ActionReady(Camouflage) && HPP < Config.GNB_ST_Camouflage_Health &&
                    (Config.GNB_ST_Camouflage_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Camouflage_SubOption == 1))
                    return Camouflage;
                if (IsEnabled(CustomComboPreset.GNB_ST_Corundum) && ActionReady(OriginalHook(HeartOfStone)) && HPP < Config.GNB_ST_Corundum_Health &&
                    (Config.GNB_ST_Corundum_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Corundum_SubOption == 1))
                    return OriginalHook(HeartOfStone);
                if (IsEnabled(CustomComboPreset.GNB_ST_Aurora) && ActionReady(Aurora) && !(HasEffect(Buffs.Aurora) || TargetHasEffectAny(Buffs.Aurora)) && GetRemainingCharges(Aurora) > Config.GNB_ST_Aurora_Charges && HPP < Config.GNB_ST_Aurora_Health &&
                    (Config.GNB_ST_Aurora_SubOption == 0 || TargetIsBoss() && Config.GNB_ST_Aurora_SubOption == 1))
                    return Aurora;
            }
            #endregion

            #endregion

            #region Standard
            if (IsEnabled(CustomComboPreset.GNB_ST_Advanced_Opener) &&
                Opener().FullOpener(ref actionID))
                return actionID;
            if (IsEnabled(CustomComboPreset.GNB_ST_RangedUptime) && ShouldUseLightningShot())
                return LightningShot;
            if (IsEnabled(CustomComboPreset.GNB_ST_NoMercy) && ShouldUseNoMercy() &&
                (Config.GNB_ST_NoMercy_SubOption == 0 || Config.GNB_ST_NoMercy_SubOption == 1 && InBossEncounter()))
                return NoMercy;
            if (IsEnabled(CustomComboPreset.GNB_ST_Advanced_Cooldowns) && IsEnabled(CustomComboPreset.GNB_ST_Continuation) &&
                JustUsed(BurstStrike, 5f) &&  LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy) && NmCD is > 1 or <= 0.1f)
                return Hypervelocity;
            if (IsEnabled(CustomComboPreset.GNB_ST_Advanced_Cooldowns) && IsEnabled(CustomComboPreset.GNB_ST_Continuation) && ShouldUseContinuation())
                return OriginalHook(Continuation);
            if (LevelChecked(DoubleDown) && HasNM && GunStep == 0 && ComboAction is BrutalShell && Ammo == 1)
                return SolidBarrel;
            if (IsEnabled(CustomComboPreset.GNB_ST_Advanced_Cooldowns))
            {
                if (IsEnabled(CustomComboPreset.GNB_ST_Bloodfest) && ShouldUseBloodfest())
                    return Bloodfest;
                if (IsEnabled(CustomComboPreset.GNB_ST_Zone) && ShouldUseZone())
                    return OriginalHook(DangerZone);
                if (IsEnabled(CustomComboPreset.GNB_ST_BowShock) && ShouldUseBowShock())
                    return BowShock;
                if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang) && ShouldUseGnashingFang())
                    return GnashingFang;
                if (IsEnabled(CustomComboPreset.GNB_ST_DoubleDown) && ShouldUseDoubleDown())
                    return DoubleDown;
                if (IsEnabled(CustomComboPreset.GNB_ST_SonicBreak) && ShouldUseSonicBreak())
                    return SonicBreak;
                if (IsEnabled(CustomComboPreset.GNB_ST_Reign) && ShouldUseReignOfBeasts())
                    return OriginalHook(ReignOfBeasts);
                if (ShouldUseBurstStrike())
                    return BurstStrike;
            }
            if (IsEnabled(CustomComboPreset.GNB_ST_GnashingFang) && GunStep is 1 or 2)
                return OriginalHook(GnashingFang);
            if (IsEnabled(CustomComboPreset.GNB_ST_Reign) && GunStep is 3 or 4)
                return OriginalHook(ReignOfBeasts);
            if (ComboTimer > 0)
            {
                if (LevelChecked(BrutalShell) && ComboAction == KeenEdge)
                    return BrutalShell;
                if (LevelChecked(SolidBarrel) && ComboAction == BrutalShell)
                {
                    if (IsEnabled(CustomComboPreset.GNB_ST_Continuation) && IsEnabled(CustomComboPreset.GNB_ST_NoMercy) &&
                        LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast) && NmCD is > 1 or <= 0.1f &&
                        (Config.GNB_ST_NoMercy_SubOption == 0 || (Config.GNB_ST_NoMercy_SubOption == 1 && InBossEncounter())))
                        return Hypervelocity;
                    return SolidBarrel;
                }
            }
            #endregion

            return KeenEdge;
        }
    }
    #endregion

    #region Simple Mode - AoE

    internal class GNB_AoE_Simple : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AoE_Simple;
        protected override uint Invoke(uint actionID)
        {
            //Our button
            if (actionID is not DemonSlice)
                return actionID;

            #region Non-Standard
            //Variant
            uint variantAction = GetVariantAction();
            if (variantAction != 0)
                return variantAction;

            //Bozja
            if (Bozja.IsInBozja)
            {
                uint bozjaAction = GetBozjaAction();
                if (bozjaAction != 0)
                    return bozjaAction;
            }

            #region Stuns
            if (ActionReady(All.Interject) && CanInterruptEnemy())
                return All.Interject;
            if (ActionReady(All.LowBlow) && TargetIsCasting())
                return All.LowBlow;
            #endregion

            #region Mitigations
            if (Config.GNB_AoE_MitsOptions != 1)
            {
                if (InCombat() && !MitUsed)
                {
                    if (ActionReady(Superbolide) && HPP < 30)
                        return Superbolide;
                    if (IsPlayerTargeted())
                    {
                        if (ActionReady(OriginalHook(Nebula)) && HPP < 60)
                            return OriginalHook(Nebula);
                        if (ActionReady(All.Rampart) && HPP < 80)
                            return All.Rampart;
                        if (ActionReady(All.Reprisal) && InActionRange(All.Reprisal) && HPP < 90)
                            return All.Reprisal;
                    }
                    if (ActionReady(Camouflage) && HPP < 70)
                        return Camouflage;
                    if (ActionReady(OriginalHook(HeartOfStone)) && HPP < 90)
                        return OriginalHook(HeartOfStone);
                    if (ActionReady(Aurora) && !(HasEffect(Buffs.Aurora) || TargetHasEffectAny(Buffs.Aurora)) && HPP < 85)
                        return Aurora;
                }
            }
            #endregion
            #endregion

            #region Standard
            if (InCombat())
            {
                if (CanWeave())
                {
                    if (ActionReady(NoMercy) && GetTargetHPPercent() > 10)
                        return NoMercy;
                    if (LevelChecked(FatedBrand) && HasEffect(Buffs.ReadyToRaze))
                        return FatedBrand;
                }
                if (ShouldUseBowShock())
                    return BowShock;
                if (ShouldUseZone())
                    return OriginalHook(DangerZone);
                if (ShouldUseBloodfest())
                    return Bloodfest;
                if (CanBreak && HasNM && !HasEffect(Buffs.ReadyToRaze))
                    return SonicBreak;
                if (CanDD && HasNM)
                    return DoubleDown;
                if (CanReign || GunStep is 3 or 4)
                    return OriginalHook(ReignOfBeasts);
                if (CanFC && ((HasNM && ((IsOnCooldown(DoubleDown) || !LevelChecked(DoubleDown)) && GunStep == 0)) || BfCD < 6))
                    return FatedCircle;
                if (Ammo > 0 && !LevelChecked(FatedCircle) && LevelChecked(BurstStrike) && HasNM && GunStep == 0)
                    return BurstStrike;
            }
            if (ComboTimer > 0)
            {
                if (ComboAction == DemonSlice && LevelChecked(DemonSlaughter))
                {
                    if (Ammo == MaxCartridges())
                    {
                        if (LevelChecked(FatedCircle))
                            return FatedCircle;
                        if (!LevelChecked(FatedCircle))
                            return BurstStrike;
                    }
                    if (Ammo != MaxCartridges())
                        return DemonSlaughter;
                }
            }
            #endregion

            return DemonSlice;
        }
    }

    #endregion

    #region Advanced Mode - AoE

    internal class GNB_AoE_Advanced : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AoE_Advanced;
        protected override uint Invoke(uint actionID)
        {
            //Our Button
            if (actionID is not DemonSlice)
                return actionID;

            #region Non-Standard
            //Variant
            uint variantAction = GetVariantAction();
            if (variantAction != 0)
                return variantAction;

            //Bozja
            if (Bozja.IsInBozja)
            {
                uint bozjaAction = GetBozjaAction();
                if (bozjaAction != 0)
                    return bozjaAction;
            }

            #region Stuns
            if (IsEnabled(CustomComboPreset.GNB_AoE_Interrupt) && ActionReady(All.Interject) && CanInterruptEnemy())
                return All.Interject;
            if (IsEnabled(CustomComboPreset.GNB_AoE_Stun) && ActionReady(All.LowBlow) && TargetIsCasting())
                return All.LowBlow;
            #endregion

            #region Mitigations
            if (IsEnabled(CustomComboPreset.GNB_AoE_Mitigation) && InCombat() && !MitUsed)
            {
                if (IsEnabled(CustomComboPreset.GNB_AoE_Superbolide) && ActionReady(Superbolide) && HPP < Config.GNB_AoE_Superbolide_Health &&
                    (Config.GNB_AoE_Superbolide_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Superbolide_SubOption == 1)))
                    return Superbolide;
                if (IsPlayerTargeted())
                {
                    if (IsEnabled(CustomComboPreset.GNB_AoE_Nebula) && ActionReady(OriginalHook(Nebula)) && HPP < Config.GNB_AoE_Nebula_Health &&
                        (Config.GNB_AoE_Nebula_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Nebula_SubOption == 1)))
                        return OriginalHook(Nebula);
                    if (IsEnabled(CustomComboPreset.GNB_AoE_Rampart) && ActionReady(All.Rampart) && HPP < Config.GNB_AoE_Rampart_Health &&
                        (Config.GNB_AoE_Rampart_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Rampart_SubOption == 1)))
                        return All.Rampart;
                    if (IsEnabled(CustomComboPreset.GNB_AoE_Reprisal) && ActionReady(All.Reprisal) && InActionRange(All.Reprisal) && HPP < Config.GNB_AoE_Reprisal_Health &&
                        (Config.GNB_AoE_Reprisal_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Reprisal_SubOption == 1)))
                        return All.Reprisal;
                    if (IsEnabled(CustomComboPreset.GNB_AoE_ArmsLength) && ActionReady(All.ArmsLength) && HPP < Config.GNB_AoE_ArmsLength_Health && !InBossEncounter())
                        return All.ArmsLength;
                }

                if (IsEnabled(CustomComboPreset.GNB_AoE_Camouflage) && ActionReady(Camouflage) && HPP < Config.GNB_AoE_Camouflage_Health &&
                    (Config.GNB_AoE_Camouflage_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Camouflage_SubOption == 1)))
                    return Camouflage;
                if (IsEnabled(CustomComboPreset.GNB_AoE_Corundum) && ActionReady(OriginalHook(HeartOfStone)) && HPP < Config.GNB_AoE_Corundum_Health &&
                    (Config.GNB_AoE_Corundum_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Corundum_SubOption == 1)))
                    return OriginalHook(HeartOfStone);
                if (IsEnabled(CustomComboPreset.GNB_AoE_Aurora) && ActionReady(Aurora) && GetRemainingCharges(Aurora) > Config.GNB_AoE_Aurora_Charges &&
                    !(HasEffect(Buffs.Aurora) || TargetHasEffectAny(Buffs.Aurora)) && HPP < Config.GNB_AoE_Aurora_Health &&
                    (Config.GNB_AoE_Aurora_SubOption == 0 || (TargetIsBoss() && Config.GNB_AoE_Aurora_SubOption == 1)))
                    return Aurora;
            }
            #endregion

            #endregion

            #region Standard
            if (InCombat())
            {
                if (CanWeave())
                {
                    if (IsEnabled(CustomComboPreset.GNB_AoE_NoMercy) && ActionReady(NoMercy) && GetTargetHPPercent() > NmStop)
                        return NoMercy;
                    if (IsEnabled(CustomComboPreset.GNB_AoE_BowShock) && ShouldUseBowShock())
                        return BowShock;
                    if (IsEnabled(CustomComboPreset.GNB_AoE_Zone) && ShouldUseZone())
                        return OriginalHook(DangerZone);
                    if (IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && ShouldUseBloodfest())
                        return Bloodfest;
                    if (LevelChecked(FatedBrand) && HasEffect(Buffs.ReadyToRaze))
                        return FatedBrand;
                }
                if (IsEnabled(CustomComboPreset.GNB_AoE_SonicBreak) && CanBreak && HasNM && !HasEffect(Buffs.ReadyToRaze))
                    return SonicBreak;
                if (IsEnabled(CustomComboPreset.GNB_AoE_DoubleDown) && CanDD && HasNM)
                    return DoubleDown;
                if (IsEnabled(CustomComboPreset.GNB_AoE_Reign) && (CanReign || GunStep is 3 or 4))
                    return OriginalHook(ReignOfBeasts);
                if (IsEnabled(CustomComboPreset.GNB_AoE_FatedCircle) && CanFC &&  (HasNM && !ActionReady(DoubleDown) && GunStep == 0 || IsEnabled(CustomComboPreset.GNB_AoE_Bloodfest) && BfCD < 6))
                    return FatedCircle;
                if (IsEnabled(CustomComboPreset.GNB_AoE_noFatedCircle) && Ammo > 0 && !LevelChecked(FatedCircle) && LevelChecked(BurstStrike) && HasNM && GunStep == 0)
                    return BurstStrike;
            }
            if (ComboTimer > 0)
            {
                if (ComboAction == DemonSlice && LevelChecked(DemonSlaughter))
                {
                    if (Ammo == MaxCartridges())
                    {
                        if (IsEnabled(CustomComboPreset.GNB_AoE_Overcap) && LevelChecked(FatedCircle))
                            return FatedCircle;
                        if (IsEnabled(CustomComboPreset.GNB_AoE_BSOvercap) && !LevelChecked(FatedCircle))
                            return BurstStrike;
                    }
                    if (Ammo != MaxCartridges() || (Ammo == MaxCartridges() && !LevelChecked(FatedCircle) && !IsEnabled(CustomComboPreset.GNB_AoE_BSOvercap)))
                        return DemonSlaughter;
                }
            }
            #endregion

            return DemonSlice;
        }
    }

    #endregion

    #region Gnashing Fang Features
    internal class GNB_GF_Features : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_GF_Features;
        protected override uint Invoke(uint actionID)
        {
            bool gFchoice = Config.GNB_GF_Features_Choice == 0; //Gnashing Fang as button
            bool nMchoice = Config.GNB_GF_Features_Choice == 1; //No Mercy as button

            //Our Button
            if (gFchoice && actionID is not GnashingFang ||
                nMchoice && actionID is not NoMercy)
                return actionID;

            if (IsEnabled(CustomComboPreset.GNB_GF_NoMercy) && ShouldUseNoMercy())
                return NoMercy;
            if (IsEnabled(CustomComboPreset.GNB_GF_Continuation) && JustUsed(BurstStrike, 5f) && LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast) && IsEnabled(CustomComboPreset.GNB_GF_NoMercy) && NmCD is > 1 or <= 0.1f)
                return Hypervelocity;
            if (IsEnabled(CustomComboPreset.GNB_GF_Continuation) && ShouldUseContinuation())
                return OriginalHook(Continuation);
            if (LevelChecked(DoubleDown) && HasNM && GunStep == 0 && ComboAction is BrutalShell && Ammo == 1)
                return SolidBarrel;
            if (IsEnabled(CustomComboPreset.GNB_GF_Bloodfest) && ShouldUseBloodfest())
                return Bloodfest;
            if (IsEnabled(CustomComboPreset.GNB_GF_Zone) && ShouldUseZone())
                return OriginalHook(DangerZone);
            if (IsEnabled(CustomComboPreset.GNB_GF_BowShock) && ShouldUseBowShock())
                return BowShock;
            if (IsEnabled(CustomComboPreset.GNB_GF_Features) && ShouldUseGnashingFang())
                return GnashingFang;
            if (IsEnabled(CustomComboPreset.GNB_GF_DoubleDown) && ShouldUseDoubleDown())
                return DoubleDown;
            if (IsEnabled(CustomComboPreset.GNB_GF_SonicBreak) && ShouldUseSonicBreak())
                return SonicBreak;
            if (IsEnabled(CustomComboPreset.GNB_GF_Reign) && ShouldUseReignOfBeasts())
                return OriginalHook(ReignOfBeasts);
            if (ShouldUseBurstStrike())
                return BurstStrike;
            if (IsEnabled(CustomComboPreset.GNB_GF_Features) && GunStep is 1 or 2)
                return OriginalHook(GnashingFang);
            if (IsEnabled(CustomComboPreset.GNB_GF_Reign) && GunStep is 3 or 4)
                return OriginalHook(ReignOfBeasts);

            return actionID;
        }
    }
    #endregion

    #region Burst Strike Features
    internal class GNB_BS_Features : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_BS_Features;
        protected override uint Invoke(uint actionID)
        {
            //Our Button
            if (actionID is not BurstStrike) 
                return actionID;

            if (IsEnabled(CustomComboPreset.GNB_BS_Continuation))
            {
                if (IsEnabled(CustomComboPreset.GNB_BS_Hypervelocity) && LevelChecked(Hypervelocity) && (JustUsed(BurstStrike, 1) || HasEffect(Buffs.ReadyToBlast)))
                    return Hypervelocity;
                if (!IsEnabled(CustomComboPreset.GNB_BS_Hypervelocity) && CanContinue && (HasEffect(Buffs.ReadyToRip) || HasEffect(Buffs.ReadyToTear) || HasEffect(Buffs.ReadyToGouge) || (LevelChecked(Hypervelocity) && HasEffect(Buffs.ReadyToBlast))))
                    return OriginalHook(Continuation);
            }
            if (IsEnabled(CustomComboPreset.GNB_BS_Bloodfest) && ShouldUseBloodfest())
                return Bloodfest;
            if (IsEnabled(CustomComboPreset.GNB_BS_DoubleDown) && CanDD && Ammo == 1)
                return DoubleDown;
            if (IsEnabled(CustomComboPreset.GNB_BS_GnashingFang) && (CanGF || GunStep is 1 or 2))
                return OriginalHook(GnashingFang);
            //TODO: add prio hack to get rid of this redundant check
            if (IsEnabled(CustomComboPreset.GNB_BS_DoubleDown) && CanDD && Ammo > 1)
                return DoubleDown;
            if (IsEnabled(CustomComboPreset.GNB_BS_Reign) && (CanReign || GunStep is 3 or 4))
                return OriginalHook(ReignOfBeasts);

            return actionID;
        }
    }
    #endregion

    #region Fated Circle Features
    internal class GNB_FC_Features : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_FC_Features;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FatedCircle) return actionID;

            if (IsEnabled(CustomComboPreset.GNB_FC_Continuation) && HasEffect(Buffs.ReadyToRaze) && LevelChecked(FatedBrand))
                return FatedBrand;
            if (IsEnabled(CustomComboPreset.GNB_FC_DoubleDown) && IsEnabled(CustomComboPreset.GNB_FC_DoubleDown_NM) && CanDD && HasNM)
                return DoubleDown;
            if (IsEnabled(CustomComboPreset.GNB_FC_Bloodfest) && ShouldUseBloodfest())
                return Bloodfest;
            if (IsEnabled(CustomComboPreset.GNB_FC_BowShock) && CanBow)
                return BowShock;
            if (IsEnabled(CustomComboPreset.GNB_FC_DoubleDown) && !IsEnabled(CustomComboPreset.GNB_FC_DoubleDown_NM) && CanDD)
                return DoubleDown;
            if (IsEnabled(CustomComboPreset.GNB_FC_Reign) && (CanReign || GunStep is 3 or 4))
                return OriginalHook(ReignOfBeasts);

            return actionID;
        }
    }
    #endregion

    #region No Mercy Features
    internal class GNB_NM_Features : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_NM_Features;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not NoMercy)
                return actionID;

            if ((Config.GNB_NM_Features_Weave == 0 && CanWeave()) || Config.GNB_NM_Features_Weave == 1)
            {
                if (IsEnabled(CustomComboPreset.GNB_NM_Continuation) && (ShouldUseContinuation() || (LevelChecked(FatedBrand) && HasEffect(Buffs.ReadyToRaze))))
                    return OriginalHook(Continuation);
                if (IsEnabled(CustomComboPreset.GNB_NM_Bloodfest) && ShouldUseBloodfest())
                    return Bloodfest;
                if (IsEnabled(CustomComboPreset.GNB_NM_BowShock) && ShouldUseBowShock())
                    return BowShock;
                if (IsEnabled(CustomComboPreset.GNB_NM_Zone) && ShouldUseZone())
                    return OriginalHook(DangerZone);
            }

            return actionID;
        }
    }
    #endregion

    #region Aurora Protection

    internal class GNB_AuroraProtection : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_AuroraProtection;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Aurora)
                return actionID;

            if (HasFriendlyTarget() && TargetHasEffectAny(Buffs.Aurora) ||
                !HasFriendlyTarget() && HasEffectAny(Buffs.Aurora))
                return All.SavageBlade;

            return actionID;
        }
    }

    #endregion

    #region One-Button Mitigation

    internal class GNB_Mit_OneButton : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNB_Mit_OneButton;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Camouflage)
                return actionID;

            if (IsEnabled(CustomComboPreset.GNB_Mit_Superbolide_Max) &&
                ActionReady(Superbolide) &&
                HPP <= Config.GNB_Mit_Superbolide_Health &&
                ContentCheck.IsInConfiguredContent(
                    Config.GNB_Mit_Superbolide_Difficulty,
                    Config.GNB_Mit_Superbolide_DifficultyListSet
                ))
                return Superbolide;

            foreach(int priority in Config.GNB_Mit_Priorities.Items.OrderBy(x => x))
            {
                int index = Config.GNB_Mit_Priorities.IndexOf(priority);
                if (CheckMitigationConfigMeetsRequirements(index, out uint action))
                    return action;
            }

            return actionID;
        }
    }

    #endregion
}
