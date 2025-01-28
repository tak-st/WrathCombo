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
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using PunishLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WrathCombo.Attributes;
using WrathCombo.AutoRotation;
using WrathCombo.Combos;
using WrathCombo.Combos.PvE;
using WrathCombo.Combos.PvP;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Extensions;
using WrathCombo.Services;
using WrathCombo.Services.IPC;
using WrathCombo.Window;
using WrathCombo.Window.Tabs;
using IPC = WrathCombo.Services.IPC;
using Status = Dalamud.Game.ClientState.Statuses.Status;

namespace WrathCombo
{
    /// <summary> Main plugin implementation. </summary>
    public sealed partial class WrathCombo : IDalamudPlugin
    {
        private const string Command = "/wrath";

        private static TaskManager? TM;
        private readonly ConfigWindow ConfigWindow;
        private readonly SettingChangeWindow SettingChangeWindow;
        private readonly TargetHelper TargetHelper;
        internal static WrathCombo? P = null!;
        internal WindowSystem ws;
        private readonly HttpClient httpClient = new();
        private IDtrBarEntry DtrBarEntry;
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
                    UpdateCaches();
                }
                jobID = value;
            }
        }

        private static void UpdateCaches()
        {
            TM.DelayNext(1000);
            TM.Enqueue(() =>
            {
                if (!Player.Available)
                    return false;

                Service.IconReplacer.UpdateFilteredCombos();
                AST.QuickTargetCards.SelectedRandomMember = null;
                PvEFeatures.HasToOpenJob = true;
                WrathOpener.SelectOpener();
                P.IPCSearch.UpdateActiveJobPresets();
                if (Service.Configuration.RotationConfig.EnableInInstance && Content.InstanceContentRow?.RowId > 0)
                    Service.Configuration.RotationConfig.Enabled = true;

                if (Service.Configuration.RotationConfig.DisableAfterInstance && Content.InstanceContentRow?.RowId == 0)
                    Service.Configuration.RotationConfig.Enabled = false;

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
            Svc.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;

            EzCmd.Add(Command, OnCommand, "Open a window to edit custom combo settings.\n" +
                "/wrath auto → Toggle Auto-rotation on/off.\n" +
                "/wrath debug → Dumps a debug log onto your desktop for developers.\n" +
                "/scombo - Old alias from XIVSlothCombo, still works!");
            EzCmd.Add("/scombo", OnCommand);

            DtrBarEntry ??= Svc.DtrBar.Get("Wrath Combo");
            DtrBarEntry.OnClick = () =>
            {
                ToggleAutorot(!Service.Configuration.RotationConfig.Enabled);
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

        private void ClientState_TerritoryChanged(ushort obj)
        {
            UpdateCaches();
        }

        public const string OptionControlledByIPC =
            "(being overwritten by another plugin, check the setting in /wrath)";

        private void ToggleAutorot(bool value)
        {
            Service.Configuration.RotationConfig.Enabled = value;
            Service.Configuration.Save();

            var stateControlled =
                P.UIHelper.AutoRotationStateControlled() is not null;

            DuoLog.Information(
                "Auto-Rotation set to "
                + (Service.Configuration.RotationConfig.Enabled ? "ON" : "OFF")
                + (stateControlled ? " " + OptionControlledByIPC : "")
            );
        }

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
            List<int> redundantIDs = Service.Configuration.EnabledActions.Where(x => int.TryParse(x.ToString(), out _)).OrderBy(x => x).Cast<int>().ToList();
            foreach (int id in redundantIDs)
            {
                Service.Configuration.EnabledActions.RemoveWhere(x => (int)x == id);
            }

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
        }

        private void DrawUI()
        {
            SettingChangeWindow.Draw();
            ConfigWindow.Draw();
        }

        private void PrintLoginMessage()
        {
            Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => ResetFeatures());

            if (!Service.Configuration.HideMessageOfTheDay)
                Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(task => PrintMotD());
        }

        private void PrintMotD()
        {
            try
            {
                string basicMessage = $"Welcome to WrathCombo v{this.GetType().Assembly.GetName().Version}!";
                using HttpResponseMessage? motd = httpClient.GetAsync("https://raw.githubusercontent.com/PunishXIV/WrathCombo/main/res/motd.txt").Result;
                motd.EnsureSuccessStatusCode();
                string? data = motd.Content.ReadAsStringAsync().Result;
                List<Payload>? payloads =
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

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used for non-static only window initialization")]
        public string Name => "Wrath Combo";

        /// <inheritdoc/>
        public void Dispose()
        {
            ConfigWindow?.Dispose();

            // Try to force a config save if there are some pending
            if (PluginConfiguration.SaveQueue.Count > 0)
                lock (PluginConfiguration.SaveQueue)
                {
                    PluginConfiguration.SaveQueue.Clear();
                    Service.Configuration.Save();
                    PluginConfiguration.ProcessSaveQueue();
                }

            ws.RemoveAllWindows();
            Svc.DtrBar.Remove("Wrath Combo");
            Svc.Framework.Update -= OnFrameworkUpdate;
            Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
            Svc.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;
            Svc.PluginInterface.UiBuilder.Draw -= DrawUI;

            Service.IconReplacer?.Dispose();
            Service.ComboCache?.Dispose();
            ActionWatching.Dispose();
            AST.DisposeCheckCards();
            CustomComboFunctions.TimerDispose();
            IPC.Dispose();

            Svc.ClientState.Login -= PrintLoginMessage;
            ECommonsMain.Dispose();
            P = null;
        }


        private void OnOpenConfigUi() => ConfigWindow.IsOpen = !ConfigWindow.IsOpen;

        private void OnCommand(string command, string arguments)
        {
            string[]? argumentsParts = arguments.Split();

            switch (argumentsParts[0].ToLower())
            {
                case "unsetall": // unset all features
                    {
                        Service.Configuration.EnabledActions.Clear();
                        DuoLog.Information("All UNSET");
                        Service.Configuration.Save();
                        break;
                    }

                case "set": // set a feature
                    {
                        string? targetPreset = argumentsParts[1].ToLowerInvariant();
                        if (int.TryParse(targetPreset, out int number))
                        {
                            PresetStorage.EnablePreset(number, true);
                        }
                        else
                        {
                            PresetStorage.EnablePreset(targetPreset, true);
                        }
                        Service.Configuration.Save();
                        break;
                    }

                case "toggle": // toggle a feature
                    {
                        string? targetPreset = argumentsParts[1].ToLowerInvariant();
                        if (int.TryParse(targetPreset, out int number))
                        {
                            PresetStorage.TogglePreset(number, true);
                        }
                        else
                        {
                            PresetStorage.TogglePreset(targetPreset, true);
                        }
                        Service.Configuration.Save();
                        break;
                    }

                case "unset": // unset a feature
                    {
                        string? targetPreset = argumentsParts[1].ToLowerInvariant();
                        if (int.TryParse(targetPreset, out int number))
                        {
                            PresetStorage.DisablePreset(number, true);
                        }
                        else
                        {
                            PresetStorage.DisablePreset(targetPreset, true);
                        }
                        Service.Configuration.Save();
                        break;
                    }

                case "list": // list features
                    {
                        string? filter = argumentsParts.Length > 1
                            ? argumentsParts[1].ToLowerInvariant()
                            : "all";

                        if (filter == "set") // list set features
                        {
                            foreach (CustomComboPreset preset in Enum.GetValues<CustomComboPreset>().Where(preset => IPC.GetComboState(preset.ToString())!.First().Value))
                            {
                                var controlled =
                                    P.UIHelper.PresetControlled(preset) is not null;
                                var ctrlText = controlled ? " " + OptionControlledByIPC : "";
                                DuoLog.Information($"{(int)preset} - {preset}{ctrlText}");
                            }
                        }

                        else if (filter == "unset") // list unset features
                        {
                            foreach (CustomComboPreset preset in Enum.GetValues<CustomComboPreset>().Where(preset => !IPC.GetComboState(preset.ToString())!.First().Value))
                            {
                                var controlled =
                                    P.UIHelper.PresetControlled(preset) is not null;
                                var ctrlText = controlled ? " " + OptionControlledByIPC : "";
                                DuoLog.Information($"{(int)preset} - {preset}{ctrlText}");
                            }
                        }

                        else if (filter == "all") // list all features
                        {
                            foreach (CustomComboPreset preset in Enum.GetValues<CustomComboPreset>())
                            {
                                var controlled =
                                    P.UIHelper.PresetControlled(preset) is not null;
                                var ctrlText = controlled ? " " + OptionControlledByIPC : "";
                                DuoLog.Information($"{(int)preset} - {preset}{ctrlText}");
                            }
                        }

                        else
                        {
                            DuoLog.Error("Available list filters: set, unset, all");
                        }

                        break;
                    }

                case "enabled": // list all currently enabled features
                    {
                        foreach (CustomComboPreset preset in Service.Configuration.EnabledActions.OrderBy(x => x))
                        {
                            if (int.TryParse(preset.ToString(), out int pres)) continue;
                            var controlled =
                                P.UIHelper.PresetControlled(preset) is not null;
                            var ctrlText = controlled ? " " + OptionControlledByIPC : "";
                            DuoLog.Information($"{(int)preset} - {preset}{ctrlText}");
                        }

                        break;
                    }

                case "debug": // debug logging
                    {
                        try
                        {
                            string? specificJob = argumentsParts.Length == 2 ? argumentsParts[1].ToLower() : "";

                            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                            string[]? conflictingPlugins = ConflictingPluginsCheck.TryGetConflictingPlugins();
                            int conflictingPluginsCount = conflictingPlugins?.Length ?? 0;

                            int leaseesCount = P.UIHelper.ShowNumberOfLeasees();
                            (string pluginName, int configurationsCount)[] leasees = P.UIHelper.ShowLeasees();

                            string repoURL = RepoCheckFunctions.FetchCurrentRepo()?.InstalledFromUrl ?? "Unknown";
                            string currentZone = Svc.Data.GetExcelSheet<TerritoryType>()?
                                .FirstOrDefault(x => x.RowId == Svc.ClientState.TerritoryType)
                                .PlaceName.Value.Name.ToString() ?? "Unknown";

                            using StreamWriter file = new($"{desktopPath}/WrathDebug.txt", append: false);  // Output path

                            file.WriteLine("START DEBUG LOG");
                            file.WriteLine("");
                            file.WriteLine($"Plugin Version: {GetType().Assembly.GetName().Version}");                   // Plugin version
                            file.WriteLine($"Installation Repo: {repoURL}");                                             // Installation Repo
                            file.WriteLine("");
                            file.WriteLine($"Plugins controlling via IPC: {leaseesCount}");                               // IPC Leasees
                            if (leaseesCount > 0)
                            {
                                foreach (var leasee in leasees)
                                    file.WriteLine($"- {leasee.pluginName} ({leasee.configurationsCount} configurations)");
                                file.WriteLine("");
                            }
                            file.WriteLine($"Conflicting Plugins: {conflictingPluginsCount}");                           // Conflicting Plugins
                            if (conflictingPlugins != null)
                            {
                                foreach (var plugin in conflictingPlugins)
                                    file.WriteLine($"- {plugin}");                                                       // Listing Conflicting Plugin
                                file.WriteLine("");
                            }
                            file.WriteLine($"Current Job: " +                                                            // Current Job
                                $"{Svc.ClientState.LocalPlayer.ClassJob.Value.Name} / " +                                // - Client Name
                                $"{Svc.ClientState.LocalPlayer.ClassJob.Value.NameEnglish} / " +                         // - EN Name
                                $"{Svc.ClientState.LocalPlayer.ClassJob.Value.Abbreviation}");                           // - Abbreviation
                            file.WriteLine($"Current Job Index: {Svc.ClientState.LocalPlayer.ClassJob.RowId}");          // Job Index
                            file.WriteLine($"Current Job Level: {Svc.ClientState.LocalPlayer.Level}");                   // Job Level
                            file.WriteLine("");
                            file.WriteLine($"Current Zone: {currentZone}");                                              // Current zone location
                            file.WriteLine($"Current Party Size: {CustomComboFunctions.GetPartyMembers().Count}");                                   // Current party size
                            file.WriteLine("");
                            file.WriteLine($"START ENABLED FEATURES");

                            int i = 0;
                            if (string.IsNullOrEmpty(specificJob))
                            {
                                foreach (CustomComboPreset preset in Service.Configuration.EnabledActions.OrderBy(x => x))
                                {
                                    if (int.TryParse(preset.ToString(), out _)) { i++; continue; }

                                    file.Write($"{(int)preset} - {preset}");
                                    if (leaseesCount > 0)
                                        if (P.UIHelper.PresetControlled(preset) is not null)
                                            file.Write(" (IPC)");
                                    file.WriteLine();
                                }
                            }

                            else
                            {
                                foreach (CustomComboPreset preset in Service.Configuration.EnabledActions.OrderBy(x => x))
                                {
                                    if (int.TryParse(preset.ToString(), out _)) { i++; continue; }

                                    if (preset.ToString()[..3].Equals(specificJob, StringComparison.CurrentCultureIgnoreCase) ||  // Job identifier
                                        preset.ToString()[..3].Equals("all", StringComparison.CurrentCultureIgnoreCase) ||        // Adds in Globals
                                        preset.ToString()[..3].Equals("pvp", StringComparison.CurrentCultureIgnoreCase))          // Adds in PvP Globals
                                    {
                                        file.Write($"{(int)preset} - {preset}");
                                        if (leaseesCount > 0)
                                            if (P.UIHelper.PresetControlled(preset) is not null)
                                                file.Write(" (IPC)");
                                        file.WriteLine();
                                    }
                                }
                            }


                            file.WriteLine($"END ENABLED FEATURES");
                            file.WriteLine("");

                            file.WriteLine("START CONFIG SETTINGS");
                            if (string.IsNullOrEmpty(specificJob))
                            {
                                file.WriteLine("---INT VALUES---");
                                foreach (var item in PluginConfiguration.CustomIntValues.OrderBy(x => x.Key))
                                {
                                    file.WriteLine($"{item.Key.Trim()} - {item.Value}");
                                }
                                file.WriteLine("");
                                file.WriteLine("---FLOAT VALUES---");
                                foreach (var item in PluginConfiguration.CustomFloatValues.OrderBy(x => x.Key))
                                {
                                    file.WriteLine($"{item.Key.Trim()} - {item.Value}");
                                }
                                file.WriteLine("");
                                file.WriteLine("---BOOL VALUES---");
                                foreach (var item in PluginConfiguration.CustomBoolValues.OrderBy(x => x.Key))
                                {
                                    file.WriteLine($"{item.Key.Trim()} - {item.Value}");
                                }
                                file.WriteLine("");
                                file.WriteLine("---BOOL ARRAY VALUES---");
                                foreach (var item in PluginConfiguration.CustomBoolArrayValues.OrderBy(x => x.Key))
                                {
                                    file.WriteLine($"{item.Key.Trim()} - {string.Join(", ", item.Value)}");
                                }
                            }
                            else
                            {
                                var jobname = ConfigWindow.groupedPresets.Where(x => x.Value.Any(y => y.Info.JobShorthand.Equals(specificJob.ToLower(), StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault().Key;
                                var jobID = Svc.Data.GetExcelSheet<ClassJob>()?
                                    .Where(x => x.Name.ToString().Equals(jobname, StringComparison.CurrentCultureIgnoreCase))
                                    .First()
                                    .RowId;

                                var whichConfig = jobID switch
                                {
                                    1 or 19 => typeof(PLD.Config),
                                    2 or 20 => typeof(MNK.Config),
                                    3 or 21 => typeof(WAR.Config),
                                    4 or 22 => typeof(DRG.Config),
                                    5 or 23 => typeof(BRD.Config),
                                    6 or 24 => typeof(WHM.Config),
                                    7 or 25 => typeof(BLM.Config),
                                    26 or 27 => typeof(SMN.Config),
                                    28 => typeof(SCH.Config),
                                    29 or 30 => typeof(NIN.Config),
                                    31 => typeof(MCH.Config),
                                    32 => typeof(DRK.Config),
                                    33 => typeof(AST.Config),
                                    34 => typeof(SAM.Config),
                                    35 => typeof(RDM.Config),
                                    //36 => typeof(BLU.Config),
                                    37 => typeof(GNB.Config),
                                    38 => typeof(DNC.Config),
                                    39 => typeof(RPR.Config),
                                    40 => typeof(SGE.Config),
                                    41 => typeof(VPR.Config),
                                    42 => typeof(PCT.Config),
                                    _ => throw new NotImplementedException(),
                                };

                                foreach (var config in whichConfig.GetMembers().Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property))
                                {
                                    PrintConfig(file, config);
                                }

                                foreach (var config in typeof(PvPCommon.Config).GetMembers().Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property))
                                {
                                    PrintConfig(file, config);
                                }
                            }


                            file.WriteLine("END CONFIG SETTINGS");
                            file.WriteLine("");
                            file.WriteLine($"Redundant IDs found: {i}");

                            if (i > 0)
                            {
                                file.WriteLine($"START REDUNDANT IDs");
                                foreach (CustomComboPreset preset in Service.Configuration.EnabledActions.Where(x => int.TryParse(x.ToString(), out _)).OrderBy(x => x))
                                {
                                    file.WriteLine($"{(int)preset}");
                                }

                                file.WriteLine($"END REDUNDANT IDs");
                                file.WriteLine("");
                            }

                            file.WriteLine($"Status Effect Count: {Svc.ClientState.LocalPlayer.StatusList.Count(x => x != null)}");

                            if (Svc.ClientState.LocalPlayer.StatusList.Length > 0)
                            {
                                file.WriteLine($"START STATUS EFFECTS");
                                foreach (Status? status in Svc.ClientState.LocalPlayer.StatusList)
                                {
                                    file.WriteLine($"ID: {status.StatusId}, COUNT: {status.StackCount}, SOURCE: {status.SourceId} NAME: {ActionWatching.GetStatusName(status.StatusId)}");
                                }

                                file.WriteLine($"END STATUS EFFECTS");
                            }

                            file.WriteLine("END DEBUG LOG");
                            DuoLog.Information("Please check your desktop for WrathDebug.txt and upload this file where requested.");

                            break;
                        }

                        catch (Exception ex)
                        {
                            Svc.Log.Error(ex, "Debug Log");
                            DuoLog.Error("Unable to write Debug log.");
                            break;
                        }
                    }
                case "auto":
                    {
                        bool newVal = argumentsParts.Length > 1 ? argumentsParts[1].ToLower() == "on" : !Service.Configuration.RotationConfig.Enabled;

                        if (newVal != Service.Configuration.RotationConfig.Enabled)
                        {
                            ToggleAutorot(newVal);
                        }

                        break;
                    }
                case "combo":
                    {
                        if (argumentsParts.Length < 2) break;

                        switch (argumentsParts[1])
                        {
                            case "on":
                                if (!Service.IconReplacer.getIconHook.IsEnabled) Service.IconReplacer.getIconHook.Enable();
                                break;
                            case "off":
                                if (Service.IconReplacer.getIconHook.IsEnabled) Service.IconReplacer.getIconHook.Disable();
                                break;
                            case "toggle":
                                if (Service.IconReplacer.getIconHook.IsEnabled) Service.IconReplacer.getIconHook.Disable(); else Service.IconReplacer.getIconHook.Enable();
                                break;
                        }

                        break;
                    }
                case "ignore":
                    {
                        var tar = Svc.Targets.Target;
                        if (Service.Configuration.IgnoredNPCs.Any(x => x.Key == tar?.DataId))
                        {
                            DuoLog.Error($"{tar.Name} (ID: {tar.DataId}) is already on the ignored list.");
                            return;
                        }

                        if (tar != null && tar.IsHostile() && !Service.Configuration.IgnoredNPCs.Any(x => x.Key == tar.DataId))
                        {
                            Service.Configuration.IgnoredNPCs.Add(tar.DataId, tar.GetNameId());
                            Service.Configuration.Save();

                            DuoLog.Information($"Successfully added {tar.Name} (ID: {tar.DataId}) to ignored list.");
                        }
                        break;
                    }
                default:
                    ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
                    PvEFeatures.HasToOpenJob = true;
                    if (argumentsParts[0].Length > 0)
                    {
                        var jobname = ConfigWindow.groupedPresets.Where(x => x.Value.Any(y => y.Info.JobShorthand.Equals(argumentsParts[0].ToLower(), StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault().Key;
                        var header = $"{jobname} - {argumentsParts[0].ToUpper()}";
                        PvEFeatures.HeaderToOpen = header;
                    }
                    break;
            }

            Service.Configuration.Save();
        }

        private static void PrintConfig(StreamWriter file, MemberInfo? config)
        {
            string key = config.Name!;

            var field = config.ReflectedType.GetField(key);
            var val1 = field.GetValue(null);
            if (val1.GetType().BaseType == typeof(UserData))
            {
                key = val1.GetType().BaseType.GetField("pName").GetValue(val1).ToString()!;
            }

            if (PluginConfiguration.CustomIntValues.TryGetValue(key, out int intvalue)) { file.WriteLine($"{config.Name} - {intvalue}"); return; }
            if (PluginConfiguration.CustomFloatValues.TryGetValue(key, out float floatvalue)) { file.WriteLine($"{config.Name} - {floatvalue}"); return; }
            if (PluginConfiguration.CustomBoolValues.TryGetValue(key, out bool boolvalue)) { file.WriteLine($"{config.Name} - {boolvalue}"); return; }
            if (PluginConfiguration.CustomBoolArrayValues.TryGetValue(key, out bool[]? boolarrayvalue)) { file.WriteLine($"{config.Name} - {string.Join(", ", boolarrayvalue)}"); return; }
            if (PluginConfiguration.CustomIntArrayValues.TryGetValue(key, out int[]? intaraayvalue)) { file.WriteLine($"{config.Name} - {string.Join(", ", intaraayvalue)}"); return; }

            file.WriteLine($"{key} - NOT SET");
        }

        public static object GetValue(MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(forObject)!;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject)!;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
