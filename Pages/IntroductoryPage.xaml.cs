using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Button = System.Windows.Controls.Button;
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
        private void OnContentRendered(object? sender, EventArgs e)
        {
            var player = App.MusicPlayer;
            if (player == null) return;

            // Attach and show the player alongside this window
            player.AttachTo(this);
            player.Show();

            string musicFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Resources", "Music");

            // Show the first-run download prompt (synchronous — just a MessageBox).
            // If the user says YES the download runs in the background and reloads
            // the playlist automatically when complete.
            player.ShowFirstRunPrompt(musicFolder, this);

            // Load whatever songs are already in /Resources/Music and start playing.
            // (If the folder is empty the player waits quietly until the download
            // callback populates it.)
            MusicPlayerService.LoadPlaylist(musicFolder);
            if (MusicPlayerService.Playlist.Count > 0)
                MusicPlayerService.Play();
        }

        // ── Update check ──────────────────────────────────────────────────
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;

            try
            {
                // 1. Fetch version.txt from the server
                //    Expected format (two lines):
                //      1.0.1
                //      SimTools_v4_Setup.exe
                string versionUrl = AppSettings.ResolveUrl("%baseurl%/version.txt");

                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                using var resp = await http.GetAsync(versionUrl);

                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Could not reach the update server.\nPlease check your connection and try again.",
                        "Update Check Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                string content = await resp.Content.ReadAsStringAsync();
                string[] lines = content.Split(
                    new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                {
                    MessageBox.Show(
                        "The version file on the server is malformed.",
                        "Update Check Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                string remoteVersionStr = lines[0].Trim();
                string installerFilename = lines[1].Trim();

                // 2. Compare with local assembly version
                Version remoteVersion;
                if (!Version.TryParse(remoteVersionStr, out remoteVersion!))
                {
                    MessageBox.Show(
                        $"Could not parse remote version: \"{remoteVersionStr}\"",
                        "Update Check Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Version localVersion =
                    Assembly.GetExecutingAssembly().GetName().Version
                    ?? new Version(1, 0, 0);

                // 3. No update available
                if (remoteVersion <= localVersion)
                {
                    MessageBox.Show(
                        $"You are already running the latest version ({localVersion.ToString(3)}).",
                        "Up to Date",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // 4. Update available — ask the user
                var confirm = MessageBox.Show(
                    $"Version {remoteVersionStr} is available (you have {localVersion.ToString(3)}).\n\n" +
                    "Would you like to download and install it now?\n" +
                    "SimTools will close automatically once the installer launches.",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                // 5. Download and launch via UpdateDownloadWindow
                string downloadUrl = AppSettings.ResolveUrl(
                    $"%baseurl%/updates/{Uri.EscapeDataString(installerFilename)}");

                string tempDir = Path.Combine(Path.GetTempPath(), "SimToolsUpdate");
                string destPath = Path.Combine(tempDir, installerFilename);

                var dlg = new UpdateDownloadWindow(downloadUrl, destPath)
                {
                    Owner = this
                };
                dlg.ShowDialog();
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "The update check timed out.\nPlease try again later.",
                    "Update Check Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n{ex.Message}",
                    "Update Check Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                UpdateButton.IsEnabled = true;
            }
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