using System;
using System.Diagnostics;
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

        // ── Music startup + automatic update check ────────────────────────
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

            // Automatic update check — runs silently if suppressed
            _ = CheckForUpdateOnStartupAsync();
        }

        // ── Automatic update check on startup ─────────────────────────────
        private async Task CheckForUpdateOnStartupAsync()
        {
            if (IsAutoCheckSuppressed()) return;
            await CheckForUpdateAsync(isAutomatic: true);
        }

        // ── Manual update button ───────────────────────────────────────────
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;
            try
            {
                await CheckForUpdateAsync(isAutomatic: false);
            }
            finally
            {
                UpdateButton.IsEnabled = true;
            }
        }

        // ── Shared update check logic ──────────────────────────────────────
        //
        //  isAutomatic = true  → errors are swallowed silently;
        //                        no "up to date" popup;
        //                        suppression prompt shown on NO.
        //  isAutomatic = false → all errors and status shown to user;
        //                        no suppression prompt on NO.
        //
        private async Task CheckForUpdateAsync(bool isAutomatic)
        {
            try
            {
                // 1. Fetch version.txt — always try the official repo first,
                //    then fall back to the user-configured repo if the primary fails.
                const string officialVersionUrl = "https://repo.simtools-app.com/version.txt";
                string userVersionUrl = AppSettings.ResolveUrl("%baseurl%/version.txt");

                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                HttpResponseMessage? resp = null;

                // Try the official repo first regardless of user settings
                try   { resp = await http.GetAsync(officialVersionUrl); }
                catch { /* unreachable — fall through to user repo */ }

                // Fall back to the user-configured repo only if the official one failed
                if ((resp == null || !resp.IsSuccessStatusCode)
                    && !userVersionUrl.Equals(officialVersionUrl, StringComparison.OrdinalIgnoreCase))
                {
                    resp?.Dispose();
                    try   { resp = await http.GetAsync(userVersionUrl); }
                    catch { /* also unreachable */ }
                }

                if (resp == null || !resp.IsSuccessStatusCode)
                {
                    if (!isAutomatic)
                        MessageBox.Show(
                            "Could not reach the update server.\nPlease check your connection and try again.",
                            "Update Check Failed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    return;
                }

                string body = await resp.Content.ReadAsStringAsync();
                string[] lines = body.Split(
                    new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 1)
                {
                    if (!isAutomatic)
                        MessageBox.Show(
                            "The version file on the server is malformed.",
                            "Update Check Failed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    return;
                }

                string remoteVersionStr = lines[0].Trim();

                // 2. Compare with local assembly version
                if (!Version.TryParse(remoteVersionStr, out Version? remoteVersion))
                {
                    if (!isAutomatic)
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
                    if (!isAutomatic)
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
                    "Would you like to update now?\n" +
                    "SimTools will close automatically once the updater launches.",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    LaunchUpdaterAndExit();
                    return;
                }

                // 5. User declined — offer suppression only for automatic checks
                if (isAutomatic)
                    PromptSuppression();
            }
            catch (TaskCanceledException)
            {
                if (!isAutomatic)
                    MessageBox.Show(
                        "The update check timed out.\nPlease try again later.",
                        "Update Check Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                if (!isAutomatic)
                    MessageBox.Show(
                        $"An unexpected error occurred:\n{ex.Message}",
                        "Update Check Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        }

        // ── Launch SimToolsUpdater.exe and shut down SimTools ──────────────
        private static void LaunchUpdaterAndExit()
        {
            string updaterPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "SimToolsUpdater.exe");

            if (!File.Exists(updaterPath))
            {
                MessageBox.Show(
                    "SimToolsUpdater.exe was not found in the application directory.\n" +
                    "Please re-download SimTools manually.",
                    "Updater Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo(updaterPath) { UseShellExecute = true });
            App.Current.Shutdown();
        }

        // ── Suppression prompt (automatic checks only) ────────────────────
        //
        //  First dialog:  Yes/No — do you want to suppress at all?
        //  Second dialog: Yes = 30 days / No = indefinitely
        //
        private static void PromptSuppression()
        {
            var suppress = MessageBox.Show(
                "Would you like to suppress automatic update notifications?",
                "Suppress Update Notifications",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (suppress != MessageBoxResult.Yes) return;

            var duration = MessageBox.Show(
                "How long would you like to suppress update notifications?\n\n" +
                "  Yes  —  Suppress for 30 days\n" +
                "  No   —  Suppress indefinitely",
                "Suppression Duration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            string until = duration == MessageBoxResult.Yes
                ? DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
                : "indefinite";

            IniHelper.Write("Updates", "SuppressAutoCheckUntil", until);
        }

        // ── Suppression state reader ───────────────────────────────────────
        //
        //  INI key: [Updates] SuppressAutoCheckUntil
        //    ""           → not suppressed
        //    "indefinite" → suppressed forever
        //    "yyyy-MM-dd" → suppressed until that date (UTC)
        //
        internal static bool IsAutoCheckSuppressed()
        {
            string val = IniHelper.Read("Updates", "SuppressAutoCheckUntil", "");
            if (string.IsNullOrWhiteSpace(val)) return false;
            if (val.Equals("indefinite", StringComparison.OrdinalIgnoreCase)) return true;
            return DateTime.TryParse(val, out DateTime until) && DateTime.UtcNow < until;
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
