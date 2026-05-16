using System;
using System.Windows;

namespace SimTools_v4
{
    public partial class App : System.Windows.Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LanguageManager.Load();

            var splash = new SplashScreenWindow();
            splash.Show();
            await splash.RunAsync();
            splash.Close();

            bool skipLang = IniHelper.ReadBool("Language", "DoNotAskAgain", false);
            if (!skipLang)
            {
                var lang = new LanguageSelectionWindow();
                lang.ShowDialog();
                if (Dispatcher.HasShutdownStarted) return;
                LanguageManager.Load();
            }

            var intro = new IntroductoryPage();
            intro.ShowDialog();
            if (Dispatcher.HasShutdownStarted) return;

            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var main = new MainWindow();
            MainWindow = main;
            main.Show();
        }
    }
}