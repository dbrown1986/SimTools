// SimTools
// Main Application
// SimTools Intro Page Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms; // Required for TaskDialog
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

            ApplyHolidayThemes(); // <-- Holiday theme application
        }

        private void ApplyHolidayThemes()
        {
            // Hide all holiday elements by default
            Clover.Visibility = Visibility.Collapsed;
            Clover2.Visibility = Visibility.Collapsed;
            Clover3.Visibility = Visibility.Collapsed;
            Clover4.Visibility = Visibility.Collapsed;
            Clover5.Visibility = Visibility.Collapsed;
            Fireworks.Visibility = Visibility.Collapsed;
            SantaHat.Visibility = Visibility.Collapsed;

            DateTime today = DateTime.Today;
            int currentYear = today.Year;

            // 2. Check St. Patrick's Day (March 17)
            // Range: March 14 to March 20
            DateTime stPatricksDay = new DateTime(currentYear, 3, 17);
            if (today >= stPatricksDay.AddDays(-3) && today <= stPatricksDay.AddDays(3))
            {
                Clover.Visibility = Visibility.Visible;
                Clover2.Visibility = Visibility.Visible;
                Clover3.Visibility = Visibility.Visible;
                Clover4.Visibility = Visibility.Visible;
                Clover5.Visibility = Visibility.Visible;
            }

            // 3. Check Independence Day (July 4)
            // Range: July 1 to July 7
            DateTime independenceDay = new DateTime(currentYear, 7, 4);
            if (today >= independenceDay.AddDays(-3) && today <= independenceDay.AddDays(3))
            {
                Fireworks.Visibility = Visibility.Visible;
            }

            // 4. Check New Year's Eve (December 31 / January 1 window)
            // Range: Dec 28 to Jan 4. (Handles wrapping over the year bound perfectly)
            if (IsDateInNewYearsWindow(today))
            {
                Fireworks.Visibility = Visibility.Visible;
            }

            // 5. Check Christmas (December 25)
            // Range: December 22 to December 28
            DateTime christmas = new DateTime(currentYear, 12, 25);
            if (today >= christmas.AddDays(-3) && today <= christmas.AddDays(3))
            {
                SantaHat.Visibility = Visibility.Visible;
            }
        }

        private bool IsDateInNewYearsWindow(DateTime targetDate)
        {
            // Check if we are at the end of the current year (Dec 28 - Dec 31)
            DateTime nyeThisYear = new DateTime(targetDate.Year, 12, 31);
            if (targetDate >= nyeThisYear.AddDays(-3) && targetDate <= nyeThisYear)
            {
                return true;
            }

            // Check if we are at the very start of a new year (Jan 1 - Jan 4)
            DateTime nydThisYear = new DateTime(targetDate.Year, 1, 1);
            if (targetDate >= nydThisYear && targetDate <= nydThisYear.AddDays(3))
            {
                return true;
            }

            return false;
        }

        // ── Donor personalisation banner ──────────────────────────────────
        private void LoadPersonalization()
        {
            // Check whether there is any evidence of a previously configured
            // donor account (legacy INI key or a token file on disk).  Used to
            // decide whether to show the re-entry prompt if validation fails.
            bool hadLegacyKey = !string.IsNullOrWhiteSpace(
                IniHelper.Read("Personalization", "DonorKey", ""));
            bool hadTokenFile = DonorKeyHelper.TokenFileExists();

            // Attempt to read the machine-locked token file.
            // The donor key is never read from disk — names come from the token.
            if (!DonorKeyHelper.TryReadTokenFile(out string firstName, out string lastName))
            {
                // Wipe any leftover data (token file, legacy INI keys).
                DonorKeyHelper.ClearPersonalization();

                // Only prompt the user if they had previously set up personalisation.
                // A brand-new install with no key entered should stay silent.
                if (hadLegacyKey || hadTokenFile)
                {
                    MessageBox.Show(
                        LanguageManager.Get("Personalization", "MachineChanged",
                            "Your donor personalisation could not be verified on this machine. " +
                            "Please go to Support SimTools and re-enter your key."),
                        LanguageManager.Get("Personalization", "MachineChanged_Title",
                            "SimTools — Personalization"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                return;
            }

            PersonalizationText.Text =
                LanguageManager.Format("Personalization", "PersonalizationText", firstName, lastName, "");
            PersonalizationText.Visibility = System.Windows.Visibility.Visible;
            ExclusiveItemsButton.Visibility = System.Windows.Visibility.Visible;
        }

        // ── Music startup + ad dock + automatic update check ─────────────
        private void OnContentRendered(object? sender, EventArgs e)
        {
            // ── Music player (may be null if disabled in Settings) ────────
            var player = App.MusicPlayer;
            if (player != null)
            {
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

            // ── Ad dock (null if user is a verified donor) ────────────────
            App.AdDock?.AttachTo(this);
            App.AdDock?.Show();

            // Automatic update check — runs silently if suppressed
            _ = CheckForUpdateOnStartupAsync();

            // Fire off the background activation check without blocking the user interface thread
            _ = RunBackgroundActivationAuditAsync();
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
            string[] candidates = TrustedSources.Mirrors;

            if (candidates.Length == 0)
            {
                MessageBox.Show(
                    LanguageManager.Get("IntroductoryPage", "NoMirrors", "No repository mirrors are configured."),
                    LanguageManager.Get("IntroductoryPage", "RepoTitle", "SimTools — Repository Speed Test"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

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

            var best = results.OrderBy(r => r.Ms).FirstOrDefault();
            if (best.Domain == null || best.Ms == long.MaxValue)
            {
                MessageBox.Show(
                    LanguageManager.Get("IntroductoryPage", "NoResponse", "No mirrors responded. Please check your connection."),
                    LanguageManager.Get("IntroductoryPage", "RepoTitle", "SimTools — Repository Speed Test"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string currentHost = AppSettings.BaseUrl
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .TrimEnd('/');

            if (best.Domain.Equals(currentHost, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    LanguageManager.Format("IntroductoryPage", "AlreadyBest", best.Domain, best.Ms),
                    LanguageManager.Get("IntroductoryPage", "RepoTitle", "SimTools — Repository Speed Test"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var answer = MessageBox.Show(
                LanguageManager.Format("IntroductoryPage", "SpeedPrompt", best.Domain, best.Ms, currentHost),
                LanguageManager.Get("RepoSpeed", "RepoTitle", "SimTools — Repository Speed Test"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (answer == MessageBoxResult.Yes)
                IniHelper.Write("Network", "BaseUrl", best.Domain);
        }

        private async void RepoSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            RepoSpeedButton.IsEnabled = false;
            try
            {
                await RunRepoSpeedTestAsync();
            }
            finally
            {
                RepoSpeedButton.IsEnabled = true;
            }
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
            RepoSpeedButton.Content = LanguageManager.Get("Repo", "RepoSpeedTest", "Repository Speed Test");
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
                            LanguageManager.Get("IntroductoryPage", "NoServer", "Could not reach the update server."),
                            LanguageManager.Get("IntroductoryPage", "Title_Failed", "Update Check Failed"),
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
                            LanguageManager.Get("IntroductoryPage", "Malformed", "The version file on the server is malformed."),
                            LanguageManager.Get("IntroductoryPage", "Title_Failed", "Update Check Failed"),
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
                            LanguageManager.Format("IntroductoryPage", "BadVersion", remoteVersionStr),
                            LanguageManager.Get("IntroductoryPage", "Title_Failed", "Update Check Failed"),
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
                            LanguageManager.Format("IntroductoryPage", "UpToDate", localVersion.ToString(3)),
                            LanguageManager.Get("IntroductoryPage", "Title_UpToDate", "Up to Date"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    return;
                }

                // 4. Update available — ask the user
                var confirm = MessageBox.Show(
                    LanguageManager.Format("IntroductoryPage", "NewVersion", remoteVersionStr, localVersion.ToString(3)),
                    LanguageManager.Get("IntroductoryPage", "Title_Available", "Update Available"),
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
                        LanguageManager.Get("IntroductoryPage", "Timeout", "The update check timed out."),
                        LanguageManager.Get("IntroductoryPage", "Title_Failed", "Update Check Failed"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                if (!isAutomatic)
                    MessageBox.Show(
                        LanguageManager.Format("IntroductoryPage", "Unexpected", ex.Message),
                        LanguageManager.Get("IntroductoryPage", "Title_Failed", "Update Check Failed"),
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
                    LanguageManager.Get("IntroductoryPage", "UpdaterMissing", "SimToolsUpdater.exe was not found."),
                    LanguageManager.Get("IntroductoryPage", "Title_MissingUpdater", "Updater Not Found"),
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
        private void PromptSuppression()
        {
            var suppress = MessageBox.Show(
                LanguageManager.Get("IntroductoryPage", "Suppress_Ask", "Would you like to suppress automatic update notifications?"),
                LanguageManager.Get("IntroductoryPage", "Suppress_Title", "Suppress Update Notifications"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (suppress != MessageBoxResult.Yes) return;

            // Run directly via dispatcher to ensure a clean window layer focus
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 1. Create the custom buttons
                var button30Days = new TaskDialogButton(LanguageManager.Get("IntroductoryPage", "Btn_Suppress_30d", "30 Days"));
                var buttonIndefinite = new TaskDialogButton(LanguageManager.Get("IntroductoryPage", "Btn_Suppress_Inf", "Indefinitely"));
                var buttonCancel = TaskDialogButton.Cancel;

                // 2. Set up the dialog page
                var page = new TaskDialogPage()
                {
                    Heading = LanguageManager.Get("IntroductoryPage", "Suppress_DurTitle", "Suppression Duration"),
                    Text = LanguageManager.Get("IntroductoryPage", "Suppress_Duration", "How long would you like to suppress update notifications?"),
                    Icon = TaskDialogIcon.Information,
                    AllowCancel = true,
                    Buttons = { button30Days, buttonIndefinite, buttonCancel }
                };

                // 3. Set the owner directly using the Page's Window handle
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                var nativeWindow = new System.Windows.Forms.NativeWindow();
                nativeWindow.AssignHandle(helper.Handle);

                // 4. Show the dialog safely on the UI thread
                TaskDialogButton result = TaskDialog.ShowDialog(nativeWindow, page);

                // 5. Handle the logic
                if (result == button30Days)
                {
                    string until = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
                    IniHelper.Write("Updates", "SuppressAutoCheckUntil", until);
                }
                else if (result == buttonIndefinite)
                {
                    IniHelper.Write("Updates", "SuppressAutoCheckUntil", "indefinite");
                }
            });
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
                LanguageManager.Get("IntroductoryPage", "WhatIsSimTools", "SimTools (previously TS3Tools) is still the same suite of tools."),
                LanguageManager.Get("IntroductoryPage", "WhatIsSimTools_Title", "What is SimTools?"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task RunBackgroundActivationAuditAsync()
        {
            // 1. Only audit if they actually have a valid token file locally
            if (!DonorKeyHelper.TokenFileExists()) return;

            // 2. Rate-limiting check: Only look online once every 3 days
            string lastCheckStr = IniHelper.Read("Personalization", "LastOnlineAudit", "");
            if (DateTime.TryParse(lastCheckStr, out DateTime lastCheckTime))
            {
                // If 3 days haven't passed yet, skip the network request entirely
                if ((DateTime.UtcNow - lastCheckTime).TotalDays < 3)
                {
                    return;
                }
            }

            // 3. Since the token is valid, grab the current tracking metrics
            string machineGuid = MachineIdentity.GetMachineGuid();

            // We also need to supply the key. Your token file decrypts to "MachineGuid|FirstName|LastName".
            // We didn't explicitly store the key, but we can verify against the database 
            // by altering status.php to accept the machine identity. However, your app already 
            // decodes names. Let's send a quiet status payload check.
            // Tip: Since the token file doesn't store the raw key, we can simply look up by 
            // Machine ID, or look up if *any* slot is assigned to this machine.

            if (string.IsNullOrWhiteSpace(machineGuid)) return;

            try
            {
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(7) })
                {
                    // Build request pointing to your status.php script
                    // Note: We search status by sending the machineGuid parameter.
                    string url = $"https://simtools-app.com/api/status.php?machine={Uri.EscapeDataString(machineGuid)}&key=audit";

                    // If you want absolute precision, you can update your token generation down the line 
                    // to preserve key context, but verifying the Machine ID presence is more than enough!
                    var response = await http.GetAsync(url);

                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // The server successfully replied with a 403, meaning this machine 
                        // is no longer recognized in the database activation ledger.

                        // Clear out the local authorization structures seamlessly
                        DonorKeyHelper.ClearPersonalization();

                        // Switch back the UI components on the UI thread safely
                        Dispatcher.Invoke(() =>
                        {
                            // Refresh the personalization display elements back to default
                            LoadPersonalization();
                        });

                        return; // Stop here, no need to save a successful timestamp
                    }
                }

                // 4. If the request was successful or verified, update the timer stamp in the INI file
                IniHelper.Write("Personalization", "LastOnlineAudit", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch
            {
                // If the user is offline, network times out, or your site drops momentarily,
                // it catches here and fails completely silently. The user experiences NO disruption 
                // and the app remains fully personalized until the next check succeeds.
            }
        }

    }
}
