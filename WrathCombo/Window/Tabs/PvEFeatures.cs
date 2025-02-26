﻿using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using WrathCombo.Combos.PvE;
using WrathCombo.Core;
using WrathCombo.Services;
using WrathCombo.Window.Functions;
using WrathCombo.Window.MessagesNS;

namespace WrathCombo.Window.Tabs
{
    internal class PvEFeatures : ConfigWindow
    {
        internal static string OpenJob = string.Empty;

        internal static new void Draw()
        {
            //#if !DEBUG
            if (ActionReplacer.ClassLocked())
            {
                ImGui.TextWrapped("Equip your job stone to re-unlock features.");
                return;
            }
            //#endif

            using (var scrolling = ImRaii.Child("scrolling", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y), true))
            {
                int i = 1;
                var indentwidth = 12f.Scale();
                var indentwidth2 = indentwidth + 42f.Scale();
                if (OpenJob == string.Empty)
                {
                    ImGui.SameLine(indentwidth);
                    ImGuiEx.LineCentered(() =>
                    {
                        ImGuiEx.TextUnderlined("Select a job from below to enable and configure features for it.");
                    });

                    foreach (string? jobName in groupedPresets.Keys)
                    {
                        string abbreviation = groupedPresets[jobName].First().Info.JobShorthand;
                        string header = string.IsNullOrEmpty(abbreviation) ? jobName : $"{jobName} - {abbreviation}";
                        var id = groupedPresets[jobName].First().Info.JobID;
                        IDalamudTextureWrap? icon = Icons.GetJobIcon(id);
                        using (var disabled = ImRaii.Disabled(DisabledJobsPVE.Any(x => x == id)))
                        {
                            if (ImGui.Selectable($"###{header}", OpenJob == jobName, ImGuiSelectableFlags.None,
                                    icon == null ? new Vector2(0, 32f.Scale()) : new Vector2(0, (icon.Size.Y / 2f).Scale())))
                            {
                                OpenJob = jobName;
                            }
                            ImGui.SameLine(indentwidth);
                            if (icon != null)
                            {
                                ImGui.Image(icon.ImGuiHandle, new Vector2(icon.Size.X.Scale(), icon.Size.Y.Scale()) / 2f);
                                ImGui.SameLine(indentwidth2);
                            }

                            ImGui.Text($"{header} {(disabled ? "(Disabled due to update)" : "")}");

                            if (!string.IsNullOrEmpty(abbreviation) &&
                                P.UIHelper.JobControlled(id) is not null)
                            {
                                ImGui.SameLine();
                                P.UIHelper
                                    .ShowIPCControlledIndicatorIfNeeded(id, false);
                            }
                        }
                    }
                }
                else
                {
                    var id = groupedPresets[OpenJob].First().Info.JobID;
                    IDalamudTextureWrap? icon = Icons.GetJobIcon(id);

                    using (var headingTab = ImRaii.Child("HeadingTab", new Vector2(ImGui.GetContentRegionAvail().X, icon is null ? 24f.Scale() : (icon.Size.Y / 2f).Scale() + 4f)))
                    {
                        if (ImGui.Button("Back", new Vector2(0, 24f.Scale())))
                        {
                            OpenJob = "";
                            return;
                        }
                        ImGui.SameLine();
                        ImGuiEx.LineCentered(() =>
                        {
                            if (icon != null)
                            {
                                ImGui.Image(icon.ImGuiHandle, new Vector2(icon.Size.X.Scale(), icon.Size.Y.Scale()) / 2f);
                                ImGui.SameLine();
                            }
                            ImGuiEx.Text($"{OpenJob}");
                        });

                        if (P.UIHelper.JobControlled(id) is not null)
                        {
                            ImGui.SameLine();
                            P.UIHelper
                                .ShowIPCControlledIndicatorIfNeeded(id);
                        }

                    }

                    using (var contents = ImRaii.Child("Contents", new Vector2(0)))
                    {
                        currentPreset = 1;
                        try
                        {
                            if (ImGui.BeginTabBar($"subTab{OpenJob}", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs))
                            {
                                if (ImGui.BeginTabItem("Normal"))
                                {
                                    DrawHeadingContents(OpenJob);
                                    ImGui.EndTabItem();
                                }

                                if (groupedPresets[OpenJob].Any(x => PresetStorage.IsVariant(x.Preset)))
                                {
                                    if (ImGui.BeginTabItem("Variant Dungeons"))
                                    {
                                        DrawVariantContents(OpenJob);
                                        ImGui.EndTabItem();
                                    }
                                }

                                if (groupedPresets[OpenJob].Any(x => PresetStorage.IsBozja(x.Preset)))
                                {
                                    if (ImGui.BeginTabItem("Field Operations"))
                                    {
                                        DrawBozjaContents(OpenJob);
                                        ImGui.EndTabItem();
                                    }
                                }

                                if (groupedPresets[OpenJob].Any(x => PresetStorage.IsEureka(x.Preset)))
                                {
                                    if (ImGui.BeginTabItem("Eureka"))
                                    {
                                        ImGui.EndTabItem();
                                    }
                                }

                                ImGui.EndTabBar();
                            }
                        }
                        catch { }

                    }
                }

            }
        }

        private static void DrawVariantContents(string jobName)
        {
            foreach (var (preset, info) in groupedPresets[jobName].Where(x => PresetStorage.IsVariant(x.Preset)))
            {
                InfoBox presetBox = new() { Color = Colors.Grey, BorderThickness = 1f, CurveRadius = 8f, ContentsAction = () => { Presets.DrawPreset(preset, info); } };
                presetBox.Draw();
                ImGuiHelpers.ScaledDummy(12.0f);
            }
        }
        private static void DrawBozjaContents(string jobName)
        {
            foreach (var (preset, info) in groupedPresets[jobName].Where(x =>
                    PresetStorage.IsBozja(x.Preset)))
            {
                InfoBox presetBox = new() { Color = Colors.Grey, BorderThickness = 1f, CurveRadius = 8f, ContentsAction = () => { Presets.DrawPreset(preset, info); } };
                presetBox.Draw();
                ImGuiHelpers.ScaledDummy(12.0f);
            }
        }

        internal static void DrawHeadingContents(string jobName)
        {
            if (!Messages.PrintBLUMessage(jobName)) return;

            foreach (var (preset, info) in groupedPresets[jobName].Where(x => !PresetStorage.IsPvP(x.Preset) &&
                                                                                !PresetStorage.IsVariant(x.Preset) &&
                                                                                !PresetStorage.IsBozja(x.Preset) &&
                                                                                !PresetStorage.IsEureka(x.Preset)))
            {
                InfoBox presetBox = new() { Color = Colors.Grey, BorderThickness = 2f.Scale(), ContentsOffset = 5f.Scale(), ContentsAction = () => { Presets.DrawPreset(preset, info); } };

                if (Service.Configuration.HideConflictedCombos)
                {
                    var conflictOriginals = PresetStorage.GetConflicts(preset); // Presets that are contained within a ConflictedAttribute
                    var conflictsSource = PresetStorage.GetAllConflicts();      // Presets with the ConflictedAttribute

                    if (!conflictsSource.Where(x => x == preset).Any() || conflictOriginals.Length == 0)
                    {
                        presetBox.Draw();
                        ImGuiHelpers.ScaledDummy(12.0f);
                        continue;
                    }

                    if (conflictOriginals.Any(PresetStorage.IsEnabled))
                    {
                        if (Service.Configuration.EnabledActions.Remove(preset))
                            Service.Configuration.Save();

                        // Keep removed items in the counter
                        var parent = PresetStorage.GetParent(preset) ?? preset;
                        currentPreset += 1 + Presets.AllChildren(presetChildren[parent]);
                    }

                    else
                    {
                        presetBox.Draw();
                        continue;
                    }
                }

                else
                {
                    presetBox.Draw();
                    ImGuiHelpers.ScaledDummy(12.0f);
                }
            }
        }

        internal static void OpenToCurrentJob(bool onJobChange)
        {
            if ((!onJobChange || !Service.Configuration.OpenToCurrentJobOnSwitch) &&
                (onJobChange || !Service.Configuration.OpenToCurrentJob ||
                 !Player.Available)) return;

            if (Player.Job.IsDoh())
                return;

            if (Player.Job.IsDol())
            {
                OpenJob = groupedPresets
                    .FirstOrDefault(x => x.Value.Any(y => y.Info.JobID == DOL.JobID)).Key;
                return;
            }

            OpenJob = groupedPresets
                .FirstOrDefault(x =>
                    x.Value.Any(y => y.Info.JobShorthand == Player.Job.ToString()))
                .Key;

        }
    }
}
