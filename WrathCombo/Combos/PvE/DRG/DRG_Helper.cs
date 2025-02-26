﻿using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.Combos.PvE.Content;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
namespace WrathCombo.Combos.PvE;

internal partial class DRG
{
    internal static DRGGauge Gauge = GetJobGauge<DRGGauge>();
    internal static StandardOpenerLogic StandardOpener = new();
    internal static PiercingTalonOpenerLogic PiercingTalonOpener = new();

    internal static readonly List<uint> FastLocks =
    [
        BattleLitany,
        LanceCharge,
        LifeSurge,
        Geirskogul,
        Nastrond,
        MirageDive,
        WyrmwindThrust,
        RiseOfTheDragon,
        Starcross,
        Variant.VariantRampart,
        All.TrueNorth
    ];

    internal static readonly List<uint> MidLocks =
    [
        Jump,
        HighJump,
        DragonfireDive
    ];

    internal static Status? ChaosDoTDebuff =>
        FindTargetEffect(LevelChecked(ChaoticSpring)
            ? Debuffs.ChaoticSpring
            : Debuffs.ChaosThrust);

    internal static bool TrueNorthReady =>
        TargetNeedsPositionals() && ActionReady(All.TrueNorth) &&
        !HasEffect(All.Buffs.TrueNorth);

    internal static uint SlowLock => Stardiver;

    internal static WrathOpener Opener()
    {
        if (StandardOpener.LevelChecked && Config.DRG_SelectedOpener == 0)
            return StandardOpener;

        if (PiercingTalonOpener.LevelChecked && Config.DRG_SelectedOpener == 1)
            return PiercingTalonOpener;

        return WrathOpener.Dummy;
    }

    internal static bool CanDRGWeave(uint oGCD)
    {
        float gcdTimer = GetCooldownRemainingTime(TrueThrust);

        //GCD Ready - No Weave
        if (IsOffCooldown(TrueThrust))
            return false;

        if (FastLocks.Any(x => x == oGCD) && gcdTimer >= 0.6f)
            return true;

        if (MidLocks.Any(x => x == oGCD) && gcdTimer >= 0.8f)
            return true;

        if (SlowLock == oGCD && gcdTimer >= 1.5f)
            return true;

        return false;
    }

    internal static bool UseLifeSurge()
    {
        if (ActionReady(LifeSurge) && CanDRGWeave(LifeSurge) && !HasEffect(Buffs.LifeSurge))
        {
            if (LevelChecked(Drakesbane) && Gauge.IsLOTDActive &&
                (HasEffect(Buffs.LanceCharge) || HasEffect(Buffs.BattleLitany)) &&
                (JustUsed(WheelingThrust) ||
                 JustUsed(FangAndClaw) ||
                 JustUsed(OriginalHook(VorpalThrust)) && LevelChecked(HeavensThrust)))
                return true;
        }

        return false;
    }



    #region Openers

    internal class StandardOpenerLogic : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            TrueThrust,
            SpiralBlow,
            LanceCharge,
            ChaoticSpring,
            BattleLitany,
            Geirskogul,
            WheelingThrust,
            HighJump,
            LifeSurge,
            Drakesbane,
            DragonfireDive,
            Nastrond,
            RaidenThrust,
            Stardiver,
            LanceBarrage,
            Starcross,
            LifeSurge,
            HeavensThrust,
            RiseOfTheDragon,
            MirageDive,
            FangAndClaw,
            Drakesbane,
            RaidenThrust,
            WyrmwindThrust
        ];
        internal override UserData ContentCheckConfig => Config.DRG_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(LifeSurge) < 2)
                return false;

            if (!IsOffCooldown(BattleLitany))
                return false;

            if (!IsOffCooldown(DragonfireDive))
                return false;

            if (!IsOffCooldown(LanceCharge))
                return false;

            return true;
        }
    }

    internal class PiercingTalonOpenerLogic : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            PiercingTalon,
            TrueThrust,
            SpiralBlow,
            LanceCharge,
            BattleLitany,
            ChaoticSpring,
            Geirskogul,
            WheelingThrust,
            HighJump,
            LifeSurge,
            Drakesbane,
            DragonfireDive,
            Nastrond,
            RaidenThrust,
            Stardiver,
            LanceBarrage,
            Starcross,
            LifeSurge,
            HeavensThrust,
            RiseOfTheDragon,
            MirageDive,
            FangAndClaw,
            Drakesbane,
            RaidenThrust,
            WyrmwindThrust
        ];
        internal override UserData ContentCheckConfig => Config.DRG_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(LifeSurge) < 2)
                return false;

            if (!IsOffCooldown(BattleLitany))
                return false;

            if (!IsOffCooldown(DragonfireDive))
                return false;

            if (!IsOffCooldown(LanceCharge))
                return false;

            return true;
        }
    }

    #endregion

    #region ID's

    public const byte ClassID = 4;
    public const byte JobID = 22;

    public const uint
        PiercingTalon = 90,
        ElusiveJump = 94,
        LanceCharge = 85,
        BattleLitany = 3557,
        Jump = 92,
        LifeSurge = 83,
        HighJump = 16478,
        MirageDive = 7399,
        BloodOfTheDragon = 3553,
        Stardiver = 16480,
        CoerthanTorment = 16477,
        DoomSpike = 86,
        SonicThrust = 7397,
        ChaosThrust = 88,
        RaidenThrust = 16479,
        TrueThrust = 75,
        Disembowel = 87,
        FangAndClaw = 3554,
        WheelingThrust = 3556,
        FullThrust = 84,
        VorpalThrust = 78,
        WyrmwindThrust = 25773,
        DraconianFury = 25770,
        ChaoticSpring = 25772,
        DragonfireDive = 96,
        Geirskogul = 3555,
        Nastrond = 7400,
        HeavensThrust = 25771,
        Drakesbane = 36952,
        RiseOfTheDragon = 36953,
        LanceBarrage = 36954,
        SpiralBlow = 36955,
        Starcross = 36956;

    public static class Buffs
    {
        public const ushort
            LanceCharge = 1864,
            BattleLitany = 786,
            DiveReady = 1243,
            RaidenThrustReady = 1863,
            PowerSurge = 2720,
            LifeSurge = 116,
            DraconianFire = 1863,
            NastrondReady = 3844,
            StarcrossReady = 3846,
            DragonsFlight = 3845;
    }

    public static class Debuffs
    {
        public const ushort
            ChaosThrust = 118,
            ChaoticSpring = 2719;
    }

    public static class Traits
    {
        public const uint
            EnhancedLifeSurge = 438;
    }

    #endregion
}
