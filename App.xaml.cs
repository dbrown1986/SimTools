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

            // Load language strings before any window is shown
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
                // Reload language strings after the user makes a selection
                LanguageManager.Load();
            }

            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var main = new MainWindow();
            MainWindow = main;
            main.Show();
        }
    }
}