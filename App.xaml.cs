using System;
using System.Windows;

namespace SimTools
{
    public partial class App : System.Windows.Application
    {
        /// <summary>Global music player window — null if music is disabled in Settings.</summary>
        public static MusicPlayerWindow? MusicPlayer { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LanguageManager.Load();

            // ── Splash ────────────────────────────────────────────────────
            var splash = new SplashScreenWindow();
            splash.Show();
            await splash.RunAsync();
            splash.Close();

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
    }
}
