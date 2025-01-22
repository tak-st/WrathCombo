#region

using Dalamud.Interface.Utility;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System.Numerics;
using WrathCombo.Combos.PvP;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Window.Functions;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace WrathCombo.Combos.PvE;

internal partial class DRK
{
    /// <summary>
    ///     Configuration options for Dark Knight.<br />
    ///     <see cref="UserInt" />s and GUI for options.
    /// </summary>
    internal static class Config
    {
        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                #region Advanced Single Target

                case CustomComboPreset.DRK_ST_BalanceOpener:
                    UserConfig.DrawBossOnlyChoice(DRK_ST_OpenerDifficulty);
                    break;

                case CustomComboPreset.DRK_ST_Delirium:
                    UserConfig.DrawSliderInt(0, 25, DRK_ST_DeliriumThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_ST_DeliriumThresholdDifficulty,
                        DRK_ST_DeliriumThresholdDifficultyListSet
                    );

                    break;

                case CustomComboPreset.DRK_ST_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 30, DRK_ST_LivingShadowThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_ST_LivingShadowThresholdDifficulty,
                        DRK_ST_LivingShadowThresholdDifficultyListSet
                    );

                    break;

                case CustomComboPreset.DRK_ST_ManaSpenderPooling:
                    UserConfig.DrawSliderInt(0, 3000, DRK_ST_ManaSpenderPooling,
                        "Mana to save for TBN (0 = Use All)",
                        itemWidth: biggest,
                        sliderIncrement: SliderIncrements.Thousands);
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_ST_ManaSpenderPoolingDifficulty,
                        DRK_ST_ManaSpenderPoolingDifficultyListSet
                    );

                    break;

                case CustomComboPreset.DRK_ST_Mitigation:
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_ST_MitDifficulty,
                        DRK_ST_MitDifficultyListSet,
                        "Select what difficulties mitigation should be used in:"
                    );
                    ImGuiHelpers.ScaledDummy(15.0f);

                    break;

                case CustomComboPreset.DRK_ST_TBN:
                    UserConfig.DrawSliderInt(5, 40, DRK_ST_TBNThreshold,
                        startUsingAtDescription,
                        itemWidth: medium, sliderIncrement: SliderIncrements.Fives);

                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_TBNBossRestriction, "All Enemies",
                        "Will use The Blackest Night regardless of the type of enemy.",
                        outputValue: (int) BossAvoidance.Off, itemWidth: 125f);
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_TBNBossRestriction, "Avoid Bosses",
                        "Will try not to use Blackest Night when your target is a boss.\n" +
                        "(Note: don't rely on this 100%, square sometimes marks enemies inconsistently)",
                        outputValue: (int) BossAvoidance.On, itemWidth: 125f);

                    break;

                case CustomComboPreset.DRK_ST_ShadowedVigil:
                    UserConfig.DrawSliderInt(5, 55, DRK_ST_ShadowedVigilThreshold,
                        startUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_ST_LivingDead:
                    UserConfig.DrawSliderInt(5, 40, DRK_ST_LivingDeadSelfThreshold,
                        startUsingAtDescription,
                        itemWidth: medium, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawSliderInt(0, 10, DRK_ST_LivingDeadTargetThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_LivingDeadBossRestriction, "All Enemies",
                        "Will use Living Dead regardless of the type of enemy.",
                        outputValue: (int) BossAvoidance.Off, itemWidth: 125f);
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_ST_LivingDeadBossRestriction, "Avoid Bosses",
                        "Will try not to use Living Dead when your target is a boss.\n" +
                        "(Note: don't rely on this 100%, square sometimes marks enemies inconsistently)",
                        outputValue: (int) BossAvoidance.On, itemWidth: 125f);

                    break;

                #endregion

                #region Advanced AoE

                case CustomComboPreset.DRK_AoE_Delirium:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_DeliriumThreshold,
                        stopUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_AoE_DeliriumThresholdDifficulty,
                        DRK_AoE_DeliriumThresholdDifficultyListSet
                    );

                    break;

                case CustomComboPreset.DRK_AoE_CDs_LivingShadow:
                    UserConfig.DrawSliderInt(0, 60, DRK_AoE_LivingShadowThreshold,
                        stopUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_AoE_LivingShadowThresholdDifficulty,
                        DRK_AoE_LivingShadowThresholdDifficultyListSet
                    );

                    break;

                case CustomComboPreset.DRK_AoE_ShadowedVigil:
                    UserConfig.DrawSliderInt(5, 55, DRK_AoE_ShadowedVigilThreshold,
                        startUsingAtDescription,
                        itemWidth: bigger, sliderIncrement: SliderIncrements.Fives);

                    break;

                case CustomComboPreset.DRK_AoE_LivingDead:
                    UserConfig.DrawSliderInt(5, 30, DRK_AoE_LivingDeadSelfThreshold,
                        startUsingAtDescription,
                        itemWidth: medium, sliderIncrement: SliderIncrements.Fives);
                    UserConfig.DrawSliderInt(0, 40,
                        DRK_AoE_LivingDeadTargetThreshold,
                        stopUsingAtDescription,
                        itemWidth: little, sliderIncrement: SliderIncrements.Tens);

                    break;

                #endregion

                case CustomComboPreset.DRK_Variant_Cure:
                    UserConfig.DrawSliderInt(5, 70, DRK_VariantCure,
                        startUsingAtDescription,
                        itemWidth: biggest, sliderIncrement: SliderIncrements.Fives);

                    break;

                #region PVP

                case CustomComboPreset.DRKPvP_Shadowbringer:
                    UserConfig.DrawSliderInt(20, 100,
                        DRKPvP.Config.ShadowbringerThreshold,
                        "HP% to be at or Above to use ",
                        itemWidth: 150f, sliderIncrement: SliderIncrements.Fives);

                    break;

                #endregion

                #region One-Button Mitigation

                case CustomComboPreset.DRK_Mit_LivingDead_Max:
                    UserConfig.DrawDifficultyMultiChoice(
                        DRK_Mit_EmergencyLivingDead_Difficulty,
                        DRK_Mit_EmergencyLivingDead_DifficultyListSet,
                        "Select what difficulties Emergency Living Dead should be used in:"
                    );

                    UserConfig.DrawSliderInt(1, 100, DRK_Mit_LivingDead_Health,
                        startUsingAtDescription,
                        itemWidth: medium, sliderIncrement: SliderIncrements.Ones);

                    break;

                case CustomComboPreset.DRK_Mit_TheBlackestNight:
                    UserConfig.DrawSliderInt(1, 100, DRK_Mit_TBN_Health,
                        startUsingAtDescription + " (100 = Disable check)",
                        itemWidth: medium, sliderIncrement: SliderIncrements.Tens);

                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 0,
                        "The Blackest Night Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_Oblation:
                    UserConfig.DrawSliderInt(0, 1, DRK_Mit_Oblation_Charges,
                        "How many charges to keep ready? (0 = Use All)",
                        itemWidth: little, sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 1,
                        "Oblation Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_Reprisal:
                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 2,
                        "Reprisal Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_DarkMissionary:
                    ImGui.Dummy(new Vector2(15f.Scale(), 0f));
                    ImGui.SameLine();
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_Mit_DarkMissionary_PartyRequirement,
                        "Require party",
                        "Will not use Dark Missionary unless there are 2 or more party members.",
                        outputValue: (int) PartyRequirement.Yes, itemWidth: medium);
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_Mit_DarkMissionary_PartyRequirement,
                        "Use Always",
                        "Will not require a party for Dark Missionary.",
                        outputValue: (int) PartyRequirement.No, itemWidth: medium);

                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 3,
                        "Dark Missionary Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_Rampart:
                    UserConfig.DrawSliderInt(1, 100, DRK_Mit_Rampart_Health,
                        startUsingAtDescription + " (100 = Disable check)",
                        itemWidth: medium, sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 4,
                        "Rampart Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_DarkMind:
                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 5,
                        "Dark Mind Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_ArmsLength:
                    ImGui.Dummy(new Vector2(15f.Scale(), 0f));
                    ImGui.SameLine();
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_Mit_ArmsLength_Boss, "All Enemies",
                        "Will use Arm's Length regardless of the type of enemy.",
                        outputValue: (int) BossAvoidance.Off, itemWidth: 125f);
                    UserConfig.DrawHorizontalRadioButton(
                        DRK_Mit_ArmsLength_Boss, "Avoid Bosses",
                        "Will try not to use Arm's Length when in a boss fight.",
                        outputValue: (int) BossAvoidance.On, itemWidth: 125f);

                    UserConfig.DrawSliderInt(0, 3, DRK_Mit_ArmsLength_EnemyCount,
                        "How many enemies should be nearby? (0 = No Requirement)",
                        itemWidth: little, sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 6,
                        "Arm's Length Priority:");

                    break;

                case CustomComboPreset.DRK_Mit_ShadowWall:
                    UserConfig.DrawSliderInt(1, 100, DRK_Mit_ShadowWall_Health,
                        startUsingAtDescription + " (100 = Disable check)",
                        itemWidth: medium, sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(DRK_Mit_Priorities,
                        numberMitigationOptions, 7,
                        "Shadow Wall / Vigil Priority:");

                    break;

                    #endregion
            }
        }

        #region Constants

        /// Number of Mitigation Options
        private const int numberMitigationOptions = 8;

        /// Smallest bar width
        private const float little = 100f;

        /// 2nd smallest bar width
        private const float medium = 150f;

        /// 2nd biggest bar width
        private const float bigger = 175f;

        /// Biggest bar width
        private const float biggest = 200f;

        /// Bar Description for HP% to stop using
        private const string stopUsingAtDescription =
            "Target HP% to stop using (0 = Use Always)";

        /// Bar Description for HP% to start using
        private const string startUsingAtDescription =
            "HP% to use at or below";

        /// <summary>
        ///     Whether abilities should be restricted to Bosses or not.
        /// </summary>
        /// <seealso cref="DRK_ST_TBNBossRestriction" />
        /// <seealso cref="DRK_ST_LivingDeadBossRestriction" />
        internal enum BossAvoidance
        {
            Off = 1,
            On = 2
        }

        internal enum PartyRequirement
        {
            No,
            Yes
        }

        #endregion

        #region Options

        #region Advanced Single Target

        /// <summary>
        ///     Content type of Balance Opener for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.IsInBossOnlyContent" /> <br />
        ///     <b>Options</b>: All Content or
        ///     <see cref="ContentCheck.IsInBossOnlyContent" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_BalanceOpener" />
        public static readonly UserBoolArray DRK_ST_OpenerDifficulty =
            new("DRK_ST_OpenerDifficulty", [false, true]);

        /// <summary>
        ///     Target HP% to use Delirium above for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 0<br />
        ///     <b>Range</b>: 0 - 25 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_Delirium" />
        public static readonly UserInt DRK_ST_DeliriumThreshold =
            new("DRK_ST_DeliriumThreshold", 0);

        /// <summary>
        ///     Difficulty of Delirium Threshold for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="DRK_ST_DeliriumThreshold" />
        public static readonly UserBoolArray DRK_ST_DeliriumThresholdDifficulty =
            new("DRK_ST_DeliriumThresholdDifficulty", [true, false]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_ST_DeliriumThresholdDifficulty" /> is set to.
        /// </summary>
        /// <seealso cref="DRK_ST_DeliriumThresholdDifficulty" />
        public static readonly ContentCheck.ListSet
            DRK_ST_DeliriumThresholdDifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     Target HP% to use Living Shadow above for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 5 <br />
        ///     <b>Range</b>: 0 - 30 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_CDs_LivingShadow" />
        public static readonly UserInt DRK_ST_LivingShadowThreshold =
            new("DRK_ST_LivingShadowThreshold", 5);

        /// <summary>
        ///     Difficulty of Living Shadow Threshold for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="DRK_ST_LivingShadowThreshold" />
        public static readonly UserBoolArray DRK_ST_LivingShadowThresholdDifficulty =
            new("DRK_ST_LivingShadowThresholdDifficulty", [true, false]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_ST_LivingShadowThresholdDifficulty" /> is set to.
        /// </summary>
        public static readonly ContentCheck.ListSet
            DRK_ST_LivingShadowThresholdDifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     How much mana to save for TBN.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 3000 <br />
        ///     <b>Range</b>: 0 - 3000 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Thousands" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_ManaSpenderPooling" />
        public static readonly UserInt DRK_ST_ManaSpenderPooling =
            new("DRK_ST_ManaSpenderPooling", 3000);

        /// <summary>
        ///     Difficulty of Mana Spender Pooling for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="DRK_ST_ManaSpenderPooling" />
        public static readonly UserBoolArray DRK_ST_ManaSpenderPoolingDifficulty =
            new("DRK_ST_ManaSpenderPoolingDifficulty", [false, true]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_ST_ManaSpenderPoolingDifficulty" /> is set to.
        /// </summary>
        /// <seealso cref="DRK_ST_ManaSpenderPoolingDifficulty" />
        public static readonly ContentCheck.ListSet
            DRK_ST_ManaSpenderPoolingDifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     Self HP% to use TBN below for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 25 <br />
        ///     <b>Range</b>: 5 - 40 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_TBN" />
        public static readonly UserInt DRK_ST_TBNThreshold =
            new("DRK_ST_TBNThreshold", 25);

        /// <summary>
        ///     Difficulty of Mitigation for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_Mitigation" />
        public static readonly UserBoolArray DRK_ST_MitDifficulty =
            new("DRK_ST_MitDifficulty", [true, false]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_ST_MitDifficulty" /> is set to.
        /// </summary>
        /// <seealso cref="DRK_ST_MitDifficulty" />
        public static readonly ContentCheck.ListSet
            DRK_ST_MitDifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     TBN Boss Restriction for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="BossAvoidance.Off" /> <br />
        ///     <b>Options</b>: <see cref="BossAvoidance">BossAvoidance Enum</see>
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_TBN" />
        public static readonly UserInt DRK_ST_TBNBossRestriction =
            new("DRK_ST_TBNBossRestriction", (int) BossAvoidance.Off);

        /// <summary>
        ///     Self HP% to use Shadowed Vigil below for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 40 <br />
        ///     <b>Range</b>: 5 - 55 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_ShadowedVigil" />
        public static readonly UserInt DRK_ST_ShadowedVigilThreshold =
            new("DRK_ST_ShadowedVigilThreshold", 40);

        /// <summary>
        ///     Self HP% to use Living Dead below for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 14 <br />
        ///     <b>Range</b>: 5 - 40 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_LivingDead" />
        public static readonly UserInt DRK_ST_LivingDeadSelfThreshold =
            new("DRK_ST_LivingDeadSelfThreshold", 15);

        /// <summary>
        ///     Target HP% to use Living Dead above for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 1 <br />
        ///     <b>Range</b>: 0 - 10 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Ones" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_LivingDead" />
        public static readonly UserInt DRK_ST_LivingDeadTargetThreshold =
            new("DRK_ST_LivingDeadTargetThreshold", 1);

        /// <summary>
        ///     Living Dead Boss Restriction for Single Target.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="BossAvoidance.On" /> <br />
        ///     <b>Options</b>: <see cref="BossAvoidance">BossAvoidance Enum</see>
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_ST_LivingDead" />
        public static readonly UserInt DRK_ST_LivingDeadBossRestriction =
            new("DRK_ST_LivingDeadBossRestriction", (int) BossAvoidance.On);

        #endregion

        #region Advanced AoE

        /// <summary>
        ///     Target HP% to use Delirium above for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 25 <br />
        ///     <b>Range</b>: 0 - 60 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_AoE_Delirium" />
        public static readonly UserInt DRK_AoE_DeliriumThreshold =
            new("DRK_AoE_DeliriumThreshold", 25);

        /// <summary>
        ///     Difficulty of Delirium Threshold for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="DRK_AoE_DeliriumThreshold" />
        public static readonly UserBoolArray DRK_AoE_DeliriumThresholdDifficulty =
            new("DRK_AoE_DeliriumThresholdDifficulty", [true, false]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_AoE_DeliriumThresholdDifficulty" /> is set to.
        /// </summary>
        /// <seealso cref="DRK_AoE_DeliriumThresholdDifficulty" />
        public static readonly ContentCheck.ListSet
            DRK_AoE_DeliriumThresholdDifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     Target HP% to use Living Shadow above for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 40 <br />
        ///     <b>Range</b>: 0 - 60 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_AoE_CDs_LivingShadow" />
        public static readonly UserInt DRK_AoE_LivingShadowThreshold =
            new("DRK_AoE_LivingShadowThreshold", 40);

        /// <summary>
        ///     Difficulty of Living Shadow Threshold for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="DRK_AoE_LivingShadowThreshold" />
        public static readonly UserBoolArray
            DRK_AoE_LivingShadowThresholdDifficulty =
                new("DRK_AoE_LivingShadowThresholdDifficulty", [true, false]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_AoE_LivingShadowThresholdDifficulty" /> is set to.
        /// </summary>
        /// <seealso cref="DRK_AoE_LivingShadowThresholdDifficulty" />
        public static readonly ContentCheck.ListSet
            DRK_AoE_LivingShadowThresholdDifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     Self HP% to use Shadowed Vigil below for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 50 <br />
        ///     <b>Range</b>: 5 - 55 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_AoE_ShadowedVigil" />
        public static readonly UserInt DRK_AoE_ShadowedVigilThreshold =
            new("DRK_AoE_ShadowedVigilThreshold", 50);

        /// <summary>
        ///     Self HP% to use Living Dead below for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 20 <br />
        ///     <b>Range</b>: 5 - 30 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_AoE_LivingDead" />
        public static readonly UserInt DRK_AoE_LivingDeadSelfThreshold =
            new("DRK_AoE_LivingDeadSelfThreshold", 20);

        /// <summary>
        ///     Target HP% to use Living Dead above for AoE.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 15 <br />
        ///     <b>Range</b>: 0 - 40 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Tens" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_AoE_LivingDead" />
        public static readonly UserInt DRK_AoE_LivingDeadTargetThreshold =
            new("DRK_AoE_LivingDeadTargetThreshold", 15);

        #endregion

        /// <summary>
        ///     Variant Cure.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 30 <br />
        ///     <b>Range</b>: 5 - 70 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Variant_Cure" />
        public static readonly UserInt DRK_VariantCure =
            new("DRKVariantCure", 30);

        #region One-Button Mitigation

        /// <summary>
        ///     Difficulty of Emergency Living Dead for Mitigation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="ContentCheck.BottomHalfContent" /> <br />
        ///     <b>Options</b>: <see cref="ContentCheck.BottomHalfContent" />
        ///     and/or <see cref="ContentCheck.TopHalfContent" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_LivingDead_Max" />
        public static readonly UserBoolArray
            DRK_Mit_EmergencyLivingDead_Difficulty =
                new("DRK_Mit_EmergencyLivingDead_Difficulty", [true, false]);

        /// <summary>
        ///     What Difficulty List Set
        ///     <see cref="DRK_Mit_EmergencyLivingDead_Difficulty" /> is set to.
        /// </summary>
        /// <seealso cref="CustomComboPreset.DRK_Mit_LivingDead_Max" />
        public static readonly ContentCheck.ListSet
            DRK_Mit_EmergencyLivingDead_DifficultyListSet =
                ContentCheck.ListSet.Halved;

        /// <summary>
        ///     Self HP% to use Living Dead below in the Mitigation Rotation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 20 <br />
        ///     <b>Range</b>: 5 - 30 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_LivingDead_Max" />
        public static UserInt DRK_Mit_LivingDead_Health =
            new("DRK_Mit_LivingDead_Health", 20);

        /// <summary>
        ///     Mitigation Ability Priority List.
        /// </summary>
        public static readonly UserIntArray
            DRK_Mit_Priorities =
                new("DRK_Mit_Priorities");

        /// <summary>
        ///     Self HP% to use The Blackest Night below in the Mitigation Rotation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 60 <br />
        ///     <b>Range</b>: 10 - 100 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Tens" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_TheBlackestNight" />
        public static UserInt DRK_Mit_TBN_Health =
            new("DRK_Mit_TBN_Health", 60);

        /// <summary>
        ///     Party requirement for using Dark Missionary in the Mitigation Rotation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="PartyRequirement.Yes" /> <br />
        ///     <b>Options</b>: <see cref="PartyRequirement">PartyRequirement Enum</see>
        /// </value>
        public static readonly UserInt
            DRK_Mit_DarkMissionary_PartyRequirement =
                new("DRK_Mit_DarkMissionary_PartyRequirement",
                    (int) PartyRequirement.Yes);

        /// <summary>
        ///     Self HP% to use Rampart below in the Mitigation Rotation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 65 <br />
        ///     <b>Range</b>: 40 - 100 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_Rampart" />
        public static UserInt DRK_Mit_Rampart_Health =
            new("DRK_Mit_Rampart_Health", 65);

        /// <summary>
        ///     Arm's Length Boss Restriction for Mitigation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: <see cref="BossAvoidance.On" /> <br />
        ///     <b>Options</b>: <see cref="BossAvoidance">BossAvoidance Enum</see>
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_ArmsLength" />
        public static readonly UserInt DRK_Mit_ArmsLength_Boss =
            new("DRK_Mit_ArmsLength_Boss", (int) BossAvoidance.On);

        /// <summary>
        ///     The number of enemies to be nearby for Arm's Length.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 0 <br />
        ///     <b>Range</b>: 0 - 3 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Ones" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_ArmsLength" />
        public static UserInt DRK_Mit_ArmsLength_EnemyCount =
            new("DRK_Mit_ArmsLength_EnemyCount", 0);

        /// <summary>
        ///     Self HP% to use Shadow Wall below in the Mitigation Rotation.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 45 <br />
        ///     <b>Range</b>: 30 - 100 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Fives" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_ShadowWall" />
        public static UserInt DRK_Mit_ShadowWall_Health =
            new("DRK_Mit_ShadowWall_Health", 45);

        /// <summary>
        ///     The number of Oblation charges to keep for manual use.
        /// </summary>
        /// <value>
        ///     <b>Default</b>: 0 <br />
        ///     <b>Range</b>: 0 - 1 <br />
        ///     <b>Step</b>: <see cref="SliderIncrements.Ones" />
        /// </value>
        /// <seealso cref="CustomComboPreset.DRK_Mit_Oblation" />
        public static UserInt DRK_Mit_Oblation_Charges =
            new("DRK_Mit_Oblation_Charges", 0);

        #endregion

        #endregion
    }
}
