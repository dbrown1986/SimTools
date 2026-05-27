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
        Loaded += (_, _) => StartSlideshow();
        Closing += (_, _) => _slideTimer?.Stop();
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
            // 1. Fetch version.txt from the server
            //    Expected format (one line minimum):
            //      1.0.1
            string versionUrl = AppSettings.ResolveUrl("%baseurl%/version.txt");

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            using var resp = await http.GetAsync(versionUrl);

            if (!resp.IsSuccessStatusCode)
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
