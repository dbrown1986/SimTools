using System;
using System.Diagnostics;
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

        // ── EasterEgg5: type "rosebud" from any window ───────────────────────
        private static string _ee5Buffer = string.Empty;
        private const  string _ee5Cheat  = "rosebud";

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // EasterEgg5 — global "rosebud" key sequence listener
            EventManager.RegisterClassHandler(
                typeof(Window),
                UIElement.PreviewKeyDownEvent,
                new KeyEventHandler(OnGlobalKeyDown));

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

            main.Closed += (_, _) =>
            {
                MusicPlayer?.AppClose();
                MusicPlayer = null;
            };

            main.Show();
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

            // Sequence matched — clear buffer and open the link
            _ee5Buffer = string.Empty;
            Process.Start(new ProcessStartInfo
            {
                FileName        = "https://archive.org/details/turner_video_390885",
                UseShellExecute = true
            });
        }
    }
}