using ImGuiNET;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Window.Functions;
using static WrathCombo.Window.Functions.UserConfig;

namespace WrathCombo.Combos.PvE;

internal partial class BRD
{
    internal static class Config
    {
        public static UserInt
            BRD_RagingJawsRenewTime = new("ragingJawsRenewTime"),
            BRD_NoWasteHPPercentage = new("noWasteHpPercentage"),
            BRD_AoENoWasteHPPercentage = new("AoENoWasteHpPercentage"),
            BRD_STSecondWindThreshold = new("BRD_STSecondWindThreshold"),
            BRD_AoESecondWindThreshold = new("BRD_AoESecondWindThreshold"),
            BRD_VariantCure = new("BRD_VariantCure"),
            BRDPvP_HarmonicArrowCharges = new("BRDPvP_HarmonicArrowCharges"),
            BRD_Adv_Opener_Selection = new("BRD_Adv_Opener_Selection", 0),
            BRD_Balance_Content = new("BRD_Balance_Content", 1);

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.BRD_ST_Adv_Balance_Standard:
                    DrawRadioButton(BRD_Adv_Opener_Selection, $"Standard Opener", "", 0);
                    DrawRadioButton(BRD_Adv_Opener_Selection, $"2.48 Adjusted Standard Opener", "", 1);
                    DrawRadioButton(BRD_Adv_Opener_Selection, $"2.49 Standard Comfy", "", 2);

                    ImGui.Indent();
                    DrawBossOnlyChoice(BRD_Balance_Content);
                    ImGui.Unindent();
                    break;                

                case CustomComboPreset.BRD_Adv_RagingJaws:
                    DrawSliderInt(3, 10, BRD_RagingJawsRenewTime,
                        "Remaining time (In seconds). Recommended 5, increase little by little if refresh is outside of radiant window");

                    break;

                case CustomComboPreset.BRD_Adv_NoWaste:
                    DrawSliderInt(1, 10, BRD_NoWasteHPPercentage, "Remaining target HP percentage");

                    break;

                case CustomComboPreset.BRD_AoE_Adv_NoWaste:
                    DrawSliderInt(1, 50, BRD_AoENoWasteHPPercentage,
                        "Remaining target HP percentage");

                    break;

                case CustomComboPreset.BRD_ST_SecondWind:
                    DrawSliderInt(0, 100, BRD_STSecondWindThreshold,
                        "HP percent threshold to use Second Wind below.");

                    break;

                case CustomComboPreset.BRD_AoE_SecondWind:
                    DrawSliderInt(0, 100, BRD_AoESecondWindThreshold,
                        "HP percent threshold to use Second Wind below.");

                    break;

                case CustomComboPreset.BRD_Variant_Cure:
                    DrawSliderInt(1, 100, BRD_VariantCure, "HP% to be at or under", 200);

                    break;

                case CustomComboPreset.BRDPvP_HarmonicArrow:
                    DrawSliderInt(1, 4, BRDPvP_HarmonicArrowCharges, "How many Charges to use it at \n 1 charge 8000 damage \n 2 charge 12000 damage \n 3 charge 15000 damage \n 4 charge 17000 damage");

                    break;
            }
        }
    }
}
