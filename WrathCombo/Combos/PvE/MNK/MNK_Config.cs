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
            MNK_AoE_RiddleOfFire_HP = new("MNK_AoE_RiddleOfFire_HP", 0),
            MNK_ST_SecondWind_Threshold = new("MNK_ST_SecondWindThreshold", 25),
            MNK_ST_Bloodbath_Threshold = new("MNK_ST_BloodbathThreshold", 40),
            MNK_AoE_SecondWind_Threshold = new("MNK_AoE_SecondWindThreshold", 25),
            MNK_AoE_Bloodbath_Threshold = new("MNK_AoE_BloodbathThreshold", 40),
            MNK_AoE_RiddleOfEarth_Threshold = new("MNK_AoE_RiddleOfEarth_Threshold", 90),
            MNK_VariantCure = new("MNK_Variant_Cure"),
            MNK_SelectedOpener = new("MNK_SelectedOpener", 4),
            MNK_Balance_Content = new("MNK_Balance_Content", 1),
            MNK_ST_FiresReply_Order = new("MNK_ST_FiresReply_Order", 0),
            MNK_ST_Phoenix_Order = new("MNK_ST_Phoenix_Order", 0),
            MNK_ST_Fast_Phoenix = new("MNK_ST_Fast_Phoenix", 0),
            MNK_ST_Many_PerfectBalance = new("MNK_ST_Many_PerfectBalance", 0);

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.MNK_ST_ComboHeals:
                    DrawSliderInt(0, 100, MNK_ST_SecondWind_Threshold,
                        $"PT平均から自身のHP%が乖離したら {All.SecondWind.ActionName()} を使用 (0で無効)");

                    DrawSliderInt(0, 100, MNK_ST_Bloodbath_Threshold,
                        $"PT平均から自身のHP%が乖離したら {All.Bloodbath.ActionName()} を使用 (0で無効)");

                    break;

                case CustomComboPreset.MNK_AoE_ComboHeals:
                    DrawSliderInt(0, 100, MNK_AoE_SecondWind_Threshold,
                        $"PT平均から自身のHP%が乖離したら {All.SecondWind.ActionName()} を使用 (0で無効)");

                    DrawSliderInt(0, 100, MNK_AoE_Bloodbath_Threshold,
                        $"PT平均から自身のHP%が乖離したら {All.Bloodbath.ActionName()} を使用 (0で無効)");

                    DrawSliderInt(0, 100, MNK_AoE_RiddleOfEarth_Threshold,
                        $"{RiddleOfEarth.ActionName()} を使用するPT平均の最低HP% (0で無効)");

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
                    DrawHorizontalRadioButton(MNK_SelectedOpener, "真真 自動", "5秒バースト 踊り子・ピクト存在時、7秒バースト",
                        4);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "真真 5秒", "真真回し / 5秒バースト",
                        0);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "鳳真 5秒", "鳳真回し / 5秒バースト",
                        1);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "真真 7秒", "真真回し / 7秒バースト",
                        2);

                    DrawHorizontalRadioButton(MNK_SelectedOpener, "鳳真 7秒", "鳳真回し / 7秒バースト",
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
                    break;

                case CustomComboPreset.MNK_STUsePerfectBalance:
                    DrawHorizontalRadioButton(MNK_ST_Many_PerfectBalance, "バーストチャクラ優先", "奇数バースト時、両チャクラが溜まっている状態に調整します。",
                        0);
                    DrawHorizontalRadioButton(MNK_ST_Many_PerfectBalance, "踏鳴使用回数優先", "チャクラの状態よりも踏鳴の使用回数を優先します。",
                        1);
                    ImGui.NewLine();
                    DrawHorizontalRadioButton(MNK_ST_Fast_Phoenix, "紅蓮後踏鳴", "奇数バースト時、紅蓮の極意の後に踏鳴を使用します",
                        0);

                    DrawHorizontalRadioButton(MNK_ST_Fast_Phoenix, "紅蓮前踏鳴", "奇数バースト時、紅蓮の極意の前に踏鳴を使用します",
                        1);

                    break;

                case CustomComboPreset.MNK_STUseMasterfulBlitz:
                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "1→2→3", "鳳凰の舞の使用順番",
                        0);

                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "2→3→1", "鳳凰の舞の使用順番",
                        1);

                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "3→2→1", "鳳凰の舞の使用順番",
                        2);

                    DrawHorizontalRadioButton(MNK_ST_Phoenix_Order, "直前の型依存", "3の型の時に踏鳴の場合、3→2→1 それ以外は、2→3→1",
                        3);
                    break;



                case CustomComboPreset.MNK_Variant_Cure:
                    DrawSliderInt(1, 100, MNK_VariantCure, "HP% to be at or under", 200);

                    break;
            }
        }
    }
}
