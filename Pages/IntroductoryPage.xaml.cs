using System;
using System.IO;
using System.Windows;

using Button     = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    public partial class IntroductoryPage : Window
    {
        public IntroductoryPage()
        {
            InitializeComponent();
            ContentRendered += OnContentRendered;
        }

        // ── Music startup ─────────────────────────────────────────────────
        private async void OnContentRendered(object? sender, EventArgs e)
        {
            var player = App.MusicPlayer;
            if (player == null) return;

            // Attach and show the player alongside this window
            player.AttachTo(this);
            player.Show();

            string musicFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "res", "music");

            // First-run prompt: offer to download the music pack
            await player.ShowFirstRunPromptAsync(musicFolder);
            if (App.Current == null || !IsLoaded) return;

            // Load the playlist (may include newly downloaded files)
            MusicPlayerService.LoadPlaylist(musicFolder);
            MusicPlayerService.Play();
        }

        // ── Button handlers ───────────────────────────────────────────────
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SimToolsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "SimTools (previously TS3Tools) is still the same suite of tools previously developed, " +
                "but now includes options for a variety of Maxis' sim genre of games. SimTools includes " +
                "options for Sims 1, Sims 2, Sims Stories, Sims 3, Sims 4, Simcity 2000, Simcity 3000, " +
                "Simcity 3000 Unlimited, Simcity 4, Simcity 2K13, SimCopter and Streets of Simcity, " +
                "with more to come in later versions.",
                "What is SimTools?",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
