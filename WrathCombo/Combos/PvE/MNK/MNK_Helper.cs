﻿using Dalamud.Game.ClientState.JobGauge.Enums;
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
        if (HasEffect(Buffs.OpoOpoForm) || HasEffect(Buffs.FormlessFist))
            return Gauge.OpoOpoFury == 0 && LevelChecked(OriginalHook(DragonKick))
                ? OriginalHook(DragonKick)
                : OriginalHook(Bootshine);

        if (HasEffect(Buffs.RaptorForm))
            return Gauge.RaptorFury == 0 && LevelChecked(TwinSnakes)
                ? TwinSnakes
                : OriginalHook(TrueStrike);

        if (HasEffect(Buffs.CoeurlForm))
        {
            if (Gauge.CoeurlFury == 0 && LevelChecked(Demolish))
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

        return actionId;
    }

    internal static bool UsePerfectBalance()
    {
        if (ActionReady(PerfectBalance) && !HasEffect(Buffs.PerfectBalance) && !HasEffect(Buffs.FormlessFist) && (Config.MNK_ST_FiresReply_Order != 0 || !HasEffect(Buffs.FiresRumination)))
        {
            // Odd window
            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick), GCD)) &&
                !JustUsed(PerfectBalance, 20) &&
                HasEffect(Buffs.RiddleOfFire) && !HasEffect(Buffs.Brotherhood) &&
                !BothNadisOpen)
                return true;

            // Even window
            if ((JustUsed(OriginalHook(Bootshine), GCD * 2) || JustUsed(OriginalHook(DragonKick), GCD * 2) || GetCooldownRemainingTime(Brotherhood) <= GCD * 1.8 || GetBuffRemainingTime(Buffs.WindsRumination) > 12 || GetCooldownRemainingTime(RiddleOfWind) < 20) &&
                (GetCooldownRemainingTime(Brotherhood) <= GCD * 2.8 || HasEffect(Buffs.Brotherhood)))
                return true;

            // Low level
            if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(OriginalHook(DragonKick))) &&
                ((HasEffect(Buffs.RiddleOfFire) && !LevelChecked(Brotherhood)) ||
                 !LevelChecked(RiddleOfFire)))
                return true;
        }

        if (ActionReady(PerfectBalance) && !HasEffect(Buffs.PerfectBalance) && !HasBattleTarget() && HasEffect(Buffs.RiddleOfFire))
        {
            // Odd window
            if (!JustUsed(PerfectBalance, 20) && !HasEffect(Buffs.Brotherhood) && !BothNadisOpen && GetBuffRemainingTime(Buffs.RiddleOfFire) <= 20 - GCD)
                return true;

            // Even window
            if (HasEffect(Buffs.Brotherhood))
                return true;
        }

        return false;
    }

    #region ID's

    public const byte ClassID = 2;
    public const byte JobID = 20;

    public const uint
        Bootshine = 53,
        TrueStrike = 54,
        SnapPunch = 56,
        SteeledMeditation = 36940,
        SteelPeak = 25761,
        TwinSnakes = 61,
        ArmOfTheDestroyer = 62,
        Demolish = 66,
        Mantra = 65,
        DragonKick = 74,
        Rockbreaker = 70,
        Thunderclap = 25762,
        HowlingFist = 25763,
        FourPointFury = 16473,
        PerfectBalance = 69,
        FormShift = 4262,
        TheForbiddenChakra = 3547,
        MasterfulBlitz = 25764,
        RiddleOfEarth = 7394,
        EarthsReply = 36944,
        RiddleOfFire = 7395,
        Brotherhood = 7396,
        RiddleOfWind = 25766,
        InspiritedMeditation = 36941,
        EnlightenedMeditation = 36943,
        Enlightenment = 16474,
        SixSidedStar = 16476,
        ShadowOfTheDestroyer = 25767,
        RisingPhoenix = 25768,
        WindsReply = 36949,
        ForbiddenMeditation = 36942,
        LeapingOpo = 36945,
        RisingRaptor = 36946,
        PouncingCoeurl = 36947,
        TrueNorth = 7546,
        ElixirBurst = 36948,
        FiresReply = 36950;

    internal static class Buffs
    {
        public const ushort
            TwinSnakes = 101,
            OpoOpoForm = 107,
            RaptorForm = 108,
            CoeurlForm = 109,
            PerfectBalance = 110,
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

    #region Openers

    internal static WrathOpener Opener()
    {
        if (Config.MNK_SelectedOpener == 4)
            return GetPartyMembers().Any(x => new[] { DNC.JobID, PCT.JobID }.Contains(x.BattleChara.ClassJob.RowId)) ? MNKOpenerLL7 : MNKOpenerLL;

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
}
