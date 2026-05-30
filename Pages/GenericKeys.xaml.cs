using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        SetupGenKeysContextMenu();
    }

    // ══════════════════════════════════════════════════════════════════════
    //   Context Menu
    // ══════════════════════════════════════════════════════════════════════

    private void SetupGenKeysContextMenu()
    {
        var menu = new ContextMenu();

        // ── The Sims ─────────────────────────────────────────────────────
        menu.Items.Add(Header("The Sims"));
        menu.Items.Add(Leaf("The Sims",                      "ts1_base.txt"));
        menu.Items.Add(Leaf("The Sims: Livin' Large",        "ts1_livin_large.txt"));
        menu.Items.Add(Leaf("The Sims: House Party",         "ts1_house_party.txt"));
        menu.Items.Add(Leaf("The Sims: Hot Date",            "ts1_hot_date.txt"));
        menu.Items.Add(Leaf("The Sims: Vacation",            "ts1_vacation.txt"));
        menu.Items.Add(Leaf("The Sims: Unleashed",           "ts1_unleashed.txt"));
        menu.Items.Add(Leaf("The Sims: Superstar",           "ts1_superstar.txt"));
        menu.Items.Add(Leaf("The Sims: Makin' Magic",        "ts1_makin_magic.txt"));
        menu.Items.Add(Leaf("The Sims: Deluxe Edition",      "ts1_deluxe.txt"));
        menu.Items.Add(Leaf("The Sims: Complete Collection", "ts1_complete_collection.txt"));
        menu.Items.Add(new Separator());

        // ── The Sims 2 ───────────────────────────────────────────────────
        menu.Items.Add(Header("The Sims 2"));
        menu.Items.Add(Leaf("The Sims 2",                "ts2_base.txt"));
        menu.Items.Add(Leaf("The Sims 2: Deluxe",        "ts2_deluxe.txt"));
        menu.Items.Add(Leaf("The Sims 2: Double Deluxe", "ts2_double_deluxe.txt"));

        var ts2Exp = SubMenu("Expansions");
        ts2Exp.Items.Add(Leaf("The Sims 2: University",              "ts2_ep_university.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Nightlife",               "ts2_ep_nightlife.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Open for Business",       "ts2_ep_open_for_business.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Pets",                    "ts2_ep_pets.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Seasons",                 "ts2_ep_seasons.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Bon Voyage",              "ts2_ep_bon_voyage.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Free Time",               "ts2_ep_free_time.txt"));
        ts2Exp.Items.Add(Leaf("The Sims 2: Apartment Life",          "ts2_ep_apartment_life.txt"));
        menu.Items.Add(ts2Exp);

        var ts2Sp = SubMenu("Stuff Packs");
        ts2Sp.Items.Add(Leaf("The Sims 2: Family Fun Stuff",                     "ts2_sp_family_fun.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: Glamour Life Stuff",                   "ts2_sp_glamour_life.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: Celebration Stuff",                    "ts2_sp_celebration.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: H&M Fashion Stuff",                    "ts2_sp_hm_fashion.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: Teen Style Stuff",                     "ts2_sp_teen_style.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: Kitchen & Bath Interior Design Stuff", "ts2_sp_kitchen_bath.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: IKEA Home Stuff",                      "ts2_sp_ikea.txt"));
        ts2Sp.Items.Add(Leaf("The Sims 2: Mansion & Garden Stuff",               "ts2_sp_mansion_garden.txt"));
        menu.Items.Add(ts2Sp);
        menu.Items.Add(new Separator());

        // ── The Sims Stories ─────────────────────────────────────────────
        menu.Items.Add(Header("The Sims Stories"));
        menu.Items.Add(Leaf("The Sims: Castaway Stories", "tss_castaway.txt"));
        menu.Items.Add(Leaf("The Sims: Life Stories",     "tss_life.txt"));
        menu.Items.Add(Leaf("The Sims: Pet Stories",      "tss_pet.txt"));
        menu.Items.Add(new Separator());

        // ── The Sims 3 ───────────────────────────────────────────────────
        menu.Items.Add(Header("The Sims 3"));
        menu.Items.Add(Leaf("The Sims 3", "ts3_base.txt"));

        var ts3Exp = SubMenu("Expansions");
        ts3Exp.Items.Add(Leaf("The Sims 3: World Adventures", "ts3_ep_world_adventures.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Ambitions",        "ts3_ep_ambitions.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Late Night",       "ts3_ep_late_night.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Generations",      "ts3_ep_generations.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Pets",             "ts3_ep_pets.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Showtime",         "ts3_ep_showtime.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Supernatural",     "ts3_ep_supernatural.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Seasons",          "ts3_ep_seasons.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: University Life",  "ts3_ep_university_life.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Island Paradise",  "ts3_ep_island_paradise.txt"));
        ts3Exp.Items.Add(Leaf("The Sims 3: Into the Future",  "ts3_ep_into_the_future.txt"));
        menu.Items.Add(ts3Exp);

        var ts3Sp = SubMenu("Stuff Packs");
        ts3Sp.Items.Add(Leaf("The Sims 3: High-End Loft Stuff",       "ts3_sp_high_end_loft.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Fast Lane Stuff",           "ts3_sp_fast_lane.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Outdoor Living Stuff",      "ts3_sp_outdoor_living.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Town Life Stuff",           "ts3_sp_town_life.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Master Suite Stuff",        "ts3_sp_master_suite.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Katy Perry's Sweet Treats", "ts3_sp_katy_perry.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Diesel Stuff",              "ts3_sp_diesel.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: 70's, 80's & 90's Stuff",   "ts3_sp_70s_80s_90s.txt"));
        ts3Sp.Items.Add(Leaf("The Sims 3: Movie Stuff",               "ts3_sp_movie.txt"));
        menu.Items.Add(ts3Sp);
        menu.Items.Add(new Separator());

        // ── The Sims: Medieval ───────────────────────────────────────────
        menu.Items.Add(Header("The Sims: Medieval"));
        menu.Items.Add(Leaf("The Sims: Medieval",                  "tsm_base.txt"));
        menu.Items.Add(Leaf("The Sims Medieval: Pirates & Nobles", "tsm_pirates_nobles.txt"));
        menu.Items.Add(new Separator());

        // ── SimCity 4 ────────────────────────────────────────────────────
        menu.Items.Add(Header("SimCity 4"));
        menu.Items.Add(Leaf("SimCity 4",                 "sc4_base.txt"));
        menu.Items.Add(Leaf("SimCity 4: Rush Hour",      "sc4_rush_hour.txt"));
        menu.Items.Add(Leaf("SimCity 4: Deluxe Edition", "sc4_deluxe.txt"));

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
            KeyField.Text          = "Click Generate to produce a key.";
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
            using var stream = Application.GetResourceStream(uri)?.Stream;
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
            KeyField.Text = "Please select a game first.";
            return;
        }

        var keys = LoadKeys(_selectedFile);
        if (keys.Length == 0)
        {
            KeyField.Text = "No keys available for this title yet.";
            return;
        }

        KeyField.Text = keys[Random.Shared.Next(keys.Length)];
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(KeyField.Text))
            Clipboard.SetText(KeyField.Text);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
