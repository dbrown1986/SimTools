using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfBox = System.Windows.MessageBox;

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

    // Standard "* Official" sub-node with EA App + Steam + GOG leaves
    // Pass null for a platform to omit it entirely; pass "" to include with no URL yet
    private static BuyTreeNode Official(string? ea = "", string? steam = "", string? gog = "", string icon = "")
    {
        var node = new BuyTreeNode
        {
            Label = "* Official",
            Icon  = LoadIcon(string.IsNullOrEmpty(icon) ? I.EA : icon)
        };
        if (ea    is not null) node.Children.Add(Leaf("Buy on EA App", I.EA,    ea));
        if (steam is not null) node.Children.Add(Leaf("Buy on Steam",  I.Steam, steam));
        if (gog   is not null) node.Children.Add(Leaf("Buy on GOG",    I.GOG,   gog));
        return node;
    }

    // Standard "Partners" sub-node (digital grey-market vendors)
    private static BuyTreeNode Partners(params BuyTreeNode[] items) =>
        N("Partners", I.G2A, items);

    // Standard "* Retail" sub-node — Amazon and eBay
    private static BuyTreeNode RetailVendors(string amazon = "", string ebay = "") =>
        N("* Retail", I.Amazon,
            Leaf("Buy on Amazon", I.Amazon, amazon),
            Leaf("Buy on eBay",   I.eBay,   ebay));

    // ── Icon path constants ───────────────────────────────────────────────────
    private static class I
    {
        public const string Sims1        = "Images/Icons/Sims1.ico";
        public const string Sims2        = "Images/Icons/Sims2.ico";
        public const string Sims3        = "Images/Icons/Sims3.ico";
        public const string SimsMedieval = "Images/Icons/TSM.ico";
        public const string SC2K         = "Images/Icons/SC2K.ico";
        public const string SC3KU        = "Images/Icons/SC3KU.ico";
        public const string SC4DE        = "Images/Icons/SC4DE.ico";
        public const string SC2013       = "Images/Icons/SC2013.ico";
        public const string SimCopter    = "Images/Icons/Copter.ico";
        public const string Streets      = "Images/Icons/Streets.ico";
        public const string TSCastaway   = "Images/Icons/TSCastaway.ico";
        public const string TSLife       = "Images/Icons/TSLife.ico";
        public const string TSPets       = "Images/Icons/TSPets.ico";
        public const string EPs          = "Images/Icons/SimsEPs.ico";
        public const string SPs          = "Images/Icons/SimsSPs.ico";
        public const string Store        = "Images/Icons/Store.ico";
        public const string Worlds       = "Images/Icons/Worlds.ico";
        // Digital vendors
        public const string EA      = "Images/Icons/vendors/ea.ico";
        public const string Steam   = "Images/Icons/vendors/steam.ico";
        public const string GOG     = "Images/Icons/vendors/gog.ico";
        public const string Eneba   = "Images/Icons/vendors/eneba.ico";
        public const string G2A     = "Images/Icons/vendors/g2a.ico";
        public const string Gamivo  = "Images/Icons/vendors/gamivo.ico";
        public const string HRK     = "Images/Icons/vendors/hrk.ico";
        public const string IG      = "Images/Icons/vendors/ig.ico";
        public const string K4G     = "Images/Icons/vendors/k4g.ico";
        public const string Kinguin = "Images/Icons/vendors/kinguin.ico";
        public const string MMOGA   = "Images/Icons/vendors/mmoga.ico";
        public const string GVG     = "Images/Icons/vendors/gvg.ico";
        public const string GO      = "Images/Icons/vendors/gamers-outlet.ico";
        // Retail vendors
        public const string Amazon  = "Images/Icons/vendors/amazon.ico";
        public const string eBay    = "Images/Icons/vendors/ebay.ico";
        // EP icons
        public const string EP01   = "Images/Icons/Sims3EP01.ico";
        public const string EP02   = "Images/Icons/Sims3EP02.ico";
        public const string EP03   = "Images/Icons/Sims3EP03.ico";
        public const string EP04   = "Images/Icons/Sims3EP04.ico";
        public const string EP05   = "Images/Icons/Sims3EP05.ico";
        public const string EP06   = "Images/Icons/Sims3EP06.ico";
        public const string EP06CE = "Images/Icons/Sims3EP06CE.ico";
        public const string EP07   = "Images/Icons/Sims3EP07.ico";
        public const string EP08   = "Images/Icons/Sims3EP08.ico";
        public const string EP09   = "Images/Icons/Sims3EP09.ico";
        public const string EP10   = "Images/Icons/Sims3EP10.ico";
        public const string EP11   = "Images/Icons/Sims3EP11.ico";
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
        BuyGamesDigital.ItemsSource = new ObservableCollection<BuyTreeNode>
        {
            BuildSims1(),
            BuildSims2(),
            BuildSims3(),
            BuildSC2K(),
            BuildSC3KU(),
            BuildSC4DE(),
            BuildSC2013(),
            BuildSimsMedieval(),
        //  BuildSimsMedievalPN(),
        };

        BuyGamesRetail.ItemsSource = new ObservableCollection<BuyTreeNode>
        {
            BuildSims3Retail(),
            BuildSC2KRetail(),
            BuildSC3KURetail(),
            BuildSC4DERetail(),
            BuildSC2013Retail(),
            BuildSimsMedievalRetail(),
            BuildSimsMedievalPNRetail(),
        };
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DIGITAL TREES
    // ══════════════════════════════════════════════════════════════════════════

    // ── The Sims: Legacy Collection ───────────────────────────────────────────

    private static BuyTreeNode BuildSims1() =>
        N("The Sims: Legacy Collection", I.Sims1,
            Official(
                ea:    "https://www.ea.com/en/games/the-sims/the-sims-25th-anniv-edition",
                steam: "https://store.steampowered.com/app/3314060/The_Sims_Legacy_Collection/",
                gog:   null,
                icon:  I.EA));

    // ── The Sims 2: Legacy Collection ─────────────────────────────────────────

    private static BuyTreeNode BuildSims2() =>
        N("The Sims 2: Legacy Collection", I.Sims2,
            Official(
                ea:    "https://www.ea.com/games/the-sims/the-sims-2-25th-anniv-edition",
                steam: "https://store.steampowered.com/app/3314070/The_Sims_2_Legacy_Collection/",
                gog:   null,
                icon:  I.EA));

    // ── The Sims 3 ────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildSims3() =>
        N("The Sims 3", I.Sims3,
            BuildBaseGame(),
            BuildExpansions(),
            BuildStuffPacks(),
            BuildPremiumWorlds(),
            Leaf("Store Items", I.Store, "https://store.thesims3.com"));

    // ── Base Game ─────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildBaseGame() =>
        N("Base Game", I.Sims3,
            Official(
                ea:    "https://www.ea.com/games/the-sims/the-sims-3",
                steam: "https://store.steampowered.com/app/47890/The_Sims_3",
                gog:   null,
                icon:  I.EA),
            Partners(
                Leaf("Buy on Eneba (EA App)",   I.Eneba,    "https://www.eneba.com/origin-the-sims-3-origin-key-global?af_id=TS3Tools%C2%A4cy=USD%C2%AEion=global"),
                Leaf("Buy on G2A (EA App)",     I.G2A,      "https://www.g2a.com/the-sims-3-origin-key-global-i10000023727004?gname=ts3tools"),
                Leaf("Buy on G2A (Steam)",      I.G2A,      "https://www.g2a.com/the-sims-3-steam-gift-global-i10000023727001?gname=ts3tools"),
                Leaf("Buy on Gamivo (EA App)",  I.Gamivo,   "https://www.gamivo.com/product/the-sims-3?glv=p1b0e0fh"),
                Leaf("Buy on K4G (EA App)",     I.K4G,      "https://k4g.com/product/the-sims-3-origin-global-cd-key-5C2BA064?r=ts3tools"),
                Leaf("Buy on Kinguin (EA App)", I.Kinguin,  "https://www.kinguin.net/category/131/the-sims-3-origin-cd-key/?r=66716563950ad"),
                Leaf("Buy on Kinguin (Steam)",  I.Kinguin,  "https://www.kinguin.net/category/32666/the-sims-3-steam-cd-key/?r=66716563950ad"),
                Leaf("Buy on MMOGA (EA App)",   I.MMOGA,    "https://www.mmoga.com/EA-Games/Sims-3-Key-Free-download-included.html?ref=63563&Partner=TS3Tools")));

    // ── Expansions ────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildExpansions() =>
        N("Expansions", I.EPs,
            N("World Adventures", I.EP01,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-world-adventures-expansion-pack/buy-microcontent",
                steam: "https://store.steampowered.com/app/47892/The_Sims_3_World_Adventures",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/sims-3-website-the-sims-3-world-adventures-dlc-origin-key-global?af_id=TS3Tools%C2%A4cy=USD%C2%AEion=global&utm_medium=af&utm_source=TS3Tools"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-world-adventures-ea-app-key-global-i10000043589003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-world-adventures-steam-gift-global-i10000043589001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-world-adventures?glv=p1b0e0fh"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-world-adventures-origin-global-cd-key-4EBA6D3A?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/1940/the-sims-3-world-adventures-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7660/the-sims-3-world-adventures-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-World-Adventures-addon.html?ref=63563&Partner=TS3Tools"))),

            N("Ambitions", I.EP02,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/buy/addon/the-sims-3-ambitions",
                steam: "https://store.steampowered.com/app/47893/The_Sims_3_Ambitions",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/sims-3-website-the-sims-3-ambitions-dlc-origin-key-global?af_id=TS3Tools%C2%A4cy=USD%C2%AEion=global&utm_medium=af&utm_source=TS3Tools"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-ambitions-ea-app-key-global-i10000043567003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/fr/the-sims-3-ambitions-steam-gift-global-i10000043567001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-ambitions?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-ambitions#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/120-buy-the-sims-3-ambitions-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-ambitions-origin-global-cd-key-FD33BAE1?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/920/the-sims-3-ambitions-expansion-pack-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7650/the-sims-3-ambitions-expansion-pack-dlc-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Ambitions-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Late Night", I.EP03,
                Official(
                ea: "https://www.ea.com/en/games/the-sims/the-sims-3/buy/addon/the-sims-3-late-night-expansion-pack",
                steam: "https://store.steampowered.com/app/47894/The_Sims_3_Late_Night",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/sims-3-website-the-sims-3-late-night-dlc-origin-key-global?af_id=TS3Tools&currency=USD&region=global&utm_medium=af&utm_source=TS3Tools"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-late-night-ea-app-key-global-i10000043600001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-late-night-pc-steam-gift-global-i10000044185001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-late-night?glv=p1b0e0fh"),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-late-night-pc-steam-gift-global-en-de-fr-it-pl-cs-pt-es-tr-standard?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-late-night#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/81-buy-the-sims-3-late-night-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-late-night-origin-global-cd-key-81C0A15E?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/933/the-sims-3-late-night-expansion-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/28489/the-sims-3-late-night-expansion-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Late-Night-Expansion-Pack.html?ref=63563&Partner=TS3Tools"))),

            N("Generations", I.EP04,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-generations/buy-microcontent",
                steam: "https://store.steampowered.com/app/47898/The_Sims_3_Generations/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-generations-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-generations-ea-app-key-global-i10000043568005?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-generations-steam-gift-global-i10000043568003?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-generations?glv=p1b0e0fh"),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-generations-steam-gift-global-en-de-fr-it-pl-cs-pt-es-tr-standard-pc?glv=p1b0e0fh&currency=usd"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-generations#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/123-buy-the-sims-3-generations-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-generations-origin-global-cd-key-8C25D117?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/931/the-sims-3-generations-expansion-pack-ea-origin-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7654/the-sims-3-generations-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Generations-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Pets", I.EP05,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-pets/buy-microcontent",
                steam: "https://store.steampowered.com/app/47930/The_Sims_3_Pets/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-pets-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-pets-key-global-i10000043577001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-pets-steam-gift-global-i10000043582001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-pets?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-pets#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/108-buy-the-sims-3-pets-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-pets-sims-3-online-global-cd-key-CEBDE5A1?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/926/the-sims-3-pets-expansion-pack-ea-origin-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7652/the-sims-3-pets-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Pets-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Showtime", I.EP06,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-showtime/buy-microcontent",
                steam: "https://store.steampowered.com/app/47932/The_Sims_3_Showtime/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-showtime-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-showtime-ea-app-key-global-i10000001292003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-showtime-steam-gift-global-i10000001292001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-showtime?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-showtime#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/109-buy-the-sims-3-showtime-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-showtime-origin-global-cd-key-A2FA0A04?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/25314/the-sims-3-showtime-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7643/the-sims-3-showtime-dlc-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Showtime-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Showtime: Katy Perry Collector's Edition", I.EP06CE,
                Partners(
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-showtime-katy-perry-collectors-edition-ea-app-key-global-i10000020285001?gname=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/118-buy-the-sims-3-showtime-katy-perry-collector-s-edition-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/7230/the-sims-3-katy-perry-collector-s-edition-origin-key/?r=66716563950ad"))),

            N("Supernatural", I.EP07,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-supernatural/buy-bundle",
                steam: "https://store.steampowered.com/app/223593/The_Sims_3_Supernatural/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-supernatural-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-supernatural-key-global-i10000043573001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-supernatural-steam-gift-global-i10000048125001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-supernatural?glv=p1b0e0fh"),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-supernatural-steam-gift-global-en-de-fr-it-pl-cs-pt-es-tr-standard-pc?glv=p1b0e0fh&currency=usd"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-supernatural#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/80-buy-the-sims-3-supernatural-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-supernatural-origin-global-cd-key-8AD3CD23?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/1012/the-sims-3-supernatural-dlc-pack-ea-origin-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7658/the-sims-3-supernatural-dlc-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Supernatural-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Seasons", I.EP08,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-seasons/buy-microcontent",
                steam: "https://store.steampowered.com/app/223594/The_Sims_3_Seasons/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-seasons-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-seasons-origin-key-global-i10000047664003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-seasons-steam-gift-global-i10000047664001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-seasons?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-seasons#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/110-buy-the-sims-3-seasons-pc-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-seasons-origin-global-cd-key-A125D507?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/193/the-sims-3-seasons-expansion-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7653/the-sims-3-seasons-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Seasons-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("University Life", I.EP09,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-university-life/buy-microcontent",
                steam: "https://store.steampowered.com/app/223597/The_Sims_3_University_Life/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-university-life-dlc-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-university-life-ea-app-key-global-i10000043727001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-university-life-steam-gift-global-i10000050746001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-university-life?glv=p1b0e0fh"),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-university-life-steam-gift-global-en-de-fr-it-pl-cs-pt-es-tr-standard-pc?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-university-life#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/167-buy-the-sims-3-university-life-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-university-life-origin-global-cd-key-EE9A5F0D?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/1150/the-sims-3-university-life-expansion-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7651/the-sims-3-university-life-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-University-Life-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Island Paradise", I.EP10,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-island-paradise/buy-microcontent",
                steam: "https://store.steampowered.com/app/223598/The_Sims_3_Island_Paradise/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-island-paradise-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-island-paradise-ea-app-key-global-i10000047665003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-island-paradise-steam-gift-global-i10000047665001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-island-paradise?glv=p1b0e0fh"),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-island-paradise-pc-steam-gift-global-en-de-fr-it-pl-cs-pt-es-tr-standard?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-island-paradise#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/219-buy-the-sims-3-island-paradise-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-island-paradise-origin-global-cd-key-DD3F2E28?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/1939/the-sims-3-island-paradise-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7657/the-sims-3-island-paradise-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Island-Paradise-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Into The Future", I.EP11,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-into-the-future/buy-microcontent",
                steam: "https://store.steampowered.com/app/249180/The_Sims_3__Into_the_Future/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",              I.Eneba, "https://www.eneba.com/origin-the-sims-3-into-the-future-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",                I.G2A,   "https://www.g2a.com/the-sims-3-into-the-future-ea-app-key-global-i10000043634003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",                 I.G2A,   "https://www.g2a.com/the-sims-3-into-the-future-steam-gift-global-i10000043634001?gname=ts3tools"),
                    Leaf("Buy on Gamer's Outlet (EA App)",     I.GO,    "https://www.gamers-outlet.net/en/buy-the-sims-3-into-the-future-cd-key-origin-global?tracking=R5VRFA0ohSmVuSK3DWrACuIV2Nd4g0g9XjbT5xGR9RslsEozej3OEUIeEJn3hbAr"),
                    Leaf("Buy on Gamivo (EA App)",             I.Gamivo, "https://www.gamivo.com/product/the-sims-3-into-the-future?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",                I.HRK,   "https://www.hrkgame.com/en/games/product/the-sims-3-into-the-future#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)",     I.IG,    "https://www.instant-gaming.com/en/244-buy-the-sims-3-into-the-future-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",                I.K4G,   "https://k4g.com/product/the-sims-3-into-the-future-origin-global-cd-key-39F1A390?ref=63563&Partner=TS3Tools"),
                    Leaf("Buy on Kinguin (EA App)",            I.Kinguin, "https://www.kinguin.net/category/2832/the-sims-3-into-the-future-expansion-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",             I.Kinguin, "https://www.kinguin.net/category/23361/the-sims-3-into-the-future-expansion-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",              I.MMOGA, "https://www.mmoga.com/EA-Games/The-Sims-3-Into-the-Future-Addon.html?ref=63563&Partner=TS3Tools"))));

    // ── Stuff Packs ───────────────────────────────────────────────────────────

    private static BuyTreeNode BuildStuffPacks() =>
        N("Stuff Packs", I.SPs,
            N("High-End Loft Stuff", I.SP01,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-high-end-loft-stuff/buy-expansion-pack",
                steam: "https://store.steampowered.com/app/47895/The_Sims_3_HighEnd_Loft_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-high-end-loft-stuff-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-high-end-loft-stuff-ea-app-key-global-i10000043569002?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-high-end-loft-stuff-1?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-high-end-loft-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/121-buy-the-sims-3-high-end-loft-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-high-end-loft-stuff-origin-global-cd-key-266F0413?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/2337/the-sims-3-high-end-loft-stuff-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/24041/the-sims-3-high-end-loft-stuff-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-High-End-Loft-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Fast Lane Stuff", I.SP02,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-fast-lane-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/47896/The_Sims_3_Fast_Lane_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-fast-lane-stuff-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-fast-lane-stuff-ea-app-key-global-i10000043841003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-fast-lane-stuff-steam-gift-global-i10000043841001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-fast-lane-stuff?glv=p1b0e0fh"),
                    Leaf("Buy on Gamivo (Steam)",          I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-fast-lane-stuff-steam-gift-global-en-de-fr-it-pl-cs-pt-es-tr-standard-pc?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-fast-lane-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/117-buy-the-sims-3-fast-lane-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-fast-lane-stuff-origin-global-cd-key-967F7011?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/3319/the-sims-3-fast-lane-stuff-expansion-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7661/the-sims-3-fast-lane-stuff-expansion-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Fast-Lane-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Outdoor Living Stuff", I.SP03,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-outdoor-living-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/47897/The_Sims_3_Outdoor_Living_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-outdoor-living-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-outdoor-living-stuff-ea-app-key-global-i10000044314003?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-outdoor-living-stuff-steam-gift-global-i10000044314001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-outdoor-living?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-outdoor-living-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/126-buy-the-sims-3-outdoor-living-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-outdoor-living-stuff-origin-global-cd-key-91B1F1F7?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/5175/the-sims-3-outdoor-living-stuff-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7649/the-sims-3-0-outdoor-living-stuff-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Outdoor-Living-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Town Life Stuff", I.SP04,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-town-life-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/47899/The_Sims_3_Town_Life_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-town-life-stuff-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-town-life-stuff-pc-ea-app-key-global-i10000044318001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-town-life-stuff-pc-steam-gift-global-i10000047458001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-town-life-stuff?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-town-life-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/127-buy-the-sims-3-town-life-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-town-life-stuff-origin-global-cd-key-BD248F04?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/109412/the-sims-3-town-life-stuff-pack-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7659/the-sims-3-town-life-stuff-expansion-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Town-Life-Stuff.html?ref=63563&Partner=TS3Tools"))),

            N("Master Suite Stuff", I.SP05,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-master-suite-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/47931/The_Sims_3_Master_Suite_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-master-suite-stuff-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-master-suite-stuff-pc-ea-app-key-global-i10000044307001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-master-suite-stuff-steam-gift-global-i10000047462001?gname=ts3tools"),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO,        "https://www.gamers-outlet.net/en/buy-the-sims-3-master-suite-stuff-cd-key-origin-global?tracking=R5VRFA0ohSmVuSK3DWrACuIV2Nd4g0g9XjbT5xGR9RslsEozej3OEUIeEJn3hbAr"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-master-suite-stuff?glv=p1b0e0fh"),
                    Leaf("Buy on GVGMall (EA App)",        I.GVG,       "https://www.gvgmall.com/origin-games-cdkey/the-sims-3-master-suite-stuff-origin-cd-key.html?urd=ts3tools"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-simstm-3-master-suite-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/125-buy-the-sims-3-master-suite-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-master-suite-stuff-origin-global-cd-key-444BDE0B?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/3293/the-sims-3-master-suite-stuff-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7656/the-sims-3-master-suite-stuff-expansion-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Master-Suite-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))),

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
                        Leaf("Buy on Eneba (EA App)",          I.Eneba,   "https://www.eneba.com/sims-3-website-the-sims-3-katy-perrys-sweet-treats-dlc-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                        Leaf("Buy on G2A (EA App)",            I.G2A,     "https://www.g2a.com/the-sims-3-katy-perrys-sweet-treats-ea-app-key-global-i10000045460003?gname=ts3tools"),
                        Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  "https://www.gamivo.com/product/the-sims-3-katy-perrys-sweet-treats?glv=p1b0e0fh"),
                        Leaf("Buy on Instant Gaming (EA App)", I.IG,      "https://www.instant-gaming.com/en/124-buy-the-sims-3-katy-perry-s-sweet-treats-pc-mac-game/?igr=ts3tools"),
                        Leaf("Buy on HRK (EA App)",            I.HRK,     "https://www.hrkgame.com/en/games/product/the-sims-3-katy-perrys-sweet-treats#a_aid=ts3tools"),
                        Leaf("Buy on Kinguin (EA App)",        I.Kinguin, "https://www.kinguin.net/category/560/the-sims-3-katy-perry-s-sweet-treats-dlc-origin-cd-key/?r=66716563950ad"))
                }
            },

            N("Diesel Stuff", I.SP07,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-diesel-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/223592/The_Sims_3_Diesel_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-diesel-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-diesel-stuff-pack-ea-app-global-i10000044815001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-diesel-stuff-steam-gift-global-i10000049251001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-diesel-stuff?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-diesel-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/122-buy-the-sims-3-diesel-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-diesel-stuff-pack-origin-global-cd-key-E1E0B89D?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/3320/the-sims-3-diesel-stuff-pack-ea-origin-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7636/the-sims-3-diesel-stuff-dlc-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Diesel-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("70s, 80s & 90s Stuff", I.SP08,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-70s-80s-and-90s-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/223595/The_Sims_3_70s_80s_and_90s/",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-70s-80s-90s-stuff-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-70s-80s-90s-stuff-ea-app-key-global-i10000043564001?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-70s-80s-90s-steam-gift-global-i10000049255001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-70s-80s-and-90s-stuff?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-70s-80s-and-90s#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/119-buy-the-sims-3-70-s-80-s-and-90-s-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-70s-80s-90s-stuff-origin-global-cd-key-2BB56158?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/1774/the-sims-3-70s-80s-90s-dlc-pack-ea-origin-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7655/the-sims-3-70s-80s-90s-stuff-pack-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-70s-80s-and-90s-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))),

            N("Movie Stuff", I.SP09,
                Official(
                ea: "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-movie-stuff/buy-microcontent",
                steam: "https://store.steampowered.com/app/249181/The_Sims_3__Movie_Stuff/?cc=us",
                gog:  null,
                icon: I.EA),
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,     "https://www.eneba.com/origin-the-sims-3-movie-stuff-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,       "https://www.g2a.com/the-sims-3-movie-stuff-pc-ea-app-key-global-i10000043037002?gname=ts3tools"),
                    Leaf("Buy on G2A (Steam)",             I.G2A,       "https://www.g2a.com/the-sims-3-movie-stuff-steam-gift-global-i10000047454001?gname=ts3tools"),
                    Leaf("Buy on Gamer's Outlet (EA App)", I.GO,        "https://www.gamers-outlet.net/en/buy-the-sims-3-movie-stuff-cd-key-origin-global?tracking=R5VRFA0ohSmVuSK3DWrACuIV2Nd4g0g9XjbT5xGR9RslsEozej3OEUIeEJn3hbAr"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,    "https://www.gamivo.com/product/the-sims-3-movie-stuff?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (EA App)",            I.HRK,       "https://www.hrkgame.com/en/games/product/the-sims-3-movie-stuff#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,        "https://www.instant-gaming.com/en/1162-buy-the-sims-3-movie-stuff-pc-mac-game/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,       "https://k4g.com/product/the-sims-3-movie-stuff-origin-global-cd-key-001D8720?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin,   "https://www.kinguin.net/category/2576/the-sims-3-movie-stuff-dlc-origin-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)",         I.Kinguin,   "https://www.kinguin.net/category/7639/the-sims-3-movie-stuff-dlc-steam-gift/?r=66716563950ad"),
                    Leaf("Buy on MMOGA (EA App)",          I.MMOGA,     "https://www.mmoga.com/EA-Games/The-Sims-3-Movie-Stuff-Addon.html?ref=63563&Partner=TS3Tools"))));

    // ── Premium Worlds ────────────────────────────────────────────────────────

    private static BuyTreeNode BuildPremiumWorlds() =>
        N("Premium Worlds", I.Worlds,
            Leaf("Aurora Skies",                        I.Aurora,     "https://store.thesims3.com/auroraskies.html?categoryId=12642"),
            Leaf("Barnacle Bay",                        I.Barnacle,   "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-barnacle-bay/buy-microcontent"),
            Leaf("Dragon Valley: Gold Edition",         I.Dragon,     "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-dragon-valley-gold-edition/buy-microcontent"),
            Leaf("Dragon Valley: Standard Edition",     I.Dragon,     "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-dragon-valley/buy-microcontent"),
            Leaf("Hidden Springs",                      I.Hidden,     "https://store.thesims3.com/hiddenSprings.html?categoryId=12642"),
            Leaf("Lucky Palms",                         I.Lucky,      "https://store.thesims3.com/luckypalms.html?categoryId=12642"),
            Leaf("Lunar Lakes",                         I.Lunar,      "https://store.thesims3.com/lunarlakes.html?categoryId=12642"),
            Leaf("Midnight Hollow",                     I.Midnight,   "https://store.thesims3.com/midnighthollow.html?categoryId=12642"),
            Leaf("Monte Vista",                         I.Monte,      "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-monte-vista/buy-microcontent"),
            Leaf("Riverview",                           I.Riverview,  "https://store.thesims3.com/riverview.html?categoryId=12642"),
            Leaf("Roaring Heights: Gold Edition",       I.Roaring,    "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-roaring-heights-gold-edition/buy-microcontent"),
            Leaf("Roaring Heights: Standard Edition", I.Roaring,      "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-roaring-heights/buy-microcontent"),
            Leaf("Hidden Springs & Monte Vista Bundle", I.HSMV,       "https://www.ea.com/games/the-sims/the-sims-3/the-sims-3-worlds-bundle/buy-bundle?steps=pc"));

    // ── The Sims: Medieval ──────────────────────────────────────────────────────────

    // Discontinued — pops a warning when the node is selected
    private static BuyTreeNode BuildSimsMedieval() =>
        new()
        {
            Label   = "The Sims: Medieval",
            Icon    = LoadIcon(I.SimsMedieval),
            Message = "The Sims: Medieval is no longer sold or produced, and therefore " +
                      "usually comes with a hefty price tag. It may also often be out of stock.",
            Children =
            {
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,   "https://www.eneba.com/origin-the-sims-medieval-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     "https://www.g2a.com/the-sims-medieval-ea-app-key-global-i10000043987003?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  "https://www.gamivo.com/product/the-sims-medieval?glv=p1b0e0fh"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     "https://k4g.com/product/the-sims-medieval-ea-app-global-cd-key-AC51F906?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, "https://www.kinguin.net/category/1816/the-sims-medieval-ea-app-cd-key?r=66716563950ad")),
            }
        };

    // ── The Sims Medieval: Pirates & Nobles ──────────────────────────────────────────────────────────

    // Discontinued — pops a warning when the node is selected
    // private static BuyTreeNode BuildSimsMedievalPN() =>
    //    new()
    //    {
    //        Label = "The Sims Medieval: Pirates & Nobles",
    //        Icon = LoadIcon(I.SimsMedieval),
    //        Message = "The Sims Medieval: Pirates & Nobles is no longer sold or produced, and therefore " +
    //                  "usually comes with a hefty price tag. It may also often be out of stock.",
    //        Children =
    //        {
    //            Partners(
    //                Leaf("Buy on Eneba (EA App)",          I.Eneba,   ""),
    //                Leaf("Buy on G2A (EA App)",            I.G2A,     ""),
    //                Leaf("Buy on G2A (Steam)",             I.G2A,     ""),
    //                Leaf("Buy on Gamer's Outlet (EA App)", I.GO,      ""),
    //                Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  ""),
    //                Leaf("Buy on HRK (EA App)",            I.HRK,     ""),
    //                Leaf("Buy on Instant Gaming (EA App)", I.IG,      ""),
    //                Leaf("Buy on K4G (EA App)",            I.K4G,     ""),
    //                Leaf("Buy on Kinguin (EA App)",        I.Kinguin, ""),
    //                Leaf("Buy on Kinguin (Steam)",         I.Kinguin, ""),
    //                Leaf("Buy on MMOGA (EA App)",          I.MMOGA,   ""))
    //        }
    //    };

    // ── SimCity 2000 ──────────────────────────────────────────────────────────

    private static BuyTreeNode BuildSC2K() =>
        N("SimCity 2000", I.SC2K,
            Official(
                ea:    "https://www.ea.com/en/games/simcity/simcity-2000",
                steam: null,
                gog:   "https://www.gog.com/en/game/simcity_2000_special_edition",
                icon:  I.EA),
            Partners(
                Leaf("Buy on Eneba (GOG)", I.Eneba, "https://www.eneba.com/origin-simcity-2000-special-edition-gog-com-key-global?af_id=TS3Tools&currency=USD&region=global"),
                Leaf("Buy on G2A (EA App)", I.G2A, "https://www.g2a.com/simcity-2000-special-edition-ea-app-key-global-i10000000711001?gname=ts3tools"),
                Leaf("Buy on G2A (GOG)", I.G2A, "https://www.g2a.com/simcity-2000-special-edition-gogcom-key-global-i10000000711002?gname=ts3tools"),
                Leaf("Buy on Gamivo (EA App)", I.Gamivo, "https://www.gamivo.com/product/simcity-2000-pc-ea-app-global-standard?glv=p1b0e0fh"),
                Leaf("Buy on Kinguin (EA App)", I.Kinguin, "https://www.kinguin.net/category/16138/simcity-2000-special-edition-ea-app-cd-key?r=66716563950ad")));

    // ── SimCity 3000 Unlimited ────────────────────────────────────────────────

    private static BuyTreeNode BuildSC3KU() =>
        N("SimCity 3000 Unlimited", I.SC3KU,
            Official(
                ea:    null,
                steam: "https://store.steampowered.com/app/2741560/SimCity_3000_Unlimited/",
                gog:   "https://www.gog.com/en/game/simcity_3000",
                icon:  I.EA),
                        Partners(
                Leaf("Buy on G2A (GOG)", I.G2A, "https://www.g2a.com/simcity-3000-unlimited-gogcom-key-global-i10000149205002?gname=ts3tools"),
                Leaf("Buy on Kinguin (EA App)", I.Kinguin, "https://www.kinguin.net/category/63343/simcity-3000-unlimited-gog-cd-key?r=66716563950ad")));

    // ── SimCity 4 Deluxe Edition ──────────────────────────────────────────────

    private static BuyTreeNode BuildSC4DE() =>
        N("SimCity 4 Deluxe Edition", I.SC4DE,
            Official(
                ea: "https://www.ea.com/en/games/simcity/simcity-4",
                steam: "https://store.steampowered.com/app/24780/SimCity_4_Deluxe_Edition/",
                gog: "https://www.gog.com/en/game/simcity_4_deluxe_edition",
                icon: I.EA),
            Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, "https://www.eneba.com/ea-app-simcity-4-deluxe-edition-pc-ea-app-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on Eneba (Steam)", I.Eneba, "https://www.eneba.com/steam-simcity-4-deluxe-edition-steam-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (Steam)", I.G2A, "https://www.g2a.com/simcity-4-deluxe-edition-pc-steam-key-global-i10000005243004?gname=ts3tools"),
                    Leaf("Buy on Gamivo (Steam)", I.Gamivo, "https://www.gamivo.com/product/simcity-4-deluxe-edition?glv=p1b0e0fh"),
                    Leaf("Buy on HRK (Steam)", I.HRK, "https://www.hrkgame.com/en/product/simcity-4-deluxe-edition-steam-edition#a_aid=ts3tools"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG, "https://www.instant-gaming.com/en/2996-buy-simcity-4-deluxe-edition-deluxe-edition-pc-mac-game-steam//?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)", I.K4G, "https://k4g.com/product/simcity-4-steam-global-cd-key-deluxe-edition-cd-key-2A724CC8?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, "https://www.kinguin.net/category/2860/simcity-4-deluxe-edition-ea-app-cd-key/?r=66716563950ad"),
                    Leaf("Buy on Kinguin (Steam)", I.Kinguin, "https://www.kinguin.net/category/7787/simcity-4-deluxe-edition-steam-key/?r=66716563950ad")));

    // ── SimCity (2013) ────────────────────────────────────────────────────────

    private static BuyTreeNode BuildSC2013() =>
        N("SimCity 2013", I.SC2013,
            Official(
                ea:    "https://www.ea.com/en/games/simcity/simcity",
                steam: null,
                gog:   null,
                icon:  I.EA),
            Partners(
                    Leaf("Buy on Eneba (EA App)", I.Eneba, "https://www.eneba.com/origin-simcity-eng-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)", I.G2A, "https://www.g2a.com/simcity-standard-edition-english-only-ea-app-key-global-i10000043863001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)", I.Gamivo, "https://www.gamivo.com/product/simcity-eng?glv=p1b0e0fh"),
                    Leaf("Buy on K4G (EA App)", I.K4G, "https://k4g.com/product/simcity-ea-app-global-complete-edition-cd-key-B4F1E98E?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)", I.Kinguin, "https://www.kinguin.net/category/545/simcity-en-language-only-ea-app-cd-key?r=66716563950ad")));

    // ══════════════════════════════════════════════════════════════════════════
    // RETAIL TREES
    // ══════════════════════════════════════════════════════════════════════════

    // ── The Sims 3 ────────────────────────────────────────────────────────────

    private static BuyTreeNode BuildSims3Retail() =>
        N("The Sims 3", I.Sims3,
            BuildBaseGameRetail(),
            BuildExpansionsRetail(),
            BuildStuffPacksRetail());

    // ── Base Game (Retail) ────────────────────────────────────────────────────

    private static BuyTreeNode BuildBaseGameRetail() =>
        N("Base Game", I.Sims3,
            RetailVendors(
                amazon: "",
                ebay:   ""));

    // ── Expansions (Retail) ───────────────────────────────────────────────────

    private static BuyTreeNode BuildExpansionsRetail() =>
        N("Expansions", I.EPs,
            N("World Adventures",   I.EP01,   RetailVendors(amazon: "", ebay: "")),
            N("Ambitions",          I.EP02,   RetailVendors(amazon: "", ebay: "")),
            N("Late Night",         I.EP03,   RetailVendors(amazon: "", ebay: "")),
            N("Generations",        I.EP04,   RetailVendors(amazon: "", ebay: "")),
            N("Pets",               I.EP05,   RetailVendors(amazon: "", ebay: "")),
            N("Showtime",           I.EP06,   RetailVendors(amazon: "", ebay: "")),
            N("Showtime: Katy Perry Collector's Edition", I.EP06CE, RetailVendors(amazon: "", ebay: "")),
            N("Supernatural",       I.EP07,   RetailVendors(amazon: "", ebay: "")),
            N("Seasons",            I.EP08,   RetailVendors(amazon: "", ebay: "")),
            N("University Life",    I.EP09,   RetailVendors(amazon: "", ebay: "")),
            N("Island Paradise",    I.EP10,   RetailVendors(amazon: "", ebay: "")),
            N("Into The Future",    I.EP11,   RetailVendors(amazon: "", ebay: "")));

    // ── Stuff Packs (Retail) ──────────────────────────────────────────────────

    private static BuyTreeNode BuildStuffPacksRetail() =>
        N("Stuff Packs", I.SPs,
            N("High-End Loft Stuff",   I.SP01, RetailVendors(amazon: "", ebay: "")),
            N("Fast Lane Stuff",       I.SP02, RetailVendors(amazon: "", ebay: "")),
            N("Outdoor Living Stuff",  I.SP03, RetailVendors(amazon: "", ebay: "")),
            N("Town Life Stuff",       I.SP04, RetailVendors(amazon: "", ebay: "")),
            N("Master Suite Stuff",    I.SP05, RetailVendors(amazon: "", ebay: "")),

            // Discontinued — pops a warning when the node is selected
            new BuyTreeNode
            {
                Label   = "Katy Perry's Sweet Treats",
                Icon    = LoadIcon(I.SP06),
                Message = "Katy Perry's Sweet Treats is no longer sold or produced, and therefore " +
                          "usually comes with a hefty price tag. It may also often be out of stock.",
                Children = { RetailVendors(amazon: "", ebay: "") }
            },

            N("Diesel Stuff",          I.SP07, RetailVendors(amazon: "", ebay: "")),
            N("70s, 80s & 90s Stuff",  I.SP08, RetailVendors(amazon: "", ebay: "")),
            N("Movie Stuff",           I.SP09, RetailVendors(amazon: "", ebay: "")));

    // ── The Sims Medieval (Retail) ─────────────────────────────────────────────────

    // Discontinued — pops a warning when the node is selected
    private static BuyTreeNode BuildSimsMedievalRetail() =>
        new()
        {
            Label   = "The Sims: Medieval",
            Icon    = LoadIcon(I.SimsMedieval),
            Message = "The Sims: Medieval is no longer sold or produced, and therefore " +
                      "usually comes with a hefty price tag. It may also often be out of stock.",
            Children =
            {
                RetailVendors(
                    amazon: "",
                    ebay:   "")
            }
        };

    // ── The Sims Medieval: Pirates & Nobles (Retail) ─────────────────────────────────────────────────

    // Discontinued — pops a warning when the node is selected
    private static BuyTreeNode BuildSimsMedievalPNRetail() =>
        new()
        {
            Label = "The Sims Medieval: Pirates & Nobles",
            Icon = LoadIcon(I.SimsMedieval),
            Message = "The Sims Medieval: Pirates & Nobles is no longer sold or produced, and therefore " +
                      "usually comes with a hefty price tag. It may also often be out of stock.",
            Children =
            {
                RetailVendors(
                    amazon: "",
                    ebay:   "")
            }
        };

    // ── SimCity 2000 (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildSC2KRetail() =>
        N("SimCity 2000", I.SC2K,
            RetailVendors(
                amazon: "",
                ebay:   ""));

    // ── SimCity 3000 Unlimited (Retail) ───────────────────────────────────────

    private static BuyTreeNode BuildSC3KURetail() =>
        N("SimCity 3000 Unlimited", I.SC3KU,
            RetailVendors(
                amazon: "",
                ebay:   ""));

    // ── SimCity 4 Deluxe Edition (Retail) ─────────────────────────────────────

    private static BuyTreeNode BuildSC4DERetail() =>
        N("SimCity 4 Deluxe Edition", I.SC4DE,
            RetailVendors(
                amazon: "",
                ebay:   ""));

    // ── SimCity (2013) (Retail) ───────────────────────────────────────────────

    private static BuyTreeNode BuildSC2013Retail() =>
        N("SimCity 2013", I.SC2013,
            RetailVendors(
                amazon: "",
                ebay:   ""));

    // ── Navigation ────────────────────────────────────────────────────────────

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void WarningButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new BuyWarning();
        window.Owner = this;
        window.Show();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }
}
