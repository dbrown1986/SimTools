using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WpfButton   = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfColor    = System.Windows.Media.Color;
using WpfBrush    = System.Windows.Media.SolidColorBrush;
using WpfBrushes  = System.Windows.Media.Brushes;
using MessageBox     = System.Windows.MessageBox;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfGroupBox    = System.Windows.Controls.GroupBox;
using WpfHAlign      = System.Windows.HorizontalAlignment;
using WpfBrushBase   = System.Windows.Media.Brush;
using WpfFontFamily  = System.Windows.Media.FontFamily;

namespace SimTools
{
    /// <summary>
    /// Gameplay Fixes AIO — presents 13 expansion-grouped sections of
    /// .package fixes to install into the user's Sims 3 Mods folder.
    ///
    /// Each <see cref="GameplayFixItem"/> has:
    ///   DisplayName  — shown as the CheckBox label
    ///   FileName     — destination filename inside %Sims3Mods%
    ///   Url          — download source (supports %baseurl% placeholder)
    ///
    /// Items whose FileName / Url are empty strings are placeholder "TBD"
    /// rows.  They are rendered as disabled (grey) checkboxes and are
    /// silently skipped on download.
    ///
    /// To populate a section, find its block in BuildSections() and replace
    /// the TBD entries (or uncomment the TEMPLATE line to add more items).
    ///
    /// Download logic per item:
    ///   1. Check that _sims3Mods is still configured.
    ///   2. If the file already exists, send a HEAD request and compare
    ///      Last-Modified vs the local file's write time (5-second tolerance).
    ///   3. Download only if new or updated; skip silently if current.
    ///   4. Stamp the downloaded file with the server's Last-Modified time.
    /// </summary>
    public sealed class GameplayFixesWindow : Window
    {
        // ── Data model ────────────────────────────────────────────────────────
        private sealed record GameplayFixItem(string DisplayName, string FileName, string Url, string OnCheckedMessage = "");

        // ── State ─────────────────────────────────────────────────────────────
        private readonly string _sims3Mods;

        // Parallel collections built by BuildSection() — index correspondence
        // is guaranteed because both are populated in the same foreach loop.
        private readonly List<WpfCheckBox>     _allCheckBoxes = new();
        private readonly List<GameplayFixItem> _allItems      = new();

        // ── Constructor ───────────────────────────────────────────────────────
        public GameplayFixesWindow(string sims3Mods)
        {
            _sims3Mods = sims3Mods;

            Title                 = "Gameplay Fixes AIO — The Sims 3";
            Width                 = 720;
            Height                = 660;
            ResizeMode            = ResizeMode.NoResize;
            WindowStyle           = WindowStyle.ToolWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background            = new WpfBrush(WpfColor.FromRgb(26, 26, 26));

            Content = BuildLayout();
        }

        // ══════════════════════════════════════════════════════════════════════
        // SECTION DEFINITIONS
        // ══════════════════════════════════════════════════════════════════════
        // Each section is a named group of GameplayFixItem records.
        // - Items with non-empty FileName + Url are active (white, checkable).
        // - Items with empty FileName / Url are TBD placeholders (grey, disabled).
        //
        // TEMPLATE for adding a new item to any section — copy, uncomment, fill in:
        //   new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
        // ──────────────────────────────────────────────────────────────────────
        private IEnumerable<(string Header, List<GameplayFixItem> Items)> BuildSections()
        {
            // ── Base Game (23 items) ───────────────────────────────
            yield return ("Base Game", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__BaseGame__Sims_need_less_Space.package", "%baseurl%/Mods/Sims3/packages/base/OhRudi__BaseGame__Sims_need_less_Space.package"),
                new("Only Important Memories by VelocityGrass", "SimTools/Packages/velocitygrass_only_important_memories.package", "%baseurl%/Mods/Sims3/packages/base/velocitygrass_only_important_memories.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/BG_Tileable_Items_Shader_FIXED.package", "%baseurl%/Mods/Sims3/packages/base/BG_Tileable_Items_Shader_FIXED.package"),
                new("Interact on Sloped Terrain by Nikel23", "SimTools/Packages/Nikel23 - Interact on sloped terrain.package", "%baseurl%/Mods/Sims3/packages/base/Nikel23 - Interact on sloped terrain.package"),
                new("Half Wall Fix by Simsi45", "SimTools/Packages/Half walls fixed - all.package", "%baseurl%/Mods/Sims3/packages/base/Half walls fixed - all.package"),
                new("Eyeball UV Fix by S-Club", "SimTools/Packages/S-Club ts3 mod EA Eyeball F UVFix.package", "%baseurl%/Mods/Sims3/packages/base/S-Club ts3 mod EA Eyeball F UVFix.package"),
                new("EA Eyeshadow Fix by Lavsm", "SimTools/Packages/EyeshadowAlphaFix_BaseGame_DefaultReplacement.package", "%baseurl%/Mods/Sims3/packages/base/EyeshadowAlphaFix_BaseGame_DefaultReplacement.package"),
                new("Muscle Slider Fix by Nysha", "SimTools/Packages/whiteriderMTS_LNMuscleSliderNudeFix.package", "%baseurl%/Mods/Sims3/packages/base/whiteriderMTS_LNMuscleSliderNudeFix.package"),
                new("Get To Know Fix by SimBouquet", "SimTools/Packages/simbouquet_GetToKnowFix.package", "%baseurl%/Mods/Sims3/packages/base/simbouquet_GetToKnowFix.package", "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first."),
                new("Get To Know Fix Utils by SimBouquet", "SimTools/Packages/simbouquet_Utils.package", "%baseurl%/Mods/Sims3/packages/base/simbouquet_Utils.package", "This mod requires Mono Patcher by LazyDuchess to be installed. Please be sure you have installed it from the Bugfix menu first."),
                new("Welcome Matt De-Shined by CeltySims", "SimTools/Packages/celtysimsWelcomeMattdeshined.package", "%baseurl%/Mods/Sims3/packages/base/celtysimsWelcomeMattdeshined.package"),
                new("Horizontal Clapboard Fix by CircusWolf", "SimTools/Packages/CW_HorizontalClapboardFixed.package", "%baseurl%/Mods/Sims3/packages/base/CW_HorizontalClapboardFixed.package"),
                new("No Auto Placing Community Lots by Bluegenjutsu", "SimTools/Packages/bluegenjutsu_NoAutoPlacingCommunityLots.package", "%baseurl%/Mods/Sims3/packages/base/bluegenjutsu_NoAutoPlacingCommunityLots.package", "This mod requires at least one of these EP's: Ambitions, Showtime, Supernatural or Seasons. If you do not have any of these EP's, you can safely skip this mod."),
                new("Atomic Age Stair Fix by EnableLlamas", "SimTools/Packages/enablellamasAtomicAgeStairsFixDR.package", "%baseurl%/Mods/Sims3/packages/base/enablellamasAtomicAgeStairsFixDR.package"),
                new("Walk Cycle Edits by SimBouquet", "SimTools/Overrides/simbouquet_OVERRIDE_WalkCycleEdits.package", "%baseurl%/Mods/Sims3/overrides/base/simbouquet_OVERRIDE_WalkCycleEdits.package"),
                new("Medieval Facial Expressions by SimBouquet", "SimTools/Overrides/simbouquet_OVERRIDE_TSMtoTS3_FacialExpressions.package", "%baseurl%/Mods/Sims3/overrides/base/simbouquet_OVERRIDE_TSMtoTS3_FacialExpressions.package"),
                new("Random Sim Fixes by LazyDuchess", "SimTools/Packages/ld_RandomSimFixes.package", "baseurl%/Mods/Sims3/packages/base/ld_RandomSimFixes.package"),
                new("Sim Bin Genetics Male Presets by Anime_Boom", "SimTools/Packages/SimBinYAAMPresets.package", "baseurl%/Mods/Sims3/packages/base/SimBinYAAMPresets.package"),
                new("Sim Bin Genetics Female Presets by Anime_Boom", "SimTools/Packages/SimBinYAFAFPresets.package", "baseurl%/Mods/Sims3/packages/base/SimBinYAFAFPresets.package"),
                new("Pick Up Toddler Fix by TheSweetSimmer", "SimTools/Packages/TSS_PickUpToddlerFix.package", "baseurl%/Mods/Sims3/packages/base/TSS_PickUpToddlerFix.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/BASE GAME - Stencils Unlocked.package", "baseurl%/Mods/Sims3/packages/base/BASE GAME - Stencils Unlocked.package"),
                new("Fishing Box Fix by NanaBx3", "SimTools/Packages/NanaBx3_fishingBoxChest_Fix.package", "baseurl%/Mods/Sims3/packages/base/NanaBx3_fishingBoxChest_Fix.package"),
                new("Cross-Eye Fix by LazyDuchess", "SimTools/Packages/ld_CrossEyeFix.package", "baseurl%/Mods/Sims3/packages/base/ld_CrossEyeFix.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/base/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/base/*.package"),
            });

            // ── World Adventures (6 items) ─────────────────────────
            yield return ("World Adventures", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__WorldAdventures__Sims_need_less_Space.package", "baseurl%/Mods/Sims3/packages/world_adventures/OhRudi__WorldAdventures__Sims_need_less_Space.package"),
                new("Champs les Sims Distant Terrain Tree Fix by PotatoBalladSims", "SimTools/Packages/PotatoBalladSims_terraindistantFrance_FIX.package", "baseurl%/Mods/Sims3/packages/world_adventures/PotatoBalladSims_terraindistantFrance_FIX.package"),
                new("European Steam Train Fix by PotatoBalladSims", "SimTools/Packages/PotatoBalladSims_European_Steam_Train.package", "baseurl%/Mods/Sims3/packages/world_adventures/PotatoBalladSims_European_Steam_Train.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/WORLD ADVENTURES - Tileable_Items_Shader_FIXED.package", "baseurl%/Mods/Sims3/packages/world_adventures/WORLD ADVENTURES - Tileable_Items_Shader_FIXED.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/WORLD ADVENTURES - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/world_adventures/WORLD ADVENTURES - Unlocked Stencils.package"),
                new("Asian Window Reflects Light Fix by OhRudi", "SimTools/Packages/OhRudi__WorldAdventures__asian_window_fix__king_qings_window.package", "baseurl%/Mods/Sims3/packages/world_adventures/OhRudi__WorldAdventures__asian_window_fix__king_qings_window.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/world_adventures/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/world_adventures/*.package"),
            });

            // ── High-End Loft Stuff (1 item) ────────────────────────────────
            yield return ("High End Loft Stuff", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/HELS - Tileable_Items_Shader_FIXED.package", "baseurl%/Mods/Sims3/packages/high_end_loft_stuff/HELS - Tileable_Items_Shader_FIXED.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/high_end_loft_stuff/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/high_end_loft_stuff/*.package"),
            });

            // ── Ambitions (5 items) ────────────────────────────────
            yield return ("Ambitions", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Harvester Fix by Fantuanss12", "SimTools/Packages/Fantuanss12_Harverster_TempFix.package", "baseurl%/Mods/Sims3/packages/ambitions/Fantuanss12_Harverster_TempFix.package"),
                new("No Magic Clothesline Fix by Gamefreak130", "SimTools/Packages/Gamefreak130_NoMagicClothesline.package", "baseurl%/Mods/Sims3/packages/ambitions/Gamefreak130_NoMagicClothesline.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/AMBITIONS - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/ambitions/AMBITIONS - Unlocked Stencils.package"),
                new("EA Eyeshadow Fix by Lavsm", "SimTools/Packages/EyeshadowAlphaFix_Ambitions_DefaultReplacement.package", "baseurl%/Mods/Sims3/packages/ambitions/EyeshadowAlphaFix_Ambitions_DefaultReplacement.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__Ambitions__Sims_need_less_Space.package", "baseurl%/Mods/Sims3/packages/ambitions/OhRudi__Ambitions__Sims_need_less_Space.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/ambitions/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/ambitions/*.package"),
            });

            // There are currently no Fast Lane Stuff fixes, but this is here as a placeholder.
            // ── Fast Lane Stuff (0 items) ───────────────────────────────
            // yield return ("Fast Lane Stuff", new List<GameplayFixItem>
            // {
            // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
            // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/fast_lane_stuff/*.package"),
            // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/fast_lane_stuff/*.package"),
            // });

            // ── Late Night (8 items) ───────────────────────────────
            yield return ("Late Night", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Celeb Fridge Texture Fix by EnableLlamas", "SimTools/Packages/enablellamasRefrigeratorCelebSpecFix.package", "baseurl%/Mods/Sims3/packages/late_night/enablellamasRefrigeratorCelebSpecFix.package"),
                new("Enable Crane (Medium) in buydebug by Armiel", "SimTools/Packages/armiel_craneMedium.package", "baseurl%/Mods/Sims3/packages/late_night/armiel_craneMedium.package"),
                new("Enable Crane (Large) in buydebug by Armiel", "SimTools/Packages/armiel_craneLarge.package", "baseurl%/Mods/Sims3/packages/late_night/armiel_craneLarge.package"),
                new("Late Night Plant Fixes by Robodl95", "SimTools/Packages/Robodl95_ LN Plant fix.package", "baseurl%/Mods/Sims3/packages/late_night/Robodl95_ LN Plant fix.package"),
                new("Bridgeport Workbench Fix by DividingByZero", "SimTools/Packages/Bridgeport Workbench Fix.package", "baseurl%/Mods/Sims3/packages/late_night/Bridgeport Workbench Fix.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/LATE NIGHT - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/late_night/LATE NIGHT - Unlocked Stencils.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/LATE NIGHT - Tileable_Items_Shader_FIXED.package", "baseurl%/Mods/Sims3/packages/late_night/LATE NIGHT - Tileable_Items_Shader_FIXED.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__LateNight__Sims_need_less_Space.package", "baseurl%/Mods/Sims3/packages/late_night/OhRudi__LateNight__Sims_need_less_Space.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/late_night/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/late_night/*.package"),
            });

            // There are currently no Outdoor Living Stuff fixes, but this is here as a placeholder.
            // ── Outdoor Living Stuff (0 items) ───────────────────────────────
            // yield return ("Outdoor Living Stuff", new List<GameplayFixItem>
            // {
            // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
            // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/outdoor_living_stuff/*.package"),
            // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/outdoor_living_stuff/*.package"),
            // });

            // ── Generations (6 items) ──────────────────────────────
            yield return ("Generations", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Generations Shirt & Sweater Top - Channel Fix by sweetdevil", "SimTools/Packages/sweetdevil_GENShirtSweaterTopChannelFix_TAM_DR.package", "baseurl%/Mods/Sims3/packages/generations/sweetdevil_GENShirtSweaterTopChannelFix_TAM_DR.package"),
                new("Teen Fantasy Painting Fix by ThomasRiordan", "SimTools/Packages/PTS3_ThomasRiordan_paintingGenTeenFantasyFixed.package", "baseurl%/Mods/Sims3/packages/generations/PTS3_ThomasRiordan_paintingGenTeenFantasyFixed.package"),
                new("Awkward Family Photo Fixed by ThomasRiordan", "SimTools/Packages/PTS3_ThomasRiordan_paintingGenAwkwardFamilyPhotoFixed.package", "baseurl%/Mods/Sims3/packages/generations/PTS3_ThomasRiordan_paintingGenAwkwardFamilyPhotoFixed.package"),
                new("Read Toddler to Sleep Fix by Danjaley", "SimTools/Packages/danjaley_read2sleepfix.package", "baseurl%/Mods/Sims3/packages/generations/danjaley_read2sleepfix.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/GENERATIONS - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/generations/GENERATIONS - Unlocked Stencils.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/DECADES -Tileable_Items_Shader_FIXED.package", "baseurl%/Mods/Sims3/packages/generations/DECADES -Tileable_Items_Shader_FIXED.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__Generations__Sims_need_less_Space.package", "baseurl%/Mods/Sims3/packages/generations/OhRudi__Generations__Sims_need_less_Space.package"),
             // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/generations/*.package"),
             // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/generations/*.package"),
            });

            // There are currently no Town Life Stuff fixes, but this is here as a placeholder.
            // ── Town Life Stuff (0 items) ───────────────────────────────
            // yield return ("Town Life Stuff", new List<GameplayFixItem>
            // {
            // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
            // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/town_life_stuff/*.package"),
            // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/town_life_stuff/*.package"),
            // });

            // ── Pets (6 items) ─────────────────────────────────────
            yield return ("Pets", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Pets Need Less Space by OhRudi", "SimTools/Packages/OhRudi__Routing Fix__Pets_need_less_space.package", "baseurl%/Mods/Sims3/packages/pets/OhRudi__Routing Fix__Pets_need_less_space.package"),
                new("Gallop Faster Animation Fix by Shimrod101", "SimTools/Packages/ShimrodsAnimHorseGallopFastestFix.package", "baseurl%/Mods/Sims3/packages/pets/ShimrodsAnimHorseGallopFastestFix.package"),
                new("Pet Tombstone Shadow Fix by MenaceMan44", "SimTools/Packages/MM_PetTombstoneShadowFix.package", "baseurl%/Mods/Sims3/packages/pets/MM_PetTombstoneShadowFix.package"),
                new("Horse Tail Fixes by Simsi45", "SimTools/Packages/Simsi45_Horse_Braided_Tail_NoRandom.package", "baseurl%/Mods/Sims3/packages/pets/Simsi45_Horse_Braided_Tail_NoRandom.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/PETS - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/pets/PETS - Unlocked Stencils.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/PETS - Tileable_Items_Shader_FIXED.package", "baseurl%/Mods/Sims3/packages/pets/PETS - Tileable_Items_Shader_FIXED.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/pets/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/pets/*.package"),
            });

            // ── Master Suite Stuff (1 item) ─────────────────────────────────────
            yield return ("Master Suite Stuff", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/MASTER SUITE STUFF - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/master_suite_stuff/MASTER SUITE STUFF - Unlocked Stencils.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/master_suite_stuff/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/master_suite_stuff/*.package"),
            });

            // ── Showtime / Showtime Collectors Edition (1 item) ──────────────────────────────────
            yield return ("Showtime", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("No NPC's Performing For Tips by Spicsshane", "SimTools/Packages/NoNPCsPerformingforTips.package", "baseurl%/Mods/Sims3/packages/showtime/NoNPCsPerformingforTips.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/SHOWTIME - Unlocked Stencils.package", "baseurl%/Mods/Sims3/packages/showtime/SHOWTIME - Unlocked Stencils.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/SHOWTIME - Tileable_Items_Shader_FIXED.package", "baseurl%/Mods/Sims3/packages/showtime/SHOWTIME - Tileable_Items_Shader_FIXED.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__Showtime__Sims_need_less_Space.package", "baseurl%/Mods/Sims3/packages/showtime/OhRudi__Showtime__Sims_need_less_Space.package"),
                // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/showtime/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/showtime/*.package"),
            });

            // There are currently no Katy Perry Sweet Treat fixes, but this is here as a placeholder.
            // ── Town Life Stuff (0 items) ───────────────────────────────
            // yield return ("Town Life Stuff", new List<GameplayFixItem>
            // {
            // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
            // new("Name", "SimTools/Packages/*.package", "baseurl%/Mods/Sims3/packages/kpst/*.package"),
            // new("Name", "SimTools/Overrides/*.package", "baseurl%/Mods/Sims3/overrides/kpst/*.package"),
            // });

            // ── Diesel Stuff (1 item) ─────────────────────────────
            yield return ("Diesel Stuff", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/DIESEL - Unlocked Stencils.package", "%baseurl%/Mods/Sims3/packages/diesel_stuff/DIESEL - Unlocked Stencils.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/diesel_stuff/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/diesel_stuff/*.package"),
            });

            // ── Supernatural (8 items) ─────────────────────────────
            yield return ("Supernatural", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("A Proper Fix for A Proper Trait by SpotlessLeopard", "SimTools/Packages/ProperTraitFix_[spotlessleopard].package", "%baseurl%/Mods/Sims3/packages/supernatural/ProperTraitFix_[spotlessleopard].package"),
                new("Restoration Spell Fix by Arsil", "SimTools/Packages/Arsil_RestorationSpellFix.package", "%baseurl%/Mods/Sims3/packages/supernatural/Arsil_RestorationSpellFix.package"),
                new("Money Tree Bug Fix by Chicken0895", "SimTools/Packages/Chicken0895 Money Tree Bug Fix.package", "%baseurl%/Mods/Sims3/packages/supernatural/Chicken0895 Money Tree Bug Fix.package"),
                new("No More Zombies Generated At Full Moon by PersonCalledJoy", "SimTools/Packages/ByeByeZombie.package", "%baseurl%/Mods/Sims3/packages/supernatural/ByeByeZombie.package"),
                new("EA Eyeshadow Fix by Lavsm", "SimTools/Packages/EyeshadowAlphaFix_Supernatural_DefaultReplacement.package", "%baseurl%/Mods/Sims3/packages/supernatural/EyeshadowAlphaFix_Supernatural_DefaultReplacement.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/SUPERNATURAL - Tileable_Items_Shader_FIXED.package", "%baseurl%/Mods/Sims3/packages/supernatural/SUPERNATURAL - Tileable_Items_Shader_FIXED.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__Supernatural__Sims_need_less_Space.package", "%baseurl%/Mods/Sims3/packages/supernatural/OhRudi__Supernatural__Sims_need_less_Space.package"),
                new("Motive Mobile Workaround by YamiTheDragon", "SimTools/Overrides/YTD_MotiveMobile_NoHygiene.package", "%baseurl%/Mods/Sims3/overrides/supernatural/YTD_MotiveMobile_NoHygiene.package", "This fix presents a potential workaround to the Motive Mobile by greatly nerfing it so that it only fills Fun & Social. This will keep it from breaking needs that would have been made static by a sim becoming supernatural. Alternatively, you can simply avoid buying or using the Motive Mobile."),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/supernatural/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/supernatural/*.package"),
            });

            // ── Seasons (6 items) ──────────────────────────────────
            yield return ("Seasons", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Faster Raking by Mikey", "SimTools/Packages/faster_raking.package", "%baseurl%/Mods/Sims3/packages/seasons/faster_raking.package"),
                new("Weather Forecast Tweaks by Gamefreak130", "SimTools/Packages/Gamefreak130_WeatherForecastTweaks.package", "%baseurl%/Mods/Sims3/packages/seasons/Gamefreak130_WeatherForecastTweaks.package"),
                new("Truly Eclipsing Fog by Gamefreak130", "SimTools/Packages/Gamefreak130_TrulyEclipsingFog.package", "%baseurl%/Mods/Sims3/packages/seasons/Gamefreak130_TrulyEclipsingFog.package"),
                new("Hidden Stencils Unlocked & Fixed by Simsi45", "SimTools/Packages/SEASONS - Unlocked Stencils.package", "%baseurl%/Mods/Sims3/packages/seasons/SEASONS - Unlocked Stencils.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__Seasons__Sims_need_less_Space.package", "%baseurl%/Mods/Sims3/packages/seasons/OhRudi__Seasons__Sims_need_less_Space.package"),
                new("No More Ugly Snowprints by Lyralei", "SimTools/Overrides/Lyralei - NoMoreUglySnowPrints.package", "%baseurl%/Mods/Sims3/overrides/seasons/Lyralei - NoMoreUglySnowPrints.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/seasons/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/seasons/*.package"),
            });

            // ── 70s, 80s & 90s Stuff (1 item) ──────────────────────────────────
            yield return ("70s, 80s & 90s Stuff", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("EA Eyeshadow Fix by Lavsm", "SimTools/Packages/EyeshadowAlphaFix_70s80s90sStuff_DefaultReplacement.package", "%baseurl%/Mods/Sims3/packages/70s80s90s/EyeshadowAlphaFix_70s80s90sStuff_DefaultReplacement.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/70s80s90s/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/70s80s90s/*.package"),
            });

            // ── University Life (6 items) ─────────────────────────
            yield return ("University Life", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("University - More Shell Entries by AussomeDays", "SimTools/Overrides/University - More Shell Entries.package", "%baseurl%/Mods/Sims3/overrides/university/University - More Shell Entries.package"),
                new("University - Replacement Shells by AussomeDays", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/university/*.package"),
                new("University Life Visual Fixes by SimBouquet", "SimTools/Overrides/simbouquet_OVERRIDE_EP9visualfixes.package", "%baseurl%/Mods/Sims3/packages/university/simbouquet_OVERRIDE_EP9visualfixes.package"),
                new("No More University Protests by Don Babilon", "SimTools/Packages/DB_ImprovedProtestSituation_NoNPCProtests.package", "%baseurl%/Mods/Sims3/packages/university/DB_ImprovedProtestSituation_NoNPCProtests.package"),
                new("Alpha & Omega Door Fix by CeltySims", "SimTools/Packages/AlphaOmegaDoorFixed.package", "%baseurl%/Mods/Sims3/packages/university/AlphaOmegaDoorFixed.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/UNIVERSITY LIFE - Tileable_Items_FIXED.package", "%baseurl%/Mods/Sims3/packages/university/UNIVERSITY LIFE - Tileable_Items_FIXED.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__UniversityLife__Sims_need_less_Space.package", "%baseurl%/Mods/Sims3/packages/university/OhRudi__UniversityLife__Sims_need_less_Space.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/university/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/university/*.package"),
            });

            // ── Island Paradise (2 items) ─────────────────────────
            yield return ("Island Paradise", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Hanging Lantern Fix by Heaven", "SimTools/Packages/heaven_IPHangingLanternFix.package", "%baseurl%/Mods/Sims3/packages/island_paradise/heaven_IPHangingLanternFix.package"),
                new("Reduce Houseboat Lag by MaryDeHoyos", "SimTools/Packages/StopAutopilot_Helm_Updated_03_29_22.package", "%baseurl%/Mods/Sims3/packages/island_paradise/StopAutopilot_Helm_Updated_03_29_22.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__IslandParadise__Sims_need_less_Space.package", "%baseurl%/Mods/Sims3/packages/island_paradise/OhRudi__IslandParadise__Sims_need_less_Space.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/island_paradise/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/island_paradise/*.package"),
            });

            // There are currently no Movie Stuff fixes, but this is here as a placeholder.
            // ── Movie Stuff (2 items) ─────────────────────────
            // yield return ("Movie Stuff", new List<GameplayFixItem>
            // {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/movie_stuff/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/movie_stuff/*.package"),
            // });

            // ── Into the Future (4 items) ─────────────────────────
            yield return ("Into the Future", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Watch Laser Rhythm-A-Con Fix by SimsAddict777", "SimTools/Packages/MTS_simsaddict777_lazerharp_FIX.package", "%baseurl%/Mods/Sims3/packages/into_the_future/MTS_simsaddict777_lazerharp_FIX.package"),
                new("Perigree Planter Fix by Heaven", "SimTools/Packages/heaven_fenceFuturePlanterFix.package", "%baseurl%/Mods/Sims3/packages/into_the_future/heaven_fenceFuturePlanterFix.package"),
                new("Tileable Items Shader Fix by Simsi45", "SimTools/Packages/INTO THE FUTURE - Tileable_Items_FIXED.package", "%baseurl%/Mods/Sims3/packages/into_the_future/INTO THE FUTURE - Tileable_Items_FIXED.package"),
                new("Sims Need Less Space by OhRudi", "SimTools/Packages/OhRudi__IntoTheFuture__Sims_need_less_Space.package", "%baseurl%/Mods/Sims3/packages/into_the_future/OhRudi__IntoTheFuture__Sims_need_less_Space.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/into_the_future/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/into_the_future/*.package"),
            });

            // ── Store Fixes (5 items) ─────────────────────────────
            yield return ("Store Fixes", new List<GameplayFixItem>
            {
                // new GameplayFixItem("Fix Display Name", "filename.package", "%baseurl%/Mods/Sims3/packages/filename.package"),
                new("Armoure Perfume Moodlet Fix by Gamefreak130", "SimTools/Packages/Gamefreak130_PerfumeMoodletFix.package.package", "%baseurl%/Mods/Sims3/packages/store/Gamefreak130_PerfumeMoodletFix.package"),
                new("Banquet Fan Routing Fix by OmegaStarr82", "SimTools/Packages/BanquetFanFix.package", "%baseurl%/Mods/Sims3/packages/store/BanquetFanFix.package"),
                new("More Efficient Upgrade Name Fix by Buzzler", "SimTools/Packages/Buzz_MoreEfficientUpgradeNameFix.package", "%baseurl%/Mods/Sims3/packages/store/Buzz_MoreEfficientUpgradeNameFix.package"),
                new("Haute Hacienda Fireplace Fix by Qahne", "SimTools/Packages/Qahne_MOD_HauteHaciendaFireplace Fixes.package", "%baseurl%/Mods/Sims3/packages/store/Qahne_MOD_HauteHaciendaFireplace Fixes.package"),
                new("Now & Then Staircase Fix by Simsi45", "SimTools/Packages/Simsi45_Curved_Staircase_FIX_RECAT.package", "%baseurl%/Mods/Sims3/packages/store/Simsi45_Curved_Staircase_FIX_RECAT.package"),
                // new("Name", "SimTools/Packages/*.package", "%baseurl%/Mods/Sims3/packages/store/*.package"),
                // new("Name", "SimTools/Overrides/*.package", "%baseurl%/Mods/Sims3/overrides/store/*.package"),
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        // LAYOUT
        // ══════════════════════════════════════════════════════════════════════
        private UIElement BuildLayout()
        {
            var root = new DockPanel();

            // ── Bottom button bar ──────────────────────────────────────────────
            var btnBar = new StackPanel
            {
                Orientation         = WpfOrientation.Horizontal,
                HorizontalAlignment = WpfHAlign.Right,
                Margin              = new Thickness(12, 8, 12, 12),
            };
            DockPanel.SetDock(btnBar, Dock.Bottom);

            var downloadBtn = MakeButton("Download Selected", WpfColor.FromRgb(255, 210, 0));
            downloadBtn.Foreground = WpfBrushes.Black;
            downloadBtn.Click     += DownloadSelected_Click;

            var cancelBtn = MakeButton("Cancel", WpfColor.FromRgb(55, 55, 55));
            cancelBtn.Margin = new Thickness(8, 0, 0, 0);
            cancelBtn.Click += (_, _) => Close();

            btnBar.Children.Add(downloadBtn);
            btnBar.Children.Add(cancelBtn);
            root.Children.Add(btnBar);

            // ── Global "Select All" ────────────────────────────────────────────
            var globalSelectAll = new WpfCheckBox
            {
                Content    = "Select All",
                Foreground = WpfBrushes.White,
                FontWeight = FontWeights.Bold,
                Margin     = new Thickness(12, 10, 12, 4),
            };
            // Only toggle enabled (non-TBD) checkboxes
            globalSelectAll.Checked   += (_, _) =>
            {
                foreach (var cb in _allCheckBoxes.Where(c => c.IsEnabled))
                    cb.IsChecked = true;
            };
            globalSelectAll.Unchecked += (_, _) =>
            {
                foreach (var cb in _allCheckBoxes.Where(c => c.IsEnabled))
                    cb.IsChecked = false;
            };
            DockPanel.SetDock(globalSelectAll, Dock.Top);
            root.Children.Add(globalSelectAll);

            // ── Scrollable section list ────────────────────────────────────────
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(8, 4, 8, 4),
            };

            var sectionsPanel = new StackPanel();
            foreach (var (header, items) in BuildSections())
                sectionsPanel.Children.Add(BuildSection(header, items));

            scroll.Content = sectionsPanel;
            root.Children.Add(scroll);

            return root;
        }

        // ── BuildSection ──────────────────────────────────────────────────────
        // Creates a GroupBox for one expansion pack containing a per-section
        // "Select All" checkbox followed by one checkbox per fix item.
        // TBD items (empty FileName) are disabled and rendered in grey.
        private UIElement BuildSection(string header, List<GameplayFixItem> items)
        {
            var group = new WpfGroupBox
            {
                Header      = header,
                Foreground  = new WpfBrush(WpfColor.FromRgb(255, 210, 0)),
                BorderBrush = new WpfBrush(WpfColor.FromRgb(70, 70, 70)),
                Margin      = new Thickness(4, 4, 4, 6),
                Padding     = new Thickness(8),
            };

            var panel            = new StackPanel();
            var sectionCheckBoxes = new List<WpfCheckBox>();

            // Per-section "Select All" (only affects enabled/non-TBD items)
            var sectionSelectAll = new WpfCheckBox
            {
                Content    = "Select All",
                Foreground = new WpfBrush(WpfColor.FromRgb(180, 180, 180)),
                FontStyle  = FontStyles.Italic,
                Margin     = new Thickness(0, 0, 0, 5),
            };
            panel.Children.Add(sectionSelectAll);

            // Individual fix checkboxes
            foreach (var item in items)
            {
                bool isTbd = string.IsNullOrEmpty(item.FileName);

                var cb = new WpfCheckBox
                {
                    Content = item.DisplayName,
                    Foreground = isTbd
                        ? new WpfBrush(WpfColor.FromRgb(75, 75, 75))
                        : (WpfBrushBase)WpfBrushes.White,
                    IsEnabled = !isTbd,
                    Margin = new Thickness(0, 2, 0, 2),
                };

                // ↓ ADD THIS BLOCK
                if (!string.IsNullOrEmpty(item.OnCheckedMessage))
                {
                    var msg = item.OnCheckedMessage;
                    var title = item.DisplayName;
                    cb.Checked += (_, _) =>
                        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                panel.Children.Add(cb);
                sectionCheckBoxes.Add(cb);
                _allCheckBoxes.Add(cb);
                _allItems.Add(item);
            }

            // Wire section-level select-all to only the enabled boxes in this section
            sectionSelectAll.Checked   += (_, _) =>
            {
                foreach (var cb in sectionCheckBoxes.Where(c => c.IsEnabled))
                    cb.IsChecked = true;
            };
            sectionSelectAll.Unchecked += (_, _) =>
            {
                foreach (var cb in sectionCheckBoxes.Where(c => c.IsEnabled))
                    cb.IsChecked = false;
            };

            group.Content = panel;
            return group;
        }

        // ══════════════════════════════════════════════════════════════════════
        // DOWNLOAD HANDLER
        // ══════════════════════════════════════════════════════════════════════
        private async void DownloadSelected_Click(object sender, RoutedEventArgs e)
        {
            // Re-validate the mods directory (user may have closed Settings between
            // opening this window and clicking Download)
            if (!GamePaths.IsConfigured(_sims3Mods))
            {
                MessageBox.Show(
                    "Your Sims 3 Mods directory is no longer configured.\nPlease open Settings and set it first.",
                    "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Collect selected, non-TBD items.
            // _allCheckBoxes and _allItems share the same index order (built together).
            var toDownload = _allCheckBoxes
                .Zip(_allItems, (cb, item) => (cb, item))
                .Where(x => x.cb.IsChecked == true && !string.IsNullOrEmpty(x.item.FileName))
                .Select(x => x.item)
                .ToList();

            if (toDownload.Count == 0)
            {
                MessageBox.Show(
                    "No fixes are selected.\n\nNote: greyed-out items are placeholders not yet defined.",
                    "Nothing to Download", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Disable button for the duration of all downloads
            var btn = sender as WpfButton;
            if (btn != null) btn.IsEnabled = false;

            int downloaded = 0, skipped = 0, failed = 0;

            try
            {
                foreach (var item in toDownload)
                {
                    var url      = AppSettings.ResolveUrl(item.Url);
                    var destPath = Path.Combine(_sims3Mods, item.FileName);

                    // ── HEAD check — skip if local file is already current ──────
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

                    // ── Download ──────────────────────────────────────────────
                    var progressWindow = new DownloadProgressWindow(item.FileName) { Owner = this };
                    progressWindow.Show();

                    try
                    {
                        using var http     = new HttpClient();
                        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        var  remoteLastModified = response.Content.Headers.LastModified;
                        long? totalBytes        = response.Content.Headers.ContentLength;

                        await using var contentStream = await response.Content.ReadAsStreamAsync();
                        await using var fileStream    = new FileStream(
                            destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                        var buffer      = new byte[8192];
                        long bytesRead  = 0;
                        int lastPercent = 0, chunk;

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
                        if (File.Exists(destPath)) File.Delete(destPath);
                        failed++;
                        MessageBox.Show(
                            $"Failed to download {item.FileName}:\n{ex.Message}",
                            "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        progressWindow.Close();
                    }
                }

                MessageBox.Show(
                    $"Done.\n\nDownloaded: {downloaded}  |  Already up-to-date: {skipped}  |  Failed: {failed}",
                    "Gameplay Fixes — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        // ── UI helper ─────────────────────────────────────────────────────────
        private static WpfButton MakeButton(string content, WpfColor bg, double width = 150) => new()
        {
            Content    = content,
            Width      = width,
            Height     = 30,
            Padding    = new Thickness(10, 0, 10, 0),
            FontFamily = new WpfFontFamily("Tahoma"),
            FontWeight = FontWeights.Bold,
            Foreground = WpfBrushes.White,
            Background = new WpfBrush(bg),
        };
    }
}
