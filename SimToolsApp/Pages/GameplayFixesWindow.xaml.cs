// SimTools
// Main Application
// SimTools Gameplay Fixes Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

// 'ERE'S THE BEEF! This is where the meat is. The bulkiest bit of fixes for The Sims 3, all in one place.

using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;

using WpfMessageBox = System.Windows.MessageBox;

namespace SimTools;

// ══════════════════════════════════════════════════════════════════════════════
//  Window code-behind
// ══════════════════════════════════════════════════════════════════════════════
public partial class GameplayFixesWindow : Window
{
    // ── State ─────────────────────────────────────────────────────────────────
    private readonly string                         _sims3Mods;
    private readonly List<GameplaySectionViewModel> _sections;

    // ── Constructor ───────────────────────────────────────────────────────────
    public GameplayFixesWindow(string sims3Mods)
    {
        _sims3Mods = sims3Mods;
        InitializeComponent();
        ApplyLanguage();

        _sections = BuildSections()
            .Select(s =>
            {
                var vmItems = s.Items.Select(item =>
                {
                    var vm = new GameplayFixViewModel(item);
                    if (!string.IsNullOrEmpty(item.OnCheckedMessage))
                    {
                        var msg   = item.OnCheckedMessage;
                        var title = item.DisplayName;
                        vm.CheckedMessageRequested += (_, _) =>
                            WpfMessageBox.Show(msg, title,
                                MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return vm;
                });
                return new GameplaySectionViewModel(s.Header, vmItems);
            })
            .ToList();

        SectionsControl.ItemsSource = _sections;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private IEnumerable<GameplayFixViewModel> AllActiveItems()
        => _sections.SelectMany(s => s.Items).Where(i => i.IsActive);

    // ── Global Select All ─────────────────────────────────────────────────────
    private void GlobalSelectAll_Click(object sender, RoutedEventArgs e)
    {
        bool check = GlobalSelectAll.IsChecked == true;

        if (check)
        {
            var result = WpfMessageBox.Show(
                LanguageManager.Get("GameplayFixes", "SelectAll_Ask",
                    "You are about to select every available fix. You need to be sure about this. This could potentially slow your game. Furthermore, if there are packs that are not installed, your game may crash. "),
                LanguageManager.Get("GameplayFixes", "SelectAll_Title",
                    "SimTools — Select All Warning"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                GlobalSelectAll.IsChecked = false;
                return;
            }
        }

        foreach (var item in AllActiveItems())
            item.IsChecked = check;
    }

    // ── Cancel ────────────────────────────────────────────────────────────────
    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    // ══════════════════════════════════════════════════════════════════════════
    //  Download handler
    // ══════════════════════════════════════════════════════════════════════════
    private async void DownloadSelected_Click(object sender, RoutedEventArgs e)
    {
        if (!GamePaths.IsConfigured(_sims3Mods))
        {
            WpfMessageBox.Show(
                LanguageManager.Get("Paths", "Sims3Mods",
                    "Your Sims 3 Mods directory is not configured.\nPlease open Settings and set it first."),
                LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!ModFrameworkHelper.EnsureInstalled(_sims3Mods)) return;

        var toDownload = _sections
            .SelectMany(s => s.Items)
            .Where(i => i.IsChecked && !string.IsNullOrEmpty(i.FileName))
            .ToList();

        if (toDownload.Count == 0)
        {
            WpfMessageBox.Show(
                LanguageManager.Get("GameplayFixes", "NoneSelected",
                    "No fixes are selected."),
                LanguageManager.Get("GameplayFixes", "NoneSelected_Title",
                    "Nothing to Download"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DownloadBtn.IsEnabled = false;

        int downloaded = 0, skipped = 0, failed = 0;

        try
        {
            foreach (var item in toDownload)
            {
                var url      = AppSettings.ResolveUrl(item.Url);
                var destPath = Path.Combine(_sims3Mods, item.FileName);

                var dir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                // ── HEAD check: skip if local file is already current ─────────
                bool needsDownload = !File.Exists(destPath);

                if (!needsDownload)
                {
                    try
                    {
                        using var hc = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                        using var hr = await hc.SendAsync(
                            new HttpRequestMessage(HttpMethod.Head, url));

                        if (hr.IsSuccessStatusCode)
                        {
                            var remote = hr.Content.Headers.LastModified;
                            if (remote.HasValue)
                            {
                                var local = File.GetLastWriteTimeUtc(destPath);
                                needsDownload = remote.Value.UtcDateTime > local.AddSeconds(5);
                            }
                        }
                    }
                    catch { /* HEAD unavailable — keep local copy */ }
                }

                if (!needsDownload) { skipped++; continue; }

                // ── Download ──────────────────────────────────────────────────
                var progressWindow = new DownloadProgressWindow(item.FileName) { Owner = this };
                progressWindow.Show();

                try
                {
                    using var http     = new HttpClient();
                    using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var   remoteLastModified = response.Content.Headers.LastModified;
                    long? totalBytes         = response.Content.Headers.ContentLength;

                    await using var contentStream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream    = new FileStream(
                        destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    var  buffer      = new byte[8192];
                    long bytesRead   = 0;
                    int  lastPercent = 0, chunk;

                    while ((chunk = await contentStream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, chunk));
                        bytesRead += chunk;

                        if (totalBytes.HasValue)
                        {
                            int pct = (int)(bytesRead * 100 / totalBytes.Value);
                            if (pct != lastPercent)
                            {
                                lastPercent = pct;
                                progressWindow.UpdateProgress(pct);
                            }
                        }
                        else
                        {
                            progressWindow.SetIndeterminate();
                        }
                    }

                    if (remoteLastModified.HasValue)
                        File.SetLastWriteTimeUtc(destPath, remoteLastModified.Value.UtcDateTime);

                    downloaded++;
                }
                catch (Exception ex)
                {
                    try { if (File.Exists(destPath)) File.Delete(destPath); } catch { }
                    failed++;
                    WpfMessageBox.Show(
                        LanguageManager.Format("GameplayFix", "DownloadError",
                            item.FileName, ex.Message),
                        LanguageManager.Get("GameplayFix", "DownloadError_Title",
                            "Download Error"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    progressWindow.Close();
                }
            }

            WpfMessageBox.Show(
                LanguageManager.Format("GameplayFix", "Done",
                    downloaded, skipped, failed),
                LanguageManager.Get("GameplayFix", "Done_Title",
                    "Gameplay Fixes — The Sims 3"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        finally
        {
            DownloadBtn.IsEnabled = true;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Section definitions
    // ══════════════════════════════════════════════════════════════════════════
    private static IEnumerable<(string Header, List<GameplayFixItem> Items)> BuildSections()
    {
        // ── Base Game (23 items) ──────────────────────────────────────────────
        yield return (LanguageManager.Get("BuyTS3", "Sims3BG", "Base Game"), new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),                   "SimTools/Packages/OhRudi__BaseGame__Sims_need_less_Space.package",                          "%baseurl%/Mods/Sims3/Fixes/Packages/base/OhRudi__BaseGame__Sims_need_less_Space.package"),
            new(LanguageManager.Get("GameplayFix", "ImpMemories", "Only Important Memories by VelocityGrass"),         "SimTools/Packages/velocitygrass_only_important_memories.package",                          "%baseurl%/Mods/Sims3/Fixes/Packages/base/velocitygrass_only_important_memories.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),             "SimTools/Packages/BG_Tileable_Items_Shader_FIXED.package",                                 "%baseurl%/Mods/Sims3/Fixes/Packages/base/BG_Tileable_Items_Shader_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "SlopedTerrainFix", "Interact on Sloped Terrain by Nikel23"),            "SimTools/Packages/Nikel23 - Interact on sloped terrain.package",                           "%baseurl%/Mods/Sims3/Fixes/Packages/base/Nikel23 - Interact on sloped terrain.package"),
            new(LanguageManager.Get("GameplayFix", "HalfWallFix", "Half Wall Fix by Simsi45"),                         "SimTools/Packages/Half walls fixed - all.package",                                         "%baseurl%/Mods/Sims3/Fixes/Packages/base/Half walls fixed - all.package"),
            new(LanguageManager.Get("GameplayFix", "EyeballUVFix", "Eyeball UV Fix by S-Club"),                         "SimTools/Packages/S-Club ts3 mod EA Eyeball F UVFix.package",                              "%baseurl%/Mods/Sims3/Fixes/Packages/base/S-Club ts3 mod EA Eyeball F UVFix.package"),
            new(LanguageManager.Get("GameplayFix", "EyeshadowFix", "EA Eyeshadow Fix by Lavsm"),                        "SimTools/Packages/EyeshadowAlphaFix_BaseGame_DefaultReplacement.package",                  "%baseurl%/Mods/Sims3/Fixes/Packages/base/EyeshadowAlphaFix_BaseGame_DefaultReplacement.package"),
            new(LanguageManager.Get("GameplayFix", "MuscleSliderFix", "Muscle Slider Fix by Nysha"),                       "SimTools/Packages/whiteriderMTS_LNMuscleSliderNudeFix.package",                            "%baseurl%/Mods/Sims3/Fixes/Packages/base/whiteriderMTS_LNMuscleSliderNudeFix.package"),
            new(LanguageManager.Get("GameplayFix", "GTKFix", "Get To Know Fix by SimBouquet"),                    "SimTools/Packages/simbouquet_GetToKnowFix.package",                                        "%baseurl%/Mods/Sims3/Fixes/Packages/base/simbouquet_GetToKnowFix.package",
                LanguageManager.Get("GameplayFix", "MonoPatcherWarning", "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first.")),
            new(LanguageManager.Get("GameplayFix", "GTKUtils", "Get To Know Fix Utils by SimBouquet"),              "SimTools/Packages/simbouquet_Utils.package",                                               "%baseurl%/Mods/Sims3/Fixes/Packages/base/simbouquet_Utils.package",
                LanguageManager.Get("GameplayFix", "MonoPatcherWarning", "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first.")),
            new(LanguageManager.Get("GameplayFix", "WelcomeMattFix", "Welcome Matt De-Shined by CeltySims"),              "SimTools/Packages/celtysimsWelcomeMattdeshined.package",                                   "%baseurl%/Mods/Sims3/Fixes/Packages/base/celtysimsWelcomeMattdeshined.package"),
            new(LanguageManager.Get("GameplayFix", "ClapboardFix", "Horizontal Clapboard Fix by CircusWolf"),           "SimTools/Packages/CW_HorizontalClapboardFixed.package",                                    "%baseurl%/Mods/Sims3/Fixes/Packages/base/CW_HorizontalClapboardFixed.package"),
            new(LanguageManager.Get("GameplayFix", "NoCommLots", "No Auto Placing Community Lots by Bluegenjutsu"),   "SimTools/Packages/bluegenjutsu_NoAutoPlacingCommunityLots.package",                        "%baseurl%/Mods/Sims3/Fixes/Packages/base/bluegenjutsu_NoAutoPlacingCommunityLots.package",
                LanguageManager.Get("GameplayFix", "NoCommLotsWarning", "CAUTION: No Auto Placing Community Lots will disable the script which places lots such as the Fire Station, Salon, Laundromat, etc etc. If you like these lots, do not install this package. This mod will also require at least one of these EP's: Ambitions, Showtime, Supernatural or Seasons. If you do not have any of these EP's, you can safely skip this mod.")),
            new(LanguageManager.Get("GameplayFix", "AtomicStairFix", "Atomic Age Stair Fix by EnableLlamas"),             "SimTools/Packages/enablellamasAtomicAgeStairsFixDR.package",                               "%baseurl%/Mods/Sims3/Fixes/Packages/base/enablellamasAtomicAgeStairsFixDR.package"),
            new(LanguageManager.Get("GameplayFix", "AtomicStairFix", "Walk Cycle Edits by SimBouquet"),                   "SimTools/Overrides/simbouquet_OVERRIDE_WalkCycleEdits.package",                            "%baseurl%/Mods/Sims3/Fixes/Overrides/base/simbouquet_OVERRIDE_WalkCycleEdits.package"),
            new(LanguageManager.Get("GameplayFix", "TSMFaceExpressions", "Medieval Facial Expressions by SimBouquet"),        "SimTools/Overrides/simbouquet_OVERRIDE_TSMtoTS3_FacialExpressions.package",                "%baseurl%/Mods/Sims3/Fixes/Overrides/base/simbouquet_OVERRIDE_TSMtoTS3_FacialExpressions.package"),
            new(LanguageManager.Get("GameplayFix", "RandomSimFixes", "Random Sim Fixes by LazyDuchess"),                  "SimTools/Packages/ld_RandomSimFixes.package",                                              "%baseurl%/Mods/Sims3/Fixes/Packages/base/ld_RandomSimFixes.package"),
            new(LanguageManager.Get("GameplayFix", "SimBinGeneticsMale", "Sim Bin Genetics Male Presets by Anime_Boom"),      "SimTools/Packages/SimBinYAAMPresets.package",                                              "%baseurl%/Mods/Sims3/Fixes/Packages/base/SimBinYAAMPresets.package"),
            new(LanguageManager.Get("GameplayFix", "SimBinGeneticsFemale", "Sim Bin Genetics Female Presets by Anime_Boom"),    "SimTools/Packages/SimBinYAFAFPresets.package",                                             "%baseurl%/Mods/Sims3/Fixes/Packages/base/SimBinYAFAFPresets.package"),
            new(LanguageManager.Get("GameplayFix", "PickUpToddlerFix", "Pick Up Toddler Fix by TheSweetSimmer"),            "SimTools/Packages/TSS_PickUpToddlerFix.package",                                           "%baseurl%/Mods/Sims3/Fixes/Packages/base/TSS_PickUpToddlerFix.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"),      "SimTools/Packages/BASE GAME - Stencils Unlocked.package",                                  "%baseurl%/Mods/Sims3/Fixes/Packages/base/BASE GAME - Stencils Unlocked.package"),
            new(LanguageManager.Get("GameplayFix", "FishingBoxFix", "Fishing Box Fix by NanaBx3 & caoride"),             "SimTools/Packages/NanaBx3_fishingBoxChest_collectionFix.package",                          "%baseurl%/Mods/Sims3/Fixes/Packages/base/NanaBx3_fishingBoxChest_collectionFix.package"),
            new(LanguageManager.Get("GameplayFix", "CrossEyeFix", "Cross-Eye Fix by LazyDuchess"),                     "SimTools/Packages/ld_CrossEyeFix.package",                                                 "%baseurl%/Mods/Sims3/Fixes/Packages/base/ld_CrossEyeFix.package"),
            new(LanguageManager.Get("GameplayFix", "PigtailsFix", "Pigtails Glitch Fix by Phantom99"),                 "SimTools/Packages/PigtailGlitchFix.package",                                               "%baseurl%/Mods/Sims3/Fixes/Packages/base/PigtailGlitchFix.package"),
            new(LanguageManager.Get("GameplayFix", "WateryGraveFix", "Watery Grave Plaque Fix by fantuanss12"),           "SimTools/Packages/Fantuanss12_UrnstonePlaqueEditV2.package",                               "%baseurl%/Mods/Sims3/Fixes/Packages/base/Fantuanss12_UrnstonePlaqueEditV2.package"),
            new(LanguageManager.Get("GameplayFix", "GroceryFixes", "Grocery Fixes by Swiffy"),                          "SimTools/Packages/swiffyMisc.GroceryFixes-RequiresMonoPatcher-v1.1.package",               "%baseurl%/Mods/Sims3/Fixes/Packages/base/swiffyMisc.GroceryFixes-RequiresMonoPatcher-v1.1.package", "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first."),
        });


        // ── World Adventures (6 items) ────────────────────────────────────────
        yield return ("World Adventures", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),                              "SimTools/Packages/OhRudi__WorldAdventures__Sims_need_less_Space.package",                     "%baseurl%/Mods/Sims3/Fixes/Packages/world_adventures/OhRudi__WorldAdventures__Sims_need_less_Space.package"),
            new(LanguageManager.Get("GameplayFix", "DisTerrainFix", "Champs les Sims Distant Terrain Tree Fix by PotatoBalladSims"),"SimTools/Packages/PotatoBalladSims_terraindistantFrance_FIX.package",                         "%baseurl%/Mods/Sims3/Fixes/Packages/world_adventures/PotatoBalladSims_terraindistantFrance_FIX.package"),
            new(LanguageManager.Get("GameplayFix", "SteamTrainFix", "European Steam Train Fix by PotatoBalladSims"),                "SimTools/Packages/PotatoBalladSims_European_Steam_Train.package",                             "%baseurl%/Mods/Sims3/Fixes/Packages/world_adventures/PotatoBalladSims_European_Steam_Train.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),                        "SimTools/Packages/WORLD ADVENTURES - Tileable_Items_Shader_FIXED.package",                   "%baseurl%/Mods/Sims3/Fixes/Packages/world_adventures/WORLD ADVENTURES - Tileable_Items_Shader_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"),                 "SimTools/Packages/WORLD ADVENTURES - Unlocked Stencils.package",                             "%baseurl%/Mods/Sims3/Fixes/Packages/world_adventures/WORLD ADVENTURES - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "AsianWindowFix", "Asian Window Reflects Light Fix by OhRudi"),                   "SimTools/Packages/OhRudi__WorldAdventures__asian_window_fix__king_qings_window.package",      "%baseurl%/Mods/Sims3/Fixes/Packages/world_adventures/OhRudi__WorldAdventures__asian_window_fix__king_qings_window.package"),
        });

        // ── High End Loft Stuff (1 item) ──────────────────────────────────────
        yield return ("High End Loft Stuff", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"), "SimTools/Packages/HELS - Tileable_Items_Shader_FIXED.package", "%baseurl%/Mods/Sims3/Fixes/Packages/high_end_loft_stuff/HELS - Tileable_Items_Shader_FIXED.package"),
        });

        // ── Ambitions (6 items) ───────────────────────────────────────────────
        yield return ("Ambitions", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "HarvesterFix", "Harvester Fix by Fantuanss12"),                "SimTools/Packages/Fantuanss12_Harverster_TempFix.package",                         "%baseurl%/Mods/Sims3/Fixes/Packages/ambitions/Fantuanss12_Harverster_TempFix.package"),
            new(LanguageManager.Get("GameplayFix", "NoMagicClothesFix", "No Magic Clothesline Fix by Gamefreak130"),    "SimTools/Packages/Gamefreak130_NoMagicClothesline.package",                        "%baseurl%/Mods/Sims3/Fixes/Packages/ambitions/Gamefreak130_NoMagicClothesline.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"), "SimTools/Packages/AMBITIONS - Unlocked Stencils.package",                          "%baseurl%/Mods/Sims3/Fixes/Packages/ambitions/AMBITIONS - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "EyeshadowFix", "EA Eyeshadow Fix by Lavsm"),                   "SimTools/Packages/EyeshadowAlphaFix_Ambitions_DefaultReplacement.package",         "%baseurl%/Mods/Sims3/Fixes/Packages/ambitions/EyeshadowAlphaFix_Ambitions_DefaultReplacement.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),              "SimTools/Packages/OhRudi__Ambitions__-Sims_need_less_Space.package",                "%baseurl%/Mods/Sims3/Fixes/Packages/ambitions/OhRudi__Ambitions__Sims_need_less_Space.package"),
            new(LanguageManager.Get("GameplayFix", "InvestigatorFix", "No More Non-Cases for Investigators by jm2k"),         "SimTools/Packages/jm2k_InvestigatorCaseFix.package",                "%baseurl%/Mods/Sims3/Fixes/Packages/ambitions/jm2k_InvestigatorCaseFix.package", "Fixes an issue were investigators could roll university and supernatural opportunities instead of the opportunities from the investigator career. University Life or Supernatural are required to also be installed otherwise the bug will not be present."),
        });

        // ── Late Night (8 items) ──────────────────────────────────────────────
        yield return ("Late Night", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "CelebFridgeFix", "Celeb Fridge Texture Fix by EnableLlamas"),        "SimTools/Packages/enablellamasRefrigeratorCelebSpecFix.package",           "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/enablellamasRefrigeratorCelebSpecFix.package"),
            new(LanguageManager.Get("GameplayFix", "CraneMed", "Enable Crane (Medium) in buydebug by Armiel"),     "SimTools/Packages/armiel_craneMedium.package",                            "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/armiel_craneMedium.package"),
            new(LanguageManager.Get("GameplayFix", "CraneLg", "Enable Crane (Large) in buydebug by Armiel"),      "SimTools/Packages/armiel_craneLarge.package",                             "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/armiel_craneLarge.package"),
            new(LanguageManager.Get("GameplayFix", "LNPlantFix", "Late Night Plant Fixes by Robodl95"),               "SimTools/Packages/Robodl95_ LN Plant fix.package",                        "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/Robodl95_ LN Plant fix.package"),
            new(LanguageManager.Get("GameplayFix", "BPWorkbenchFix", "Bridgeport Workbench Fix by DividingByZero"),       "SimTools/Packages/Bridgeport Workbench Fix.package",                      "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/Bridgeport Workbench Fix.package"),
            new(LanguageManager.Get("GameplayFix", "LNElevatorFix", "Elevator Shaft Placement Fix by Vesko"),       "SimTools/Packages/[vesko_sims3] Elevator Shaft Placement FIX.package",                      "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/[vesko_sims3] Elevator Shaft Placement FIX.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"),     "SimTools/Packages/LATE NIGHT - Unlocked Stencils.package",                "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/LATE NIGHT - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),             "SimTools/Packages/LATE NIGHT - Tileable_Items_Shader_FIXED.package",      "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/LATE NIGHT - Tileable_Items_Shader_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),                   "SimTools/Packages/OhRudi__LateNight__Sims_need_less_Space.package",       "%baseurl%/Mods/Sims3/Fixes/Packages/late_night/OhRudi__LateNight__Sims_need_less_Space.package"),
        });

        // ── Generations (7 items) ─────────────────────────────────────────────
        yield return ("Generations", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "GenShirtFix", "Generations Shirt & Sweater Top - Channel Fix by sweetdevil"),"SimTools/Packages/sweetdevil_GENShirtSweaterTopChannelFix_TAM_DR.package",     "%baseurl%/Mods/Sims3/Fixes/Packages/generations/sweetdevil_GENShirtSweaterTopChannelFix_TAM_DR.package"),
            new(LanguageManager.Get("GameplayFix", "GenPaintingFix1", "Teen Fantasy Painting Fix by ThomasRiordan"),       "SimTools/Packages/PTS3_ThomasRiordan_paintingGenTeenFantasyFixed.package",          "%baseurl%/Mods/Sims3/Fixes/Packages/generations/PTS3_ThomasRiordan_paintingGenTeenFantasyFixed.package"),
            new(LanguageManager.Get("GameplayFix", "GenPaintingFix2", "Awkward Family Photo Fixed by ThomasRiordan"),      "SimTools/Packages/PTS3_ThomasRiordan_paintingGenAwkwardFamilyPhotoFixed.package",   "%baseurl%/Mods/Sims3/Fixes/Packages/generations/PTS3_ThomasRiordan_paintingGenAwkwardFamilyPhotoFixed.package"),
            new(LanguageManager.Get("GameplayFix", "RTSFix", "Read Toddler to Sleep Fix by Danjaley"),            "SimTools/Packages/danjaley_read2sleepfix.package",                                 "%baseurl%/Mods/Sims3/Fixes/Packages/generations/danjaley_read2sleepfix.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"),      "SimTools/Packages/GENERATIONS - Unlocked Stencils.package",                        "%baseurl%/Mods/Sims3/Fixes/Packages/generations/GENERATIONS - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "PaintingStencilsMerged", "Stencils & Paintings Fixes Merged by SimTools"),      "SimTools/Packages/Gen_PortraitFixes_StencilsUnlockedFix_Merged.package",                        "%baseurl%/Mods/Sims3/Fixes/Packages/generations/Gen_PortraitFixes_StencilsUnlockedFix_Merged.package", "This is a merged package of the Family Portrait Fixes by ThomasRiordan and the Unlocked Stencils Fix by Simsi45, which usually conflict with each other. It should only be used if you want both. Please be sure you do not install either of the other three mods."),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),             "SimTools/Packages/DECADES -Tileable_Items_Shader_FIXED.package",                   "%baseurl%/Mods/Sims3/Fixes/Packages/generations/DECADES -Tileable_Items_Shader_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),                   "SimTools/Packages/OhRudi__Generations__Sims_need_less_Space.package",              "%baseurl%/Mods/Sims3/Fixes/Packages/generations/OhRudi__Generations__Sims_need_less_Space.package"),
        });

        // ── Pets (6 items) ────────────────────────────────────────────────────
        yield return ("Pets", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "PetsLessSpaceMod", "Pets Need Less Space by OhRudi"),              "SimTools/Packages/OhRudi__Routing Fix__Pets_need_less_space.package",      "%baseurl%/Mods/Sims3/Fixes/Packages/pets/OhRudi__Routing Fix__Pets_need_less_space.package"),
            new(LanguageManager.Get("GameplayFix", "GallopFasterFix", "Gallop Faster Animation Fix by Shimrod101"),   "SimTools/Packages/ShimrodsAnimHorseGallopFastestFix.package",             "%baseurl%/Mods/Sims3/Fixes/Packages/pets/ShimrodsAnimHorseGallopFastestFix.package"),
            new(LanguageManager.Get("GameplayFix", "PetTombstoneFix", "Pet Tombstone Shadow Fix by MenaceMan44"),     "SimTools/Packages/MM_PetTombstoneShadowFix.package",                      "%baseurl%/Mods/Sims3/Fixes/Packages/pets/MM_PetTombstoneShadowFix.package"),
            new(LanguageManager.Get("GameplayFix", "HorseTailFix", "Horse Tail Fixes by Simsi45"),                 "SimTools/Packages/Simsi45_Horse_Braided_Tail_NoRandom.package",           "%baseurl%/Mods/Sims3/Fixes/Packages/pets/Simsi45_Horse_Braided_Tail_NoRandom.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"), "SimTools/Packages/PETS - Unlocked Stencils.package",                     "%baseurl%/Mods/Sims3/Fixes/Packages/pets/PETS - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),        "SimTools/Packages/PETS - Tileable_Items_Shader_FIXED.package",            "%baseurl%/Mods/Sims3/Fixes/Packages/pets/PETS - Tileable_Items_Shader_FIXED.package"),
        });

        // ── Master Suite Stuff (1 item) ───────────────────────────────────────
        yield return ("Master Suite Stuff", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"), "SimTools/Packages/MASTER SUITE STUFF - Unlocked Stencils.package", "%baseurl%/Mods/Sims3/Fixes/Packages/master_suite_stuff/MASTER SUITE STUFF - Unlocked Stencils.package"),
        });

        // ── Showtime (4 items) ────────────────────────────────────────────────
        yield return ("Showtime", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "NoNPCTips", "No NPC's Performing For Tips by Spicsshane"),  "SimTools/Packages/NoNPCsPerformingforTips.package",                      "%baseurl%/Mods/Sims3/Fixes/Packages/showtime/NoNPCsPerformingforTips.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"), "SimTools/Packages/SHOWTIME - Unlocked Stencils.package",                 "%baseurl%/Mods/Sims3/Fixes/Packages/showtime/SHOWTIME - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),        "SimTools/Packages/SHOWTIME - Tileable_Items_Shader_FIXED.package",        "%baseurl%/Mods/Sims3/Fixes/Packages/showtime/SHOWTIME - Tileable_Items_Shader_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),              "SimTools/Packages/OhRudi__Showtime__Sims_need_less_Space.package",        "%baseurl%/Mods/Sims3/Fixes/Packages/showtime/OhRudi__Showtime__Sims_need_less_Space.package"),
        });

        // ── Diesel Stuff (1 item) ─────────────────────────────────────────────
        yield return ("Diesel Stuff", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"), "SimTools/Packages/DIESEL - Unlocked Stencils.package", "%baseurl%/Mods/Sims3/Fixes/Packages/diesel_stuff/DIESEL - Unlocked Stencils.package"),
        });

        // ── Supernatural (8 items) ────────────────────────────────────────────
        yield return ("Supernatural", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "ProperTraitFix", "A Proper Fix for A Proper Trait by SpotlessLeopard"),"SimTools/Packages/ProperTraitFix_[spotlessleopard].package",          "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/ProperTraitFix_[spotlessleopard].package"),
            new(LanguageManager.Get("GameplayFix", "RestoSpellFix", "Restoration Spell Fix by Arsil"),               "SimTools/Packages/Arsil_RestorationSpellFix.package",                    "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/Arsil_RestorationSpellFix.package"),
            new(LanguageManager.Get("GameplayFix", "MoneyTreeFix", "Money Tree Bug Fix by Chicken0895"),            "SimTools/Packages/Chicken0895 Money Tree Bug Fix.package",                "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/Chicken0895 Money Tree Bug Fix.package"),
            new(LanguageManager.Get("GameplayFix", "ProperTraitMoneyTreeFix", "Proper Trait & Money Tree Bug Fix Merged by SimTools"),            "SimTools/Packages/ProperTraitFix_MoneyTreeFix_Merged_SimTools.package",                "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/ProperTraitFix_MoneyTreeFix_Merged_SimTools.package", "This is a merged package of the Money Tree Fix by Chicken0895 and the Proper Trait Fix by SpotlessLeopard, which usually conflict with each other. It should only be used if you want both. Please be sure you do not install either of the other two mods."),
            new(LanguageManager.Get("GameplayFix", "ByeByeZombies", "No More Zombies Generated At Full Moon by PersonCalledJoy"),"SimTools/Packages/ByeByeZombie.package",                      "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/ByeByeZombie.package"),
            new(LanguageManager.Get("GameplayFix", "LLAMASoundFix", "LLAMA Sound Fix by ProtectusCZ"),                    "SimTools/Packages/ProtectusCZ_Llama_Sound_Fix.package", "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/ProtectusCZ_Llama_Sound_Fix.package"),
            new(LanguageManager.Get("GameplayFix", "EyeshadowFix", "EA Eyeshadow Fix by Lavsm"),                    "SimTools/Packages/EyeshadowAlphaFix_Supernatural_DefaultReplacement.package","%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/EyeshadowAlphaFix_Supernatural_DefaultReplacement.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),         "SimTools/Packages/SUPERNATURAL - Tileable_Items_Shader_FIXED.package",   "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/SUPERNATURAL - Tileable_Items_Shader_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),               "SimTools/Packages/OhRudi__Supernatural__Sims_need_less_Space.package",   "%baseurl%/Mods/Sims3/Fixes/Packages/supernatural/OhRudi__Supernatural__Sims_need_less_Space.package"),
            new(LanguageManager.Get("GameplayFix", "MotiveMobileFix", "Motive Mobile Workaround by YamiTheDragon"),    "SimTools/Overrides/YTD_MotiveMobile_NoHygiene.package",                  "%baseurl%/Mods/Sims3/Fixes/Overrides/supernatural/YTD_MotiveMobile_NoHygiene.package",
                LanguageManager.Get("GameplayFix", "MotiveMobileMsg", "This fix presents a potential workaround to the Motive Mobile by greatly nerfing it so that it only fills Fun & Social. This will keep it from breaking needs that would have been made static by a sim becoming supernatural. Alternatively, you can simply avoid buying or using the Motive Mobile.")),
        });

        // ── Seasons (6 items) ─────────────────────────────────────────────────
        yield return ("Seasons", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "nR_Tempest", "nRaas Tempest by Chain_Reaction"),                       "SimTools/Packages/NRaas_Tempest.package",                             "%baseurl%/Mods/Sims3/nRaas/NRaas_Tempest.package"),
            new(LanguageManager.Get("GameplayFix", "FasterRaking", "Faster Raking by Mikey"),                       "SimTools/Packages/faster_raking.package",                             "%baseurl%/Mods/Sims3/Fixes/Packages/seasons/faster_raking.package"),
            new(LanguageManager.Get("GameplayFix", "ForecastTweaks", "Weather Forecast Tweaks by Gamefreak130"),      "SimTools/Packages/Gamefreak130_WeatherForecastTweaks.package",         "%baseurl%/Mods/Sims3/Fixes/Packages/seasons/Gamefreak130_WeatherForecastTweaks.package"),
            new(LanguageManager.Get("GameplayFix", "EclipsingFog", "Truly Eclipsing Fog by Gamefreak130"),          "SimTools/Packages/Gamefreak130_TrulyEclipsingFog.package",            "%baseurl%/Mods/Sims3/Fixes/Packages/seasons/Gamefreak130_TrulyEclipsingFog.package"),
            new(LanguageManager.Get("GameplayFix", "AppleBobbingFix", "Apple Bobbing Tank - Fix for Children by Hundefreund"),          "SimTools/Packages/appleBobbingTank_FixForChildren.package",            "%baseurl%/Mods/Sims3/Fixes/Packages/seasons/appleBobbingTank_FixForChildren.package"),
            new(LanguageManager.Get("GameplayFix", "HiddenStencils", "Hidden Stencils Unlocked & Fixed by Simsi45"),  "SimTools/Packages/SEASONS - Unlocked Stencils.package",              "%baseurl%/Mods/Sims3/Fixes/Packages/seasons/SEASONS - Unlocked Stencils.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),               "SimTools/Packages/OhRudi__Seasons__Sims_need_less_Space.package",     "%baseurl%/Mods/Sims3/Fixes/Packages/seasons/OhRudi__Seasons__Sims_need_less_Space.package"),
            new(LanguageManager.Get("GameplayFix", "NoUglySnowprints", "No More Ugly Snowprints by Lyralei"),           "SimTools/Overrides/Lyralei - NoMoreUglySnowPrints.package",           "%baseurl%/Mods/Sims3/Fixes/Overrides/seasons/Lyralei - NoMoreUglySnowPrints.package"),
        });

        // ── 70s, 80s & 90s Stuff (1 item) ────────────────────────────────────
        yield return ("70s, 80s & 90s Stuff", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "EyeshadowFix", "EA Eyeshadow Fix by Lavsm"), "SimTools/Packages/EyeshadowAlphaFix_70s80s90sStuff_DefaultReplacement.package", "%baseurl%/Mods/Sims3/Fixes/Packages/70s80s90s/EyeshadowAlphaFix_70s80s90sStuff_DefaultReplacement.package"),
        });

        // ── University Life (7 items) ─────────────────────────────────────────
        yield return ("University Life", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "ADUniShellEntry", "University - More Shell Entries by AussomeDays"),   "SimTools/Overrides/University - More Shell Entries.package",                   "%baseurl%/Mods/Sims3/Fixes/Overrides/university/University - More Shell Entries.package"),
            new(LanguageManager.Get("GameplayFix", "ADUniShellReplace", "University - Replacement Shells by AussomeDays"),   "SimTools/Overrides/University - Replacement Shells (overrides).package",       "%baseurl%/Mods/Sims3/Fixes/Overrides/university/University - Replacement Shells (overrides).package"),
            new(LanguageManager.Get("GameplayFix", "UniVisFix", "University Life Visual Fixes by SimBouquet"),       "SimTools/Overrides/simbouquet_OVERRIDE_EP9visualfixes.package",                "%baseurl%/Mods/Sims3/Fixes/Overrides/university/simbouquet_OVERRIDE_EP9visualfixes.package"),
            new(LanguageManager.Get("GameplayFix", "UniNoProtests", "No More University Protests by Don Babilon"),       "SimTools/Packages/DB_ImprovedProtestSituation_NoNPCProtests.package",          "%baseurl%/Mods/Sims3/Fixes/Packages/university/DB_ImprovedProtestSituation_NoNPCProtests.package"),
            new(LanguageManager.Get("GameplayFix", "UniDoorFix", "Alpha & Omega Door Fix by CeltySims"),              "SimTools/Packages/AlphaOmegaDoorFixed.package",                               "%baseurl%/Mods/Sims3/Fixes/Packages/university/AlphaOmegaDoorFixed.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),             "SimTools/Packages/UNIVERSITY LIFE - Tileable_Items_FIXED.package",             "%baseurl%/Mods/Sims3/Fixes/Packages/university/UNIVERSITY LIFE - Tileable_Items_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),                   "SimTools/Packages/OhRudi__UniversityLife__Sims_need_less_Space.package",       "%baseurl%/Mods/Sims3/Fixes/Packages/university/OhRudi__UniversityLife__Sims_need_less_Space.package"),
        });

        // ── Island Paradise (3 items) ─────────────────────────────────────────
        yield return ("Island Paradise", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "HangingLanternFix", "Hanging Lantern Fix by Heaven"),        "SimTools/Packages/heaven_IPHangingLanternFix.package",                    "%baseurl%/Mods/Sims3/Fixes/Packages/island_paradise/heaven_IPHangingLanternFix.package"),
            new(LanguageManager.Get("GameplayFix", "HouseboatLagFix", "Reduce Houseboat Lag by MaryDeHoyos"),  "SimTools/Packages/StopAutopilot_Helm_Updated_03_29_22.package",           "%baseurl%/Mods/Sims3/Fixes/Packages/island_paradise/StopAutopilot_Helm_Updated_03_29_22.package"),
            new(LanguageManager.Get("GameplayFix", "PITO_Fix", "Autonomous Play In The Ocean Fix by Phantom99"),       "SimTools/Packages/GoAndPlayInOceanFix.package",  "%baseurl%/Mods/Sims3/Fixes/Packages/island_paradise/GoAndPlayInOceanFix.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),       "SimTools/Packages/OhRudi__IslandParadise__Sims_need_less_Space.package",  "%baseurl%/Mods/Sims3/Fixes/Packages/island_paradise/OhRudi__IslandParadise__Sims_need_less_Space.package"),
        });

        // ── Into the Future (4 items) ─────────────────────────────────────────
        yield return ("Into the Future", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "LaserRhythmFix", "Watch Laser Rhythm-A-Con Fix by SimsAddict777"),"SimTools/Packages/MTS_simsaddict777_lazerharp_FIX.package",        "%baseurl%/Mods/Sims3/Fixes/Packages/into_the_future/MTS_simsaddict777_lazerharp_FIX.package"),
            new(LanguageManager.Get("GameplayFix", "ITFPlanterFix", "Perigree Planter Fix by Heaven"),               "SimTools/Packages/heaven_fenceFuturePlanterFix.package",           "%baseurl%/Mods/Sims3/Fixes/Packages/into_the_future/heaven_fenceFuturePlanterFix.package"),
            new(LanguageManager.Get("GameplayFix", "TISFix", "Tileable Items Shader Fix by Simsi45"),         "SimTools/Packages/INTO THE FUTURE - Tileable_Items_FIXED.package", "%baseurl%/Mods/Sims3/Fixes/Packages/into_the_future/INTO THE FUTURE - Tileable_Items_FIXED.package"),
            new(LanguageManager.Get("GameplayFix", "LessSpaceMod", "Sims Need Less Space by OhRudi"),               "SimTools/Packages/OhRudi__IntoTheFuture__Sims_need_less_Space.package","%baseurl%/Mods/Sims3/Fixes/Packages/into_the_future/OhRudi__IntoTheFuture__Sims_need_less_Space.package"),
        });

        // ── Store Fixes (5 items) ─────────────────────────────────────────────
        yield return ("Store Fixes", new List<GameplayFixItem>
        {
            new(LanguageManager.Get("GameplayFix", "PerfumeFix", "Armoure Perfume Moodlet Fix by Gamefreak130"), "SimTools/Packages/Gamefreak130_PerfumeMoodletFix.package.package",           "%baseurl%/Mods/Sims3/Fixes/Packages/store/Gamefreak130_PerfumeMoodletFix.package",
            LanguageManager.Get("GameplayFix", "StorePiracyWarning2", "Please make sure you are not using any related hacked/pirated store items prior to installing store fixes.")),
            new(LanguageManager.Get("GameplayFix", "BanquetFanFix", "Banquet Fan Routing Fix by OmegaStarr82"),     "SimTools/Packages/BanquetFanFix.package",                                   "%baseurl%/Mods/Sims3/Fixes/Packages/store/BanquetFanFix.package",
            LanguageManager.Get("GameplayFix", "StorePiracyWarning2", "Please make sure you are not using any related hacked/pirated store items prior to installing store fixes.")),
            new(LanguageManager.Get("GameplayFix", "EfficientUpgradeNameFix", "More Efficient Upgrade Name Fix by Buzzler"),  "SimTools/Packages/Buzz_MoreEfficientUpgradeNameFix.package",                 "%baseurl%/Mods/Sims3/Fixes/Packages/store/Buzz_MoreEfficientUpgradeNameFix.package",
            LanguageManager.Get("GameplayFix", "StorePiracyWarning2", "Please make sure you are not using any related hacked/pirated store items prior to installing store fixes.")),
            new(LanguageManager.Get("GameplayFix", "HaciendaFireplaceFix", "Haute Hacienda Fireplace Fix by Qahne"),       "SimTools/Packages/Qahne_MOD_HauteHaciendaFireplace Fixes.package",           "%baseurl%/Mods/Sims3/Fixes/Packages/store/Qahne_MOD_HauteHaciendaFireplace Fixes.package",
            LanguageManager.Get("GameplayFix", "StorePiracyWarning2", "Please make sure you are not using any related hacked/pirated store items prior to installing store fixes.")),
            new(LanguageManager.Get("GameplayFix", "NTStairFix" , "Now & Then Staircase Fix by Simsi45"),         "SimTools/Packages/Simsi45_Curved_Staircase_FIX_RECAT.package",               "%baseurl%/Mods/Sims3/Fixes/Packages/store/Simsi45_Curved_Staircase_FIX_RECAT.package",
            LanguageManager.Get("GameplayFix", "StorePiracyWarning2", "Please make sure you are not using any related hacked/pirated store items prior to installing store fixes.")),
        });

        // ── Probationary Mods (0 items) ─────────────────────────────────────────────
        yield return (LanguageManager.Get("GameplayFix", "ProbationMods", "Probationary Mods (In Testing)"), new List<GameplayFixItem>
        {
            new("", "", ""),
        });
    }

        // Called on load and by SettingsWindow after a language change
    public void ApplyLanguage()
    {
        // ── RTL / LTR layout direction ─────────────────────────────────────
        var lang = IniHelper.Read("Language", "SelectedLanguage", "en");

        // Add any future RTL languages to this set
        var rtlLanguages = new HashSet<string> { "ar" };
        FlowDirection = rtlLanguages.Contains(lang)
        ? System.Windows.FlowDirection.RightToLeft
        : System.Windows.FlowDirection.LeftToRight;

        // ── Text strings ───────────────────────────────────────────────────
        GlobalSelectAll.Content = LanguageManager.Get("GameplayFix", "GlobalSelectAll", "Select All");
    }
}

