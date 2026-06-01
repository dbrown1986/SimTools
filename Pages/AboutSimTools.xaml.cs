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

    // ── Launch SimToolsUpdater.exe and shut down SimTools ─────────────────────

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

    // ── Suppression prompt (automatic checks only) ────────────────────────────
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

    // ── Slideshow ─────────────────────────────────────────────────────────────────

    private static readonly string[] _slides =
    [
        "pack://application:,,,/Images/Dev/001.png",
    "pack://application:,,,/Images/Dev/002.jpg",
    "pack://application:,,,/Images/Dev/003.png",
    "pack://application:,,,/Images/Dev/004.jpg",
    "pack://application:,,,/Images/Dev/005.jpg",
    "pack://application:,,,/Images/Dev/006.jpg",
    "pack://application:,,,/Images/Dev/007.jpg",
    "pack://application:,,,/Images/Dev/008.jpg",
    "pack://application:,,,/Images/Dev/009.jpg",
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
}
