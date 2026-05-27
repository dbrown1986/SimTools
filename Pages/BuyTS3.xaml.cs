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
            BuildSimsMedievalPN(),
        };

        BuyGamesRetail.ItemsSource = new ObservableCollection<BuyTreeNode>
        {
            BuildSims3Retail(),
            BuildSC2KRetail(),
            BuildSC3KURetail(),
            BuildSC4DERetail(),
            BuildSC2013Retail(),
            BuildCopterRetail(),
            BuildStreetsRetail(),
            BuildTSCastawayRetail(),
            BuildTSLifeRetail(),
            BuildTSPetsRetail(),
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
     private static BuyTreeNode BuildSimsMedievalPN() =>
        new()
        {
            Label = "The Sims Medieval: Pirates & Nobles",
            Icon = LoadIcon(I.SimsMedieval),
            Message = "The Sims Medieval: Pirates & Nobles is no longer sold or produced, and therefore " +
                      "usually comes with a hefty price tag. It may also often be out of stock.",
            Children =
            {
                Partners(
                    Leaf("Buy on Eneba (EA App)",          I.Eneba,       "https://www.eneba.com/origin-the-sims-medieval-pirates-and-nobles-origin-key-global?af_id=TS3Tools&currency=USD&region=global"),
                    Leaf("Buy on G2A (EA App)",            I.G2A,     "https://www.g2a.com/the-sims-medieval-pirates-and-nobles-ea-app-global-i10000010786001?gname=ts3tools"),
                    Leaf("Buy on Gamivo (EA App)",         I.Gamivo,  "https://www.gamivo.com/product/the-sims-medieval-pirates-and-nobles?glv=p1b0e0fh"),
                    Leaf("Buy on Instant Gaming (EA App)", I.IG,      "https://www.instant-gaming.com/en/129-buy-the-sims-medieval-pirates-and-nobles-pc-game-ea-app/?igr=ts3tools"),
                    Leaf("Buy on K4G (EA App)",            I.K4G,     "https://k4g.com/product/the-sims-medieval-pirates-and-nobles-ea-app-global-cd-key-7A8EC2E3?r=ts3tools"),
                    Leaf("Buy on Kinguin (EA App)",        I.Kinguin, "https://www.kinguin.net/category/4245/the-sims-medieval-pirates-and-nobles-dlc-ea-app-cd-key?r=66716563950ad")),
            }
        };

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
                amazon: "https://www.amazon.com/NEW-Sims-WIN-MAC-Videogame-Software/dp/B004Y3C6RY?crid=24CGYTAXJCM4N&dib=eyJ2IjoiMSJ9.VIMiU1xPN1C7kpF2XTQueQfPoDfsSH8QCvUWRByfafu7jzSf28yWrAx4v1hBblva9Q_qSG2C6x6HPtX8lreKujvQqBhPJjLFHpJw8EeudPhCfcbbqo2em66igvsBnwDm92aMnixvxtvFZJWGjvmTeoOkAFmOHBHeM_OdQ3P6SPiLHmAcBzHSLapbS7odza5qE5wxh7lLkUCoqsniDcLY6pJqtmOTtwWwBHoLhqn7kRA.n_MwX1DAAzxsrHuAWALPIv9FyBPQgYa-d0TTE61KFp0&dib_tag=se&keywords=the+sims+3+base+game&qid=1718752858&sprefix=the+sims+3+base+game%2Caps%2C217&sr=8-1&linkCode=ll1&tag=ts3tools-20&linkId=bdc6cba0426c559ff1308497bab62b6a&language=en_US&ref_=as_li_ss_tl",
                ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p4432023.m570.l1312&_nkw=sims+3+base+game+dvd&_sacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1"));

    // ── Expansions (Retail) ───────────────────────────────────────────────────

    private static BuyTreeNode BuildExpansionsRetail() =>
        N("Expansions", I.EPs,
            N("World Adventures",   I.EP01,   RetailVendors(amazon: "https://www.amazon.com/Sims-World-Adventures-Expansion-Pack-Mac/dp/B002NILFB0?crid=2GZLD3BJZ6UZV&dib=eyJ2IjoiMSJ9.0KHOcZHioyr2nq3I51HvZrEYw8TO_T2exIxwLN83wkKEnHMtEY5n1C13Q3IBHzdu6pE3ozzHgAXrM7Ioc2GxxDCN7w_QD6VmEDdFDaY693p8LTlXUPwg-kLji19iFJzcFPW_mdNOVpJ9C8I1A95aSwHbBp2mka48t2jnyWqUSMWfAmApeOpOeKakPdZgtCOZM-NH7FLUirHYxJM7DF0B8QW1pTqny6LhM1Qs9rF9dm8.8XCcuG1PoBVW35NQufnCLUA9Z86vYcj_Iq4DpXVCuCI&dib_tag=se&keywords=the%2Bsims%2B3%2Bworld%2Badventures&qid=1718753008&sprefix=the%2Bsims%2B3%2Bw%2Caps%2C202&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=97bf8a28a050c9eb7858ea7d04a84958&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+world+adventures+dvd&_sacat=0&_odkw=sims+3+base+game+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Ambitions",          I.EP02,   RetailVendors(amazon: "https://www.amazon.com/Sims-3-Ambitions-Mac/dp/B003AMOK9M?crid=3EH18S25QUUWI&dib=eyJ2IjoiMSJ9.6NEMNn8iNJK6iLnKxTmk3zBnhqlH1tsN7lY27ciZw5dWiA9htgeZ2kIJiPhA4Vraq82sNEspkgwBLhTdhRPuV99oGWPmQCzufiBaJVEzpstPPNXq-VWdBVgXP_os5GpeqmgGaqdNuJY6BX2FvA_vgeNfqwh6Cce6ReyEoN9Brpo.WSjNzZK5B_vdLdCEk1K2JNmsOIIegBSiJE88XCr0rSM&dib_tag=se&keywords=the+sims+3+ambitions&qid=1718753092&sprefix=the+sims+3+ambi%2Caps%2C189&sr=8-1&linkCode=ll1&tag=ts3tools-20&linkId=24f8195b19996ee030426a73b7e3b8c4&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+ambitions+dvd&_sacat=0&_odkw=sims+3+world+adventures+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Late Night",         I.EP03,   RetailVendors(amazon: "https://www.amazon.com/Sims-Late-Night-PC-Mac/dp/B003WU3CDW?crid=2VM0Y882QERML&dib=eyJ2IjoiMSJ9.8-LmIcIO4iQjOcYeR8Dcls5aXtqeLStmcUjuIdXoBckM1NAKbdm_TMP5Li6a3AMSaAKASbQh4jfi5aYZBBrgo6G0Jd9hnGCTpLBqYypodfU.fgouBhLMJuIvNGoVlkqt7fXlRM3PGNcQxKA1uwvhVjU&dib_tag=se&keywords=the%2Bsims%2B3%2Blate%2Bnight&qid=1718753165&sprefix=the%2Bsims%2B3%2Blate%2Bnight%2Caps%2C190&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=83f898d28e1b66661107631d990a6c2f&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+late+night+dvd&_sacat=0&_odkw=sims+3+ambitions+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Generations",        I.EP04,   RetailVendors(amazon: "https://www.amazon.com/Sims-Generations-Expansion-Pack-PC-Mac/dp/B004UJNN5G?crid=3LY8JIX2UCLF8&dib=eyJ2IjoiMSJ9.dAbylGBfv5dew3kOeKLOlf0mbJXEw6kZdLDt_ccpWeqc1RF11NWQDXv6mUw60lSc3IuTvOxadNotj-t2sJ3O40ma4GDiM6Sy01WAlQbUUgdmxbo8TITym4kRYrJEpf6O1JJadaC7Ml-GzsyZPDeTHZ4ZN_a9E-6UAFGR57-4k4IwMsHBktI4emFaIFISYzC7Jyy0JJnXeuKbnBMBTNdp0cB3Nqx2uRHl93jB4xrgAig.FxdMlCIcBWHmQ39qCjr1eT8nd7-e90G-OVBMdXp2vm4&dib_tag=se&keywords=the%2Bsims%2B3%2Bgenerations&qid=1718753242&sprefix=the%2Bsims%2B3%2Bge%2Caps%2C198&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=04f3fd6d71e8a34dfaf289f696a9f79d&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+generations+dvd&_sacat=0&_odkw=sims+3+late+night+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Pets",               I.EP05,   RetailVendors(amazon: "https://www.amazon.com/Sims-Plus-Pets-PC-Mac/dp/B0054IV21O?crid=13W5FX86T39CR&dib=eyJ2IjoiMSJ9.jwTnouZpn7ZfChlWuZmpaxvsvlY_gI8Y7oxrT7WBr5m8tDzFbQnBTRkLaeJ9EC1p-j50D8T20yp5ix5EjI56zHDr21WmJuvxRXnDvpNDM6WvI8LhIPSJdebMR9jZHbL_blSDMt2xYDLg0x2lqKSwsDtrXXRKMdqCVNtvxALKZc9LYEh5DLM9gJ28G_Pb0OeqL8fq3t86nSrEdhghG8w5BOrJNmqSReNQv50s5c2ig50.vFG_TcdcAdrnyTBzhf-Xq5La_-XnZwEY1SxJt_dF4b4&dib_tag=se&keywords=the%2Bsims%2B3%2Bpets&qid=1718753312&sprefix=the%2Bsims%2B3%2Bpets%2Caps%2C202&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=6e8a28be123b9b5de2611c7ca2e8e3a3&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+pets+dvd&_sacat=0&_odkw=sims+3+generations+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Showtime",           I.EP06,   RetailVendors(amazon: "https://www.amazon.com/Electronic-Arts-71151-Showtime-Download/dp/B006D3JT0U?crid=ZPFE2ECYUNTA&dib=eyJ2IjoiMSJ9.HRz6RTDjiiO2KBOcSQ-n2Nx4OYV-sZiE2Fe14k17X0xxsp4vSZfIV4uZwoI8jlTr_vZ4xCvpQWGXWTjDLTAlMK1eGwCx6PCfgyx5QR5-x2VuyqKJTe3F9rURNwEtLrajWPEdxy_U0hmphayh2z9ysejQRck0peodPmC6ayopDxcdKtxhBEaiIY4qRHVwyTJvtd33tkUtv01LO3CkhBijawbuRgo6wV76bOpBl_EvCyw.c62srLR4fVHP_YYvXHVADZNC4JmVCHmtwLsS6lcc8CI&dib_tag=se&keywords=the%2Bsims%2B3%2Bshowtime&qid=1718753445&sprefix=the%2Bsims%2B3%2Bshowtime%2Caps%2C198&sr=8-4&th=1&linkCode=ll1&tag=ts3tools-20&linkId=0bfb744aff5ae1cc4f8cee6f8642fbb0&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+showtime+dvd&_sacat=0&_odkw=sims+3+pets+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),

            // N("Showtime: Katy Perry Collector's Edition", I.EP06CE, RetailVendors(amazon: "https://www.amazon.com/Sims-Showtime-Perry-Collectors-Download/dp/B0050SYZX0?crid=1VLYRNFYC3PRB&dib=eyJ2IjoiMSJ9.1VSz13Xfuxs2u7of8wmNLsb9wbKbdqTGNmUaXoVBPfDGjHj071QN20LucGBJIEps.f4qoa8G5K6wZpsSJYq4cq4zsHC4Bd3f_D3m86pXGlVE&dib_tag=se&keywords=Showtime%3A%2BKaty%2BPerry%2BCollector%27s%2BEdition&qid=1718753531&sprefix=showtime%2Bkaty%2Bperry%2Bcollector%27s%2Bedition%2Caps%2C172&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=ee279e4ff037f2932808c32d72b5fbe1&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Showtime%3A+Katy+Perry+Collector%27s+Edition+dvd&_sacat=0&_odkw=sims+3+showtime+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
                        new BuyTreeNode
                        {
                            Label = "Showtime: Katy Perry Collector's Edition",
                            Icon = LoadIcon(I.EP06CE),
                            Message = "Showtime: Katy Perry Collector's Edition is no longer sold or produced, and therefore " +
                          "usually comes with a hefty price tag. It may also often be out of stock.",
                            Children = { RetailVendors(amazon: "https://www.amazon.com/Sims-Showtime-Perry-Collectors-Download/dp/B0050SYZX0?crid=1VLYRNFYC3PRB&dib=eyJ2IjoiMSJ9.1VSz13Xfuxs2u7of8wmNLsb9wbKbdqTGNmUaXoVBPfDGjHj071QN20LucGBJIEps.f4qoa8G5K6wZpsSJYq4cq4zsHC4Bd3f_D3m86pXGlVE&dib_tag=se&keywords=Showtime%3A%2BKaty%2BPerry%2BCollector%27s%2BEdition&qid=1718753531&sprefix=showtime%2Bkaty%2Bperry%2Bcollector%27s%2Bedition%2Caps%2C172&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=ee279e4ff037f2932808c32d72b5fbe1&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Showtime%3A+Katy+Perry+Collector%27s+Edition+dvd&_sacat=0&_odkw=sims+3+showtime+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1") }
                        },

            N("Supernatural",       I.EP07,   RetailVendors(amazon: "https://www.amazon.com/Sims-3-Supernatural-windows/dp/B008AT68UE?crid=1UZN84P0VOVYV&dib=eyJ2IjoiMSJ9.bbjiy_Ss7ZNlyartq9vipryxvnaOUwqiI6vHBk2CF7wjmF32exVHII_gSPFNcMeL98dPT5kb2VyWq6kYNy9oZbkEjB1hH5Ogitl1wggdyaCl1vWNZ9Jjpbgqsw7asXsf62m62FaUIeWbnJGfqs9NYr21K22xrKmHJ44fkvL7mgx_RnyQJ2bn-Y2sRI83SYokIPAV0mkGwCASo8yJr9lY0OTM-sZ4mDA652Fuo03gl7o.ooOv2fCHwQr34azqIsTh9jpQcekZdpDZD2Jq9UA9ajQ&dib_tag=se&keywords=sims%2B3%2Bsupernatural&qid=1718753658&sprefix=sims%2B3%2Bsuperna%2Caps%2C186&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=e73ca4de7ccfc8cdbb3124af9ff0cf11&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+supernatural+dvd&_sacat=0&_odkw=sims+3+Showtime%3A+Katy+Perry+Collector%27s+Edition+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Seasons",            I.EP08,   RetailVendors(amazon: "https://www.amazon.com/Sims-3-Seasons-windows/dp/B008QLUTHO?crid=1A1BR01EWV05S&dib=eyJ2IjoiMSJ9.LE44tkLfUqtGtULYxzrDyIM6LYb53MYfNppGEXkLPKJCeLULdwK2rp0_COz7S66-YMj0ZPbuJP7bzABLxX0sqSHPvjK3MN8xxs8Ft0lcJVU4EYqs7kPiA0tFb70bCJHRXECO53QBhcK29_oba9qTSUmGQrl3MNsa6nFFw_T_-HDTzvIrtJRCGiEGWcn6PC_ytK-qgKvyMI7aUFR0ibMuvo2Rizn55skrRvuwu2sUImg.gBdoNPJ5aq4QPuLfFektp84xTLguZZ-AqXVCinFLGBs&dib_tag=se&keywords=sims%2B3%2Bseasons&qid=1718753824&sprefix=sims%2B3%2Bseasons%2Caps%2C192&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=07ed57c375c8e92b24881d6e7aa6b380&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+seasons+dvd&_sacat=0&_odkw=sims+3+supernatural+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("University Life",    I.EP09,   RetailVendors(amazon: "https://www.amazon.com/Sims-3-University-Life-windows/dp/B00A39IEMY?crid=1PANRVMOXTB2H&dib=eyJ2IjoiMSJ9.5RC6UHRNy05TXmgrcSfWB1oq2Im5qafm2Vcf_8pWcG9btR2lMjUCwVB4ukjH1_eupop-iOtUmLXcuX12H9g_5CyGpaunulIAyChW1uHnh5YiMkYT42t9xkS_7ZKSzymHxRS3I17uu2u5MmROinUzoTpYuHNJOnWUhMlUgWq3JwPjOpN71gRjNUp4Cgft00mtwF8ZvNvHFACc_o7hcWhduqe5adesydYZJG_mbe7PL0M.4ZhTGXcWrz1-ZspDaQaVs-g6dIwoSVSQPQj6qbJOawc&dib_tag=se&keywords=sims%2B3%2Buniversity%2Blife&qid=1718753905&sprefix=sims%2B3%2Buni%2Caps%2C187&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=56f765fc408b5e835927cf9653516d4f&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+university+life+dvd&_sacat=0&_odkw=sims+3+seasons+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Island Paradise",    I.EP10,   RetailVendors(amazon: "https://www.amazon.com/Sims-Island-Paradise-PC-Mac/dp/B00BGGIWZM?crid=JMFX8CZTTUVI&dib=eyJ2IjoiMSJ9.VnSEEI3ZRs-7oeph1FyAyK8m4ozwdU6bU_YJSUpVz-EHxctu_QvYwYzEKfbYB4eJkiMRLq9Vgn001jmkTI3-GaksWLoriS93OI7YzkbjnhP46LDIhGubDKkjtez1DEx39lLXcMlzT0ovDNudJYzzRWTblcwH5AZVTexCb-VvxS7KxiHdwvfrPVJhTiHTD8tG.mYFPES-frmqkTk5RbLIW93SeT8WuiztwQ8EVHt5l_Tk&dib_tag=se&keywords=sims%2B3%2Bisland%2Bparadise&qid=1718753975&sprefix=sims%2B3%2Bisland%2Caps%2C190&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=35d2bdaa0e4a69c2acb079a98d9ee782&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+island+paradise+dvd&_sacat=0&_odkw=sims+3+university+life+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Into The Future",    I.EP11,   RetailVendors(amazon: "https://www.amazon.com/Sims-Into-Future-PC-Mac/dp/B00CTKHYEE?crid=29R829HKBIIOT&dib=eyJ2IjoiMSJ9.Uj3XbQIcTHwf_NGdVXRm2Y9HEsQ0h35KsqgQdlvjaQ9ah41gwRGqMXr1x-JA2k1m_8UoTtfr7AJh8jp7o2E4LsrJHq5DUWhEs2eb_TzTYjwv3Tlp8_PtRerh2ppwrFYAoU5mFO-WfC4xr3HJ3BQ6BZI1_HIuXfHLnQo-yvkIqlY.dEQUqp9TrHXtP2YAuqm9G1g__CZwZk3YkmhFAdc21Xs&dib_tag=se&keywords=sims%2B3%2Binto%2Bthe%2Bfuture&qid=1718754058&sprefix=sims%2B3%2Binto%2Caps%2C190&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=0212e9901681e53de73944fb7c4e8266&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+into+the+future+dvd&_sacat=0&_odkw=sims+3+island+paradise+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")));

    // ── Stuff Packs (Retail) ──────────────────────────────────────────────────

    private static BuyTreeNode BuildStuffPacksRetail() =>
        N("Stuff Packs", I.SPs,
            N("High-End Loft Stuff",   I.SP01, RetailVendors(amazon: "https://www.amazon.com/Sims-High-End-Loft-Stuff-PC/dp/B002WF12XK/ref=sr_1_1?sr=8-1", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+high+end+loft+stuff+dvd&_sacat=0&_odkw=sims+3+into+the+future+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Fast Lane Stuff",       I.SP02, RetailVendors(amazon: "https://www.amazon.com/Sims-Fast-Lane-Stuff-PC-Mac/dp/B003TS6Q8A?crid=1QFU39K17AF04&dib=eyJ2IjoiMSJ9.jYylht-k6MhafIfKfv3SFQYCIoIaCZaKfMZ2BjHZeJ2gd1gzOZXB6ZkEfGGutY1s3R0wqlZal-PtPOsGNb8mxQ.8pH7D5_01IZhMngWu7XHEp3uTE8CyEJ3KBKmWqS-hps&dib_tag=se&keywords=sims%2B3%2Bfast%2Blane%2Bstuff&qid=1718754233&sprefix=sims%2B3%2BFast%2Caps%2C191&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=245cadd42a74f812a85244541be59329&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+fast+lane+stuff+dvd&_sacat=0&_odkw=sims+3+high+end+loft+stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Outdoor Living Stuff",  I.SP03, RetailVendors(amazon: "https://www.amazon.com/Sims-Outdoor-Living-Stuff-PC-Mac/dp/B004DU0XHI?crid=37EM48I9DVCHP&dib=eyJ2IjoiMSJ9.AGSFcMNRyS6DomoU9cfY7NxYyCPLOKMXuXpUK10CIadVWB_peXRGlaei9bTdG4Xnd2gAKekyGAqhc8zCP5NDDfDfcEGwm45Fes8oLiLuRRU.HtTPocs7iK21mFqOXAjCYHu2dstzYow2NMcE4y9QF3s&dib_tag=se&keywords=sims%2B3%2BOutdoor%2BLiving%2BStuff&qid=1718754324&sprefix=sims%2B3%2Boutdoor%2Bliving%2Bstuff%2Caps%2C194&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=351b605cf9f04fa9987e24c5c81b712b&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Outdoor+Living+Stuff+dvd&_sacat=0&_odkw=sims+3+fast+lane+stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Town Life Stuff",       I.SP04, RetailVendors(amazon: "https://www.amazon.com/Sims-Town-Life-Stuff-PC-Mac/dp/B0051MRVHW?crid=10A8V1U4U4DG2&dib=eyJ2IjoiMSJ9.hxPnOtRyFXxe3og72hQa7qJt2DnibFPQ_e1J_cos9IgoOfWijyw-Y47JfRhKN9jwEgdkya4fqUW5jLsS3kJLqx2lqhjIZ6vU8QHLYB8MCRHPTTdWZCAPLAoU_tzxiSrpt3FhkrK3ORQ0Q2xjrZh0TGoSsLd-0IZHQ-zW4hcUt6DDv-cPTgOSBBswOovnNlqEhg_T4XsyMQrgqEV3RxBg9peuXRV9kWORzG8FdITMPu4.RyAEGaAlXYGn8cC_1ydcWp8y3LnQzGVrtx0JW2S_XlM&dib_tag=se&keywords=sims%2B3%2BTown%2BLife%2BStuff&qid=1718754406&sprefix=sims%2B3%2Btown%2Blife%2Bstuff%2Caps%2C221&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=5076c41334af6c4c4b1e297c787792af&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Town+Life+Stuff+dvd&_sacat=0&_odkw=sims+3+Outdoor+Living+Stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Master Suite Stuff",    I.SP05, RetailVendors(amazon: "https://www.amazon.com/Sims-Master-Suite-Stuff-Download/dp/B006FBFNBE?crid=2MUB9WXL2CT89&dib=eyJ2IjoiMSJ9.ilzsowVEDd4pf1ILXw9KrjGoPTeWLcJZ_RzrmwWSRO_0bFaSFq1KEu1IclFG6nhHxJ2iAUNTNAJ5jZPA8gX2p8pMoP1A0RLslpRxe9NxJHE.CW5h-pw8Dg4T1fFSjYpK1mqZg_jQiFXlhPFCXp6RnAk&dib_tag=se&keywords=sims%2B3%2BMaster%2BSuite%2BStuff&qid=1718754487&sprefix=sims%2B3%2Bmaster%2Bsuite%2Bstuff%2Caps%2C196&sr=8-2&th=1&linkCode=ll1&tag=ts3tools-20&linkId=e2f6a900d131a9502fd7f61cf397f164&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Master+Suite+Stuff+dvd&_sacat=0&_odkw=sims+3+Town+Life+Stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),

            // Discontinued — pops a warning when the node is selected
            new BuyTreeNode
            {
                Label   = "Katy Perry's Sweet Treats",
                Icon    = LoadIcon(I.SP06),
                Message = "Katy Perry's Sweet Treats is no longer sold or produced, and therefore " +
                          "usually comes with a hefty price tag. It may also often be out of stock.",
                Children = { RetailVendors(amazon: "https://www.amazon.com/Sims-Katy-Perry-Sweet-Treats-windows/dp/B007NUQICE?crid=PEJNL5Y0V15Z&dib=eyJ2IjoiMSJ9.9_sl1tFKCH7pZAv1OZtT8l7F8N2vp2ZrKNiOPQeOYQHGjHj071QN20LucGBJIEps.DSTS1FvRfTVR4vGRuLEYKsi_prpxthFQ7WFzgm1jRsQ&dib_tag=se&keywords=sims+3+Katy+Perry%27s+Sweet+Treats&qid=1718754544&sprefix=sims+3+katy+perry%27s+sweet+treats%2Caps%2C188&sr=8-1&linkCode=ll1&tag=ts3tools-20&linkId=c58918ce6cab20cc3581541e5da9ff00&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Katy+Perry%27s+Sweet+Treats+dvd&_sacat=0&_odkw=sims+3+Master+Suite+Stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1") }
            },

            N("Diesel Stuff",          I.SP07, RetailVendors(amazon: "https://www.amazon.com/Sims-3-Diesel-Stuff-windows/dp/B0081QBR7K?crid=3IM46TFYPM0F9&dib=eyJ2IjoiMSJ9.AXTpPhUDI2NLF2dLa5fTVvJh23EVkLezWxy09enK2HVa4DovD29NSvO1-KkDluYr.HW_Et6GQ19T1e_lUJAaQbkaRRuUq9MUB33m-5GCEG4w&dib_tag=se&keywords=sims+3+Diesel+Stuff&qid=1718754616&sprefix=sims+3+diesel+stuff%2Caps%2C204&sr=8-1&linkCode=ll1&tag=ts3tools-20&linkId=c8047ee3f6dfbc05e7ddf0cbeb5767d6&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Diesel+Stuff+dvd&_sacat=0&_odkw=sims+3+Katy+Perry%27s+Sweet+Treats+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("70s, 80s & 90s Stuff",  I.SP08, RetailVendors(amazon: "https://www.amazon.com/Sims-70s-80s-90s-Stuff-windows/dp/B00A39IEO2?crid=NPHFZHDEJH98&dib=eyJ2IjoiMSJ9.pAGKrn-NdS3BUC2WAIOzwcvKO48mTOEumFqFuSCYhbBVcO69y-S8SRa0LPOu6SCeJYptl6e94s-alPVx8V7ZaA.LDbgT5X88h8PDWSvIGsehCkkRBaV7al3Jln_h0UwNQk&dib_tag=se&keywords=sims%2B3%2B70s%2C%2B80s%2B%26%2B90s%2BStuff&qid=1718754696&sprefix=sims%2B3%2B70s%2C%2B80s%2B%26%2B90s%2Bstuff%2Caps%2C197&sr=8-1&th=1&linkCode=ll1&tag=ts3tools-20&linkId=096c1ed2ee4e1779ea91b054d9b266c6&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+70s%2C+80s+%26+90s+Stuff+dvd&_sacat=0&_odkw=sims+3+Diesel+Stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")),
            N("Movie Stuff",           I.SP09, RetailVendors(amazon: "https://www.amazon.com/Sims-3-Movie-Stuff-PC/dp/B00CTKHYIA?crid=173XXIH1II9WY&dib=eyJ2IjoiMSJ9.rsKZb57CnAdGNSCPbsYVI5Q4Okpzq68nRe5LO1GDHJ98SVcpZPS8ZUGOA8wXaR_PHxuiKJblh7F-kdM9gSPrOILVrE7lkdiGXUSAzGzOTHVm_rSkdmxZSBLmFrC_jIuFkKsS_vJd-iYFIeryoOFM2U8Yudq1IqxOYBYfJ7cp7PnCxhhCZDR2-p35QU4BjrzO.hVsJLbrmAAE9Z7LYRw1I1EaLhXbYIKIOAwyvbPedplw&dib_tag=se&keywords=sims+3+Movie+Stuff&qid=1718754798&sprefix=sims+3+movie+stuff%2Caps%2C198&sr=8-1&linkCode=ll1&tag=ts3tools-20&linkId=e5de8b06a57a13d5b2d69abd153493d2&language=en_US&ref_=as_li_ss_tl", ebay: "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2334524.m570.l1313&_nkw=sims+3+Movie+Stuff+dvd&_sacat=0&_odkw=sims+3+70s%2C+80s+%26+90s+Stuff+dvd&_osacat=0&mkcid=1&mkrid=711-53200-19255-0&siteid=0&campid=5339065309&customid=ts3tools&toolid=10001&mkevt=1")));

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
                    amazon: "https://www.amazon.com/s?k=the+sims+medieval",
                    ebay:   "https://www.ebay.com/sch/i.html?_nkw=the+sims+medieval&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311")
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
                    amazon: "https://www.amazon.com/s?k=sims+medieval+pirates+and+nobles+pc&crid=3U03XPTFQD3T7&sprefix=the+sims+medieval+pir%2Caps%2C157&ref=nb_sb_ss_ts-doa-p_4_21",
                    ebay:   "https://www.ebay.com/sch/i.html?_nkw=the+sims+medieval+pirates+and+nobles&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311")
            }
        };

    // ── Sims Castaway Stories (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildTSCastawayRetail() =>
        N("The Sims: Castaway Stories", I.TSCastaway,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=the+sims+castaway+stories&crid=237SI2J1FAJ7A&sprefix=the+sims+castaway+stories%2Caps%2C156&ref=nb_sb_noss_1",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=the+sims+castaway+stories&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311"));

    // ── Sims Life Stories (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildTSLifeRetail() =>
        N("The Sims: Life Stories", I.TSLife,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=the+sims+life+stories&crid=2OYOHNHXMO8UQ&sprefix=the+sims+life+stories%2Caps%2C147&ref=nb_sb_noss_1",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=the+sims+life+stories&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311"));

    // ── Sims Pet Stories (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildTSPetsRetail() =>
        N("The Sims: Pet Stories", I.TSPets,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=the+sims+pet+stories&crid=1TYK48QKMZML8&sprefix=the+sims+pet+stories%2Caps%2C155&ref=nb_sb_noss_2",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=the+sims+pet+stories&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311"));

    // ── SimCity 2000 (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildSC2KRetail() =>
        N("SimCity 2000", I.SC2K,
            RetailVendors(
                amazon: "https://www.amazon.com/SimCity-2000-Special-PC/dp/B00001NFSY/ref=sr_1_3?s=videogames&sr=1-3&tag=ts3tools-20&linkId=bdc6cba0426c559ff1308497bab62b6a&language=en_US&ref_=as_li_ss_tl",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=simcity+2000+pc&_sacat=0&_from=R40&_trksid=p4432023.m570.l1313&customid=ts3tools&toolid=10001&mkevt=1"));

    // ── SimCopter (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildCopterRetail() =>
        N("SimCopter", I.SimCopter,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=SimCopter&i=videogames",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=SimCopter+pc&_sacat=0&_from=R40&_trksid=p2334524.m570.l1313&_odkw=SimCopter&_osacat=0"));

    // ── Streets of SimCity (Retail) ─────────────────────────────────────────────────

    private static BuyTreeNode BuildStreetsRetail() =>
        N("Streets of SimCity", I.Streets,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=Streets+of+Simcity",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=Streets+of+Simcity&_sacat=0&_from=R40&_trksid=p4432023.m570.l1313"));

    // ── SimCity 3000 Unlimited (Retail) ───────────────────────────────────────

    private static BuyTreeNode BuildSC3KURetail() =>
        N("SimCity 3000 Unlimited", I.SC3KU,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=simcity+3000+unlimited",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=sim+city+3000+unlimited+pc&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311"));

    // ── SimCity 4 Deluxe Edition (Retail) ─────────────────────────────────────

    private static BuyTreeNode BuildSC4DERetail() =>
        N("SimCity 4 Deluxe Edition", I.SC4DE,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=simcity+4+deluxe",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=simcity+4+deluxe+edition+pc&_sacat=0&_from=R40&_trksid=p4432023.m570.l1311"));

    // ── SimCity (2013) (Retail) ───────────────────────────────────────────────

    private static BuyTreeNode BuildSC2013Retail() =>
        N("SimCity 2013", I.SC2013,
            RetailVendors(
                amazon: "https://www.amazon.com/s?k=Simcity+2013",
                ebay: "https://www.ebay.com/sch/i.html?_nkw=Simcity+2013&_sacat=0&_from=R40&_trksid=p4432023.m570.l1313"));

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
