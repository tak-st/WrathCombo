using WrathCombo.Combos.PvE.Content;
using WrathCombo.CustomComboNS;
namespace WrathCombo.Combos.PvE;

internal partial class MNK
{
    internal class MNK_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MNK_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Bootshine or LeapingOpo))
                return actionID;

            if ((!InCombat() || !InMeleeRange()) &&
                Gauge.Chakra < 5 &&
                !HasEffect(Buffs.RiddleOfFire) &&
                LevelChecked(SteeledMeditation))
                return OriginalHook(SteeledMeditation);

            if (!InCombat() && LevelChecked(FormShift) &&
                !HasEffect(Buffs.FormlessFist) && !HasEffect(Buffs.PerfectBalance))
                return FormShift;

            //Variant Cure
            if (IsEnabled(CustomComboPreset.MNK_Variant_Cure) &&
                IsEnabled(Variant.VariantCure) &&
                PlayerHealthPercentageHp() <= Config.MNK_VariantCure)
                return Variant.VariantCure;

            if (ActionReady(RiddleOfFire) &&
                !HasEffect(Buffs.FiresRumination) &&
                CanDelayedWeave())
                return RiddleOfFire;

            // OGCDs
            if (CanWeave())
            {
                //Variant Rampart
                if (IsEnabled(CustomComboPreset.MNK_Variant_Rampart) &&
                    IsEnabled(Variant.VariantRampart) &&
                    IsOffCooldown(Variant.VariantRampart))
                    return Variant.VariantRampart;

                if (ActionReady(Brotherhood))
                    return Brotherhood;

                if (ActionReady(RiddleOfWind) &&
                    !HasEffect(Buffs.WindsRumination))
                    return RiddleOfWind;

                //Perfect Balance
                if (UsePerfectBalance())
                    return PerfectBalance;

                if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= 25 && ActionReady(All.SecondWind))
                    return All.SecondWind;

                if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= 40 && ActionReady(All.Bloodbath))
                    return All.Bloodbath;

                if (Gauge.Chakra >= 5 && InCombat() && LevelChecked(SteeledMeditation))
                    return OriginalHook(SteeledMeditation);
            }

            // GCDs
            if (HasEffect(Buffs.FormlessFist))
                return Gauge.OpoOpoFury == 0
                    ? OriginalHook(DragonKick)
                    : OriginalHook(Bootshine);

            // Masterful Blitz
            if (LevelChecked(MasterfulBlitz) &&
                !HasEffect(Buffs.PerfectBalance) &&
                !IsOriginal(MasterfulBlitz))
                return OriginalHook(MasterfulBlitz);

            // Perfect Balance
            if (HasEffect(Buffs.PerfectBalance))
            {
                #region Open Lunar

                if (!LunarNadi || BothNadisOpen || !SolarNadi && !LunarNadi)
                    return Gauge.OpoOpoFury == 0
                        ? OriginalHook(DragonKick)
                        : OriginalHook(Bootshine);

                #endregion

                #region Open Solar

                if (!SolarNadi && !BothNadisOpen)
                {
                    if (CoeurlChakra == 0)
                        return Gauge.CoeurlFury == 0
                            ? Demolish
                            : OriginalHook(SnapPunch);

                    if (RaptorChakra == 0)
                        return Gauge.RaptorFury == 0
                            ? TwinSnakes
                            : OriginalHook(TrueStrike);

                    if (OpoOpoChakra == 0)
                        return Gauge.OpoOpoFury == 0
                            ? OriginalHook(DragonKick)
                            : OriginalHook(Bootshine);
                }

                #endregion
            }

            if (HasEffect(Buffs.FiresRumination) &&
                !HasEffect(Buffs.PerfectBalance) &&
                !HasEffect(Buffs.FormlessFist) &&
                (JustUsed(OriginalHook(Bootshine)) ||
                 JustUsed(OriginalHook(DragonKick))))
                return FiresReply;

            if (HasEffect(Buffs.WindsRumination) &&
                LevelChecked(WindsReply) &&
                HasEffect(Buffs.RiddleOfWind) &&
                GetBuffRemainingTime(Buffs.WindsRumination) < 4)
                return WindsReply;

            // Standard Beast Chakras
            return DetermineCoreAbility(actionID, true);
        }
    }

    internal class MNK_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MNK_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Bootshine or LeapingOpo or DragonKick or Thunderclap))
                return actionID;
            
            bool canMelee = HasBattleTarget() && InMeleeRange();
            bool isWindBh = (HasEffect(Buffs.Brotherhood) && JustUsed(RiddleOfWind, 15 + (20 - GetBuffRemainingTime(Buffs.Brotherhood))));
            bool readyBlitz = Gauge.BlitzTimeRemaining > 0;

            double remainingSec = actionID is Thunderclap && GetTargetDistance() > 20 ? (GetTargetDistance() - 20) / 6.13 : 0;

            if (actionID is Thunderclap && GetRemainingCharges(Thunderclap) > 0 && !canMelee && (!HasBattleTarget() || remainingSec < 0.75))
                return actionID;
                
            if (IsEnabled(CustomComboPreset.MNK_STUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_STUseFiresReply) &&
                HasEffect(Buffs.FiresRumination) &&
                HasBattleTarget() &&
                (GetBuffRemainingTime(Buffs.FiresRumination) < 2 && GetTargetDistance() <= 20))
                return FiresReply;

            if (IsEnabled(CustomComboPreset.MNK_STUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_STUseWindsReply) &&
                HasBattleTarget() &&
                HasEffect(Buffs.WindsRumination) &&
                LevelChecked(WindsReply) &&
                HasEffect(Buffs.RiddleOfWind) &&
                (GetBuffRemainingTime(Buffs.WindsRumination) < 2 || !InMeleeRange()) && GetTargetDistance() <= 10)
                return WindsReply;
                
            if (IsEnabled(CustomComboPreset.MNK_STUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_STUseFiresReply) &&
                HasEffect(Buffs.FiresRumination) &&
                HasBattleTarget() && !InMeleeRange() && GetTargetDistance() <= 20)
                return FiresReply;

            if (IsEnabled(CustomComboPreset.MNK_STUseMeditation) &&
                (!InCombat() || !HasBattleTarget() || !InMeleeRange()) &&
                Gauge.Chakra < 5 &&
                !HasEffect(Buffs.Brotherhood) &&
                LevelChecked(SteeledMeditation))
                return OriginalHook(SteeledMeditation);

            if (IsEnabled(CustomComboPreset.MNK_STUseFormShift) &&
                (!InCombat() || !HasBattleTarget() || !InMeleeRange()) && LevelChecked(FormShift) &&
                !HasEffect(Buffs.FormlessFist) && !HasEffect(Buffs.PerfectBalance) && (!isWindBh || readyBlitz) &&
                (!LevelChecked(RiddleOfFire) || !HasEffect(Buffs.RiddleOfFire) || GetBuffRemainingTime(Buffs.RiddleOfFire) <= 18) &&
                (actionID is not Thunderclap || remainingSec >= GCD - 0.25))
                return FormShift;
            
            if (IsEnabled(CustomComboPreset.MNK_STUseMeditation) &&
                (!InCombat() || !HasBattleTarget() || !InMeleeRange()) &&
                Gauge.Chakra < 5 &&
                LevelChecked(SteeledMeditation) &&
                (!isWindBh || readyBlitz))
                return OriginalHook(SteeledMeditation);

            if (IsEnabled(CustomComboPreset.MNK_STUseOpener))
                if (Opener().FullOpener(ref actionID))
                {
                    if (IsOnCooldown(RiddleOfWind) &&
                        CanWeave() && Gauge.Chakra >= 5)
                        return TheForbiddenChakra;

                    return actionID;
                }

            //Variant Cure
            if (IsEnabled(CustomComboPreset.MNK_Variant_Cure) &&
                IsEnabled(Variant.VariantCure) &&
                PlayerHealthPercentageHp() <= Config.MNK_VariantCure)
                return Variant.VariantCure;

            if (IsEnabled(CustomComboPreset.MNK_STUseBrotherhood) &&
                IsEnabled(CustomComboPreset.MNK_STUseBuffs) && InCombat() &&
                LevelChecked(Brotherhood) &&
                canMelee &&
                (IsOffCooldown(Brotherhood) || GetCooldownRemainingTime(Brotherhood) <= 0.7) && 
                ((HasEffect(Buffs.PerfectBalance) && (((JustUsed(Brotherhood, 124) && (GetBuffRemainingTime(Buffs.WindsRumination) <= 2 && GetCooldownRemainingTime(RiddleOfWind) > 17.35)) && GetBuffStacks(Buffs.PerfectBalance) <= 2) || GetBuffStacks(Buffs.PerfectBalance) <= 1)) || (!HasEffect(Buffs.PerfectBalance) && GetRemainingCharges(PerfectBalance) <= 1) || GetRemainingCharges(PerfectBalance) <= 0) &&
                GetTargetHPPercent() >= Config.MNK_ST_Brotherhood_HP &&
                actionID is not DragonKick)
                return Brotherhood;

            if (IsEnabled(CustomComboPreset.MNK_STUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_STUseROF) &&
                LevelChecked(RiddleOfFire) &&
                (IsOffCooldown(RiddleOfFire) || GetCooldownRemainingTime(RiddleOfFire) <= (GCD - 0.99)) &&
                !HasEffect(Buffs.RiddleOfFire) &&
                (!LevelChecked(Brotherhood) || !IsOffCooldown(Brotherhood) || HasEffect(Buffs.Brotherhood)) &&
                ((!HasBattleTarget() && GetCooldownRemainingTime(Brotherhood) <= 61 && GetCooldownRemainingTime(Brotherhood) >= 56) || GetTargetHPPercent() >= Config.MNK_ST_RiddleOfFire_HP) &&
                ((GetCooldownRemainingTime(Brotherhood) >= 54 && GetCooldownRemainingTime(Brotherhood) <= 66) || (GetCooldownRemainingTime(Brotherhood) >= 114)) &&
                (GetCooldownRemainingTime(Brotherhood) >= 110 || HasEffect(Buffs.Brotherhood) || RemainingGCD <= 1) &&
                actionID is not DragonKick)
                return RiddleOfFire;

            // OGCDs
            if (CanWeave() || !HasBattleTarget())
            {
                //Variant Rampart
                if (IsEnabled(CustomComboPreset.MNK_Variant_Rampart) &&
                    IsEnabled(Variant.VariantRampart) &&
                    IsOffCooldown(Variant.VariantRampart))
                    return Variant.VariantRampart;

                //Perfect Balance
                if (IsEnabled(CustomComboPreset.MNK_STUsePerfectBalance) &&
                    UsePerfectBalance() &&
                    actionID is not DragonKick)
                    return PerfectBalance;

                if (IsEnabled(CustomComboPreset.MNK_STUseBuffs) && InCombat())
                {

                    if (IsEnabled(CustomComboPreset.MNK_STUseROW) &&
                        LevelChecked(RiddleOfWind) &&
                        IsOffCooldown(RiddleOfWind) &&
                        !HasEffect(Buffs.RiddleOfWind) &&
                        HasBattleTarget() &&
                        GetTargetDistance() <= 10 &&
                        GetTargetHPPercent() >= Config.MNK_ST_RiddleOfWind_HP)
                        return RiddleOfWind;
                }

                if (IsEnabled(CustomComboPreset.MNK_STUseTheForbiddenChakra) &&
                    Gauge.Chakra >= 5 && InCombat() && canMelee && GetCooldownRemainingTime(Brotherhood) >= GCD &&
                    LevelChecked(SteeledMeditation))
                    return OriginalHook(SteeledMeditation);

                if (IsEnabled(CustomComboPreset.MNK_ST_ComboHeals) && PlayerHealthPercentageHp() <= 99)
                {
                    if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= Config.MNK_ST_SecondWind_Threshold && ActionReady(All.SecondWind))
                        return All.SecondWind;

                    if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= Config.MNK_ST_Bloodbath_Threshold && ActionReady(All.Bloodbath) && HasBattleTarget())
                        return All.Bloodbath;
                }

                if (IsEnabled(CustomComboPreset.MNK_ST_ComboHeals) &&
                    HasEffect(Buffs.EarthsRumination) &&
                    LevelChecked(EarthsReply) &&
                    GetBuffRemainingTime(Buffs.EarthsRumination) < 4)
                return EarthsReply;
            }

            // GCDs
            if (HasEffect(Buffs.FormlessFist) && canMelee && !(!BothNadisOpen && Gauge.BlitzTimeRemaining <= 4000))
                return Gauge.OpoOpoFury == 0
                    ? OriginalHook(DragonKick)
                    : OriginalHook(Bootshine);

            // Masterful Blitz
            if (
                    IsEnabled(CustomComboPreset.MNK_STUseMasterfulBlitz) &&
                    LevelChecked(MasterfulBlitz) &&
                    !HasEffect(Buffs.PerfectBalance) &&
                    !IsOriginal(MasterfulBlitz) &&
                    (
                        (!BothNadisOpen && Gauge.BlitzTimeRemaining <= 4000) || 
                        (
                            canMelee &&
                            (!LevelChecked(Brotherhood) || GetCooldownRemainingTime(Brotherhood) >= GCD * 3) &&
                            (
                                (!LevelChecked(Brotherhood) || GetCooldownRemainingTime(Brotherhood) <= 120 - (GCD * 2)) ||
                                (!LevelChecked(RiddleOfFire) || GetCooldownRemainingTime(RiddleOfFire) >= 4)
                            )
                        )
                    )
                )
                return OriginalHook(MasterfulBlitz);

            // Perfect Balance
            if (HasEffect(Buffs.PerfectBalance) && (!HasBattleTarget() || isWindBh || InMeleeRange()))
            {
                #region Open Lunar

                if ((SolarNadi && !LunarNadi) || BothNadisOpen || (!SolarNadi && !LunarNadi && (GetCooldownRemainingTime(Brotherhood) <= 20 || HasEffect(Buffs.Brotherhood))) || OpoOpoChakra >= 2)
                    return canMelee ?
                        Gauge.OpoOpoFury == 0
                            ? OriginalHook(DragonKick)
                            : OriginalHook(Bootshine)
                        : OriginalHook(ArmOfTheDestroyer);

                #endregion

                #region Open Solar

                if (!SolarNadi && !BothNadisOpen)
                {
                    uint OpoOpoAction = canMelee ?
                                Gauge.OpoOpoFury == 0
                                    ? OriginalHook(DragonKick)
                                    : OriginalHook(Bootshine)
                                : OriginalHook(ArmOfTheDestroyer);

                    uint RaptorAction = canMelee ?
                                Gauge.RaptorFury == 0
                                    ? TwinSnakes
                                    : OriginalHook(TrueStrike)
                                : FourPointFury;

                    uint CoeurlAction = canMelee ?
                                Gauge.CoeurlFury == 0
                                    ? Demolish
                                    : OriginalHook(SnapPunch)
                                : Rockbreaker;

                    if (Config.MNK_ST_Phoenix_Order == 1) {
                        if (RaptorChakra == 0) return RaptorAction;
                        if (CoeurlChakra == 0) return CoeurlAction;
                        if (OpoOpoChakra == 0) return OpoOpoAction;
                    } else if (Config.MNK_ST_Phoenix_Order == 2) {
                        if (CoeurlChakra == 0) return CoeurlAction;
                        if (RaptorChakra == 0) return RaptorAction;
                        if (OpoOpoChakra == 0) return OpoOpoAction;
                    } else if (Config.MNK_ST_Phoenix_Order == 3) {
                        if (CoeurlChakra == 0 && (JustUsed(TwinSnakes) || JustUsed(OriginalHook(TrueStrike)) || JustUsed(FourPointFury))) return CoeurlAction;
                        if (RaptorChakra == 0) return RaptorAction;
                        if (CoeurlChakra == 0) return CoeurlAction;
                        if (OpoOpoChakra == 0) return OpoOpoAction;
                    } else {
                        if (OpoOpoChakra == 0) return OpoOpoAction;
                        if (RaptorChakra == 0) return RaptorAction;
                        if (CoeurlChakra == 0) return CoeurlAction;
                    }
                }

                #endregion
            }

            if (IsEnabled(CustomComboPreset.MNK_STUseBuffs))
            {
                if (IsEnabled(CustomComboPreset.MNK_STUseFiresReply) &&
                    HasEffect(Buffs.FiresRumination) &&
                    LevelChecked(FiresReply) &&
                    !HasEffect(Buffs.PerfectBalance) &&
                    !HasEffect(Buffs.FormlessFist) &&
                    HasBattleTarget() &&
                    GetTargetDistance() <= 20 &&
                    (Config.MNK_ST_FiresReply_Order != 2 || GetBuffRemainingTime(Buffs.FiresRumination) <= GCD * 4) &&
                    (JustUsed(OriginalHook(Bootshine)) ||
                     JustUsed(OriginalHook(DragonKick)) ||
                     (HasEffect(Buffs.Brotherhood) && GetBuffRemainingTime(Buffs.Brotherhood) < 4) ||
                     GetBuffRemainingTime(Buffs.FiresRumination) < 4)
                    )
                    return FiresReply;

                if (IsEnabled(CustomComboPreset.MNK_STUseWindsReply) &&
                    HasEffect(Buffs.WindsRumination) &&
                    LevelChecked(WindsReply) &&
                    HasEffect(Buffs.RiddleOfWind) &&
                    HasBattleTarget() &&
                    GetTargetDistance() <= 10 &&
                    (
                        (HasEffect(Buffs.Brotherhood) && GetBuffRemainingTime(Buffs.Brotherhood) < 4) ||
                        (HasEffect(Buffs.RiddleOfFire) && GetBuffRemainingTime(Buffs.RiddleOfFire) < 4) ||
                        GetBuffRemainingTime(Buffs.WindsRumination) < 6)
                    )
                    return WindsReply;
            }

            // Standard Beast Chakras
            return DetermineCoreAbility(actionID, IsEnabled(CustomComboPreset.MNK_STUseTrueNorth) &&
                                        (GetRemainingCharges(TrueNorth) >= 2 ||
                                         (GetRemainingCharges(TrueNorth) >= 1 && (!LevelChecked(RiddleOfFire) || GetCooldownRemainingTime(Brotherhood) <= 45 || GetCooldownRemainingTime(RiddleOfFire) >= GetCooldownRemainingTime(TrueNorth))) ||
                                         HasEffect(Buffs.RiddleOfFire) || HasEffect(Buffs.Brotherhood)));
        }
    }

    internal class MNK_AOE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MNK_AOE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (ArmOfTheDestroyer or ShadowOfTheDestroyer))
                return actionID;

            if (!InCombat() && Gauge.Chakra < 5 &&
                LevelChecked(InspiritedMeditation))
                return OriginalHook(InspiritedMeditation);

            if (!InCombat() && LevelChecked(FormShift) &&
                !HasEffect(Buffs.FormlessFist) &&
                !HasEffect(Buffs.PerfectBalance))
                return FormShift;

            //Variant Cure
            if (IsEnabled(CustomComboPreset.MNK_Variant_Cure) &&
                IsEnabled(Variant.VariantCure) &&
                PlayerHealthPercentageHp() <= Config.MNK_VariantCure)
                return Variant.VariantCure;

            if (ActionReady(RiddleOfFire) &&
                !HasEffect(Buffs.FiresRumination) &&
                CanDelayedWeave())
                return RiddleOfFire;

            // Buffs
            if (CanWeave())
            {
                //Variant Rampart
                if (IsEnabled(CustomComboPreset.MNK_Variant_Rampart) &&
                    IsEnabled(Variant.VariantRampart) &&
                    IsOffCooldown(Variant.VariantRampart))
                    return Variant.VariantRampart;

                if (ActionReady(Brotherhood))
                    return Brotherhood;

                if (ActionReady(RiddleOfWind) &&
                    !HasEffect(Buffs.WindsRumination))
                    return RiddleOfWind;

                if (ActionReady(PerfectBalance) &&
                    !HasEffect(Buffs.PerfectBalance) &&
                    (GetRemainingCharges(PerfectBalance) == GetMaxCharges(PerfectBalance) ||
                     GetCooldownRemainingTime(PerfectBalance) <= 4 ||
                     HasEffect(Buffs.Brotherhood) ||
                     HasEffect(Buffs.RiddleOfFire) && GetBuffRemainingTime(Buffs.RiddleOfFire) < 10 ||
                     GetCooldownRemainingTime(RiddleOfFire) < 4 && GetCooldownRemainingTime(Brotherhood) < 8))
                    return PerfectBalance;

                if (Gauge.Chakra >= 5 &&
                    LevelChecked(InspiritedMeditation) &&
                    HasBattleTarget() && InCombat())
                    return OriginalHook(InspiritedMeditation);

                if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= 25 && ActionReady(All.SecondWind))
                    return All.SecondWind;

                if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= 40 && ActionReady(All.Bloodbath))
                    return All.Bloodbath;
            }

            if (LevelChecked(FiresReply) &&
                HasEffect(Buffs.FiresRumination) &&
                !HasEffect(Buffs.PerfectBalance) &&
                !HasEffect(Buffs.FormlessFist))
                return FiresReply;

            if (HasEffect(Buffs.WindsRumination) &&
                LevelChecked(WindsReply) &&
                HasEffect(Buffs.RiddleOfWind) &&
                GetBuffRemainingTime(Buffs.WindsRumination) < 4)
                return WindsReply;

            // Masterful Blitz
            if (LevelChecked(MasterfulBlitz) && !HasEffect(Buffs.PerfectBalance) &&
                OriginalHook(MasterfulBlitz) != MasterfulBlitz)
                return OriginalHook(MasterfulBlitz);

            // Perfect Balance
            if (HasEffect(Buffs.PerfectBalance))
            {
                #region Open Lunar

                if (!LunarNadi || BothNadisOpen || !SolarNadi && !LunarNadi)
                    return LevelChecked(ShadowOfTheDestroyer)
                        ? ShadowOfTheDestroyer
                        : Rockbreaker;

                #endregion

                #region Open Solar

                if (!SolarNadi && !BothNadisOpen)
                    switch (GetBuffStacks(Buffs.PerfectBalance))
                    {
                        case 3:
                            return OriginalHook(ArmOfTheDestroyer);

                        case 2:
                            return FourPointFury;

                        case 1:
                            return Rockbreaker;
                    }

                #endregion
            }

            // Monk Rotation
            if (HasEffect(Buffs.OpoOpoForm))
                return OriginalHook(ArmOfTheDestroyer);

            if (HasEffect(Buffs.RaptorForm))
            {
                if (LevelChecked(FourPointFury))
                    return FourPointFury;

                if (LevelChecked(TwinSnakes))
                    return TwinSnakes;
            }

            if (HasEffect(Buffs.CoeurlForm) && LevelChecked(Rockbreaker))
                return Rockbreaker;

            return actionID;
        }
    }

    internal class MNK_AOE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MNK_AOE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (ArmOfTheDestroyer or ShadowOfTheDestroyer))
                return actionID;

            bool canMelee = HasBattleTarget() && InMeleeRange();
            uint raptorAction = LevelChecked(FourPointFury) ? FourPointFury : 
                            canMelee ?
                                LevelChecked(TwinSnakes) && Gauge.RaptorFury == 0 
                                    ? TwinSnakes
                                    : OriginalHook(TrueStrike)
                                : actionID;

            uint coeurlAction = LevelChecked(Rockbreaker) ? Rockbreaker : 
                            canMelee ?
                                LevelChecked(Demolish) && Gauge.CoeurlFury == 0
                                    ? Demolish
                                    : OriginalHook(SnapPunch)
                                : actionID;

            uint maxPowerSkill = LevelChecked(ShadowOfTheDestroyer) ? ShadowOfTheDestroyer : LevelChecked(Rockbreaker) ? Rockbreaker : LevelChecked(FourPointFury) ? FourPointFury : OriginalHook(ArmOfTheDestroyer);

            if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_AoEUseFiresReply) &&
                HasEffect(Buffs.FiresRumination) &&
                HasBattleTarget() &&
                (GetBuffRemainingTime(Buffs.FiresRumination) < 2 && GetTargetDistance() <= 20))
                return FiresReply;

            if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_AoEUseWindsReply) &&
                HasBattleTarget() &&
                HasEffect(Buffs.WindsRumination) &&
                LevelChecked(WindsReply) &&
                HasEffect(Buffs.RiddleOfWind) &&
                (GetBuffRemainingTime(Buffs.WindsRumination) < 2 || !InMeleeRange()) && GetTargetDistance() <= 10)
                return WindsReply;

            if (IsEnabled(CustomComboPreset.MNK_AoEUseMeditation) &&
                !InCombat() && Gauge.Chakra < 5 &&
                LevelChecked(InspiritedMeditation))
                return OriginalHook(InspiritedMeditation);

            if (IsEnabled(CustomComboPreset.MNK_AoEUseFormShift) &&
                !InCombat() && LevelChecked(FormShift) &&
                !HasEffect(Buffs.FormlessFist) && !HasEffect(Buffs.PerfectBalance))
                return FormShift;

            //Variant Cure
            if (IsEnabled(CustomComboPreset.MNK_Variant_Cure) &&
                IsEnabled(Variant.VariantCure) &&
                PlayerHealthPercentageHp() <= Config.MNK_VariantCure)
                return Variant.VariantCure;

            if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs) &&
                IsEnabled(CustomComboPreset.MNK_AoEUseROF) &&
                !HasEffect(Buffs.RiddleOfFire) &&
                (!IsOffCooldown(Brotherhood) || HasEffect(Buffs.Brotherhood)) &&
                ((GetCooldownRemainingTime(Brotherhood) >= 54 && GetCooldownRemainingTime(Brotherhood) <= 66) || (GetCooldownRemainingTime(Brotherhood) >= 114)) &&
                (IsOffCooldown(RiddleOfFire) || GetCooldownRemainingTime(RiddleOfFire) <= (GCD - 0.99)) &&
                (GetCooldownRemainingTime(Brotherhood) >= 110 || HasEffect(Buffs.Brotherhood) || RemainingGCD <= 1) &&
                GetTargetHPPercent() >= Config.MNK_AoE_RiddleOfFire_HP)
                return RiddleOfFire;

            // Buffs
            if (CanWeave())
            {
                //Variant Rampart
                if (IsEnabled(CustomComboPreset.MNK_Variant_Rampart) &&
                    IsEnabled(Variant.VariantRampart) &&
                    IsOffCooldown(Variant.VariantRampart))
                    return Variant.VariantRampart;

                if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs))
                {
                    if (IsEnabled(CustomComboPreset.MNK_AoEUseBrotherhood) &&
                        ActionReady(Brotherhood) &&
                        GetTargetHPPercent() >= Config.MNK_AoE_Brotherhood_HP)
                        return Brotherhood;

                    if (IsEnabled(CustomComboPreset.MNK_AoEUseROW) &&
                        ActionReady(RiddleOfWind) &&
                        !HasEffect(Buffs.WindsRumination) &&
                        GetTargetHPPercent() >= Config.MNK_AoE_RiddleOfWind_HP)
                        return RiddleOfWind;
                }

                if (IsEnabled(CustomComboPreset.MNK_AoEUsePerfectBalance) &&
                    ActionReady(PerfectBalance) &&
                    JustUsed(maxPowerSkill, GCD) &&
                    !HasEffect(Buffs.PerfectBalance) &&
                    (!LevelChecked(Brotherhood) || HasEffect(Buffs.Brotherhood) ||
                     ((!LevelChecked(RiddleOfFire) || HasEffect(Buffs.RiddleOfFire)) && !JustUsed(PerfectBalance, 20))))
                    return PerfectBalance;

                if (IsEnabled(CustomComboPreset.MNK_AoEUseHowlingFist) &&
                    Gauge.Chakra >= 5 && HasBattleTarget() && InCombat() &&
                    LevelChecked(InspiritedMeditation))
                    return OriginalHook(Enlightenment);

                if (IsEnabled(CustomComboPreset.MNK_AoE_ComboHeals))
                {
                    if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= Config.MNK_AoE_SecondWind_Threshold &&
                        ActionReady(All.SecondWind))
                        return All.SecondWind;

                    if ((GetPartyAvgHPPercent() - PlayerHealthPercentageHp()) >= Config.MNK_AoE_Bloodbath_Threshold &&
                        ActionReady(All.Bloodbath))
                        return All.Bloodbath;

                    if (LevelChecked(RiddleOfEarth) &&
                        GetPartyAvgHPPercent() <= Config.MNK_AoE_RiddleOfEarth_Threshold &&
                        ActionReady(RiddleOfEarth))
                        return RiddleOfEarth;

                    if (HasEffect(Buffs.EarthsRumination) &&
                        LevelChecked(EarthsReply) &&
                        GetPartyAvgHPPercent() <= Config.MNK_AoE_RiddleOfEarth_Threshold)
                    return EarthsReply;
                }
            }

            // GCDs
            if (HasEffect(Buffs.FormlessFist) && !(!BothNadisOpen && Gauge.BlitzTimeRemaining <= 4000))
                return maxPowerSkill;

            // Masterful Blitz
            if (
                    IsEnabled(CustomComboPreset.MNK_STUseMasterfulBlitz) &&
                    LevelChecked(MasterfulBlitz) &&
                    !HasEffect(Buffs.PerfectBalance) &&
                    !IsOriginal(MasterfulBlitz) &&
                    (
                        (!BothNadisOpen && Gauge.BlitzTimeRemaining <= 4000) || 
                        (
                            (!LevelChecked(Brotherhood) || GetCooldownRemainingTime(Brotherhood) >= GCD * 3) &&
                            (
                                (!LevelChecked(Brotherhood) || GetCooldownRemainingTime(Brotherhood) <= 120 - (GCD * 2)) ||
                                (!LevelChecked(RiddleOfFire) || GetCooldownRemainingTime(RiddleOfFire) >= 4)
                            )
                        )
                    )
                )
                return OriginalHook(MasterfulBlitz);
            
            // Perfect Balance
            if (HasEffect(Buffs.PerfectBalance))
            {
                #region Open Lunar

                if ((SolarNadi && !LunarNadi) || BothNadisOpen || (!SolarNadi && !LunarNadi && (GetCooldownRemainingTime(Brotherhood) <= 20 || HasEffect(Buffs.Brotherhood))) || OpoOpoChakra >= 2 || CoeurlChakra >= 2)
                    return maxPowerSkill;
                #endregion

                #region Open Solar

                if (!SolarNadi && !BothNadisOpen) {
                    if (OpoOpoChakra == 0 && RaptorChakra == 0 && CoeurlChakra == 0) return maxPowerSkill;
                    if (CoeurlChakra == 0) return coeurlAction;
                    if (RaptorChakra == 0) return raptorAction;
                    if (OpoOpoChakra == 0) return OriginalHook(ArmOfTheDestroyer);
                }
                #endregion
            }

            if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs))
            {
                if (IsEnabled(CustomComboPreset.MNK_AoEUseFiresReply) &&
                    LevelChecked(FiresReply) &&
                    HasEffect(Buffs.FiresRumination) &&
                    !HasEffect(Buffs.PerfectBalance) &&
                    !HasEffect(Buffs.FormlessFist) &&
                    HasBattleTarget() &&
                    GetTargetDistance() <= 20 &&
                    (JustUsed(maxPowerSkill) ||
                     (HasEffect(Buffs.Brotherhood) && GetBuffRemainingTime(Buffs.Brotherhood) < 4) ||
                     GetBuffRemainingTime(Buffs.FiresRumination) < 4)
                    )
                    return FiresReply;

                if (IsEnabled(CustomComboPreset.MNK_AoEUseWindsReply) &&
                    HasEffect(Buffs.WindsRumination) &&
                    LevelChecked(WindsReply) &&
                    HasEffect(Buffs.RiddleOfWind) &&
                    HasBattleTarget() &&
                    GetTargetDistance() <= 10 &&
                    (
                        (HasEffect(Buffs.Brotherhood) && GetBuffRemainingTime(Buffs.Brotherhood) < 4) ||
                        (HasEffect(Buffs.RiddleOfFire) && GetBuffRemainingTime(Buffs.RiddleOfFire) < 4) ||
                        GetBuffRemainingTime(Buffs.WindsRumination) < 6
                    ))
                    return WindsReply;
            }

            // Monk Rotation
            if (HasEffect(Buffs.OpoOpoForm))
                return OriginalHook(ArmOfTheDestroyer);

            if (HasEffect(Buffs.RaptorForm))
                return raptorAction;

            if (HasEffect(Buffs.CoeurlForm))
                return coeurlAction;

            return actionID;
        }
    }

    internal class MNK_PerfectBalance : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_PerfectBalance;

        protected override uint Invoke(uint actionID) =>
            actionID is PerfectBalance &&
            OriginalHook(MasterfulBlitz) != MasterfulBlitz &&
            LevelChecked(MasterfulBlitz)
                ? OriginalHook(MasterfulBlitz)
                : actionID;
    }

    internal class MNK_Riddle_Brotherhood : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MNK_Riddle_Brotherhood;

        protected override uint Invoke(uint actionID) =>
            (actionID is RiddleOfFire &&
            (IsOffCooldown(Brotherhood) || (GetCooldownRemainingTime(Brotherhood) < GetCooldownRemainingTime(RiddleOfFire) + 3)))
                ? Brotherhood
                : actionID;
    }

    internal class MNK_BeastChakras : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_ST_BeastChakras;

        protected override uint Invoke(uint actionID)
        {
            if (IsEnabled(CustomComboPreset.MNK_BC_OPOOPO) &&
                actionID is Bootshine or LeapingOpo)
                return Gauge.OpoOpoFury == 0 && LevelChecked(OriginalHook(DragonKick))
                    ? OriginalHook(DragonKick)
                    : OriginalHook(Bootshine);

            if (IsEnabled(CustomComboPreset.MNK_BC_RAPTOR) &&
                actionID is TrueStrike or RisingRaptor)
                return Gauge.RaptorFury == 0 && LevelChecked(TwinSnakes)
                    ? TwinSnakes
                    : OriginalHook(TrueStrike);

            if (IsEnabled(CustomComboPreset.MNK_BC_COEURL) &&
                actionID is SnapPunch or PouncingCoeurl)
                return Gauge.CoeurlFury == 0 && LevelChecked(Demolish)
                    ? Demolish
                    : OriginalHook(SnapPunch);

            return actionID;
        }
    }
}
