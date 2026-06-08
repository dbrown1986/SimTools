using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Clipboard = System.Windows.Clipboard;
using SystemColors = System.Windows.SystemColors;

namespace SimTools;

/// <summary>
/// Generic key generator — randomly picks one of 50 keys from a compiled
/// resource file for the game title selected via GenKeysButton.
/// </summary>
public partial class GenericKeys : Window
{
    // Resource file name for the currently selected game (e.g. "ts3_base.txt")
    private string? _selectedFile;

    public GenericKeys()
    {
        InitializeComponent();
        ApplyLanguage();
        SetupGenKeysContextMenu();
    }

    // ══════════════════════════════════════════════════════════════════════
    //   Context Menu
    // ══════════════════════════════════════════════════════════════════════

    private void SetupGenKeysContextMenu()
    {
        var menu = new ContextMenu();

        // ── The Sims ─────────────────────────────────────────────────────
        menu.Items.Add(Header(LanguageManager.Get("BuyTS3", "Sims1_Disc", "The Sims")));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Sims1_Disc", "The Sims"), "ts1_base.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1LL", "The Sims: Livin' Large"),        "ts1_livin_large.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1HP", "The Sims: House Party"),         "ts1_house_party.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1HD", "The Sims: Hot Date"),            "ts1_hot_date.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1V", "The Sims: Vacation"),            "ts1_vacation.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1U", "The Sims: Unleashed"),           "ts1_unleashed.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1S", "The Sims: Superstar"),           "ts1_superstar.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1MM", "The Sims: Makin' Magic"),        "ts1_makin_magic.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1DE", "The Sims: Deluxe Edition"),      "ts1_deluxe.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS1CC", "The Sims: Complete Collection"), "ts1_complete_collection.txt"));
        menu.Items.Add(new Separator());

        // ── The Sims 2 ───────────────────────────────────────────────────
        menu.Items.Add(Header(LanguageManager.Get("BuyTS3", "Sims2_Disc", "The Sims 2")));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "Sims2_Disc", "The Sims 2"),                "ts2_base.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2D", "The Sims 2: Deluxe"),        "ts2_deluxe.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2DD", "The Sims 2: Double Deluxe"), "ts2_double_deluxe.txt"));

        var ts2Exp = SubMenu("Expansions");
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2U", "The Sims 2: University"),              "ts2_ep_university.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2NL", "The Sims 2: Nightlife"),               "ts2_ep_nightlife.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2OFB", "The Sims 2: Open for Business"),       "ts2_ep_open_for_business.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2P", "The Sims 2: Pets"),                    "ts2_ep_pets.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2S", "The Sims 2: Seasons"),                 "ts2_ep_seasons.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2BV", "The Sims 2: Bon Voyage"),              "ts2_ep_bon_voyage.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2FT", "The Sims 2: Free Time"),               "ts2_ep_free_time.txt"));
        ts2Exp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2AL", "The Sims 2: Apartment Life"),          "ts2_ep_apartment_life.txt"));
        menu.Items.Add(ts2Exp);

        var ts2Sp = SubMenu("Stuff Packs");
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2FFS", "The Sims 2: Family Fun Stuff"),                     "ts2_sp_family_fun.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2GLS", "The Sims 2: Glamour Life Stuff"),                   "ts2_sp_glamour_life.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2CS", "The Sims 2: Celebration Stuff"),                    "ts2_sp_celebration.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2HMFS", "The Sims 2: H&M Fashion Stuff"),                    "ts2_sp_hm_fashion.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2TSS", "The Sims 2: Teen Style Stuff"),                     "ts2_sp_teen_style.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2KBIS", "The Sims 2: Kitchen & Bath Interior Design Stuff"), "ts2_sp_kitchen_bath.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2IHS", "The Sims 2: IKEA Home Stuff"),                      "ts2_sp_ikea.txt"));
        ts2Sp.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "TS2MGS", "The Sims 2: Mansion & Garden Stuff"),               "ts2_sp_mansion_garden.txt"));
        menu.Items.Add(ts2Sp);
        menu.Items.Add(new Separator());

        // ── The Sims Stories ─────────────────────────────────────────────
        menu.Items.Add(Header(LanguageManager.Get("BuyTS3", "TS_Stories", "The Sims Stories")));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "TSCast", "The Sims: Castaway Stories"), "tss_castaway.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "TSLife", "The Sims: Life Stories"),     "tss_life.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "TSPets", "The Sims: Pet Stories"),      "tss_pet.txt"));
        menu.Items.Add(new Separator());

        // ── The Sims 3 ───────────────────────────────────────────────────
        menu.Items.Add(Header(LanguageManager.Get("BuyTS3", "Sims3BG", "The Sims 3")));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Sims3BG", "The Sims 3"), "ts3_base.txt"));

        var ts3Exp = SubMenu("Expansions");
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "WorldAdventures", "The Sims 3: World Adventures"), "ts3_ep_world_adventures.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Ambitions", "The Sims 3: Ambitions"),        "ts3_ep_ambitions.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "LateNight", "The Sims 3: Late Night"),       "ts3_ep_late_night.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Generations", "The Sims 3: Generations"),      "ts3_ep_generations.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Pets", "The Sims 3: Pets"),             "ts3_ep_pets.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Showtime", "The Sims 3: Showtime"),         "ts3_ep_showtime.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Supernatural", "The Sims 3: Supernatural"),     "ts3_ep_supernatural.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "Seasons", "The Sims 3: Seasons"),          "ts3_ep_seasons.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "University", "The Sims 3: University Life"),  "ts3_ep_university_life.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "IslandParadise", "The Sims 3: Island Paradise"),  "ts3_ep_island_paradise.txt"));
        ts3Exp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "ITFuture", "The Sims 3: Into the Future"),  "ts3_ep_into_the_future.txt"));
        menu.Items.Add(ts3Exp);

        var ts3Sp = SubMenu("Stuff Packs");
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "HELStuff", "The Sims 3: High-End Loft Stuff"),       "ts3_sp_high_end_loft.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "FLStuff", "The Sims 3: Fast Lane Stuff"),           "ts3_sp_fast_lane.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "OLStuff", "The Sims 3: Outdoor Living Stuff"),      "ts3_sp_outdoor_living.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "TLStuff", "The Sims 3: Town Life Stuff"),           "ts3_sp_town_life.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "MSStuff", "The Sims 3: Master Suite Stuff"),        "ts3_sp_master_suite.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "KPSweetTreats", "The Sims 3: Katy Perry's Sweet Treats"), "ts3_sp_katy_perry.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "DStuff", "The Sims 3: Diesel Stuff"),              "ts3_sp_diesel.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "789Stuff", "The Sims 3: 70's, 80's & 90's Stuff"),   "ts3_sp_70s_80s_90s.txt"));
        ts3Sp.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "MStuff", "The Sims 3: Movie Stuff"),               "ts3_sp_movie.txt"));
        menu.Items.Add(ts3Sp);
        menu.Items.Add(new Separator());

        // ── The Sims: Medieval ───────────────────────────────────────────
        menu.Items.Add(Header(LanguageManager.Get("BuyTS3", "TSM1", "The Sims: Medieval")));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "TSM1", "The Sims: Medieval"),                  "tsm_base.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "TSM3", "The Sims Medieval: Pirates & Nobles"), "tsm_pirates_nobles.txt"));
        menu.Items.Add(new Separator());

        // ── SimCity 4 ────────────────────────────────────────────────────
        menu.Items.Add(Header(LanguageManager.Get("GenericKeysPage", "SC4", "SimCity 4")));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "SC4", "SimCity 4"),                 "sc4_base.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("GenericKeysPage", "SC4RH", "SimCity 4: Rush Hour"),      "sc4_rush_hour.txt"));
        menu.Items.Add(Leaf(LanguageManager.Get("BuyTS3", "SC4DE", "SimCity 4: Deluxe Edition"), "sc4_deluxe.txt"));

        GenKeysButton.ContextMenu = menu;
    }

    // ══════════════════════════════════════════════════════════════════════
    //   Menu Item Factories
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Non-clickable bold section header.</summary>
    
    private static MenuItem Header(string title) => new()
    {
        Header     = title,
        IsEnabled  = false,
        FontWeight = FontWeights.Bold,
        Foreground = SystemColors.WindowTextBrush
    };

    /// <summary>Non-clickable italicised sub-menu container (Expansions / Stuff Packs).</summary>
    private static MenuItem SubMenu(string title) => new()
    {
        Header    = title,
        FontStyle = FontStyles.Italic
    };

    /// <summary>
    /// Clickable leaf item. On click: stores the resource file name and
    /// updates the GenKeysButton label to show the selected title.
    /// </summary>
    private MenuItem Leaf(string title, string resourceFile)
    {
        var item = new MenuItem { Header = title };
        item.Click += (_, _) =>
        {
            _selectedFile          = resourceFile;
            GenKeysButton.Content  = $"{title} ▸";
            KeyField.Text          = LanguageManager.Get("GenericKeysPage", "ClickToGen", "Click Generate to produce a key...");
        };
        return item;
    }

    // ══════════════════════════════════════════════════════════════════════
    //   Key Loader
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Reads newline-separated key strings from a compiled resource TXT file.
    /// Returns an empty array if the file is missing or empty.
    /// </summary>
    private static string[] LoadKeys(string fileName)
    {
        var uri = new Uri($"pack://application:,,,/Resources/Keys/{fileName}");
        try
        {
            using var stream = App.GetResourceStream(uri)?.Stream;
            if (stream is null) return [];

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd()
                         .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(l => l.Trim('\r', ' '))
                         .Where(l => l.Length > 0)
                         .ToArray();
        }
        catch
        {
            return [];
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //   Event Handlers
    // ══════════════════════════════════════════════════════════════════════

    private void GenKeysButton_Click(object sender, RoutedEventArgs e)
    {
        if (GenKeysButton.ContextMenu is { } cm)
        {
            cm.PlacementTarget = GenKeysButton;
            cm.IsOpen          = true;
        }
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedFile is null)
        {
            KeyField.Text = LanguageManager.Get("GenericKeysPage", "SelectFirst", "Please select a game first.");
            return;
        }

        var keys = LoadKeys(_selectedFile);
        if (keys.Length == 0)
        {
            KeyField.Text = LanguageManager.Get("GenericKeysPage", "NoKeys", "No keys available for this title yet.");
            return;
        }

        KeyField.Text = keys[Random.Shared.Next(keys.Length)];
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(KeyField.Text))
            Clipboard.SetText(KeyField.Text);
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
        GenKeysButton.Content = LanguageManager.Get("GenericKeysPage", "GenKeysButton1", "Select Game ▸");
        KeysInfoText.Text = LanguageManager.Get("GenericKeysPage", "KeysInfo", KeysInfoText.Text);
        KeysInfoText_Copy.Text = LanguageManager.Get("GenericKeysPage", "KeysInfoCopy", KeysInfoText_Copy.Text);
        GenerateButton.Content = LanguageManager.Get("GenericKeysPage", "GenerateButton", "Generate");
        CopyButton.Content = LanguageManager.Get("GenericKeysPage", "CopyButton", "Copy");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
