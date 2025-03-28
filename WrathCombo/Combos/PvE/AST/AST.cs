﻿using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using WrathCombo.Extensions;
namespace WrathCombo.Combos.PvE;

internal partial class AST : HealerJob
{
    internal class AST_Benefic : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.AST_Benefic;

        protected override uint Invoke(uint actionID) =>
            actionID is Benefic2 && !ActionReady(Benefic2)
                ? Benefic
                : actionID;
    }

    internal class AST_Raise_Alternative : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.AST_Raise_Alternative;

        protected override uint Invoke(uint actionID) =>
            actionID is Role.Swiftcast && IsOnCooldown(Role.Swiftcast)
                ? Ascend
                : actionID;
    }

    internal class AST_ST_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.AST_ST_DPS;

        protected override uint Invoke(uint actionID)
        {
            bool alternateMode = GetIntOptionAsBool(Config.AST_DPS_AltMode); //(0 or 1 radio values)
            bool actionFound = !alternateMode && MaleficList.Contains(actionID) ||
                               alternateMode && CombustList.ContainsKey(actionID);

            if (!actionFound)
                return actionID;

            // Out of combat Card Draw
            if (!InCombat())
            {
                if (IsEnabled(CustomComboPreset.AST_DPS_AutoDraw) &&
                    ActionReady(OriginalHook(AstralDraw)) &&
                    (Gauge.DrawnCards.All(x => x is CardType.None) || DrawnCard == CardType.None && Config.AST_ST_DPS_OverwriteCards))
                    return OriginalHook(AstralDraw);
            }

            if (IsEnabled(CustomComboPreset.AST_ST_DPS_Opener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            //In combat
            if (InCombat())
            {
                //Variant stuff
                if (Variant.CanSpiritDart(CustomComboPreset.AST_Variant_Rampart))
                    return Variant.SpiritDart;

                if (IsEnabled(CustomComboPreset.AST_DPS_LightSpeed) &&
                    ActionReady(Lightspeed) &&
                    GetTargetHPPercent() > Config.AST_DPS_LightSpeedOption &&
                    IsMoving() &&
                    !HasEffect(Buffs.Lightspeed))
                    return Lightspeed;

                if (IsEnabled(CustomComboPreset.AST_DPS_Lucid) &&
                    Role.CanLucidDream(Config.AST_LucidDreaming))
                    return Role.LucidDreaming;

                //Play Card
                if (IsEnabled(CustomComboPreset.AST_DPS_AutoPlay) &&
                    ActionReady(Play1) &&
                    Gauge.DrawnCards[0] is not CardType.None &&
                    CanSpellWeave())
                    return OriginalHook(Play1);

                //Card Draw
                if (IsEnabled(CustomComboPreset.AST_DPS_AutoDraw) &&
                    ActionReady(OriginalHook(AstralDraw)) &&
                    (Gauge.DrawnCards.All(x => x is CardType.None) || DrawnCard == CardType.None && Config.AST_ST_DPS_OverwriteCards) &&
                    CanDelayedWeave())
                    return OriginalHook(AstralDraw);

                //Divination
                if (IsEnabled(CustomComboPreset.AST_DPS_Divination) &&
                    ActionReady(Divination) &&
                    !HasEffectAny(Buffs.Divination) && //Overwrite protection
                    GetTargetHPPercent() > Config.AST_DPS_DivinationOption &&
                    CanDelayedWeave() &&
                    ActionWatching.NumberOfGcdsUsed >= 3)
                    return Divination;

                //Earthly Star
                if (IsEnabled(CustomComboPreset.AST_ST_DPS_EarthlyStar) &&
                    !HasEffect(Buffs.EarthlyDominance) &&
                    ActionReady(EarthlyStar) &&
                    CanSpellWeave())
                    return EarthlyStar;

                if (IsEnabled(CustomComboPreset.AST_DPS_Oracle) &&
                    HasEffect(Buffs.Divining) &&
                    CanSpellWeave())
                    return Oracle;

                //Minor Arcana / Lord of Crowns
                if (ActionReady(OriginalHook(MinorArcana)) &&
                    IsEnabled(CustomComboPreset.AST_DPS_LazyLord) &&
                    Gauge.DrawnCrownCard is CardType.Lord &&
                    HasBattleTarget() && CanDelayedWeave())
                    return OriginalHook(MinorArcana);

                if (HasBattleTarget())
                {
                    //Combust
                    if (IsEnabled(CustomComboPreset.AST_ST_DPS_CombustUptime) &&
                        !GravityList.Contains(actionID) &&
                        LevelChecked(Combust) &&
                        CombustList.TryGetValue(OriginalHook(Combust), out ushort dotDebuffID))
                    {
                        if (Variant.CanSpiritDart(CustomComboPreset.AST_Variant_SpiritDart))
                            return Variant.SpiritDart;

                        float refreshTimer = Config.AST_ST_DPS_CombustUptime_Adv ? Config.AST_ST_DPS_CombustUptime_Threshold : 3;
                        int hpThreshold = Config.AST_ST_DPS_CombustSubOption == 1 || !InBossEncounter() ? Config.AST_DPS_CombustOption : 0;
                        if (GetDebuffRemainingTime(dotDebuffID) <= refreshTimer &&
                            GetTargetHPPercent() > hpThreshold)
                            return OriginalHook(Combust);

                        //Alternate Mode (idles as Malefic)
                        if (alternateMode)
                            return OriginalHook(Malefic);
                    }
                }
            }
            return actionID;
        }
    }

    internal class AST_AOE_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.AST_AOE_DPS;
        protected override uint Invoke(uint actionID)
        {
            if (!GravityList.Contains(actionID))
                return actionID;

            //Variant stuff
            if (Variant.CanRampart(CustomComboPreset.AST_Variant_Rampart))
                return Variant.Rampart;

            if (Variant.CanSpiritDart(CustomComboPreset.AST_Variant_SpiritDart))
                return Variant.SpiritDart;

            if (IsEnabled(CustomComboPreset.AST_AOE_LightSpeed) &&
                ActionReady(Lightspeed) &&
                GetTargetHPPercent() > Config.AST_AOE_LightSpeedOption &&
                IsMoving() &&
                !HasEffect(Buffs.Lightspeed))
                return Lightspeed;

            if (IsEnabled(CustomComboPreset.AST_AOE_Lucid) &&
                Role.CanLucidDream(Config.AST_LucidDreaming))
                return Role.LucidDreaming;

            //Play Card
            if (IsEnabled(CustomComboPreset.AST_AOE_AutoPlay) &&
                ActionReady(Play1) &&
                Gauge.DrawnCards[0] is not CardType.None &&
                CanSpellWeave())
                return OriginalHook(Play1);

            //Card Draw
            if (IsEnabled(CustomComboPreset.AST_AOE_AutoDraw) &&
                ActionReady(OriginalHook(AstralDraw)) &&
                (Gauge.DrawnCards.All(x => x is CardType.None) || DrawnCard == CardType.None && Config.AST_AOE_DPS_OverwriteCards) &&
                CanDelayedWeave())
                return OriginalHook(AstralDraw);

            //Divination
            if (IsEnabled(CustomComboPreset.AST_AOE_Divination) &&
                ActionReady(Divination) &&
                !HasEffectAny(Buffs.Divination) && //Overwrite protection
                GetTargetHPPercent() > Config.AST_AOE_DivinationOption &&
                CanDelayedWeave() &&
                ActionWatching.NumberOfGcdsUsed >= 3)
                return Divination;
            //Earthly Star
            if (IsEnabled(CustomComboPreset.AST_AOE_DPS_EarthlyStar) && !IsMoving() &&
                !HasEffect(Buffs.EarthlyDominance) &&
                ActionReady(EarthlyStar) &&
                CanSpellWeave())
                return EarthlyStar;

            if (IsEnabled(CustomComboPreset.AST_AOE_Oracle) &&
                HasEffect(Buffs.Divining) &&
                CanSpellWeave())
                return Oracle;

            //Minor Arcana / Lord of Crowns
            if (ActionReady(OriginalHook(MinorArcana)) &&
                IsEnabled(CustomComboPreset.AST_AOE_LazyLord) && Gauge.DrawnCrownCard is CardType.Lord &&
                HasBattleTarget() &&
                CanDelayedWeave())
                return OriginalHook(MinorArcana);
            return actionID;
        }
    }

    internal class AST_AoE_SimpleHeals_AspectedHelios : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.AST_AoE_SimpleHeals_AspectedHelios;

        protected override uint Invoke(uint actionID)
        {
            bool nonAspectedMode = GetIntOptionAsBool(Config.AST_AoEHeals_AltMode); //(0 or 1 radio values)

            if ((!nonAspectedMode || actionID is not Helios) &&
                (nonAspectedMode || actionID is not (AspectedHelios or HeliosConjuction)))
                return actionID;

            bool canLady = Config.AST_AoE_SimpleHeals_WeaveLady && CanSpellWeave() || !Config.AST_AoE_SimpleHeals_WeaveLady;
            bool canHoroscope = Config.AST_AoE_SimpleHeals_Horoscope && CanSpellWeave() || !Config.AST_AoE_SimpleHeals_Horoscope;
            bool canOppose = Config.AST_AoE_SimpleHeals_Opposition && CanSpellWeave() || !Config.AST_AoE_SimpleHeals_Opposition;

            if (!LevelChecked(AspectedHelios)) //Level check to return helios immediately below 40
                return Helios;

            if (IsEnabled(CustomComboPreset.AST_AoE_SimpleHeals_LazyLady) &&
                ActionReady(MinorArcana) &&
                Gauge.DrawnCrownCard is CardType.Lady
                && canLady)
                return OriginalHook(MinorArcana);

            if (IsEnabled(CustomComboPreset.AST_AoE_SimpleHeals_CelestialOpposition) &&
                ActionReady(CelestialOpposition) &&
                canOppose)
                return CelestialOpposition;

            if (IsEnabled(CustomComboPreset.AST_AoE_SimpleHeals_Horoscope))
            {
                if (ActionReady(Horoscope) &&
                    !HasEffect(Buffs.Horoscope) &&
                    !HasEffect(Buffs.HoroscopeHelios) &&
                    canHoroscope)
                    return Horoscope;

                if (HasEffect(Buffs.HoroscopeHelios) &&
                    canHoroscope)
                    return HoroscopeHeal;
            }

            // Only check for our own HoTs
            Status? hotCheck = HeliosConjuction.LevelChecked() ? FindEffect(Buffs.HeliosConjunction, LocalPlayer, LocalPlayer?.GameObjectId) : FindEffect(Buffs.AspectedHelios, LocalPlayer, LocalPlayer?.GameObjectId);

            if (IsEnabled(CustomComboPreset.AST_AoE_SimpleHeals_Aspected) && nonAspectedMode || // Helios mode: option must be on
                !nonAspectedMode) // Aspected mode: option is not required
            {
                if (ActionReady(AspectedHelios)
                    && hotCheck is null
                    || HasEffect(Buffs.NeutralSect) && !HasEffect(Buffs.NeutralSectShield))
                    return OriginalHook(AspectedHelios);
            }

            if (hotCheck is not null && hotCheck.RemainingTime > GetActionCastTime(OriginalHook(AspectedHelios)) + 1f)
                return Helios;

            return actionID;
        }
    }

    internal class AST_ST_SimpleHeals : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.AST_ST_SimpleHeals;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Benefic2)
                return actionID;

            bool canDignity = Config.AST_ST_SimpleHeals_WeaveDignity && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveDignity;
            bool canIntersect = Config.AST_ST_SimpleHeals_WeaveIntersection && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveIntersection;
            bool canExalt = Config.AST_ST_SimpleHeals_WeaveExalt && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveExalt;
            bool canEwer = Config.AST_ST_SimpleHeals_WeaveEwer && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveEwer;
            bool canSpire = Config.AST_ST_SimpleHeals_WeaveSpire && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveSpire;
            bool canBole = Config.AST_ST_SimpleHeals_WeaveBole && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveBole;
            bool canArrow = Config.AST_ST_SimpleHeals_WeaveArrow && CanSpellWeave() || !Config.AST_ST_SimpleHeals_WeaveArrow;

            //Grab our target (Soft->Hard->Self)
            IGameObject? healTarget = OptionalTarget ?? GetHealTarget(Config.AST_ST_SimpleHeals_Adv && Config.AST_ST_SimpleHeals_UIMouseOver);

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_Esuna) && ActionReady(Role.Esuna) &&
                GetTargetHPPercent(healTarget, Config.AST_ST_SimpleHeals_IncludeShields) >= Config.AST_ST_SimpleHeals_Esuna &&
                HasCleansableDebuff(healTarget))
                return Role.Esuna;

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_Spire) &&
                Gauge.DrawnCards[2] == CardType.Spire &&
                GetTargetHPPercent(healTarget, Config.AST_ST_SimpleHeals_IncludeShields) <= Config.AST_Spire &&
                ActionReady(Play3) &&
                canSpire)
                return OriginalHook(Play3);

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_Ewer) &&
                Gauge.DrawnCards[2] == CardType.Ewer &&
                GetTargetHPPercent(healTarget, Config.AST_ST_SimpleHeals_IncludeShields) <= Config.AST_Ewer &&
                ActionReady(Play3) &&
                canEwer)
                return OriginalHook(Play3);

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_Arrow) &&
                Gauge.DrawnCards[1] == CardType.Arrow &&
                GetTargetHPPercent(healTarget, Config.AST_ST_SimpleHeals_IncludeShields) <= Config.AST_Arrow &&
                ActionReady(Play2) &&
                canArrow)
                return OriginalHook(Play2);

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_Bole) &&
                Gauge.DrawnCards[1] == CardType.Bole &&
                GetTargetHPPercent(healTarget, Config.AST_ST_SimpleHeals_IncludeShields) <= Config.AST_Bole &&
                ActionReady(Play2) &&
                canBole)
                return OriginalHook(Play2);

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_EssentialDignity) &&
                ActionReady(EssentialDignity) &&
                GetTargetHPPercent(healTarget, Config.AST_ST_SimpleHeals_IncludeShields) <= Config.AST_EssentialDignity &&
                canDignity)
                return EssentialDignity;

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_Exaltation) &&
                ActionReady(Exaltation) &&
                canExalt)
                return Exaltation;

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_CelestialIntersection) &&
                ActionReady(CelestialIntersection) &&
                canIntersect &&
                !(healTarget as IBattleChara)!.HasShield())
                return CelestialIntersection;

            if (IsEnabled(CustomComboPreset.AST_ST_SimpleHeals_AspectedBenefic) && ActionReady(AspectedBenefic))
            {
                Status? aspectedBeneficHoT = FindEffect(Buffs.AspectedBenefic, healTarget, LocalPlayer?.GameObjectId);
                Status? neutralSectShield = FindEffect(Buffs.NeutralSectShield, healTarget, LocalPlayer?.GameObjectId);
                Status? neutralSectBuff = FindEffect(Buffs.NeutralSect, healTarget, LocalPlayer?.GameObjectId);
                if (aspectedBeneficHoT is null || aspectedBeneficHoT.RemainingTime <= 3
                                               || neutralSectShield is null && neutralSectBuff is not null)
                    return AspectedBenefic;
            }
            return actionID;
        }
    }
}
