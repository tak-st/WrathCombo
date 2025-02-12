﻿using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ECommons;
using ECommons.Automation.LegacyTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Newtonsoft.Json.Linq;
using PunishLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WrathCombo.Attributes;
using WrathCombo.AutoRotation;
using WrathCombo.Combos;
using WrathCombo.Combos.PvE;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Services;
using WrathCombo.Services.IPC;
using WrathCombo.Window;
using WrathCombo.Window.Tabs;

namespace WrathCombo;

/// <summary> Main plugin implementation. </summary>
public sealed partial class WrathCombo : IDalamudPlugin
{
    private static TaskManager? TM;
    private readonly ConfigWindow ConfigWindow;
    private readonly SettingChangeWindow SettingChangeWindow;
    private readonly TargetHelper TargetHelper;
    internal static WrathCombo? P;
    private readonly WindowSystem ws;
    private readonly HttpClient httpClient = new();
    private readonly IDtrBarEntry DtrBarEntry;
    internal Provider IPC;
    internal Search IPCSearch = null!;
    internal UIHelper UIHelper = null!;

    private readonly TextPayload starterMotd = new("[Wrath Message of the Day] ");
    private static uint? jobID;

    public static readonly List<uint> DisabledJobsPVE =
    [
        //ADV.JobID,
        //AST.JobID,
        //BLM.JobID,
        //BLU.JobID,
        //BRD.JobID,
        //DNC.JobID,
        //DOL.JobID,
        //DRG.JobID,
        //DRK.JobID,
        //GNB.JobID,
        //MCH.JobID,
        //MNK.JobID,
        //NIN.JobID,
        //PCT.JobID,
        //PLD.JobID,
        //RDM.JobID,
        //RPR.JobID,
        //SAM.JobID,
        //SCH.JobID,
        //SGE.JobID,
        //SMN.JobID,
        //VPR.JobID,
        //WAR.JobID,
        //WHM.JobID
    ];

    public static readonly List<uint> DisabledJobsPVP = [];

    public static uint? JobID
    {
        get => jobID;
        set
        {
            if (jobID != value && value != null)
            {
                UpdateCaches(true, false);
            }
            jobID = value;
        }
    }

    private static void UpdateCaches(bool onJobChange, bool onTerritoryChange)
    {
        TM.DelayNext(1000);
        TM.Enqueue(() =>
        {
            if (!Player.Available)
                return false;

            AST.QuickTargetCards.SelectedRandomMember = null;
            if (onJobChange)
            {
                PvEFeatures.OpenToCurrentJob(true);
                Service.IconReplacer.UpdateFilteredCombos();
                WrathOpener.SelectOpener();
                P.IPCSearch.UpdateActiveJobPresets();
            }

            if (onTerritoryChange)
            {
                if (Service.Configuration.RotationConfig.EnableInInstance && Content.InstanceContentRow?.RowId > 0)
                    Service.Configuration.RotationConfig.Enabled = true;

                if (Service.Configuration.RotationConfig.DisableAfterInstance && Content.InstanceContentRow?.RowId == 0)
                    Service.Configuration.RotationConfig.Enabled = false;
            }

            return true;
        }, "UpdateCaches");
    }

    /// <summary> Initializes a new instance of the <see cref="WrathCombo"/> class. </summary>
    /// <param name="pluginInterface"> Dalamud plugin interface. </param>
    public WrathCombo(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        pluginInterface.Create<Service>();
        ECommonsMain.Init(pluginInterface, this);
        PunishLibMain.Init(pluginInterface, "Wrath Combo");

        TM = new();
        RemoveNullAutos(); 
        Service.Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        Service.Address = new PluginAddressResolver();
        Service.Address.Setup(Svc.SigScanner);
        PresetStorage.Init();

        Service.ComboCache = new CustomComboCache();
        Service.IconReplacer = new IconReplacer();
        ActionWatching.Enable();
        AST.InitCheckCards();
        IPC = Provider.InitAsync().Result;

        ConfigWindow = new ConfigWindow();
        SettingChangeWindow = new SettingChangeWindow();
        TargetHelper = new();
        ws = new();
        ws.AddWindow(ConfigWindow);
        ws.AddWindow(SettingChangeWindow);
        ws.AddWindow(TargetHelper);

        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;

        RegisterCommands();

        DtrBarEntry ??= Svc.DtrBar.Get("Wrath Combo");
        DtrBarEntry.OnClick = () =>
        {
            ToggleAutoRotation(!Service.Configuration.RotationConfig.Enabled);
        };
        DtrBarEntry.Tooltip = new SeString(
        new TextPayload("Click to toggle Wrath Combo's Auto-Rotation.\n"),
        new TextPayload("Disable this icon in /xlsettings -> Server Info Bar"));

        Svc.ClientState.Login += PrintLoginMessage;
        if (Svc.ClientState.IsLoggedIn) ResetFeatures();

        Svc.Framework.Update += OnFrameworkUpdate;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;

        KillRedundantIDs();
        HandleConflictedCombos();
        CustomComboFunctions.TimerSetup();

#if DEBUG
        ConfigWindow.IsOpen = true;
#endif
    }

    private void RemoveNullAutos()
    {
        try
        {
            var save = false;
            if (!Svc.PluginInterface.ConfigFile.Exists) return;

            var json = JObject.Parse(File.ReadAllText(Svc.PluginInterface.ConfigFile.FullName));
            if (json["AutoActions"] is JObject autoActions)
            {
                var clone = autoActions.JSONClone();
                foreach (var a in clone)
                {
                    if (a.Key == "$type")
                        continue;

                    if (Enum.TryParse(typeof(CustomComboPreset), a.Key, out _))
                        continue;

                    Svc.Log.Debug($"Couldn't find {a.Key}");
                    autoActions[a.Key].Parent.Remove();
                    save = true;
                }
            }
            if (save)
                File.WriteAllText(Svc.PluginInterface.ConfigFile.FullName, json.ToString());
        }
        catch (Exception e)
        {
            e.Log();
        }
    }

    private void ClientState_TerritoryChanged(ushort obj)
    {
        UpdateCaches(false, true);
    }

    public const string OptionControlledByIPC =
        "(being overwritten by another plugin, check the setting in /wrath)";

    private static void HandleConflictedCombos()
    {
        var enabledCopy = Service.Configuration.EnabledActions.ToHashSet(); //Prevents issues later removing during enumeration
        foreach (var preset in enabledCopy)
        {
            if (!PresetStorage.IsEnabled(preset)) continue;

            var conflictingCombos = preset.GetAttribute<ConflictingCombosAttribute>();
            if (conflictingCombos != null)
            {
                foreach (var conflict in conflictingCombos.ConflictingPresets)
                {
                    if (PresetStorage.IsEnabled(conflict))
                    {
                        Service.Configuration.EnabledActions.Remove(conflict);
                        Service.Configuration.Save();
                    }
                }
            }
        }
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (Svc.ClientState.LocalPlayer is not null)
        {
            JobID = Svc.ClientState.LocalPlayer?.ClassJob.RowId;
            CustomComboFunctions.IsMoving(); //Hacky workaround to ensure it's always running
        }

        BlueMageService.PopulateBLUSpells();
        TargetHelper.Draw();
        AutoRotationController.Run();
        PluginConfiguration.ProcessSaveQueue();

        // Skip the IPC checking if hidden
        if (DtrBarEntry.UserHidden) return;

        var autoOn = IPC.GetAutoRotationState();
        var icon = new IconPayload(autoOn
            ? BitmapFontIcon.SwordUnsheathed
            : BitmapFontIcon.SwordSheathed);

        var text = autoOn ? ": On" : ": Off";
        if (!Service.Configuration.ShortDTRText && autoOn)
            text += $" ({P.IPCSearch.ActiveJobPresets} active)";
        var ipcControlledText =
            P.UIHelper.AutoRotationStateControlled() is not null
                ? " (Locked)"
                : "";

        var payloadText = new TextPayload(text + ipcControlledText);
        DtrBarEntry.Text = new SeString(icon, payloadText);
    }

    private static void KillRedundantIDs()
    {
        var redundantIDs = Service.Configuration.EnabledActions.Where(x => int.TryParse(x.ToString(), out _)).OrderBy(x => x).Cast<int>().ToList();
        foreach (var id in redundantIDs)
            Service.Configuration.EnabledActions.RemoveWhere(x => (int)x == id);

        Service.Configuration.Save();
    }

    private static void ResetFeatures()
    {
        // Enumerable.Range is a start and count, not a start and end.
        // Enumerable.Range(Start, Count)
        Service.Configuration.ResetFeatures("v3.0.17.0_NINRework", Enumerable.Range(10000, 100).ToArray());
        Service.Configuration.ResetFeatures("v3.0.17.0_DRGCleanup", Enumerable.Range(6100, 400).ToArray());
        Service.Configuration.ResetFeatures("v3.0.18.0_GNBCleanup", Enumerable.Range(7000, 700).ToArray());
        Service.Configuration.ResetFeatures("v3.0.18.0_PvPCleanup", Enumerable.Range(80000, 11000).ToArray());
        Service.Configuration.ResetFeatures("v3.0.18.1_PLDRework", Enumerable.Range(11000, 100).ToArray());
        Service.Configuration.ResetFeatures("v3.1.0.1_BLMRework", Enumerable.Range(2000, 100).ToArray());
        Service.Configuration.ResetFeatures("v3.1.1.0_DRGRework", Enumerable.Range(6000, 800).ToArray());
        Service.Configuration.ResetFeatures("1.0.0.6_DNCRework", Enumerable.Range(4000, 150).ToArray());
        Service.Configuration.ResetFeatures("1.0.0.11_DRKRework", Enumerable.Range(5000, 200).ToArray());
    }

    private void DrawUI()
    {
        SettingChangeWindow.Draw();
        ConfigWindow.Draw();
    }

    private void PrintLoginMessage()
    {
        Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ => ResetFeatures());

        if (!Service.Configuration.HideMessageOfTheDay)
            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ => PrintMotD());
    }

    private void PrintMotD()
    {
        try
        {
            var basicMessage = $"Welcome to WrathCombo v{this.GetType().Assembly
                .GetName().Version}!";
            using var motd =
                httpClient.GetAsync("https://raw.githubusercontent.com/PunishXIV/WrathCombo/main/res/motd.txt").Result;
            motd.EnsureSuccessStatusCode();
            var data = motd.Content.ReadAsStringAsync().Result;
            List<Payload> payloads =
            [
                starterMotd,
                EmphasisItalicPayload.ItalicsOn,
                string.IsNullOrEmpty(data) ? new TextPayload(basicMessage) : new TextPayload(data.Trim()),
                EmphasisItalicPayload.ItalicsOff
            ];

            Svc.Chat.Print(new XivChatEntry
            {
                Message = new SeString(payloads),
                Type = XivChatType.Echo
            });
        }

        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Unable to retrieve MotD");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used for non-static only window initialization")]
    public string Name => "Wrath Combo";

    /// <inheritdoc/>
    public void Dispose()
    {
        ConfigWindow.Dispose();

        // Try to force a config save if there are some pending
        if (PluginConfiguration.SaveQueue.Count > 0)
            lock (PluginConfiguration.SaveQueue)
            {
                PluginConfiguration.SaveQueue.Clear();
                Service.Configuration.Save();
                PluginConfiguration.ProcessSaveQueue();
            }

        Debug.Dispose();

        ws.RemoveAllWindows();
        Svc.DtrBar.Remove("Wrath Combo");
        Svc.Framework.Update -= OnFrameworkUpdate;
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        Svc.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
        Svc.PluginInterface.UiBuilder.Draw -= DrawUI;

        Service.IconReplacer.Dispose();
        Service.ComboCache.Dispose();
        ActionWatching.Dispose();
        AST.DisposeCheckCards();
        CustomComboFunctions.TimerDispose();
        IPC.Dispose();

        Svc.ClientState.Login -= PrintLoginMessage;
        ECommonsMain.Dispose();
        P = null;
    }

    private void OnOpenMainUi() =>
        HandleOpenCommand(forceOpen: true);

    private void OnOpenConfigUi() =>
        HandleOpenCommand(tab: OpenWindow.Settings, forceOpen: true);
}
