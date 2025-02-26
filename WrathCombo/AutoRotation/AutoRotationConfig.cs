﻿namespace WrathCombo.AutoRotation
{
    public class AutoRotationConfig
    {
        public bool Enabled;
        public bool InCombatOnly;
        public bool BypassQuest;
        public bool BypassFATE;
        public int CombatDelay = 1;
        public bool EnableInInstance;
        public bool DisableAfterInstance;
        public DPSRotationMode DPSRotationMode;
        public HealerRotationMode HealerRotationMode;
        public HealerSettings HealerSettings = new();
        public DPSSettings DPSSettings = new();
        public int Throttler = 50;
        public bool OrbwalkerIntegration;
    }

    public class DPSSettings
    {
        public bool FATEPriority = false;
        public bool QuestPriority = false;
        public int? DPSAoETargets = 3;
        public bool PreferNonCombat = false;
        public bool OnlyAttackInCombat = false;
        public bool AlwaysSelectTarget = true;
        public float MaxDistance = 25;
        public bool AoEIgnoreManual = false;
    }

    public class HealerSettings
    {
        public int SingleTargetHPP = 70;
        public int AoETargetHPP = 80;
        public int SingleTargetRegenHPP = 60;
        public int? AoEHealTargetCount = 2;
        public int HealDelay = 1;
        public bool ManageKardia = false;
        public bool KardiaTanksOnly = false;
        public bool AutoRez = false;
        public bool AutoRezRequireSwift = false;
        public bool AutoRezDPSJobs = false;
        public bool AutoCleanse = false;
        public bool PreEmptiveHoT = false;
        public bool IncludeNPCs = false;

    }
}
