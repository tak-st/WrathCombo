using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Extensions;
using static WrathCombo.Window.Functions.UserConfig;
namespace WrathCombo.Combos.PvE;

internal partial class MNK
{
    internal static class Config
    {
        public static UserInt
            MNK_ST_Brotherhood_HP = new("MNK_ST_Brotherhood_HP", 0),
            MNK_ST_RiddleOfFire_HP = new("MNK_ST_RiddleOfFire_HP", 0),
            MNK_ST_RiddleOfWind_HP = new("MNK_ST_RiddleOfWind_HP", 0),
            MNK_AoE_Brotherhood_HP = new("MNK_AoE_Brotherhood_HP", 5),
            MNK_AoE_RiddleOfWind_HP = new("MNK_AoE_RiddleOfWind_HP", 5),
            MNK_AoE_RiddleOfFire_HP = new("MNK_AoE_RiddleOfFire_HP", 5),
            MNK_ST_SecondWind_Threshold = new("MNK_ST_SecondWindThreshold", 25),
            MNK_ST_Bloodbath_Threshold = new("MNK_ST_BloodbathThreshold", 40),
            MNK_AoE_SecondWind_Threshold = new("MNK_AoE_SecondWindThreshold", 25),
            MNK_AoE_Bloodbath_Threshold = new("MNK_AoE_BloodbathThreshold", 40),
            MNK_VariantCure = new("MNK_Variant_Cure"),
            MNK_SelectedOpener = new("MNK_SelectedOpener", 0),
            MNK_Balance_Content = new("MNK_Balance_Content", 1);
            MNK_ST_FiresReply_Order = new("MNK_ST_FiresReply_Order", 0);
            MNK_ST_Phoenix_Order = new("MNK_ST_Phoenix_Order", 0);

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.MNK_ST_ComboHeals:
                    DrawSliderInt(0, 100, MNK_ST_SecondWind_Threshold,
                        $"HP percent threshold to use {All.SecondWind.ActionName()} (0 = Disabled)");

                    DrawSliderInt(0, 100, MNK_ST_Bloodbath_Threshold,
                        $"HP percent threshold to use {All.Bloodbath.ActionName()} (0 = Disabled)");

                    break;

                case CustomComboPreset.MNK_AoE_ComboHeals:
                    DrawSliderInt(0, 100, MNK_AoE_SecondWind_Threshold,
                        $"HP percent threshold to use {All.SecondWind.ActionName()} (0 = Disabled)");

                    DrawSliderInt(0, 100, MNK_AoE_Bloodbath_Threshold,
                        $"HP percent threshold to use {All.Bloodbath.ActionName()} (0 = Disabled)");

                    break;

                case CustomComboPreset.MNK_STUseBrotherhood:
                    DrawSliderInt(0, 100, MNK_ST_Brotherhood_HP,
                        $"{Brotherhood.ActionName()} を使用する対象の最低HP% (0で無効)");

                    break;

                case CustomComboPreset.MNK_STUseROF:
                    DrawSliderInt(0, 100, MNK_ST_RiddleOfFire_HP,
                        $"{RiddleOfFire.ActionName()} を使用する対象の最低HP% (0で無効)");

                    break;

                case CustomComboPreset.MNK_STUseROW:
                    DrawSliderInt(0, 100, MNK_ST_RiddleOfWind_HP,
                        $"{RiddleOfWind.ActionName()} を使用する対象の最低HP% (0で無効)");

                    break;

                case CustomComboPreset.MNK_AoEUseBrotherhood:
                    DrawSliderInt(0, 100, MNK_AoE_Brotherhood_HP,
                        $"{Brotherhood.ActionName()} を使用する対象の最低HP% (0で無効)");

                    break;

                case CustomComboPreset.MNK_AoEUseROF:
                    DrawSliderInt(0, 100, MNK_AoE_RiddleOfFire_HP,
                        $"{RiddleOfFire.ActionName()} を使用する対象の最低HP% (0で無効)");

                    break;

                case CustomComboPreset.MNK_AoEUseROW:
                    DrawSliderInt(0, 100, MNK_AoE_RiddleOfWind_HP,
                        $"{RiddleOfWind.ActionName()} を使用する対象の最低HP% (0で無効)");

                    break;

                case CustomComboPreset.MNK_STUseOpener:
                    DrawHorizontalRadioButton(MNK_SelectedOpener, "Double 5s", "Uses Lunar/Lunar opener",
                        0);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "Solar 5s", "Uses Solar/Lunar opener",
                        1);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "Double 7s", "Uses Lunar/Lunar opener",
                        2);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "Solar 7s", "Uses Solar/Lunar opener",
                        3);

                    DrawBossOnlyChoice(MNK_Balance_Content);

                    break;

                case CustomComboPreset.MNK_STUseFiresReply:
                    DrawHorizontalRadioButton(MNK_ST_FiresReply_Order, "乾坤→必殺技", "早期に乾坤を使用します",
                        0);

                    DrawHorizontalRadioButton(MNK_ST_FiresReply_Order, "必殺技→乾坤", "遅らせて乾坤を使用します",
                        1);

                    DrawHorizontalRadioButton(MNK_ST_FiresReply_Order, "最大遅らせ乾坤", "乾坤を最大まで遅らせます",
                        2);
                        
                case MNK_STUseMasterfulBlitz:
                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "1→2→3", "鳳凰の舞の使用順番",
                        0);

                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "2→3→1", "鳳凰の舞の使用順番",
                        1);

                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "3→2→1", "鳳凰の舞の使用順番",
                        2);



                case CustomComboPreset.MNK_Variant_Cure:
                    DrawSliderInt(1, 100, MNK_VariantCure, "HP% to be at or under", 200);

                    break;
            }
        }
    }
}
