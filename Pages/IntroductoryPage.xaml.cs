using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    public partial class IntroductoryPage : Window
    {
        public IntroductoryPage()
        {
            InitializeComponent();
            ApplyLanguage();
            ContentRendered += OnContentRendered;
            LoadPersonalization();
        }

        // ── Donor personalisation banner ──────────────────────────────────
        private void LoadPersonalization()
        {
            string key = IniHelper.Read("Personalization", "DonorKey", "");
            if (string.IsNullOrWhiteSpace(key)) return;

            if (!DonorKeyHelper.TryDecodeKey(key, out string firstName, out string lastName))
                return;

            PersonalizationText.Text =
                LanguageManager.Format("Personalization", "PersonalizationText", firstName, lastName, "");
            PersonalizationText.Visibility = System.Windows.Visibility.Visible;
            ExclusiveItemsButton.Visibility = System.Windows.Visibility.Visible;
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

            // One-time repo speed test — recommends the fastest mirror
            _ = RunRepoSpeedTestAsync();
        }

        // ── One-time repository speed test ───────────────────────────────────
        //
        //  Fires once per install (flagged in INI under [Network] RepoSpeedTestDone).
        //  Pings every non-localhost domain in TrustedSources.Domains in parallel,
        //  measures the first successful HTTP response time, and recommends switching
        //  if a faster repo than the current one is found.
        //
        private async Task RunRepoSpeedTestAsync()
        {
            const string iniSection = "Network";
            const string iniKey     = "RepoSpeedTestDone";

            if (IniHelper.ReadBool(iniSection, iniKey, false)) return;

            // Exclude localhost — not a useful remote mirror
            string[] candidates = TrustedSources.Domains
                .Where(d => !d.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (candidates.Length == 0)
            {
                IniHelper.WriteBool(iniSection, iniKey, true);
                return;
            }

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            // Fire all pings in parallel and collect (domain, elapsed ms) pairs
            var tasks = candidates.Select(async domain =>
            {
                string url = $"https://{domain}/version.txt";
                var sw = Stopwatch.StartNew();
                try
                {
                    using var resp = await http.GetAsync(url);
                    sw.Stop();
                    return (Domain: domain, Ms: resp.IsSuccessStatusCode ? sw.ElapsedMilliseconds : long.MaxValue);
                }
                catch
                {
                    return (Domain: domain, Ms: long.MaxValue);
                }
            });

            var results = await Task.WhenAll(tasks);

            // Mark done regardless of outcome so we never re-run
            IniHelper.WriteBool(iniSection, iniKey, true);

            var best = results.OrderBy(r => r.Ms).FirstOrDefault();
            if (best.Domain == null || best.Ms == long.MaxValue) return;

            // Strip protocol + trailing slash for a clean comparison
            string currentHost = AppSettings.BaseUrl
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://",  "", StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/');

            if (best.Domain.Equals(currentHost, StringComparison.OrdinalIgnoreCase)) return;

            // Show recommendation on the UI thread
            Dispatcher.Invoke(() =>
            {
                var answer = MessageBox.Show(
                    LanguageManager.Format("RepoSpeed", "Prompt", best.Domain, best.Ms, currentHost),
                    LanguageManager.Get("RepoSpeed", "Title", "SimTools — Repository Speed Test"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (answer == MessageBoxResult.Yes)
                    IniHelper.Write(iniSection, "BaseUrl", best.Domain);
            });
        }

        // ── Translation strings (with fallback) ─────────────────────────────────
        public void ApplyLanguage()
        {
            // ── RTL / LTR layout direction ─────────────────────────────────────
            var lang = IniHelper.Read("Language", "SelectedLanguage", "en");

            // Add any future RTL languages to this set
            var rtlLanguages = new HashSet<string> { "ar" };
            FlowDirection = rtlLanguages.Contains(lang)
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;

            // ── Text strings ───────────────────────────────────────────────────
            IntroText.Text = LanguageManager.Get("IntroductoryPage", "IntroText1", IntroText.Text);
            UpdateButton.Content = LanguageManager.Get("IntroductoryPage", "UpdateButton_Text", "Check for Updates");
            ContinueButton.Content = LanguageManager.Get("IntroductoryPage", "ContinueButton_Text", "Continue");
            SimToolsButton.Content = LanguageManager.Get("IntroductoryPage", "SimToolsButton_Text", "What is SimTools?");
            ExclusiveItemsButton.Content = LanguageManager.Get("IntroductoryPage", "ExclusiveItemsButton_Text", "Exclusive Items");
        }

        // ── Automatic update check on startup ─────────────────────────────────
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
                const string officialVersionUrl = "https://us1-repo.simtools-app.com/version.txt";
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
                            LanguageManager.Get("Updates", "NoServer", "Could not reach the update server."),
                            LanguageManager.Get("Updates", "Title_Failed", "Update Check Failed"),
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
                            LanguageManager.Get("Updates", "Malformed", "The version file on the server is malformed."),
                            LanguageManager.Get("Updates", "Title_Failed", "Update Check Failed"),
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
                            LanguageManager.Format("Updates", "BadVersion", remoteVersionStr),
                            LanguageManager.Get("Updates", "Title_Failed", "Update Check Failed"),
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
                            LanguageManager.Format("Updates", "UpToDate", localVersion.ToString(3)),
                            LanguageManager.Get("Updates", "Title_UpToDate", "Up to Date"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    return;
                }

                // 4. Update available — ask the user
                var confirm = MessageBox.Show(
                    LanguageManager.Format("Updates", "NewVersion", remoteVersionStr, localVersion.ToString(3)),
                    LanguageManager.Get("Updates", "Title_Available", "Update Available"),
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
                        LanguageManager.Get("Updates", "Timeout", "The update check timed out."),
                        LanguageManager.Get("Updates", "Title_Failed", "Update Check Failed"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                if (!isAutomatic)
                    MessageBox.Show(
                        LanguageManager.Format("Updates", "Unexpected", ex.Message),
                        LanguageManager.Get("Updates", "Title_Failed", "Update Check Failed"),
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
                    LanguageManager.Get("Updates", "UpdaterMissing", "SimToolsUpdater.exe was not found."),
                    LanguageManager.Get("Updates", "Title_MissingUpdater", "Updater Not Found"),
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
                LanguageManager.Get("Updates", "Suppress_Ask", "Would you like to suppress automatic update notifications?"),
                LanguageManager.Get("Updates", "Suppress_Title", "Suppress Update Notifications"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (suppress != MessageBoxResult.Yes) return;

            var duration = MessageBox.Show(
                LanguageManager.Get("Updates", "Suppress_Duration", "How long would you like to suppress update notifications?"),
                LanguageManager.Get("Updates", "Suppress_DurTitle", "Suppression Duration"),
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

        private void ExclusiveItemsButton_Click(object sender, RoutedEventArgs e)
        {
            new ExclusiveItems { Owner = this }.Show();
        }

        private void SimToolsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                LanguageManager.Get("Messages", "WhatIsSimTools", "SimTools (previously TS3Tools) is still the same suite of tools."),
                LanguageManager.Get("Messages", "WhatIsSimTools_Title", "What is SimTools?"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
