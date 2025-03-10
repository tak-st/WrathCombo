﻿using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;

namespace WrathCombo.Extensions
{
    internal static class UIntExtensions
    {
        internal static bool LevelChecked(this uint value) => CustomComboFunctions.LevelChecked(value);

        internal static bool TraitLevelChecked(this uint value) => CustomComboFunctions.TraitLevelChecked(value);

        internal static string ActionName(this uint value) => ActionWatching.GetActionName(value);

        internal static int Role(this uint value) => CustomComboFunctions.JobIDs.JobIDToRole(value);
    }

    internal static class UShortExtensions
    {
        internal static string StatusName(this ushort value) => ActionWatching.GetStatusName(value);
    }
}
