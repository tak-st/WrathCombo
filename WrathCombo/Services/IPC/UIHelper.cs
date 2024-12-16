﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons.ImGuiMethods;
using ImGuiNET;
using WrathCombo.Combos;
using WrathCombo.CustomComboNS.Functions;

#endregion

namespace WrathCombo.Services.IPC;

public class UIHelper(ref Leasing leasing, ref Search search)
{
    private readonly Leasing _leasing = leasing;
    private readonly Search _search = search;

    #region Checks for the UI

    #region Auto-Rotation

    private DateTime? _autoRotationUpdated;

    private string AutoRotationControlled { get; set; } = string.Empty;

    private string? AutoRotationStateControlled()
    {
        // Return the cached value if it is valid, fastest
        if (string.IsNullOrEmpty(AutoRotationControlled) &&
            _autoRotationUpdated is not null &&
            _autoRotationUpdated == _leasing.AutoRotationStateUpdated)
            return AutoRotationControlled;

        // Bail if the state is not controlled, fast
        var controlled = _leasing.CheckAutoRotationControlled();
        if (controlled is null)
            return null;

        // Re-populate the cache with the current state, slowest
        var controllingLeases = _leasing.Registrations.Values
            .Where(l => l.AutoRotationControlled.Count != 0)
            .OrderByDescending(l => l.LastUpdated)
            .Select(l => l.PluginName);
        AutoRotationControlled = string.Join(", ", controllingLeases);
        _autoRotationUpdated = _leasing.AutoRotationStateUpdated;

        return AutoRotationControlled;
    }

    #endregion

    #region Jobs

    private DateTime? _jobsUpdated;

    private Dictionary<string, string> JobsControlled { get; } = new();

    private string? JobControlled(uint job)
    {
        var jobName = CustomComboFunctions.JobIDs.JobIDToShorthand(job);

        // Return the cached value if it is valid, fastest
        if (_jobsUpdated is not null &&
            _jobsUpdated == _search.LastCacheUpdateForAllJobsControlled &&
            JobsControlled.TryGetValue(jobName, out var jobControlled))
            return jobControlled;

        // Bail if the job is not controlled, fast
        var controlled = _leasing.CheckJobControlled();
        if (controlled is null)
            return null;

        // Re-populate the cache with the current set of controlled jobs, slowest
        JobsControlled.Clear();
        foreach (var controlledJob in _search.AllJobsControlled)
            JobsControlled[controlledJob.Key.ToString()] =
                string.Join(", ", controlledJob.Value.Keys);
        _jobsUpdated = _search.LastCacheUpdateForAllJobsControlled;

        return JobsControlled[jobName];
    }

    #endregion

    #region Presets

    private DateTime? _presetsUpdated;

    private Dictionary<string, string> PresetsControlled { get; } = new();

    private string? PresetControlled(CustomComboPreset preset)
    {
        var presetName = preset.ToString();

        // Return the cached value if it is valid, fastest
        if (_presetsUpdated is not null &&
            _presetsUpdated == _search.LastCacheUpdateForAllPresetsControlled &&
            PresetsControlled.TryGetValue(presetName, out var presetControlled))
            return presetControlled;

        // Bail if the preset is not controlled, fast-ish
        var controlledAsCombo = _leasing.CheckComboControlled(presetName);
        var controlledAsOption = _leasing.CheckComboOptionControlled(presetName);
        if (controlledAsCombo is null && controlledAsOption is null)
            return null;

        // Re-populate the cache with the current set of controlled presets, slowest
        PresetsControlled.Clear();
        foreach (var controlledPreset in _search.AllPresetsControlled)
            PresetsControlled[controlledPreset.Key.ToString()] =
                string.Join(", ", controlledPreset.Value.Keys);
        _presetsUpdated = _search.LastCacheUpdateForAllPresetsControlled;

        return PresetsControlled[presetName];
    }

    #endregion

    #region Auto-Rotation Configs

    private DateTime? _autoRotationConfigsUpdated;

    private Dictionary<string, string> AutoRotationConfigsControlled { get; } =
        new();

    private string? AutoRotationConfigControlled(string configName)
    {
        var configOption = Enum.Parse<AutoRotationConfigOption>(configName);

        // Return the cached value if it is valid, fastest
        if (_autoRotationConfigsUpdated is not null &&
            _autoRotationConfigsUpdated ==
            _search.LastCacheUpdateForAutoRotationConfigs &&
            AutoRotationConfigsControlled.TryGetValue(configName,
                out var configControlled))
            return configControlled;

        // Bail if the config is not controlled, fast-ish
        var controlled = _leasing.CheckAutoRotationConfigControlled(configOption);
        if (controlled is null)
            return null;

        // Re-populate the cache with the current set of controlled configs, slowest
        AutoRotationConfigsControlled.Clear();
        foreach (var controlledConfig in _search.AllAutoRotationConfigsControlled)
            AutoRotationConfigsControlled[controlledConfig.Key.ToString()] =
                string.Join(", ", controlledConfig.Value.Keys);
        _autoRotationConfigsUpdated = _search.LastCacheUpdateForAutoRotationConfigs;

        return AutoRotationConfigsControlled[configName];
    }

    #endregion

    #endregion

    #region Helper methods for the UI

    private readonly Vector4 _backgroundColor = new(0.68f, 0.77f, 0.80f, 1);
    private readonly Vector4 _hoverColor = new(0.84f, 0.92f, 0.96f, 1);
    private readonly Vector4 _textColor = new(0.05f, 0.05f, 0.05f, 1);
    private readonly Vector2 _padding = new(14f.Scale(), 2f);
    private readonly Vector2 _spacing = new(0, 0);
    private readonly float _rounding = 8f;

    // Method to display the controlled indicator, which lists the plugins
    private void ShowIPCControlledIndicator
    (bool? forAutoRotation = null,
        uint? forJob = null,
        CustomComboPreset? forPreset = null,
        string? forAutoRotationConfig = null)
    {
        string? controlled = null;

        #region Bail if not needed

        if (forAutoRotation is not null)
            if ((controlled = AutoRotationStateControlled()) is null)
                return;
        if (forJob is not null)
            if ((controlled = JobControlled((uint)forJob)) is null)
                return;
        if (forPreset is not null)
            if ((controlled = PresetControlled((CustomComboPreset)forPreset)) is null)
                return;
        if (forAutoRotationConfig is not null)
            if ((controlled = AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return;

        if (controlled is null)
            return;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.Button, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, _padding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, _rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, _spacing);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _backgroundColor);
        ImGui.SmallButton($"Controlled by: {controlled}");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - _rounding.Scale() - 3f.Scale());
        if (ImGui.SmallButton("X"))
            RevokeControl(controlled);
        ImGui.PopStyleColor(3);

        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(2);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("This option is controlled by another plugin.\n" +
                "Click the X to revoke control.");
    }

    // Method to display a differently-styled and disabled checkbox if controlled
    private void ShowIPCControlledCheckbox
    (bool? forAutoRotation = null,
        uint? forJob = null,
        CustomComboPreset? forPreset = null,
        string? forAutoRotationConfig = null)
    {
        string? controlled = null;

        #region Bail if not needed

        if (forAutoRotation is not null)
            if ((controlled = AutoRotationStateControlled()) is null)
                return;
        if (forJob is not null)
            if ((controlled = JobControlled((uint)forJob)) is null)
                return;
        if (forPreset is not null)
            if ((controlled = PresetControlled((CustomComboPreset)forPreset)) is null)
                return;
        if (forAutoRotationConfig is not null)
            if ((controlled = AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return;

        if (controlled is null)
            return;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.Button, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, _padding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, _rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, _spacing);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _backgroundColor);
        ImGui.SmallButton($"Controlled by: {controlled}");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - _rounding.Scale() - 3f.Scale());
        if (ImGui.SmallButton("X"))
            RevokeControl(controlled);
        ImGui.PopStyleColor(3);

        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(2);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("This option is controlled by another plugin.\n" +
                "Click the X to revoke control.");
    }

    private void ShowIPCControlledSlider (string? forAutoRotationConfig = null)
    {
        string? controlled = null;

        #region Bail if not needed

        if (forAutoRotationConfig is not null)
            if ((controlled = AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return;

        if (controlled is null)
            return;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.Button, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, _padding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, _rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, _spacing);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _backgroundColor);
        ImGui.SmallButton($"Controlled by: {controlled}");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - _rounding.Scale() - 3f.Scale());
        if (ImGui.SmallButton("X"))
            RevokeControl(controlled);
        ImGui.PopStyleColor(3);

        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(2);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("This option is controlled by another plugin.\n" +
                "Click the X to revoke control.");
    }

    private void ShowIPCControlledCombo (string? forAutoRotationConfig = null)
    {
        string? controlled = null;

        #region Bail if not needed

        if (forAutoRotationConfig is not null)
            if ((controlled = AutoRotationConfigControlled(forAutoRotationConfig)) is null)
                return;

        if (controlled is null)
            return;

        #endregion

        ImGui.BeginGroup();
        ImGui.PushStyleColor(ImGuiCol.Button, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, _padding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, _rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, _spacing);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _backgroundColor);
        ImGui.SmallButton($"Controlled by: {controlled}");
        ImGui.PopStyleColor(2);

        ImGui.SameLine();

        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, _hoverColor);
        ImGui.PushStyleColor(ImGuiCol.Text, _textColor);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - _rounding.Scale() - 3f.Scale());
        if (ImGui.SmallButton("X"))
            RevokeControl(controlled);
        ImGui.PopStyleColor(3);

        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(2);
        ImGui.EndGroup();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("This option is controlled by another plugin.\n" +
                             "Click the X to revoke control.");
    }

    /// <summary>
    ///     Button click method for Indicator to cancel plugin control.
    /// </summary>
    /// <param name="controllers">The displayed list of plugins to revoke.</param>
    private void RevokeControl(string controllers)
    {
        var controllerNames = controllers.Split(", ");
        var leases = _leasing.Registrations.Values
            .Where(l => controllerNames.Contains(l.PluginName))
            .Select(l => l.ID)
            .ToList();
        foreach (var lease in leases)
            _leasing.RemoveRegistration(
                lease, CancellationReason.WrathUserManuallyCancelled);
    }

    #region Actual UI Method overloads

    #region Indicator

    public void ShowIPCControlledIndicatorIfNeeded() =>
        ShowIPCControlledIndicator(forAutoRotation: true);
    public void ShowIPCControlledIndicatorIfNeeded(uint job) =>
        ShowIPCControlledIndicator(forJob: job);
    public void ShowIPCControlledIndicatorIfNeeded(CustomComboPreset preset) =>
        ShowIPCControlledIndicator(forPreset: preset);
    public void ShowIPCControlledIndicatorIfNeeded(string configName) =>
        ShowIPCControlledIndicator(forAutoRotationConfig: configName);

    #endregion

    #region Disabled Inputs

    // todo: these require me to also cache the controlled state, not just the controlling leases

    public void ShowIPCControlledCheckboxIfNeeded() =>
        ShowIPCControlledCheckbox(forAutoRotation: true);
    public void ShowIPCControlledCheckboxIfNeeded(uint job) =>
        ShowIPCControlledCheckbox(forJob: job);
    public void ShowIPCControlledCheckboxIfNeeded(CustomComboPreset preset) =>
        ShowIPCControlledCheckbox(forPreset: preset);
    public void ShowIPCControlledCheckboxIfNeeded(string configName) =>
        ShowIPCControlledCheckbox(forAutoRotationConfig: configName);
    public void ShowIPCControlledSliderIfNeeded(string configName) =>
        ShowIPCControlledSlider(forAutoRotationConfig: configName);
    public void ShowIPCControlledComboIfNeeded(string configName) =>
        ShowIPCControlledCombo(forAutoRotationConfig: configName);

    #endregion

    #endregion

    #endregion
}
