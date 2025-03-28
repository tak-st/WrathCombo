using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
namespace WrathCombo.Combos.PvE;

internal partial class MNK
{
    internal static MNKGauge Gauge = GetJobGauge<MNKGauge>();
    internal static MNKOpenerLogicSL MNKOpenerSL = new();
    internal static MNKOpenerLogicLL MNKOpenerLL = new();
    internal static MNKOpenerLogicSL7 MNKOpenerSL7 = new();
    internal static MNKOpenerLogicLL7 MNKOpenerLL7 = new();

    internal static float GCD => GetCooldown(OriginalHook(Bootshine)).CooldownTotal;

    internal static bool BothNadisOpen => Gauge.Nadi.ToString() == "LUNAR, SOLAR";

    internal static bool SolarNadi => Gauge.Nadi == Nadi.SOLAR;

    internal static bool LunarNadi => Gauge.Nadi == Nadi.LUNAR;

    internal static int OpoOpoChakra => Gauge.BeastChakra.Count(x => x == BeastChakra.OPOOPO);

    internal static int RaptorChakra => Gauge.BeastChakra.Count(x => x == BeastChakra.RAPTOR);

    internal static int CoeurlChakra => Gauge.BeastChakra.Count(x => x == BeastChakra.COEURL);

    internal static uint DetermineCoreAbility(uint actionId, bool useTrueNorthIfEnabled)
    {
        bool isPreserveMode = actionId is DragonKick;
        bool canMelee = HasBattleTarget() && InMeleeRange();
        if (HasEffect(Buffs.OpoOpoForm) || HasEffect(Buffs.FormlessFist))
            return (Gauge.OpoOpoFury == 0 || isPreserveMode) && LevelChecked(OriginalHook(DragonKick))
                ? OriginalHook(DragonKick)
                : OriginalHook(Bootshine);

        if (HasEffect(Buffs.RaptorForm))
            return (Gauge.RaptorFury == 0 || isPreserveMode) && LevelChecked(TwinSnakes)
                ? TwinSnakes
                : OriginalHook(TrueStrike);

        if (HasEffect(Buffs.CoeurlForm))
        {
            if ((Gauge.CoeurlFury == 0 || isPreserveMode) && LevelChecked(Demolish))
            {
                if (!OnTargetsRear() &&
                    TargetNeedsPositionals() &&
                    !HasEffect(Buffs.TrueNorth) &&
                    CanWeave() &&
                    ActionReady(TrueNorth) &&
                    useTrueNorthIfEnabled)
                    return TrueNorth;

                return Demolish;
            }

            if (LevelChecked(SnapPunch))
            {
                if (!OnTargetsFlank() &&
                    TargetNeedsPositionals() &&
                    !HasEffect(Buffs.TrueNorth) &&
                    CanWeave() &&
                    ActionReady(TrueNorth) &&
                    useTrueNorthIfEnabled)
                    return TrueNorth;

                return OriginalHook(SnapPunch);
            }
        }

        if (canMelee)
            return (Gauge.OpoOpoFury == 0 || isPreserveMode) && LevelChecked(OriginalHook(DragonKick))
                ? OriginalHook(DragonKick)
                : OriginalHook(Bootshine);

        return actionId;
    }

    internal static bool compareCooldownTime(uint action1, uint action2, float needTime = 0)
    {
        if (!LevelChecked(action1)) return true;
        if (!LevelChecked(action2)) return true;
        var ac1Cd = GetCooldownRemainingTime(action1) + needTime;
        var ac2Cd = GetCooldownRemainingTime(action2);
        return ac1Cd < ac2Cd;
    }

    internal static bool compareNextBurstTime(uint action, float needTime = 0)
    {
        var acCd = GetCooldownRemainingTime(action) + needTime;
        var burstCd = GetCooldownRemainingTime(RiddleOfFire);
        if (!LevelChecked(Brotherhood)) return acCd < burstCd;
        if (!LevelChecked(RiddleOfFire)) return true;
        var bhCd = GetCooldownRemainingTime(Brotherhood);
        var diff = bhCd - burstCd;
        if (diff <= 0 || !(diff >= 54 && diff <= 66))
        {
            burstCd = bhCd;
        }
        else if ((bhCd - 54) > burstCd)
        {
            burstCd = (bhCd - 54);
        }

        return acCd < burstCd;
    }

    internal static bool positionCheck(uint actionId, bool WeaveOnly = true)
    {
        bool isPreserveMode = actionId is DragonKick;
        if ((Gauge.CoeurlFury == 0 || isPreserveMode) && LevelChecked(Demolish))
        {
            if (!OnTargetsRear() &&
                TargetNeedsPositionals() &&
                !HasEffect(Buffs.TrueNorth) &&
                (!WeaveOnly || (
                CanWeave() &&
                ActionReady(TrueNorth))))
                return false;
            return true;
        }

        if (LevelChecked(SnapPunch))
        {
            if (!OnTargetsFlank() &&
                TargetNeedsPositionals() &&
                !HasEffect(Buffs.TrueNorth) &&
                (!WeaveOnly || (
                CanWeave() &&
                ActionReady(TrueNorth))))
                return false;
            return true;
        }

        return true;
    }


    internal static bool UsePerfectBalance()
    {
        if (ActionReady(PerfectBalance) && !HasEffect(Buffs.PerfectBalance) && !HasEffect(Buffs.FormlessFist) && Gauge.BlitzTimeRemaining <= 0 && (Config.MNK_ST_FiresReply_Order != 0 || !HasEffect(Buffs.FiresRumination)))
        {
            // Odd window
            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD)) &&
                compareCooldownTime(PerfectBalance, Brotherhood, 30) &&
                (
                    (HasEffect(Buffs.RiddleOfFire) && GetBuffRemainingTime(Buffs.RiddleOfFire) > GCD * 4 + RemainingGCD) || (
                        (Config.MNK_ST_Fast_Phoenix == 1 &&
                            (GetCooldownRemainingTime(RiddleOfFire) < (GCD * 3 + RemainingGCD + 1) ||
                                (
                                    GetRemainingCharges(PerfectBalance) == 2 && GetCooldownRemainingTime(RiddleOfFire) < 20
                                )
                            )
                        )
                    )
                ) && !HasEffect(Buffs.Brotherhood) &&
                (
                    Config.MNK_ST_Many_PerfectBalance == 1 ||
                    !BothNadisOpen ||
                    (
                        Config.MNK_ST_Fast_Phoenix == 1 &&
                        GetRemainingCharges(PerfectBalance) == 2 &&
                        GetCooldownRemainingTime(RiddleOfFire) > 12 &&
                        GetCooldownRemainingTime(RiddleOfFire) < 20
                    )
                )
            )
                return true;

            // Even window
            if ((
                    JustUsed(OriginalHook(Bootshine), GCD * 2) ||
                    JustUsed(OriginalHook(DragonKick), GCD * 2) ||
                    GetCooldownRemainingTime(Brotherhood) < (GCD * 1 + RemainingGCD) ||
                    GetBuffRemainingTime(Buffs.WindsRumination) >= (GCD * 3 + RemainingGCD) ||
                    GetCooldownRemainingTime(RiddleOfWind) < (17.35 + GCD * 2 + RemainingGCD)
                ) &&
                (
                    GetCooldownRemainingTime(Brotherhood) < (GCD * 2 + RemainingGCD) ||
                    HasEffect(Buffs.Brotherhood)
                )
            )
                return true;

            // Low level
            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick))) &&
                ((HasEffect(Buffs.RiddleOfFire) && !LevelChecked(Brotherhood)) ||
                 !LevelChecked(RiddleOfFire)))
                return true;

            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD)) &&
                compareNextBurstTime(PerfectBalance, 30) &&
                (Config.MNK_ST_Many_PerfectBalance == 1 || !BothNadisOpen))
            {
                return true;
            }
        }

        if (ActionReady(PerfectBalance) && !HasEffect(Buffs.PerfectBalance) && !HasBattleTarget() && HasEffect(Buffs.RiddleOfFire))
        {
            // Odd window
            if (compareCooldownTime(PerfectBalance, Brotherhood, 30) && !HasEffect(Buffs.Brotherhood) && !BothNadisOpen && GetBuffRemainingTime(Buffs.RiddleOfFire) <= 20 - GCD)
                return true;

            // Even window
            if (HasEffect(Buffs.Brotherhood))
                return true;
        }

        return false;
    }

    internal static bool UsePerfectBalanceAoE(uint maxPowerSkill = 0)
    {
        if (ActionReady(PerfectBalance) && !HasEffect(Buffs.PerfectBalance) && !HasEffect(Buffs.FormlessFist) && Gauge.BlitzTimeRemaining <= 0 && !HasEffect(Buffs.FiresRumination))
        {
            // Odd window
            if (
                (JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD) || JustUsed(maxPowerSkill, GCD)) &&
                compareCooldownTime(PerfectBalance, Brotherhood, 30) &&
                HasEffect(Buffs.RiddleOfFire) && !HasEffect(Buffs.Brotherhood)
            )
                return true;

            // Even window
            if (
                (JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD) || JustUsed(maxPowerSkill, GCD)) &&
                HasEffect(Buffs.Brotherhood)
            )
                return true;

            // Low level
            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD) || JustUsed(maxPowerSkill, GCD)) &&
                ((HasEffect(Buffs.RiddleOfFire) && !LevelChecked(Brotherhood)) ||
                 !LevelChecked(RiddleOfFire)))
                return true;

            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD) || JustUsed(maxPowerSkill, GCD)) &&
                compareNextBurstTime(PerfectBalance, 30) &&
                (Config.MNK_ST_Many_PerfectBalance == 1 || !BothNadisOpen))
            {
                return true;
            }
        }

        if (ActionReady(PerfectBalance) && !HasEffect(Buffs.PerfectBalance) && !HasBattleTarget() && HasEffect(Buffs.RiddleOfFire))
        {
            // Odd window
            if (compareCooldownTime(PerfectBalance, Brotherhood, 30) && !HasEffect(Buffs.Brotherhood) && !BothNadisOpen && GetBuffRemainingTime(Buffs.RiddleOfFire) <= 20 - GCD)
                return true;

            // Even window
            if (HasEffect(Buffs.Brotherhood))
                return true;
        }

        return false;
    }

    internal static bool InMasterfulRange()
    {
        if (NumberOfEnemiesInRange(ElixirField, null) >= 1 &&
            (OriginalHook(MasterfulBlitz) == ElixirField || OriginalHook(MasterfulBlitz) == FlintStrike ||
             OriginalHook(MasterfulBlitz) == ElixirBurst || OriginalHook(MasterfulBlitz) == RisingPhoenix))
            return true;

        if (NumberOfEnemiesInRange(TornadoKick, CurrentTarget) >= 1 &&
            (OriginalHook(MasterfulBlitz) == TornadoKick ||
             OriginalHook(MasterfulBlitz) == CelestialRevolution ||
             OriginalHook(MasterfulBlitz) == PhantomRush))
            return true;

        return false;
    }

    #region Openers

    internal static WrathOpener Opener()
    {
        if (Config.MNK_SelectedOpener == 4)
            return GetPartyMembers().Any(x => x.BattleChara.ClassJob.RowId == DNC.JobID) ? MNKOpenerLL7 : MNKOpenerLL;

        if (Config.MNK_SelectedOpener == 0)
            return MNKOpenerLL;

        if (Config.MNK_SelectedOpener == 1)
            return MNKOpenerSL;

        if (Config.MNK_SelectedOpener == 2)
            return MNKOpenerLL7;

        if (Config.MNK_SelectedOpener == 3)
            return MNKOpenerSL7;

        return WrathOpener.Dummy;
    }

    internal class MNKOpenerLogicSL : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            DragonKick,
            PerfectBalance,
            TwinSnakes,
            Demolish,
            Brotherhood,
            RiddleOfFire,
            LeapingOpo,
            TheForbiddenChakra,
            RiddleOfWind,
            RisingPhoenix,
            DragonKick,
            WindsReply,
            FiresReply,
            LeapingOpo,
            PerfectBalance,
            DragonKick,
            LeapingOpo,
            DragonKick,
            ElixirBurst,
            LeapingOpo
        ];

        internal override UserData ContentCheckConfig => Config.MNK_Balance_Content;
        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(PerfectBalance) < 2)
                return false;

            if (!IsOffCooldown(Brotherhood))
                return false;

            if (!IsOffCooldown(RiddleOfFire))
                return false;

            if (!IsOffCooldown(RiddleOfWind))
                return false;

            if (Gauge.Nadi != Nadi.NONE)
                return false;

            if (Gauge.RaptorFury != 0)
                return false;

            if (Gauge.CoeurlFury != 0)
                return false;

            return true;
        }
    }

    internal class MNKOpenerLogicSL7 : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            DragonKick,
            PerfectBalance,
            TwinSnakes,
            LeapingOpo,
            Demolish,
            Brotherhood,
            RiddleOfFire,
            RisingPhoenix,
            TheForbiddenChakra,
            RiddleOfWind,
            DragonKick,
            WindsReply,
            FiresReply,
            LeapingOpo,
            PerfectBalance,
            DragonKick,
            LeapingOpo,
            DragonKick,
            ElixirBurst,
            LeapingOpo
        ];

        internal override UserData? ContentCheckConfig => Config.MNK_Balance_Content;
        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(PerfectBalance) < 2)
                return false;

            if (!IsOffCooldown(Brotherhood))
                return false;

            if (!IsOffCooldown(RiddleOfFire))
                return false;

            if (!IsOffCooldown(RiddleOfWind))
                return false;

            if (Gauge.Nadi != Nadi.NONE)
                return false;

            if (Gauge.RaptorFury != 0)
                return false;

            if (Gauge.CoeurlFury != 0)
                return false;

            return true;
        }
    }

    internal class MNKOpenerLogicLL : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            DragonKick,
            PerfectBalance,
            LeapingOpo,
            DragonKick,
            Brotherhood,
            RiddleOfFire,
            LeapingOpo,
            TheForbiddenChakra,
            RiddleOfWind,
            ElixirBurst,
            DragonKick,
            WindsReply,
            FiresReply,
            LeapingOpo,
            PerfectBalance,
            DragonKick,
            LeapingOpo,
            DragonKick,
            ElixirBurst,
            LeapingOpo
        ];
        internal override UserData ContentCheckConfig => Config.MNK_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(PerfectBalance) < 2)
                return false;

            if (!IsOffCooldown(Brotherhood))
                return false;

            if (!IsOffCooldown(RiddleOfFire))
                return false;

            if (!IsOffCooldown(RiddleOfWind))
                return false;

            if (Gauge.Nadi != Nadi.NONE)
                return false;

            if (Gauge.RaptorFury != 0)
                return false;

            if (Gauge.CoeurlFury != 0)
                return false;

            return true;
        }
    }

    internal class MNKOpenerLogicLL7 : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            DragonKick,
            PerfectBalance,
            TheForbiddenChakra,
            LeapingOpo,
            DragonKick,
            LeapingOpo,
            Brotherhood,
            RiddleOfFire,
            ElixirBurst,
            RiddleOfWind,
            DragonKick,
            WindsReply,
            FiresReply,
            LeapingOpo,
            PerfectBalance,
            DragonKick,
            LeapingOpo,
            DragonKick,
            ElixirBurst,
            LeapingOpo
        ];
        internal override UserData? ContentCheckConfig => Config.MNK_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(PerfectBalance) < 2)
                return false;

            if (!IsOffCooldown(Brotherhood))
                return false;

            if (!IsOffCooldown(RiddleOfFire))
                return false;

            if (!IsOffCooldown(RiddleOfWind))
                return false;

            if (Gauge.Nadi != Nadi.NONE)
                return false;

            if (Gauge.RaptorFury != 0)
                return false;

            if (Gauge.CoeurlFury != 0)
                return false;

            return true;
        }
    }

    #endregion

    #region ID's

    public const byte ClassID = 2;
    public const byte JobID = 20;

    public const uint
        Bootshine = 53,
        TrueStrike = 54,
        SnapPunch = 56,
        TwinSnakes = 61,
        ArmOfTheDestroyer = 62,
        Demolish = 66,
        DragonKick = 74,
        Rockbreaker = 70,
        Thunderclap = 25762,
        HowlingFist = 25763,
        FourPointFury = 16473,
        FormShift = 4262,
        SixSidedStar = 16476,
        ShadowOfTheDestroyer = 25767,
        LeapingOpo = 36945,
        RisingRaptor = 36946,
        PouncingCoeurl = 36947,
        TrueNorth = 7546,

        //Blitzes
        PerfectBalance = 69,
        MasterfulBlitz = 25764,
        ElixirField = 3545,
        ElixirBurst = 36948,
        FlintStrike = 25882,
        RisingPhoenix = 25768,
        CelestialRevolution = 25765,
        TornadoKick = 3543,
        PhantomRush = 25769,

        //Riddles + Buffs
        RiddleOfEarth = 7394,
        EarthsReply = 36944,
        RiddleOfFire = 7395,
        FiresReply = 36950,
        RiddleOfWind = 25766,
        WindsReply = 36949,	
        Brotherhood = 7396,
        Mantra = 65,
        TrueNorth = 7546,

        //Meditations
        InspiritedMeditation = 36941,
        SteeledMeditation = 36940,
        EnlightenedMeditation = 36943,
        ForbiddenMeditation = 36942,
        TheForbiddenChakra = 3547,
        Enlightenment = 16474,
        SteelPeak = 25761;

    internal static class Buffs
    {
        public const ushort
            TwinSnakes = 101,
            OpoOpoForm = 107,
            RaptorForm = 108,
            CoeurlForm = 109,
            PerfectBalance = 110,
            EarthsResolve = 1180,
            RiddleOfFire = 1181,
            RiddleOfWind = 2687,
            FormlessFist = 2513,
            TrueNorth = 1250,
            EarthsRumination = 3841,
            WindsRumination = 3842,
            FiresRumination = 3843,
            Brotherhood = 1185;
    }

    #endregion
}
