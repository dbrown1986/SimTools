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
    private static BuyTreeNode Official(string ea = "", string steam = "") =>
        N("* Official", I.EA,
            Leaf("Buy on EA App", I.EA,    ea),
            Leaf("Buy on Steam",  I.Steam, steam));

    // Standard "Partners" sub-node
    private static BuyTreeNode Partners(params BuyTreeNode[] items) =>
        N("Partners", I.G2A, items);

    // ── Icon path constants ───────────────────────────────────────────────────
    private static class I
    {
        public const string Sims3   = "Images/Icons/Sims3.ico";
        public const string EPs     = "Images/Icons/SimsEPs.ico";
        public const string SPs     = "Images/Icons/SimsSPs.ico";
        public const string Store   = "Images/Icons/Store.ico";
        public const string EA      = "Images/Icons/vendors/ea.ico";
        public const string Steam   = "Images/Icons/vendors/steam.ico";
        public const string Eneba   = "Images/Icons/vendors/eneba.ico";
        public const string G2A     = "Images/Icons/vendors/g2a.ico";
        public const string Gamivo  = "Images/Icons/vendors/gamivo.ico";
        public const string HRK     = "Images/Icons/vendors/hrk.ico";
        public const string IG      = "Images/Icons/vendors/ig.ico";
        public const string Kinguin = "Images/Icons/vendors/kinguin.ico";
        public const string K4G     = "Images/Icons/vendors/k4g.ico";
        public const string MMOGA   = "Images/Icons/vendors/mmoga.ico";
        public const string GVG     = "Images/Icons/vendors/gvg.ico";
        public const string GO      = "Images/Icons/vendors/gamers-outlet.ico";
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
        public const string Worlds = "Images/Icons/worlds.ico";
        public const string Aurora    = "Images/Icons/AuroraSkies.ico";
        public const string Barnacle  = "Images/Icons/BarnacleBay.ico";
        public const string Dragon    = "Images/Icons/DragonValley.ico";
        public const string Hidden    = "Images/Icons/HiddenSprings.ico";
        public const string Lucky     = "Images/Icons/LuckyPalms.ico";
        public const string Lunar     = "Images/Icons/LunarLakes.ico";
        public const string Midnight  = "Images/Icons/MidnightHollow.ico";
        public const string Monte     = "Images/Icons/MonteVista.ico";
        public const string Riverview = "Images/Icons/Riverview.ico";
        public const string Roaring   = "Images/Icons/RoaringHeights.ico";
        public const string HSMV      = "Images/Icons/HSMVBundle.ico";
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
        BuyGamesDigital.ItemsSource = new ObservableCollection<BuyTreeNode> { BuildDigitalTree() };
    }

    private static BuyTreeNode BuildDigitalTree() =>
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
            steam: "https://store.steampowered.com/app/47890/The_Sims_3/"),
            Partners(
                Leaf("Buy on Eneba (EA App)",   I.Eneba, "https://www.eneba.com/origin-the-sims-3-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                Leaf("Buy on G2A (EA App)",     I.G2A, "https://www.g2a.com/the-sims-3-origin-key-global-i10000023727004?gname=ts3tools"),
                Leaf("Buy on G2A (Steam)",      I.G2A, "https://www.g2a.com/the-sims-3-steam-gift-global-i10000023727001?gname=ts3tools"),
                Leaf("Buy on Gamivo (EA App)",  I.Gamivo, "https://www.gamivo.com/product/the-sims-3?glv=p1b0e0fh"),
                Leaf("Buy on K4G (EA App)",     I.K4G, "https://k4g.com/product/the-sims-3-origin-global-cd-key-5C2BA064?r=ts3tools"),
                Leaf("Buy on Kinguin (EA App)", I.Kinguin, "https://www.kinguin.net/category/131/the-sims-3-origin-cd-key/?r=66716563950ad"),
                Leaf("Buy on Kinguin (Steam)",  I.Kinguin, "https://www.kinguin.net/category/32666/the-sims-3-steam-cd-key/?r=66716563950ad"),
                Leaf("Buy on MMOGA (EA App)",   I.MMOGA, "https://www.mmoga.com/EA-Games/Sims-3-Key-Free-download-included.html?ref=63563&Partner=TS3Tools")));

    // ── Expansions ────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildExpansions() =>
        N("Expansions", I.EPs,
            N("World Adventures", I.EP01,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-world-adventures-expansion-pack/buy-microcontent",
                steam: "https://store.steampowered.com/app/47892/The_Sims_3_World_Adventures/?curator_clanid=3953086"),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba, "https://www.eneba.com/origin-the-sims-3-world-adventures-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A, "https://www.g2a.com/the-sims-3-world-adventures-ea-app-key-global-i10000043589003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A, "https://www.g2a.com/the-sims-3-world-adventures-steam-gift-global-i10000043589001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo, "https://www.gamivo.com/product/the-sims-3-world-adventures?glv=p1b0e0fh"),
                    Leaf("Buy on K4G (EA App)",            I.K4G, "https://k4g.com/product/the-sims-3-world-adventures-origin-global-cd-key-4EBA6D3A?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, "https://www.kinguin.net/category/1940/the-sims-3-world-adventures-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, "https://www.kinguin.net/category/7660/the-sims-3-world-adventures-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA, "https://www.mmoga.com/EA-Games/The-Sims-3-World-Adventures-addon.html?ref=63563&Partner=TS3Tools"))),

            N("Ambitions", I.EP02,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-ambitions/buy-microcontent",    
                steam: "https://store.steampowered.com/app/47893/The_Sims_3_Ambitions/?curator_clanid=3953086"),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba, "https://www.eneba.com/origin-the-sims-3-ambitions-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A, "https://www.g2a.com/the-sims-3-ambitions-ea-app-key-global-i10000043567003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A, "https://www.g2a.com/fr/the-sims-3-ambitions-steam-gift-global-i10000043567001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo, "https://www.gamivo.com/product/the-sims-3-ambitions?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK, "https://www.hrkgame.com/en/games/product/the-simstm-3-ambitions#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, "https://www.instant-gaming.com/en/120-buy-the-sims-3-ambitions-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G, "https://k4g.com/product/the-sims-3-ambitions-origin-global-cd-key-FD33BAE1?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, "https://www.kinguin.net/category/920/the-sims-3-ambitions-expansion-pack-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, "https://www.kinguin.net/category/7650/the-sims-3-ambitions-expansion-pack-dlc-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA, "https://www.mmoga.com/EA-Games/The-Sims-3-Ambitions-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Late Night", I.EP03,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Generations", I.EP04,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Pets", I.EP05,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Showtime", I.EP06,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Showtime: Katy Perry Collector's Edition", I.EP06CE,
                Partners(
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""))),

            N("Supernatural", I.EP07,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Seasons", I.EP08,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("University Life", I.EP09,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Island Paradise", I.EP10,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Into The Future", I.EP11,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",              I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",                I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",                 I.G2A,     ""),
                    Leaf("Buy on Gamer's Outlet (EA App)",     I.GO,      ""),
                    Leaf("Buy on Gamivo (EA App)",             I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",                I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)",     I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",                I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",            I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",             I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",              I.MMOGA,   ""))));

    // ── Stuff Packs ───────────────────────────────────────────────────────────

    private static BuyTreeNode BuildStuffPacks() =>
        N("Stuff Packs", I.SPs,
            N("High-End Loft Stuff", I.SP01,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Fast Lane Stuff", I.SP02,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Outdoor Living Stuff", I.SP03,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Town Life Stuff", I.SP04,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Master Suite Stuff", I.SP05,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO,      ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on GVGMall (EA App)",        I.GVG,     ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            // Discontinued — pops a warning when the node is selected
            new BuyTreeNode
            {
                Label   = "Katy Perry's Sweet Treats",
                Icon    = LoadIcon(I.SP06),
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
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("70s, 80s & 90s Stuff", I.SP08,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))),

            N("Movie Stuff", I.SP09,
                Official(),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
                    Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO,      ""),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
                    Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))));

    // ── Premium Worlds ────────────────────────────────────────────────────────

    private static BuyTreeNode BuildPremiumWorlds() =>
        N("Premium Worlds", I.Worlds,
            Leaf("Aurora Skies",                        I.Aurora,    ""),
            Leaf("Barnacle Bay",                        I.Barnacle,  ""),
            Leaf("Dragon Valley",                       I.Dragon,    ""),
            Leaf("Hidden Springs",                      I.Hidden,    ""),
            Leaf("Lucky Palms",                         I.Lucky,     ""),
            Leaf("Lunar Lakes",                         I.Lunar,     ""),
            Leaf("Midnight Hollow",                     I.Midnight,  ""),
            Leaf("Monte Vista",                         I.Monte,     ""),
            Leaf("Riverview",                           I.Riverview, ""),
            Leaf("Roaring Heights",                     I.Roaring,   ""),
            Leaf("Hidden Springs & Monte Vista Bundle", I.HSMV,      ""));

    // ── Navigation ────────────────────────────────────────────────────────────

    private void TweakButton_Click(object sender, RoutedEventArgs e) => Close();

    private void TweakButton_Context(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        => e.Handled = true;
}
