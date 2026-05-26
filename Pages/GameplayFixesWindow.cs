using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using WpfMessageBox = System.Windows.MessageBox;

namespace SimTools;

// ══════════════════════════════════════════════════════════════════════════════
//  Data record — top-level so all ViewModels can reference it
// ══════════════════════════════════════════════════════════════════════════════
public sealed record GameplayFixItem(
    string DisplayName,
    string FileName,
    string Url,
    string OnCheckedMessage = "");

// ══════════════════════════════════════════════════════════════════════════════
//  Item ViewModel
// ══════════════════════════════════════════════════════════════════════════════
public sealed class GameplayFixViewModel : INotifyPropertyChanged
{
    public string DisplayName      { get; }
    public string FileName         { get; }
    public string Url              { get; }
    public string OnCheckedMessage { get; }

    /// <summary>True for real items; false for TBD placeholders (empty FileName).</summary>
    public bool IsActive => !string.IsNullOrEmpty(FileName);

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            OnPropertyChanged();

            if (value && !string.IsNullOrEmpty(OnCheckedMessage))
                CheckedMessageRequested?.Invoke(this, OnCheckedMessage);
        }
    }

    public event EventHandler<string>? CheckedMessageRequested;

    public GameplayFixViewModel(GameplayFixItem item)
    {
        DisplayName      = item.DisplayName;
        FileName         = item.FileName;
        Url              = item.Url;
        OnCheckedMessage = item.OnCheckedMessage;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

// ══════════════════════════════════════════════════════════════════════════════
//  Section ViewModel
// ══════════════════════════════════════════════════════════════════════════════
public sealed class GameplaySectionViewModel : INotifyPropertyChanged
{
    public string                                     Header { get; }
    public ObservableCollection<GameplayFixViewModel> Items  { get; }
    public ICommand                                   ToggleAllCommand { get; }

    /// <summary>
    /// Drives the section-header three-state checkbox (OneWay binding).
    /// Considers only active (non-TBD) items.
    /// </summary>
    public bool? IsAllSelected
    {
        get
        {
            var active = Items.Where(i => i.IsActive).ToList();
            if (active.Count == 0) return false;
            bool any = active.Any(i => i.IsChecked);
            bool all = active.All(i => i.IsChecked);
            return all ? true : any ? null : false;
        }
    }

    public GameplaySectionViewModel(string header, IEnumerable<GameplayFixViewModel> items)
    {
        Header = header;
        Items  = new ObservableCollection<GameplayFixViewModel>(items);

        ToggleAllCommand = new RelayCommand(_ =>
        {
            var active = Items.Where(i => i.IsActive).ToList();
            bool check = active.Any(i => !i.IsChecked);
            foreach (var item in active)
                item.IsChecked = check;
        });

        foreach (var item in Items)
            item.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(GameplayFixViewModel.IsChecked))
                    OnPropertyChanged(nameof(IsAllSelected));
            };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

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
                "Your Sims 3 Mods directory is no longer configured.\nPlease open Settings and set it first.",
                "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var toDownload = _sections
            .SelectMany(s => s.Items)
            .Where(i => i.IsChecked && !string.IsNullOrEmpty(i.FileName))
            .ToList();

        if (toDownload.Count == 0)
        {
            WpfMessageBox.Show(
                "No fixes are selected.\n\nNote: greyed-out items are placeholders not yet defined.",
                "Nothing to Download", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        $"Failed to download {item.FileName}:\n{ex.Message}",
                        "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    progressWindow.Close();
                }
            }

            WpfMessageBox.Show(
                $"Done.\n\nDownloaded: {downloaded}  |  Already up-to-date: {skipped}  |  Failed: {failed}",
                "Gameplay Fixes — The Sims 3",
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
        yield return ("Base Game", new List<GameplayFixItem>
        {
            new("Sims Need Less Space by OhRudi",                   "SimTools/Packages/OhRudi__BaseGame__Sims_need_less_Space.package",                          "%baseurl%/Mods/Sims3/packages/base/OhRudi__BaseGame__Sims_need_less_Space.package"),
            new("Only Important Memories by VelocityGrass",         "SimTools/Packages/velocitygrass_only_important_memories.package",                          "%baseurl%/Mods/Sims3/packages/base/velocitygrass_only_important_memories.package"),
            new("Tileable Items Shader Fix by Simsi45",             "SimTools/Packages/BG_Tileable_Items_Shader_FIXED.package",                                 "%baseurl%/Mods/Sims3/packages/base/BG_Tileable_Items_Shader_FIXED.package"),
            new("Interact on Sloped Terrain by Nikel23",            "SimTools/Packages/Nikel23 - Interact on sloped terrain.package",                           "%baseurl%/Mods/Sims3/packages/base/Nikel23 - Interact on sloped terrain.package"),
            new("Half Wall Fix by Simsi45",                         "SimTools/Packages/Half walls fixed - all.package",                                         "%baseurl%/Mods/Sims3/packages/base/Half walls fixed - all.package"),
            new("Eyeball UV Fix by S-Club",                         "SimTools/Packages/S-Club ts3 mod EA Eyeball F UVFix.package",                              "%baseurl%/Mods/Sims3/packages/base/S-Club ts3 mod EA Eyeball F UVFix.package"),
            new("EA Eyeshadow Fix by Lavsm",                        "SimTools/Packages/EyeshadowAlphaFix_BaseGame_DefaultReplacement.package",                  "%baseurl%/Mods/Sims3/packages/base/EyeshadowAlphaFix_BaseGame_DefaultReplacement.package"),
            new("Muscle Slider Fix by Nysha",                       "SimTools/Packages/whiteriderMTS_LNMuscleSliderNudeFix.package",                            "%baseurl%/Mods/Sims3/packages/base/whiteriderMTS_LNMuscleSliderNudeFix.package"),
            new("Get To Know Fix by SimBouquet",                    "SimTools/Packages/simbouquet_GetToKnowFix.package",                                        "%baseurl%/Mods/Sims3/packages/base/simbouquet_GetToKnowFix.package",
                "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first."),
            new("Get To Know Fix Utils by SimBouquet",              "SimTools/Packages/simbouquet_Utils.package",                                               "%baseurl%/Mods/Sims3/packages/base/simbouquet_Utils.package",
                "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first."),
            new("Welcome Matt De-Shined by CeltySims",              "SimTools/Packages/celtysimsWelcomeMattdeshined.package",                                   "%baseurl%/Mods/Sims3/packages/base/celtysimsWelcomeMattdeshined.package"),
            new("Horizontal Clapboard Fix by CircusWolf",           "SimTools/Packages/CW_HorizontalClapboardFixed.package",                                    "%baseurl%/Mods/Sims3/packages/base/CW_HorizontalClapboardFixed.package"),
            new("No Auto Placing Community Lots by Bluegenjutsu",   "SimTools/Packages/bluegenjutsu_NoAutoPlacingCommunityLots.package",                        "%baseurl%/Mods/Sims3/packages/base/bluegenjutsu_NoAutoPlacingCommunityLots.package",
                "CAUTION: No Auto Placing Community Lots will disable the script which places lots such as the Fire Station, Salon, Laundromat, etc etc. If you like these lots, do not install this package. This mod will also require at least one of these EP's: Ambitions, Showtime, Supernatural or Seasons. If you do not have any of these EP's, you can safely skip this mod."),
            new("Atomic Age Stair Fix by EnableLlamas",             "SimTools/Packages/enablellamasAtomicAgeStairsFixDR.package",                               "%baseurl%/Mods/Sims3/packages/base/enablellamasAtomicAgeStairsFixDR.package"),
            new("Walk Cycle Edits by SimBouquet",                   "SimTools/Overrides/simbouquet_OVERRIDE_WalkCycleEdits.package",                            "%baseurl%/Mods/Sims3/overrides/base/simbouquet_OVERRIDE_WalkCycleEdits.package"),
            new("Medieval Facial Expressions by SimBouquet",        "SimTools/Overrides/simbouquet_OVERRIDE_TSMtoTS3_FacialExpressions.package",                "%baseurl%/Mods/Sims3/overrides/base/simbouquet_OVERRIDE_TSMtoTS3_FacialExpressions.package"),
            new("Random Sim Fixes by LazyDuchess",                  "SimTools/Packages/ld_RandomSimFixes.package",                                              "%baseurl%/Mods/Sims3/packages/base/ld_RandomSimFixes.package"),
            new("Sim Bin Genetics Male Presets by Anime_Boom",      "SimTools/Packages/SimBinYAAMPresets.package",                                              "%baseurl%/Mods/Sims3/packages/base/SimBinYAAMPresets.package"),
            new("Sim Bin Genetics Female Presets by Anime_Boom",    "SimTools/Packages/SimBinYAFAFPresets.package",                                             "%baseurl%/Mods/Sims3/packages/base/SimBinYAFAFPresets.package"),
            new("Pick Up Toddler Fix by TheSweetSimmer",            "SimTools/Packages/TSS_PickUpToddlerFix.package",                                           "%baseurl%/Mods/Sims3/packages/base/TSS_PickUpToddlerFix.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45",      "SimTools/Packages/BASE GAME - Stencils Unlocked.package",                                  "%baseurl%/Mods/Sims3/packages/base/BASE GAME - Stencils Unlocked.package"),
            new("Fishing Box Fix by NanaBx3",                       "SimTools/Packages/NanaBx3_fishingBoxChest_Fix.package",                                   "%baseurl%/Mods/Sims3/packages/base/NanaBx3_fishingBoxChest_Fix.package"),
            new("Cross-Eye Fix by LazyDuchess",                     "SimTools/Packages/ld_CrossEyeFix.package",                                                "%baseurl%/Mods/Sims3/packages/base/ld_CrossEyeFix.package"),
        });

        // ── World Adventures (6 items) ────────────────────────────────────────
        yield return ("World Adventures", new List<GameplayFixItem>
        {
            new("Sims Need Less Space by OhRudi",                              "SimTools/Packages/OhRudi__WorldAdventures__Sims_need_less_Space.package",                     "%baseurl%/Mods/Sims3/packages/world_adventures/OhRudi__WorldAdventures__Sims_need_less_Space.package"),
            new("Champs les Sims Distant Terrain Tree Fix by PotatoBalladSims","SimTools/Packages/PotatoBalladSims_terraindistantFrance_FIX.package",                         "%baseurl%/Mods/Sims3/packages/world_adventures/PotatoBalladSims_terraindistantFrance_FIX.package"),
            new("European Steam Train Fix by PotatoBalladSims",                "SimTools/Packages/PotatoBalladSims_European_Steam_Train.package",                             "%baseurl%/Mods/Sims3/packages/world_adventures/PotatoBalladSims_European_Steam_Train.package"),
            new("Tileable Items Shader Fix by Simsi45",                        "SimTools/Packages/WORLD ADVENTURES - Tileable_Items_Shader_FIXED.package",                   "%baseurl%/Mods/Sims3/packages/world_adventures/WORLD ADVENTURES - Tileable_Items_Shader_FIXED.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45",                 "SimTools/Packages/WORLD ADVENTURES - Unlocked Stencils.package",                             "%baseurl%/Mods/Sims3/packages/world_adventures/WORLD ADVENTURES - Unlocked Stencils.package"),
            new("Asian Window Reflects Light Fix by OhRudi",                   "SimTools/Packages/OhRudi__WorldAdventures__asian_window_fix__king_qings_window.package",      "%baseurl%/Mods/Sims3/packages/world_adventures/OhRudi__WorldAdventures__asian_window_fix__king_qings_window.package"),
        });

        // ── High End Loft Stuff (1 item) ──────────────────────────────────────
        yield return ("High End Loft Stuff", new List<GameplayFixItem>
        {
            new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/HELS - Tileable_Items_Shader_FIXED.package", "%baseurl%/Mods/Sims3/packages/high_end_loft_stuff/HELS - Tileable_Items_Shader_FIXED.package"),
        });

        // ── Ambitions (5 items) ───────────────────────────────────────────────
        yield return ("Ambitions", new List<GameplayFixItem>
        {
            new("Harvester Fix by Fantuanss12",                "SimTools/Packages/Fantuanss12_Harverster_TempFix.package",                         "%baseurl%/Mods/Sims3/packages/ambitions/Fantuanss12_Harverster_TempFix.package"),
            new("No Magic Clothesline Fix by Gamefreak130",    "SimTools/Packages/Gamefreak130_NoMagicClothesline.package",                        "%baseurl%/Mods/Sims3/packages/ambitions/Gamefreak130_NoMagicClothesline.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/AMBITIONS - Unlocked Stencils.package",                          "%baseurl%/Mods/Sims3/packages/ambitions/AMBITIONS - Unlocked Stencils.package"),
            new("EA Eyeshadow Fix by Lavsm",                   "SimTools/Packages/EyeshadowAlphaFix_Ambitions_DefaultReplacement.package",         "%baseurl%/Mods/Sims3/packages/ambitions/EyeshadowAlphaFix_Ambitions_DefaultReplacement.package"),
            new("Sims Need Less Space by OhRudi",              "SimTools/Packages/OhRudi__Ambitions__Sims_need_less_Space.package",                "%baseurl%/Mods/Sims3/packages/ambitions/OhRudi__Ambitions__Sims_need_less_Space.package"),
        });

        // ── Late Night (8 items) ──────────────────────────────────────────────
        yield return ("Late Night", new List<GameplayFixItem>
        {
            new("Celeb Fridge Texture Fix by EnableLlamas",        "SimTools/Packages/enablellamasRefrigeratorCelebSpecFix.package",           "%baseurl%/Mods/Sims3/packages/late_night/enablellamasRefrigeratorCelebSpecFix.package"),
            new("Enable Crane (Medium) in buydebug by Armiel",     "SimTools/Packages/armiel_craneMedium.package",                            "%baseurl%/Mods/Sims3/packages/late_night/armiel_craneMedium.package"),
            new("Enable Crane (Large) in buydebug by Armiel",      "SimTools/Packages/armiel_craneLarge.package",                             "%baseurl%/Mods/Sims3/packages/late_night/armiel_craneLarge.package"),
            new("Late Night Plant Fixes by Robodl95",               "SimTools/Packages/Robodl95_ LN Plant fix.package",                        "%baseurl%/Mods/Sims3/packages/late_night/Robodl95_ LN Plant fix.package"),
            new("Bridgeport Workbench Fix by DividingByZero",       "SimTools/Packages/Bridgeport Workbench Fix.package",                      "%baseurl%/Mods/Sims3/packages/late_night/Bridgeport Workbench Fix.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45",     "SimTools/Packages/LATE NIGHT - Unlocked Stencils.package",                "%baseurl%/Mods/Sims3/packages/late_night/LATE NIGHT - Unlocked Stencils.package"),
            new("Tileable Items Shader Fix by Simsi45",             "SimTools/Packages/LATE NIGHT - Tileable_Items_Shader_FIXED.package",      "%baseurl%/Mods/Sims3/packages/late_night/LATE NIGHT - Tileable_Items_Shader_FIXED.package"),
            new("Sims Need Less Space by OhRudi",                   "SimTools/Packages/OhRudi__LateNight__Sims_need_less_Space.package",       "%baseurl%/Mods/Sims3/packages/late_night/OhRudi__LateNight__Sims_need_less_Space.package"),
        });

        // ── Generations (7 items) ─────────────────────────────────────────────
        yield return ("Generations", new List<GameplayFixItem>
        {
            new("Generations Shirt & Sweater Top - Channel Fix by sweetdevil","SimTools/Packages/sweetdevil_GENShirtSweaterTopChannelFix_TAM_DR.package",     "%baseurl%/Mods/Sims3/packages/generations/sweetdevil_GENShirtSweaterTopChannelFix_TAM_DR.package"),
            new("Teen Fantasy Painting Fix by ThomasRiordan",       "SimTools/Packages/PTS3_ThomasRiordan_paintingGenTeenFantasyFixed.package",          "%baseurl%/Mods/Sims3/packages/generations/PTS3_ThomasRiordan_paintingGenTeenFantasyFixed.package"),
            new("Awkward Family Photo Fixed by ThomasRiordan",      "SimTools/Packages/PTS3_ThomasRiordan_paintingGenAwkwardFamilyPhotoFixed.package",   "%baseurl%/Mods/Sims3/packages/generations/PTS3_ThomasRiordan_paintingGenAwkwardFamilyPhotoFixed.package"),
            new("Read Toddler to Sleep Fix by Danjaley",            "SimTools/Packages/danjaley_read2sleepfix.package",                                 "%baseurl%/Mods/Sims3/packages/generations/danjaley_read2sleepfix.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45",      "SimTools/Packages/GENERATIONS - Unlocked Stencils.package",                        "%baseurl%/Mods/Sims3/packages/generations/GENERATIONS - Unlocked Stencils.package"),
            new("Tileable Items Shader Fix by Simsi45",             "SimTools/Packages/DECADES -Tileable_Items_Shader_FIXED.package",                   "%baseurl%/Mods/Sims3/packages/generations/DECADES -Tileable_Items_Shader_FIXED.package"),
            new("Sims Need Less Space by OhRudi",                   "SimTools/Packages/OhRudi__Generations__Sims_need_less_Space.package",              "%baseurl%/Mods/Sims3/packages/generations/OhRudi__Generations__Sims_need_less_Space.package"),
        });

        // ── Pets (6 items) ────────────────────────────────────────────────────
        yield return ("Pets", new List<GameplayFixItem>
        {
            new("Pets Need Less Space by OhRudi",              "SimTools/Packages/OhRudi__Routing Fix__Pets_need_less_space.package",      "%baseurl%/Mods/Sims3/packages/pets/OhRudi__Routing Fix__Pets_need_less_space.package"),
            new("Gallop Faster Animation Fix by Shimrod101",   "SimTools/Packages/ShimrodsAnimHorseGallopFastestFix.package",             "%baseurl%/Mods/Sims3/packages/pets/ShimrodsAnimHorseGallopFastestFix.package"),
            new("Pet Tombstone Shadow Fix by MenaceMan44",     "SimTools/Packages/MM_PetTombstoneShadowFix.package",                      "%baseurl%/Mods/Sims3/packages/pets/MM_PetTombstoneShadowFix.package"),
            new("Horse Tail Fixes by Simsi45",                 "SimTools/Packages/Simsi45_Horse_Braided_Tail_NoRandom.package",           "%baseurl%/Mods/Sims3/packages/pets/Simsi45_Horse_Braided_Tail_NoRandom.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/PETS - Unlocked Stencils.package",                     "%baseurl%/Mods/Sims3/packages/pets/PETS - Unlocked Stencils.package"),
            new("Tileable Items Shader Fix by Simsi45",        "SimTools/Packages/PETS - Tileable_Items_Shader_FIXED.package",            "%baseurl%/Mods/Sims3/packages/pets/PETS - Tileable_Items_Shader_FIXED.package"),
        });

        // ── Master Suite Stuff (1 item) ───────────────────────────────────────
        yield return ("Master Suite Stuff", new List<GameplayFixItem>
        {
            new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/MASTER SUITE STUFF - Unlocked Stencils.package", "%baseurl%/Mods/Sims3/packages/master_suite_stuff/MASTER SUITE STUFF - Unlocked Stencils.package"),
        });

        // ── Showtime (4 items) ────────────────────────────────────────────────
        yield return ("Showtime", new List<GameplayFixItem>
        {
            new("No NPC's Performing For Tips by Spicsshane",  "SimTools/Packages/NoNPCsPerformingforTips.package",                      "%baseurl%/Mods/Sims3/packages/showtime/NoNPCsPerformingforTips.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/SHOWTIME - Unlocked Stencils.package",                 "%baseurl%/Mods/Sims3/packages/showtime/SHOWTIME - Unlocked Stencils.package"),
            new("Tileable Items Shader Fix by Simsi45",        "SimTools/Packages/SHOWTIME - Tileable_Items_Shader_FIXED.package",        "%baseurl%/Mods/Sims3/packages/showtime/SHOWTIME - Tileable_Items_Shader_FIXED.package"),
            new("Sims Need Less Space by OhRudi",              "SimTools/Packages/OhRudi__Showtime__Sims_need_less_Space.package",        "%baseurl%/Mods/Sims3/packages/showtime/OhRudi__Showtime__Sims_need_less_Space.package"),
        });

        // ── Diesel Stuff (1 item) ─────────────────────────────────────────────
        yield return ("Diesel Stuff", new List<GameplayFixItem>
        {
            new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/DIESEL - Unlocked Stencils.package", "%baseurl%/Mods/Sims3/packages/diesel_stuff/DIESEL - Unlocked Stencils.package"),
        });

        // ── Supernatural (8 items) ────────────────────────────────────────────
        yield return ("Supernatural", new List<GameplayFixItem>
        {
            new("A Proper Fix for A Proper Trait by SpotlessLeopard","SimTools/Packages/ProperTraitFix_[spotlessleopard].package",          "%baseurl%/Mods/Sims3/packages/supernatural/ProperTraitFix_[spotlessleopard].package"),
            new("Restoration Spell Fix by Arsil",               "SimTools/Packages/Arsil_RestorationSpellFix.package",                    "%baseurl%/Mods/Sims3/packages/supernatural/Arsil_RestorationSpellFix.package"),
            new("Money Tree Bug Fix by Chicken0895",            "SimTools/Packages/Chicken0895 Money Tree Bug Fix.package",                "%baseurl%/Mods/Sims3/packages/supernatural/Chicken0895 Money Tree Bug Fix.package"),
            new("No More Zombies Generated At Full Moon by PersonCalledJoy","SimTools/Packages/ByeByeZombie.package",                      "%baseurl%/Mods/Sims3/packages/supernatural/ByeByeZombie.package"),
            new("EA Eyeshadow Fix by Lavsm",                    "SimTools/Packages/EyeshadowAlphaFix_Supernatural_DefaultReplacement.package","%baseurl%/Mods/Sims3/packages/supernatural/EyeshadowAlphaFix_Supernatural_DefaultReplacement.package"),
            new("Tileable Items Shader Fix by Simsi45",         "SimTools/Packages/SUPERNATURAL - Tileable_Items_Shader_FIXED.package",   "%baseurl%/Mods/Sims3/packages/supernatural/SUPERNATURAL - Tileable_Items_Shader_FIXED.package"),
            new("Sims Need Less Space by OhRudi",               "SimTools/Packages/OhRudi__Supernatural__Sims_need_less_Space.package",   "%baseurl%/Mods/Sims3/packages/supernatural/OhRudi__Supernatural__Sims_need_less_Space.package"),
            new("Motive Mobile Workaround by YamiTheDragon",    "SimTools/Overrides/YTD_MotiveMobile_NoHygiene.package",                  "%baseurl%/Mods/Sims3/overrides/supernatural/YTD_MotiveMobile_NoHygiene.package",
                "This fix presents a potential workaround to the Motive Mobile by greatly nerfing it so that it only fills Fun & Social. This will keep it from breaking needs that would have been made static by a sim becoming supernatural. Alternatively, you can simply avoid buying or using the Motive Mobile."),
        });

        // ── Seasons (6 items) ─────────────────────────────────────────────────
        yield return ("Seasons", new List<GameplayFixItem>
        {
            new("Faster Raking by Mikey",                       "SimTools/Packages/faster_raking.package",                             "%baseurl%/Mods/Sims3/packages/seasons/faster_raking.package"),
            new("Weather Forecast Tweaks by Gamefreak130",      "SimTools/Packages/Gamefreak130_WeatherForecastTweaks.package",         "%baseurl%/Mods/Sims3/packages/seasons/Gamefreak130_WeatherForecastTweaks.package"),
            new("Truly Eclipsing Fog by Gamefreak130",          "SimTools/Packages/Gamefreak130_TrulyEclipsingFog.package",            "%baseurl%/Mods/Sims3/packages/seasons/Gamefreak130_TrulyEclipsingFog.package"),
            new("Hidden Stencils Unlocked & Fixed by Simsi45",  "SimTools/Packages/SEASONS - Unlocked Stencils.package",              "%baseurl%/Mods/Sims3/packages/seasons/SEASONS - Unlocked Stencils.package"),
            new("Sims Need Less Space by OhRudi",               "SimTools/Packages/OhRudi__Seasons__Sims_need_less_Space.package",     "%baseurl%/Mods/Sims3/packages/seasons/OhRudi__Seasons__Sims_need_less_Space.package"),
            new("No More Ugly Snowprints by Lyralei",           "SimTools/Overrides/Lyralei - NoMoreUglySnowPrints.package",           "%baseurl%/Mods/Sims3/overrides/seasons/Lyralei - NoMoreUglySnowPrints.package"),
        });

        // ── 70s, 80s & 90s Stuff (1 item) ────────────────────────────────────
        yield return ("70s, 80s & 90s Stuff", new List<GameplayFixItem>
        {
            new("EA Eyeshadow Fix by Lavsm", "SimTools/Packages/EyeshadowAlphaFix_70s80s90sStuff_DefaultReplacement.package", "%baseurl%/Mods/Sims3/packages/70s80s90s/EyeshadowAlphaFix_70s80s90sStuff_DefaultReplacement.package"),
        });

        // ── University Life (7 items) ─────────────────────────────────────────
        yield return ("University Life", new List<GameplayFixItem>
        {
            new("University - More Shell Entries by AussomeDays",   "SimTools/Overrides/University - More Shell Entries.package",                   "%baseurl%/Mods/Sims3/overrides/university/University - More Shell Entries.package"),
            new("University - Replacement Shells by AussomeDays",   "SimTools/Overrides/University - Replacement Shells (overrides).package",       "%baseurl%/Mods/Sims3/overrides/university/University - Replacement Shells (overrides).package"),
            new("University Life Visual Fixes by SimBouquet",       "SimTools/Overrides/simbouquet_OVERRIDE_EP9visualfixes.package",                "%baseurl%/Mods/Sims3/overrides/university/simbouquet_OVERRIDE_EP9visualfixes.package"),
            new("No More University Protests by Don Babilon",       "SimTools/Packages/DB_ImprovedProtestSituation_NoNPCProtests.package",          "%baseurl%/Mods/Sims3/packages/university/DB_ImprovedProtestSituation_NoNPCProtests.package"),
            new("Alpha & Omega Door Fix by CeltySims",              "SimTools/Packages/AlphaOmegaDoorFixed.package",                               "%baseurl%/Mods/Sims3/packages/university/AlphaOmegaDoorFixed.package"),
            new("Tileable Items Shader Fix by Simsi45",             "SimTools/Packages/UNIVERSITY LIFE - Tileable_Items_FIXED.package",             "%baseurl%/Mods/Sims3/packages/university/UNIVERSITY LIFE - Tileable_Items_FIXED.package"),
            new("Sims Need Less Space by OhRudi",                   "SimTools/Packages/OhRudi__UniversityLife__Sims_need_less_Space.package",       "%baseurl%/Mods/Sims3/packages/university/OhRudi__UniversityLife__Sims_need_less_Space.package"),
        });

        // ── Island Paradise (3 items) ─────────────────────────────────────────
        yield return ("Island Paradise", new List<GameplayFixItem>
        {
            new("Hanging Lantern Fix by Heaven",        "SimTools/Packages/heaven_IPHangingLanternFix.package",                    "%baseurl%/Mods/Sims3/packages/island_paradise/heaven_IPHangingLanternFix.package"),
            new("Reduce Houseboat Lag by MaryDeHoyos",  "SimTools/Packages/StopAutopilot_Helm_Updated_03_29_22.package",           "%baseurl%/Mods/Sims3/packages/island_paradise/StopAutopilot_Helm_Updated_03_29_22.package"),
            new("Sims Need Less Space by OhRudi",       "SimTools/Packages/OhRudi__IslandParadise__Sims_need_less_Space.package",  "%baseurl%/Mods/Sims3/packages/island_paradise/OhRudi__IslandParadise__Sims_need_less_Space.package"),
        });

        // ── Into the Future (4 items) ─────────────────────────────────────────
        yield return ("Into the Future", new List<GameplayFixItem>
        {
            new("Watch Laser Rhythm-A-Con Fix by SimsAddict777","SimTools/Packages/MTS_simsaddict777_lazerharp_FIX.package",        "%baseurl%/Mods/Sims3/packages/into_the_future/MTS_simsaddict777_lazerharp_FIX.package"),
            new("Perigree Planter Fix by Heaven",               "SimTools/Packages/heaven_fenceFuturePlanterFix.package",           "%baseurl%/Mods/Sims3/packages/into_the_future/heaven_fenceFuturePlanterFix.package"),
            new("Tileable Items Shader Fix by Simsi45",         "SimTools/Packages/INTO THE FUTURE - Tileable_Items_FIXED.package", "%baseurl%/Mods/Sims3/packages/into_the_future/INTO THE FUTURE - Tileable_Items_FIXED.package"),
            new("Sims Need Less Space by OhRudi",               "SimTools/Packages/OhRudi__IntoTheFuture__Sims_need_less_Space.package","%baseurl%/Mods/Sims3/packages/into_the_future/OhRudi__IntoTheFuture__Sims_need_less_Space.package"),
        });

        // ── Store Fixes (5 items) ─────────────────────────────────────────────
        yield return ("Store Fixes", new List<GameplayFixItem>
        {
            new("Armoure Perfume Moodlet Fix by Gamefreak130", "SimTools/Packages/Gamefreak130_PerfumeMoodletFix.package.package",           "%baseurl%/Mods/Sims3/packages/store/Gamefreak130_PerfumeMoodletFix.package"),
            new("Banquet Fan Routing Fix by OmegaStarr82",     "SimTools/Packages/BanquetFanFix.package",                                   "%baseurl%/Mods/Sims3/packages/store/BanquetFanFix.package"),
            new("More Efficient Upgrade Name Fix by Buzzler",  "SimTools/Packages/Buzz_MoreEfficientUpgradeNameFix.package",                 "%baseurl%/Mods/Sims3/packages/store/Buzz_MoreEfficientUpgradeNameFix.package"),
            new("Haute Hacienda Fireplace Fix by Qahne",       "SimTools/Packages/Qahne_MOD_HauteHaciendaFireplace Fixes.package",           "%baseurl%/Mods/Sims3/packages/store/Qahne_MOD_HauteHaciendaFireplace Fixes.package"),
            new("Now & Then Staircase Fix by Simsi45",         "SimTools/Packages/Simsi45_Curved_Staircase_FIX_RECAT.package",               "%baseurl%/Mods/Sims3/packages/store/Simsi45_Curved_Staircase_FIX_RECAT.package"),
        });
    }
}
