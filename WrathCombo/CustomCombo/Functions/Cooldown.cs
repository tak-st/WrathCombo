﻿using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Linq;
using WrathCombo.Data;
using WrathCombo.Services;

namespace WrathCombo.CustomComboNS.Functions
{
    internal abstract partial class CustomComboFunctions
    {
        /// <summary> Gets the cooldown data for an action. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> Cooldown data. </returns>
        public static CooldownData GetCooldown(uint actionID) => Service.ComboCache.GetCooldown(actionID);

        /// <summary> Gets the cooldown total remaining time. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> Total remaining time of the cooldown. </returns>
        public static float GetCooldownRemainingTime(uint actionID) => Service.ComboCache.GetCooldown(actionID).CooldownRemaining;

        /// <summary> Gets the cooldown remaining time for the next charge. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> Remaining time for the next charge of the cooldown. </returns>
        public static float GetCooldownChargeRemainingTime(uint actionID) => Service.ComboCache.GetCooldown(actionID).ChargeCooldownRemaining;

        /// <summary> Gets the elapsed cooldown time.</summary>
        /// <param name="actionID">Action ID to check</param>
        /// <returns> Time passed since action went on cooldown.</returns>
        public static float GetCooldownElapsed(uint actionID) => Service.ComboCache.GetCooldown(actionID).CooldownElapsed;

        /// <summary> Gets a value indicating whether an action is on cooldown. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> True or false. </returns>
        public static bool IsOnCooldown(uint actionID) => GetCooldown(actionID).IsCooldown;

        /// <summary> Gets a value indicating whether an action is off cooldown. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> True or false. </returns>
        public static bool IsOffCooldown(uint actionID) => !GetCooldown(actionID).IsCooldown;

        /// <summary> Check if the Cooldown was just used. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <param name="variance"> Variance of how long to check the elapsed cooldown</param>
        /// <returns> True or false. </returns>
        public static bool JustUsed(uint actionID, float variance = 3f) => GetMaxCharges(actionID) == 0 ? IsOnCooldown(actionID) && GetCooldownElapsed(actionID) <= variance : ActionWatching.ChargeTimestamps.ContainsKey(actionID) ? (Environment.TickCount64 - ActionWatching.ChargeTimestamps[actionID]) / 1000f <= variance : false;


        /// <summary>
        /// Checks if an action has just been used on a given target
        /// </summary>
        /// <param name="actionID"></param>
        /// <param name="target"></param>
        /// <param name="variance"></param>
        /// <returns></returns>
        public static bool JustUsedOn(uint actionID, IGameObject? target, float variance = 3f) => target is null ? false : JustUsedOn(actionID, target.GameObjectId, variance);

        /// <summary>
        /// See <see cref="JustUsedOn(uint, IGameObject?, float)"/>
        /// </summary>
        /// <param name="actionID"></param>
        /// <param name="targetGameobjectId"></param>
        /// <param name="variance"></param>
        /// <returns></returns>
        public static bool JustUsedOn(uint actionID, ulong targetGameobjectId, float variance = 3f)
        {
            if (!ActionWatching.UsedOnDict.ContainsKey((actionID, targetGameobjectId)))
                return false;

            var timestamp = ActionWatching.UsedOnDict[(actionID, targetGameobjectId)];

            var timeDiff = (Environment.TickCount64 - timestamp) / 1000f;

            return timeDiff <= variance;
        }

        /// <summary> Gets a value indicating whether an action has any available charges. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> True or false. </returns>
        public static bool HasCharges(uint actionID) => GetCooldown(actionID).RemainingCharges > 0;

        /// <summary> Get the current number of charges remaining for an action. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> Number of charges. </returns>
        public static uint GetRemainingCharges(uint actionID) => GetCooldown(actionID).RemainingCharges;

        /// <summary> Get the maximum number of charges for an action. </summary>
        /// <param name="actionID"> Action ID to check. </param>
        /// <returns> Number of charges. </returns>
        public static ushort GetMaxCharges(uint actionID) => GetCooldown(actionID).MaxCharges;

        /// <summary> Get if an action is enabled.</summary>
        /// <param name="actionID"> Action ID to check</param>
        /// <returns> If the action is currently enabled.</returns>
        public unsafe static bool IsEnabled(uint actionID) => ActionManager.Instance()->GetActionStatus(ActionType.Action, actionID) == 0;

        private static uint Action1 => ActionManager.GetDutyActionId(0);
        private static uint Action2 => ActionManager.GetDutyActionId(1);

        public static bool HasActionEquipped(uint actionId) => (Action1 == actionId && HasCharges(actionId)) || (Action2 == actionId && HasCharges(actionId));

        private unsafe static RecastDetail* GCD => ActionManager.Instance()->GetRecastGroupDetail(57);

        public unsafe static float GCDTotal => GCD->Total;

        public unsafe static float RemainingGCD => GCDTotal - GCD->Elapsed;
    }
}
