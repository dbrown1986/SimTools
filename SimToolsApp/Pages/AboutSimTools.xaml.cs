using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools;

public partial class AboutSimTools : Window
{
    public AboutSimTools()
    {
        InitializeComponent();
        ApplyLanguage();
        PopulateVersionInfo();
        Loaded += (_, _) => StartSlideshow();
        Closing += (_, _) => _slideTimer?.Stop();
    }

    // ── Version info ──────────────────────────────────────────────────────────────
    //
    //  • Version + Build  — read from AssemblyName.Version (Major.Minor.Build, Build Revision)
    //  • Git hash         — read from AssemblyInformationalVersion ("x.y.z+<hash>")
    //  • Compile date     — last-write time of the executing assembly on disk
    //
    private void PopulateVersionInfo()
    {
        var asm = Assembly.GetExecutingAssembly();

        // Version: Major.Minor.Patch, Build <number>
        //   ver.Major    = Major  (4)
        //   ver.Minor    = Minor  (0)
        //   ver.Build    = Patch  (1)   ← .NET names this "Build"
        //   ver.Revision = Build  (3868) ← .NET names this "Revision"
        var ver = asm.GetName().Version ?? new Version(4, 0, 1, 0);
        string versionBase = $"{ver.Major}.{ver.Minor}.{ver.Build}";
        string buildPart = ver.Revision > 0 ? $", Build {ver.Revision}" : "";

        // Short git hash from AssemblyInformationalVersion ("4.0.1.3868+abc1234...")
        string commitSuffix = "";
        string infoVer = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                            ?.InformationalVersion ?? "";
        int plusIdx = infoVer.IndexOf('+');
        if (plusIdx >= 0 && plusIdx < infoVer.Length - 1)
        {
            string hash = infoVer[(plusIdx + 1)..];
            if (hash.Length > 7) hash = hash[..7];
            commitSuffix = $" ({hash})";
        }

        // Compile date = last-write time of the assembly file
        string asmPath = asm.Location;
        DateTime built = File.Exists(asmPath) ? File.GetLastWriteTime(asmPath) : DateTime.Now;
        string builtStr = built.ToString("MM/dd/yyyy, h:mm tt");

        // Rebuild Text5 Inlines — x:Name on <Run> does not generate code-behind fields
        Text5.Inlines.Clear();
        Text5.Inlines.Add(new System.Windows.Documents.Run($"Version: {versionBase}{buildPart}{commitSuffix}"));
        Text5.Inlines.Add(new System.Windows.Documents.LineBreak());
        Text5.Inlines.Add(new System.Windows.Documents.Run($"Released: {builtStr}"));
        Text5.Inlines.Add(new System.Windows.Documents.LineBreak());
        Text5.Inlines.Add(new System.Windows.Documents.Run("Website: http://www.simtools-app.com"));
        Text5.Inlines.Add(new System.Windows.Documents.LineBreak());
        int currentYear = DateTime.Now.Year;
        string copyrightYear = currentYear > 2024 ? $"2024\u2013{currentYear}" : "2025";
        Text5.Inlines.Add(new System.Windows.Documents.Run($"\u00a9 {copyrightYear}, Archeon Industries, LLC."));
    }

    // Called on load and by SettingsWindow after a language change
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
        Text1.Text = LanguageManager.Get("AboutSimToolsPage", "SimToolsDev", Text1.Text);
        Text2.Text = LanguageManager.Get("AboutSimToolsPage", "SimToolsDescription", Text2.Text);
        Text3.Text = LanguageManager.Get("AboutSimToolsPage", "CulminationMessage", Text3.Text);
        Text4.Text = LanguageManager.Get("AboutSimToolsPage", "LicenseMessage", Text4.Text);
        Text6.Text = LanguageManager.Get("AboutSimToolsPage", "FaithDedication", Text6.Text);
        PreviousMenu.Text = LanguageManager.Get("AboutSimToolsPage", "PreviousMenu", PreviousMenu.Text);
        UpdateButton.Content = LanguageManager.Get("AboutSimToolsPage", "UpdateButton", "Check for Updates");
        ChangelogButton.Content = LanguageManager.Get("AboutSimToolsPage", "ChangelogButton", "Changelog");
    }
        // ── Changelog button ──────────────────────────────────────────────────────

    private void ChangelogButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(
            "https://github.com/dbrown1986/SimTools/commits/master/")
        {
            UseShellExecute = true
        });
    }

    // ── Manual update button ──────────────────────────────────────────────────

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

    // ── Shared update check logic ─────────────────────────────────────────────
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
                        LanguageManager.Get("AboutSimToolsPage", "NoServer", "Could not reach the update server."),
                        LanguageManager.Get("AboutSimToolsPage", "Title_Failed", "Update Check Failed"),
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
                        LanguageManager.Get("AboutSimToolsPage", "Malformed", "The version file on the server is malformed."),
                        LanguageManager.Get("AboutSimToolsPage", "Title_Failed", "Update Check Failed"),
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
                        LanguageManager.Format("AboutSimToolsPage", "BadVersion", remoteVersionStr),
                        LanguageManager.Get("AboutSimToolsPage", "Title_Failed", "Update Check Failed"),
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
                        LanguageManager.Format("AboutSimToolsPage", "UpToDate", localVersion.ToString(3)),
                        LanguageManager.Get("AboutSimToolsPage", "Title_UpToDate", "Up to Date"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }

            // 4. Update available — ask the user
            var confirm = MessageBox.Show(
                LanguageManager.Format("AboutSimToolsPage", "NewVersion",
                    remoteVersionStr, localVersion.ToString(3)),
                LanguageManager.Get("AboutSimToolsPage", "Title_Available", "Update Available"),
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
                    LanguageManager.Get("AboutSimToolsPage", "Timeout", "The update check timed out."),
                    LanguageManager.Get("AboutSimToolsPage", "Title_Failed", "Update Check Failed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            if (!isAutomatic)
                MessageBox.Show(
                    LanguageManager.Format("AboutSimToolsPage", "Unexpected", ex.Message),
                    LanguageManager.Get("AboutSimToolsPage", "Title_Failed", "Update Check Failed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
        }
    }

    // ── Launch SimToolsUpdater.exe and shut down SimTools ─────────────────────

    private static void LaunchUpdaterAndExit()
    {
        string updaterPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "SimToolsUpdater.exe");

        if (!File.Exists(updaterPath))
        {
            MessageBox.Show(
                LanguageManager.Get("AboutSimToolsPage", "UpdaterMissing",
                    "SimToolsUpdater.exe was not found in the application directory."),
                LanguageManager.Get("AboutSimToolsPage", "Title_MissingUpdater", "Updater Not Found"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        Process.Start(new ProcessStartInfo(updaterPath) { UseShellExecute = true });
        App.Current.Shutdown();
    }

    // ── Suppression prompt (automatic checks only) ────────────────────────────
    //
    //  First dialog:  Yes/No — do you want to suppress at all?
    //  Second dialog: Yes = 30 days / No = indefinitely
    //
    private static void PromptSuppression()
    {
        var suppress = MessageBox.Show(
            LanguageManager.Get("AboutSimToolsPage", "Suppress_Ask",
                "Would you like to suppress automatic update notifications?"),
            LanguageManager.Get("AboutSimToolsPage", "Suppress_Title",
                "Suppress Update Notifications"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (suppress != MessageBoxResult.Yes) return;

        var duration = MessageBox.Show(
            LanguageManager.Get("AboutSimToolsPage", "Suppress_Duration",
                "How long would you like to suppress update notifications?"),
            LanguageManager.Get("AboutSimToolsPage", "Suppress_DurTitle",
                "Suppression Duration"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        string until = duration == MessageBoxResult.Yes
            ? DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")
            : "indefinite";

        IniHelper.Write("Updates", "SuppressAutoCheckUntil", until);
    }

    // ── Slideshow ─────────────────────────────────────────────────────────────────

    private static readonly string[] _slides =
    [
    "pack://application:,,,/Images/Dev/010.jpg",
    "pack://application:,,,/Images/Dev/009.jpg",
    "pack://application:,,,/Images/Dev/008.jpg",
    "pack://application:,,,/Images/Dev/007.jpg",
    "pack://application:,,,/Images/Dev/006.jpg",
    "pack://application:,,,/Images/Dev/005.jpg",
    "pack://application:,,,/Images/Dev/004.jpg",
    "pack://application:,,,/Images/Dev/003.png",
    "pack://application:,,,/Images/Dev/002.jpg",
    "pack://application:,,,/Images/Dev/001.png",
];

    private int _slideIndex = 0;
    private DispatcherTimer? _slideTimer;

    private void StartSlideshow()
    {
        // Show first image immediately
        DevSlideShow.Source = new BitmapImage(new Uri(_slides[0], UriKind.Absolute));

        _slideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _slideTimer.Tick += (_, _) => AdvanceSlide();
        _slideTimer.Start();
    }

    private void AdvanceSlide()
    {
        _slideIndex = (_slideIndex + 1) % _slides.Length;

        // Optional fade — comment out the two lines below and just set Source directly if you don't want it
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        fadeOut.Completed += (_, _) =>
        {
            DevSlideShow.Source = new BitmapImage(new Uri(_slides[_slideIndex], UriKind.Absolute));
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            DevSlideShow.BeginAnimation(OpacityProperty, fadeIn);
        };
        DevSlideShow.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void EasterEgg2_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Process.Start(new ProcessStartInfo(
            "https://youtu.be/Ea-gho4cjhI")
        {
            UseShellExecute = true
        });
        App.NotifyEasterEggFound(2);
    }

    private void EasterEgg4_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Process.Start(new ProcessStartInfo(
            "https://archive.org/details/the-tune-1992")
        {
            UseShellExecute = true
        });
        App.NotifyEasterEggFound(4);
    }
}
