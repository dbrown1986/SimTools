using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using WpfBox = System.Windows.MessageBox;
using System.Windows.Media.Imaging;

namespace SimTools;

public partial class BuyTS3 : Window
{
    public BuyTS3()
    {
        InitializeComponent();
        PopulateTrees();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BitmapImage LoadIcon(string path) =>
        new(new Uri($"pack://application:,,,/{path}", UriKind.Absolute));

    // Category node — children passed as params
    private static BuyTreeNode N(string label, string icon, params BuyTreeNode[] children)
    {
        var node = new BuyTreeNode { Label = label, Icon = LoadIcon(icon) };
        foreach (var c in children) node.Children.Add(c);
        return node;
    }

    // Leaf node — opens a URL when selected (leave url as "" until ready to fill in)
    private static BuyTreeNode Leaf(string label, string icon, string url = "") =>
        new() { Label = label, Icon = LoadIcon(icon), Url = url };

    // Standard "* Official" sub-node with EA App + Steam leaves
    private static BuyTreeNode Official(string ea = "", string steam = "", string gog = "", string icon = "") =>
        N("* Official", string.IsNullOrEmpty(icon) ? I.EA : icon,
            Leaf("Buy on EA App", I.EA, ea),
            Leaf("Buy on Steam", I.Steam, steam),
            Leaf("Buy on GOG", I.GOG, gog));

    // Standard "Partners" sub-node
    private static BuyTreeNode Partners(params BuyTreeNode[] items) =>
        N("Partners", I.G2A, items);

    // ── Icon path constants ───────────────────────────────────────────────────
    private static class I
    {
        public const string Sims1 = "Images/Icons/Sims1.ico";
        public const string Sims2 = "Images/Icons/Sims2.ico";
        public const string Sims3 = "Images/Icons/Sims3.ico";
        public const string SimsMedieval = "Images/Icons/TSM.ico";
        public const string SC2K = "Images/Icons/SC2K.ico";
        public const string SC3KU = "Images/Icons/SC3KU.ico";
        public const string SC4DE = "Images/Icons/SC4DE.ico";
        public const string SC2013 = "Images/Icons/SC2013.ico";
        public const string SimCopter = "Images/Icons/Copter.ico";
        public const string Streets = "Images/Icons/Streets.ico";
        public const string TSCastaway = "Images/Icons/TSCastaway.ico";
        public const string TSLife = "Images/Icons/TSLife.ico";
        public const string TSPets = "Images/Icons/TSPets.ico";
        public const string EPs = "Images/Icons/SimsEPs.ico";
        public const string SPs = "Images/Icons/SimsSPs.ico";
        public const string Store = "Images/Icons/Store.ico";
        public const string Worlds = "Images/Icons/Worlds.ico";
        public const string EA = "Images/Icons/vendors/ea.ico";
        public const string Steam = "Images/Icons/vendors/steam.ico";
        public const string GOG = "Images/Icons/vendors/gog.ico";
        public const string Eneba = "Images/Icons/vendors/eneba.ico";
        public const string G2A = "Images/Icons/vendors/g2a.ico";
        public const string Gamivo = "Images/Icons/vendors/gamivo.ico";
        public const string HRK = "Images/Icons/vendors/hrk.ico";
        public const string IG = "Images/Icons/vendors/ig.ico";
        public const string K4G = "Images/Icons/vendors/k4g.ico";
        public const string Kinguin = "Images/Icons/vendors/kinguin.ico";
        public const string MMOGA = "Images/Icons/vendors/mmoga.ico";
        public const string GVG = "Images/Icons/vendors/gvg.ico";
        public const string GO = "Images/Icons/vendors/gamers-outlet.ico";
        // EP icons
        public const string EP01 = "Images/Icons/Sims3EP01.ico";
        public const string EP02 = "Images/Icons/Sims3EP02.ico";
        public const string EP03 = "Images/Icons/Sims3EP03.ico";
        public const string EP04 = "Images/Icons/Sims3EP04.ico";
        public const string EP05 = "Images/Icons/Sims3EP05.ico";
        public const string EP06 = "Images/Icons/Sims3EP06.ico";
        public const string EP06CE = "Images/Icons/Sims3EP06CE.ico";
        public const string EP07 = "Images/Icons/Sims3EP07.ico";
        public const string EP08 = "Images/Icons/Sims3EP08.ico";
        public const string EP09 = "Images/Icons/Sims3EP09.ico";
        public const string EP10 = "Images/Icons/Sims3EP10.ico";
        public const string EP11 = "Images/Icons/Sims3EP11.ico";
        // SP icons
        public const string SP01 = "Images/Icons/Sims3SP01.ico";
        public const string SP02 = "Images/Icons/Sims3SP02.ico";
        public const string SP03 = "Images/Icons/Sims3SP03.ico";
        public const string SP04 = "Images/Icons/Sims3SP04.ico";
        public const string SP05 = "Images/Icons/Sims3SP05.ico";
        public const string SP06 = "Images/Icons/Sims3SP06.ico";
        public const string SP07 = "Images/Icons/Sims3SP07.ico";
        public const string SP08 = "Images/Icons/Sims3SP08.ico";
        public const string SP09 = "Images/Icons/Sims3SP09.ico";
        // Premium world icons
        public const string Aurora = "Images/Icons/AuroraSkies.ico";
        public const string Barnacle = "Images/Icons/BarnacleBay.ico";
        public const string Dragon = "Images/Icons/DragonValley.ico";
        public const string Hidden = "Images/Icons/HiddenSprings.ico";
        public const string Lucky = "Images/Icons/LuckyPalms.ico";
        public const string Lunar = "Images/Icons/LunarLakes.ico";
        public const string Midnight = "Images/Icons/MidnightHollow.ico";
        public const string Monte = "Images/Icons/MonteVista.ico";
        public const string Riverview = "Images/Icons/Riverview.ico";
        public const string Roaring = "Images/Icons/RoaringHeights.ico";
        public const string HSMV = "Images/Icons/HSMVBundle.ico";
    }

    // ── Selection handler ─────────────────────────────────────────────────────

    private void OnBuyTreeItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not BuyTreeNode node) return;

        if (node.Message is not null)
            WpfBox.Show(node.Message, "SimTools", MessageBoxButton.OK, MessageBoxImage.Information);

        if (!string.IsNullOrEmpty(node.Url))
        {
            try { Process.Start(new ProcessStartInfo(node.Url) { UseShellExecute = true }); }
            catch { /* silently ignore */ }
        }
    }

    // ── Tree population ───────────────────────────────────────────────────────

    private void PopulateTrees()
    {
        BuyGamesDigital.ItemsSource = new ObservableCollection<BuyTreeNode>
        {
            BuildSims1(),
            BuildSims2(),
            BuildSims3(),
            BuildSC2K(),
            BuildSC3KU(),
            BuildSC4DE(),
            BuildSC2013(),
        };
    }

    // ── The Sims: Legacy Collection ───────────────────────────────────────────

    private static BuyTreeNode BuildSims1() =>
        N("The Sims: Legacy Collection", I.Sims1,
            Official(
                ea: "https://www.ea.com/en/games/the-sims/the-sims-25th-anniv-edition",
                steam: "https://store.steampowered.com/app/3314060/The_Sims_Legacy_Collection/",
                icon: I.EA));

    // ── The Sims 2: Legacy Collection ─────────────────────────────────────────

    private static BuyTreeNode BuildSims2() =>
        N("The Sims 2: Legacy Collection", I.Sims2,
            Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-2-25th-anniv-edition",
                steam: "https://store.steampowered.com/app/3314070/The_Sims_2_Legacy_Collection/",
                icon: I.EA));

    // ── The Sims 3 ────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildSims3() =>
        N("The Sims 3", I.Sims3,
            BuildBaseGame(),
            BuildExpansions(),
            BuildStuffPacks(),
            BuildPremiumWorlds(),
            Leaf("Store Items", I.Store, ""));

    // ── Base Game ─────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildBaseGame() =>
        N("Base Game", I.Sims3,
            Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3",
                steam: "https://store.steampowered.com/app/47890/The_Sims_3",
                icon: I.EA),
            Partners(
                Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                Leaf("Buy on G2A (EA App)", I.G2A, ""),
                Leaf("Buy on G2A (Steam)", I.G2A, ""),
                Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                Leaf("Buy on K4G (EA App)", I.K4G, ""),
                Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                Leaf("Buy on MMOGA (EA App)", I.MMOGA, "")));

    // ── Expansions ────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildExpansions() =>
        N("Expansions", I.EPs,
            N("World Adventures", I.EP01,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Ambitions", I.EP02,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Late Night", I.EP03,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Generations", I.EP04,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on Gamivo (Steam)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Pets", I.EP05,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Showtime", I.EP06,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Showtime: Katy Perry Collector's Edition", I.EP06CE,
                Partners(
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""))),

            N("Supernatural", I.EP07,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on Gamivo (Steam)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Seasons", I.EP08,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("University Life", I.EP09,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on Gamivo (Steam)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Island Paradise", I.EP10,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on Gamivo (Steam)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Into The Future", I.EP11,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))));

    // ── Stuff Packs ───────────────────────────────────────────────────────────

    private static BuyTreeNode BuildStuffPacks() =>
        N("Stuff Packs", I.SPs,
            N("High-End Loft Stuff", I.SP01,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Fast Lane Stuff", I.SP02,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on Gamivo (Steam)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Outdoor Living Stuff", I.SP03,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Town Life Stuff", I.SP04,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Master Suite Stuff", I.SP05,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on GVGMall (EA App)", I.GVG, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            // Discontinued — pops a warning when the node is selected
            new BuyTreeNode
            {
                Label = "Katy Perry's Sweet Treats",
                Icon = LoadIcon(I.SP06),
                Message = "Katy Perry's Sweet Treats is no longer sold or produced, and therefore " +
                          "usually comes with a hefty price tag. It may also often be out of stock.",
                Children =
                {
                    Partners(
                        Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                        Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                        Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                        Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                        Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                        Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""))
                }
            },

            N("Diesel Stuff", I.SP07,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("70s, 80s & 90s Stuff", I.SP08,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))),

            N("Movie Stuff", I.SP09,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, ""),
                    Leaf("Buy on G2A (EA App)", I.G2A, ""),
                    Leaf("Buy on G2A (Steam)", I.G2A, ""),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO, ""),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, ""),
                    Leaf("Buy on HRK (EA App)", I.HRK, ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, ""),
                    Leaf("Buy on K4G (EA App)", I.K4G, ""),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)", I.MMOGA, ""))));

    // ── Premium Worlds ────────────────────────────────────────────────────────

    private static BuyTreeNode BuildPremiumWorlds() =>
        N("Premium Worlds", I.Worlds,
            Leaf("Aurora Skies", I.Aurora, ""),
            Leaf("Barnacle Bay", I.Barnacle, ""),
            Leaf("Dragon Valley", I.Dragon, ""),
            Leaf("Hidden Springs", I.Hidden, ""),
            Leaf("Lucky Palms", I.Lucky, ""),
            Leaf("Lunar Lakes", I.Lunar, ""),
            Leaf("Midnight Hollow", I.Midnight, ""),
            Leaf("Monte Vista", I.Monte, ""),
            Leaf("Riverview", I.Riverview, ""),
            Leaf("Roaring Heights", I.Roaring, ""),
            Leaf("Hidden Springs & Monte Vista Bundle", I.HSMV, ""));

    // Simcity 2000 / Simcity 2000 Special Edition
    private static BuyTreeNode BuildSC2K() =>
        N("Simcity 2000", I.SC2K,
            Official(
                ea: "https://www.ea.com/en/games/simcity/simcity-2000",
                gog: "https://www.gog.com/en/game/simcity_2000_special_edition",
                icon: I.EA));

    // Simcity 3000 Unlimited
    private static BuyTreeNode BuildSC3KU() =>
        N("Simcity 3000 Unlimited", I.SC3KU,
            Official(
                steam: "https://store.steampowered.com/app/2741560/SimCity_3000_Unlimited/",
                gog: "https://www.gog.com/en/game/simcity_3000",
                icon: I.EA));

    // SimCity 4 Deluxe Edition
    private static BuyTreeNode BuildSC4DE() =>
    N("SimCity 4 Deluxe Edition", I.SC4DE,
        Official(
            ea: "https://www.ea.com/en/games/simcity/simcity-4",
            steam: "https://store.steampowered.com/app/24780/SimCity_4_Deluxe_Edition/",
            gog: "https://www.gog.com/en/game/simcity_4_deluxe_edition",
            icon: I.EA));

    // SimCity (2013)
    private static BuyTreeNode BuildSC2013() =>
    N("SimCity 2013", I.SC2013,
        Official(
            ea: "https://www.ea.com/en/games/simcity/simcity",
            icon: I.EA));

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void WarningButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new BuyWarning();
        window.Owner = this;
        window.Show();
    }
}

