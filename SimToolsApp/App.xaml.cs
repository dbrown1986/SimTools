using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;

namespace SimTools
{
    public partial class App : System.Windows.Application
    {
        /// <summary>Global music player window — null if music is disabled in Settings.</summary>
        public static MusicPlayerWindow? MusicPlayer { get; private set; }

        /// <summary>
        /// Bottom-centre advertisement dock — null when the user is a verified donor.
        /// Donors have already supported the project and should not see advertisements.
        /// </summary>
        public static AdDockWindow? AdDock { get; private set; }

        // ── EasterEgg5: type "rosebud" from any window ───────────────────────
        private static string _ee5Buffer = string.Empty;
        private const  string _ee5Cheat  = "rosebud";

        // ── Easter egg completion tracking ───────────────────────────────────
        private static HashSet<int> _foundEggs     = new();
        private static bool         _bonusTriggered = false;
        private static System.Windows.Media.MediaPlayer? _sfxPlayer;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // EasterEgg5 — global "rosebud" key sequence listener
            EventManager.RegisterClassHandler(
                typeof(Window),
                UIElement.PreviewKeyDownEvent,
                new KeyEventHandler(OnGlobalKeyDown));

            // ── Restore persisted easter egg state ────────────────────────
            string eeFoundRaw = IniHelper.Read("EasterEggs", "FoundEggs", "");
            foreach (string eePart in eeFoundRaw.Split(',', StringSplitOptions.RemoveEmptyEntries))
                if (int.TryParse(eePart.Trim(), out int eeN)) _foundEggs.Add(eeN);
            _bonusTriggered = IniHelper.ReadBool("EasterEggs", "BonusTriggered", false);

            LanguageManager.Load();

            // ── Splash ────────────────────────────────────────────────────
            bool isFirstRun = !IniHelper.ReadBool("App", "HasLaunched", false);
            var splash = new SplashScreenWindow(skippable: !isFirstRun);
            splash.Show();
            await splash.RunAsync();
            splash.Close();
            if (isFirstRun)
                IniHelper.WriteBool("App", "HasLaunched", true);

            // ── Language selection ─────────────────────────────────────────
            bool skipLang = IniHelper.ReadBool("Language", "DoNotAskAgain", false);
            if (!skipLang)
            {
                var lang = new LanguageSelectionWindow();
                lang.ShowDialog();
                if (Dispatcher.HasShutdownStarted) return;
                LanguageManager.Load();
            }

            // ── Create music player (hidden until IntroductoryPage) ────────
            bool musicEnabled = IniHelper.ReadBool("Music", "Enabled", true);
            if (musicEnabled)
                MusicPlayer = new MusicPlayerWindow();

            // ── Create ad dock (suppressed for verified donors) ───────────
            //
            //  Read the donor key written by UnlockPersonalizationDialog.
            //  If it decodes successfully the user is a donor and should not
            //  see advertisements — AdDock stays null for the lifetime of the
            //  session.
            //
            // Donor check: read names directly from the machine-locked token file.
            // The donor key is never stored on disk — TryReadTokenFile is the
            // sole gate for granting donor status.
            bool isDonor = DonorKeyHelper.TryReadTokenFile(out _, out _);
            if (!isDonor)
                AdDock = new AdDockWindow();

            // ── Introductory page ─────────────────────────────────────────
            var intro = new IntroductoryPage();
            intro.ShowDialog();
            if (Dispatcher.HasShutdownStarted) return;

            // ── Main window ───────────────────────────────────────────────
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var main = new MainWindow();
            MainWindow = main;

            // Reattach music player to main window and keep it visible
            MusicPlayer?.AttachTo(main);

            // Reattach ad dock to main window (null if donor — no-op)
            AdDock?.AttachTo(main);

            main.Closed += (_, _) =>
            {
                MusicPlayer?.AppClose();
                MusicPlayer = null;

                AdDock?.AppClose();
                AdDock = null;
            };

            main.Show();
        }
        // ── Easter egg completion — call from every egg handler ─────────────────
        //
        //  Tracks which eggs have been found this session. When all five are
        //  discovered, silently downloads bonus.txt from the repo, adds the
        //  track(s) to Resources/Music, reshuffles the playlist, and plays the
        //  bonus track immediately through the existing music player.
        //
        public static void NotifyEasterEggFound(int eggNumber)
        {
            _foundEggs.Add(eggNumber);

            // Persist the updated set so progress survives across sessions
            IniHelper.Write("EasterEggs", "FoundEggs", string.Join(",", _foundEggs));

            if (_bonusTriggered) return;
            if (!_foundEggs.SetEquals(new HashSet<int> { 1, 2, 3, 4, 5 })) return;

            _bonusTriggered = true;
            // Persist the bonus-triggered flag so the download never fires again
            IniHelper.WriteBool("EasterEggs", "BonusTriggered", true);

            // Play SFX before showing the congratulations message
            string sfxPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Resources", "sfx01.mp3");
            if (File.Exists(sfxPath))
            {
                _sfxPlayer = new System.Windows.Media.MediaPlayer();
                _sfxPlayer.Open(new Uri(sfxPath, UriKind.Absolute));
                _sfxPlayer.Play();
            }

            System.Windows.MessageBox.Show(
                "WOAH! Look at you! Here's a bonus track for you.",
                "SimTools",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            _ = DownloadAndPlayBonusTrackAsync();
        }

        private static async Task DownloadAndPlayBonusTrackAsync()
        {
            try
            {
                string musicFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Resources", "Music");
                Directory.CreateDirectory(musicFolder);

                // Fetch bonus.txt track list
                string bonusUrl = AppSettings.ResolveUrl("%baseurl%/Resources/Music/bonus.txt");
                using var http  = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                using var listResp = await http.GetAsync(bonusUrl);
                if (!listResp.IsSuccessStatusCode) return;

                string manifest = await listResp.Content.ReadAsStringAsync();

                var tracks = manifest
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0 && !l.StartsWith('<') && !l.StartsWith('#'))
                    .ToList();

                if (tracks.Count == 0) return;

                // Download each track silently; record what we actually obtained
                var obtained = new List<string>();

                foreach (string name in tracks)
                {
                    string dest = Path.Combine(musicFolder, name);

                    if (!File.Exists(dest))
                    {
                        try
                        {
                            string fileUrl = AppSettings.ResolveUrl(
                                $"%baseurl%/Resources/Music/{Uri.EscapeDataString(name)}");

                            using var fileResp = await http.GetAsync(fileUrl);
                            if (fileResp.IsSuccessStatusCode)
                            {
                                await using var fs = new FileStream(
                                    dest, FileMode.Create, FileAccess.Write);
                                await fileResp.Content.CopyToAsync(fs);
                            }
                        }
                        catch { /* skip undownloadable track */ }
                    }

                    if (File.Exists(dest))
                        obtained.Add(dest);
                }

                if (obtained.Count == 0) return;

                // Reload playlist and immediately play the bonus track on the UI thread
                Current.Dispatcher.Invoke(() =>
                {
                    MusicPlayerService.LoadPlaylist(musicFolder);

                    // Find the first bonus track in the reshuffled playlist
                    var playlist = MusicPlayerService.Playlist;
                    int idx = -1;
                    for (int i = 0; i < playlist.Count; i++)
                    {
                        if (obtained.Any(o => string.Equals(o, playlist[i],
                                StringComparison.OrdinalIgnoreCase)))
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx >= 0)
                        MusicPlayerService.SelectTrack(idx);
                    else
                        MusicPlayerService.Play();
                });
            }
            catch { /* fully silent — bonus is a secret */ }
        }

        // ── EasterEgg5 handler ───────────────────────────────────────────────

        private static void OnGlobalKeyDown(object sender, KeyEventArgs e)
        {
            // Only track letter keys A–Z
            if (e.Key < Key.A || e.Key > Key.Z) return;

            // Append the lowercase character and keep only the last N chars
            _ee5Buffer += (char)('a' + (e.Key - Key.A));
            if (_ee5Buffer.Length > _ee5Cheat.Length)
                _ee5Buffer = _ee5Buffer[^_ee5Cheat.Length..];

            if (_ee5Buffer != _ee5Cheat) return;

            // Sequence matched — clear buffer, open the link, and log the egg
            _ee5Buffer = string.Empty;
            Process.Start(new ProcessStartInfo
            {
                FileName        = "https://archive.org/details/turner_video_390885",
                UseShellExecute = true
            });
            NotifyEasterEggFound(5);
        }
    }
}
