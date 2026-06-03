using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
// Add these two aliases to resolve the ambiguity:
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Image = System.Windows.Controls.Image;

namespace SimTools
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            PopulateVersionInfo();
            // Context menu handling, so event handlers don't fire off early when the XAML is loading
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        // ── Secret developer shortcut: Ctrl+Shift+Alt+G → Key Generator ─────
        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.G
                && (System.Windows.Input.Keyboard.Modifiers &
                    (System.Windows.Input.ModifierKeys.Control
                     | System.Windows.Input.ModifierKeys.Shift
                     | System.Windows.Input.ModifierKeys.Alt))
                == (System.Windows.Input.ModifierKeys.Control
                    | System.Windows.Input.ModifierKeys.Shift
                    | System.Windows.Input.ModifierKeys.Alt))
            {
                e.Handled = true;
                new KeyGeneratorWindow { Owner = this }.ShowDialog();
            }
        }

        // Populates the VersionInfo TextBlock with dynamic copyright year and build version
        private void PopulateVersionInfo()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version ?? new Version(4, 0, 1, 0);

            int currentYear = DateTime.Now.Year;
            // ver.Major.Minor.Build = 4.0.1 ; ver.Revision = build counter
            string versionStr = $"v {ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";

            VersionInfo.Inlines.Clear();
            VersionInfo.Inlines.Add(new Run($"Copyright \u00a9 2004 - {currentYear}"));
            VersionInfo.Inlines.Add(new LineBreak());
            VersionInfo.Inlines.Add(new Run("Archeon Industries, LLC."));
            VersionInfo.Inlines.Add(new LineBreak());
            VersionInfo.Inlines.Add(new Run(versionStr));
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
            ButtonInfoText.Text = LanguageManager.Get("Main", "ButtonInfoText", ButtonInfoText.Text);
            FrameworkInfoText.Text = LanguageManager.Get("Main", "FrameworkInfoText", FrameworkInfoText.Text);
            ComparisonText.Text = LanguageManager.Get("Main", "ComparisonLabel", ComparisonText.Text);
            TS3Vanilla.Content = LanguageManager.Get("Main", "Btn_TS3Vanilla", "TS3 Vanilla (No Mods)");
            TS3WithSimTools.Content = LanguageManager.Get("Main", "Btn_TS3WithSimTools", "TS3 With SimTools");
            VideoTutorial.Content = LanguageManager.Get("Main", "Btn_VideoTutorial", "SimTools Video Guide");
            NewGPUButton.Content = LanguageManager.Get("Main", "Btn_GPU", "GPU Support >");
            TweakButton.Content = LanguageManager.Get("Main", "Btn_Tweaks", "Game Tweaks >");
            GPUText.Text = LanguageManager.Get("Main", "GPU_Description", GPUText.Text);
            TweakText.Text = LanguageManager.Get("Main", "Tweaks_Description", TweakText.Text);
            BugfixText.Text = LanguageManager.Get("Main", "Bugfix_Description", BugfixText.Text);
            SaveCleanerText.Text = LanguageManager.Get("Main", "SaveCleaner_Description", SaveCleanerText.Text);
            ModsText.Text = LanguageManager.Get("Main", "Mods_Description", ModsText.Text);
            TS3StoreText.Text = LanguageManager.Get("Main", "Store_Description=", TS3StoreText.Text);
            BuyTS3Text.Text = LanguageManager.Get("Main", "BuyGame_Description", BuyTS3Text.Text);
            ModToolsText.Text = LanguageManager.Get("Main", "ModTools_Description", ModToolsText.Text);
            GenKeysText.Text = LanguageManager.Get("Main", "GenKeys_Description", GenKeysText.Text);
            BugFixButton.Content = LanguageManager.Get("Main", "Bugfix_Button", "Bugfix Central >");
            SaveCleanerButton.Content = LanguageManager.Get("Main", "SaveCleaner_Button", "Save Cleaner");
            ModButton.Content = LanguageManager.Get("Main", "Mods_Button", "Recommended Mods >");
            StoreButton.Content = LanguageManager.Get("Main", "Store_Button", "Sims 3 Store >");
            ModToolsButton.Content = LanguageManager.Get("Main", "ModTools_Button", "Mod Tools >");
            BuySimsButton.Content = LanguageManager.Get("Main", "BuyGame_Button", "Buy Sims Games");
            GenericKeysButton.Content = LanguageManager.Get("Main", "GenKeys_Button", "Generic Game Keys");
            ModFrameworkButton.Content = LanguageManager.Get("Main", "ModFramework_Button", "Mod Framework");
            DonateButton.Content = LanguageManager.Get("Main", "Donate_Button", "Support Me!");
            ThanksButton.Content = LanguageManager.Get("Main", "Thanks_Button", "Acknowledgements");
            WarningButton.Content = LanguageManager.Get("Main", "Warning_Button", "IMPORTANT NOTICE!");
            SimPortButton.Content = LanguageManager.Get("Main", "SimPort_Button", "Add Me on SimPort");
            
            // ── Rebuild context menus with localised headers ────────────────────
            SetupGPUContextMenu();
            SetupTweakContextMenu();
            SetupBugFixContextMenu();
            SetupModContextMenu();
            SetupStoreContextMenu();
            SetupModToolsContextMenu();
        }

        // Context menu icon helper
        private static Image MenuIcon(string packPath) => new Image
        {
            Source = new BitmapImage(new Uri(packPath)),
            Width = 16,
            Height = 16
        };

        // Routes user to the vanilla demo video on YouTube
        private void TS3Vanilla_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "TS3Vanilla")
            {
                OpenUrl("https://www.youtube.com/watch?v=WvsPzENc8Ps");
            }
        }

        // Routes user to the SimTools demo video on YouTube
        private void WithSimTools_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "TS3WithSimTools")
            {
                OpenUrl("https://www.youtube.com/watch?v=hLXq3eVV1Eo");
            }
        }

        // Helper method to open URLs in the default web browser
        private static void OpenUrl(string url)
        {
            try
            {
                // Let's load the URL
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            // URL not found or there was an error contacting the internet.
            catch (Exception ex)
            {
                MessageBox.Show(
                LanguageManager.Format("Messages", "Error_OpenLink", ex.Message),
                LanguageManager.Get("Messages", "Error_Title", "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Routes user to the video tutorial on YouTube
        private void VideoTutorial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "VideoTutorial")
            {
                OpenUrl("https://www.youtube.com/watch?v=IkewLXsy-CA");
            }
        }

        // GPU button handler, forces left-click context-menu interaction.
        private void NewGPUButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }


        // Context menu event handler for the "New GPU" button
        // Build the menu ONCE at startup — fixes the immediate-launch bug
        private void SetupGPUContextMenu()
        {
            var contextMenu = new ContextMenu();

            // ── The Sims 2 (sub-menu) ──────────────────────────────────────────
            var sims2Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims2.ico"), Header = LanguageManager.Get("ContextMenu", "GPU_Sims2", "The Sims 2") };

            var sims2_32 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_32", "32-Bit") };
            sims2_32.Click += (s, args) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("GPU", "Sims2Warning", "Do NOT apply this patch for Legacy Collection."),
                    LanguageManager.Get("GPU", "Sims2Title", "Graphics Rules Maker — The Sims 2"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/graphicsrulesmaker.exe",  // ← replace
                    fileName: "graphicsrulesmaker-32bit.exe",
                    downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
                );
            };

            var sims2_64 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_64", "64-Bit") };
            sims2_64.Click += (s, args) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("GPU", "Sims2Warning", "Do NOT apply this patch for Legacy Collection."),
                    LanguageManager.Get("GPU", "Sims2Title", "Graphics Rules Maker — The Sims 2"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x64/graphicsrulesmaker.exe",  // ← replace
                    fileName: "graphicsrulesmaker-64bit.exe",
                    downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
                );
            };

            sims2Item.Items.Add(sims2_32);
            sims2Item.Items.Add(sims2_64);

            // ── The Sims Stories (sub-menu) ────────────────────────────────────
            var simsStoriesItem = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSLife.ico"), Header = LanguageManager.Get("ContextMenu", "GPU_SimsStories", "The Sims Stories") };

            var simsStories_32 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_32", "32-Bit") };
            simsStories_32.Click += (s, args) => DownloadAndOpenExe(
                url: "%baseurl%/Sideload-Apps/x86/graphicsrulesmaker.exe",  // ← replace
                fileName: "graphicsrulesmaker-32bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            var simsStories_64 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_64", "64-Bit") };
            simsStories_64.Click += (s, args) => DownloadAndOpenExe(
                url: "%baseurl%/Sideload-Apps/x64/graphicsrulesmaker.exe",  // ← replace
                fileName: "graphicsrulesmaker-64bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            simsStoriesItem.Items.Add(simsStories_32);
            simsStoriesItem.Items.Add(simsStories_64);

            // ── The Sims 3 (no sub-menu) ───────────────────────────────────────
            var sims3Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims3.ico"), Header = LanguageManager.Get("ContextMenu", "GPU_Sims3", "The Sims 3") };
            sims3Item.Click += (s, args) => DownloadAndOpenExe(
                url: "%baseurl%/Sideload-Apps/x86/TS3_GPU_Addon.exe",
                fileName: "TS3_GPU_Addon.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            contextMenu.Items.Add(sims2Item);
            contextMenu.Items.Add(simsStoriesItem);
            contextMenu.Items.Add(sims3Item);

            NewGPUButton.ContextMenu = contextMenu;
        }

        // Handler is now empty — menu is pre-built, nothing to do here
        private void NewGPUButton_Context(object sender, ContextMenuEventArgs e) { e.Handled = true; }

        // ── Helper: download only if missing, then launch ─────────────────────────
        private async void DownloadAndOpenExe(string url, string fileName, string downloadDirectory)
        {
            // Resolve %baseurl% placeholder before any network call
            url = AppSettings.ResolveUrl(url);
            await RepoValidator.ValidateAndWarnAsync();

            // Create the directory if it doesn't exist yet
            Directory.CreateDirectory(downloadDirectory);

            string exePath = Path.Combine(downloadDirectory, fileName);
            bool needsDownload = !File.Exists(exePath);

            // If the file already exists locally, do a lightweight HEAD request to check
            // whether the server has a newer version (via Last-Modified header).
            if (!needsDownload)
            {
                try
                {
                    using var headClient = new HttpClient();
                    headClient.Timeout = TimeSpan.FromSeconds(10);
                    using var headResponse = await headClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, url));

                    if (headResponse.IsSuccessStatusCode)
                    {
                        DateTimeOffset? remoteDate = headResponse.Content.Headers.LastModified;
                        if (remoteDate.HasValue)
                        {
                            DateTime localWriteUtc = File.GetLastWriteTimeUtc(exePath);
                            // 5-second tolerance absorbs minor clock skew between client and server
                            needsDownload = remoteDate.Value.UtcDateTime > localWriteUtc.AddSeconds(5);
                        }
                        // If the server sends no Last-Modified, we cannot compare — keep local copy
                    }
                }
                catch
                {
                    // Network unavailable or HEAD not supported — run the local copy silently
                }
            }

            if (needsDownload)
            {
                var progressWindow = new DownloadProgressWindow(fileName)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };
                progressWindow.Show();

                DateTimeOffset? remoteLastModified = null;

                try
                {
                    using var http = new HttpClient();
                    using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    // Capture Last-Modified from the GET response so we can stamp the local file
                    remoteLastModified = response.Content.Headers.LastModified;

                    long? totalBytes = response.Content.Headers.ContentLength;

                    await using var contentStream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream = new FileStream(
                        exePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    var buffer = new byte[8192];
                    long bytesRead = 0;
                    int lastPercent = 0;
                    int chunk;

                    while ((chunk = await contentStream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, chunk));
                        bytesRead += chunk;

                        if (totalBytes.HasValue)
                        {
                            int percent = (int)(bytesRead * 100 / totalBytes.Value);
                            if (percent != lastPercent)
                            {
                                lastPercent = percent;
                                progressWindow.UpdateProgress(percent);
                            }
                        }
                        else
                        {
                            progressWindow.SetIndeterminate();
                        }
                    }

                    // Stamp the local file with the server's Last-Modified time so the next
                    // HEAD comparison works correctly even after an app restart.
                    if (remoteLastModified.HasValue)
                        File.SetLastWriteTimeUtc(exePath, remoteLastModified.Value.UtcDateTime);
                }
                catch (Exception ex)
                {
                    if (File.Exists(exePath))
                        File.Delete(exePath);

                    progressWindow.Close();

                    MessageBox.Show(
                        $"{LanguageManager.Format("Messages", "Error_DownloadFailed", fileName)}\n{ex.Message}",
                        LanguageManager.Get("Messages", "Error_DownloadTitle", "Download Error"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                finally
                {
                    progressWindow.Close();
                }
            }

            Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
        }

        // Tweaks button left-click context menu.
        private void TweakButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }


        private void SetupTweakContextMenu()
        {
            var contextMenu = new ContextMenu();
            string binDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries");

            // ── The Sims 1 ────────────────────────────────────────────────────────────
            var sims1Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims1.ico"), Header = LanguageManager.Get("ContextMenu", "Tweaks_Sims1", "The Sims 1") };

            var sims1_simitone = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Tweaks_Simitone", "Simitone") };
            sims1_simitone.Click += async (_, _) =>
            {
                if (!GamePaths.IsConfigured(GamePaths.Sims1Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Messages", "Error_Sims1PathNotSet",
                            "The Sims 1 game directory has not been configured.\nPlease set it in Settings before using this feature."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string gameDir = GamePaths.Sims1Game;
                string tempZip = Path.Combine(Path.GetTempPath(), "SimitoneWindows.zip");

                var (ok, _) = await DownloadFileOnly(
                    url: "%baseurl%/Sideload-Apps/x86/SimitoneWindows.zip",  // ← replace
                    destFilePath: tempZip);

                if (!ok) return;

                try
                {
                    ZipFile.ExtractToDirectory(tempZip, gameDir, overwriteFiles: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        LanguageManager.Format("ModResources", "Simitone_ExtractFail", ex.Message),
                        LanguageManager.Get("ModResources", "Simitone_ExtractTitle", "SimTools — Extraction Error"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string simitoneExe = Path.Combine(gameDir, "Simitone.Windows.exe");
                string message = File.Exists(simitoneExe)
                    ? LanguageManager.Get("ModResources", "Simitone_Installed", "Simitone has been installed.")
                    : LanguageManager.Get("ModResources", "Simitone_Extracted", "Simitone has been extracted.");

                var result = MessageBox.Show(message,
                    LanguageManager.Get("ModResources", "Simitone_Title", "SimTools — Simitone Installed"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    if (OperatingSystem.IsWindows())
                        CreateDesktopShortcut(
                            targetExe: File.Exists(simitoneExe) ? simitoneExe : gameDir,
                            shortcutName: "Simitone",
                            description: "Simitone — The Sims 1 for Modern Computers");
            };
            sims1Item.Items.Add(sims1_simitone);
            contextMenu.Items.Add(sims1Item);

            // ── The Sims 2 ────────────────────────────────────────────────────────────
            var sims2Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims2.ico"), Header = "The Sims 2" };

        

            var sims2_rpc = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims2RPC.ico"), Header = "Sims2RPC" };
            sims2_rpc.Click += (_, _) =>
            {
                var result = MessageBox.Show(
                    LanguageManager.Get("ModResources", "Sims2RPC_Ask", "Which version of The Sims 2 do you have installed?"),
                    LanguageManager.Get("ModResources", "Sims2RPC_Title", "SimTools — Sims2RPC / Legacy Extender"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Vanilla Sims 2 — install Sims2RPC
                    DownloadAndOpenExe(
                        url: "%baseurl%/Sideload-Apps/x86/Sims2RPCInstaller.exe",
                        fileName: "Sims2RPCInstaller.exe",
                        downloadDirectory: binDir);
                }
                else
                {
                    // Legacy Edition — open LazyDuchess Legacy Extender GitHub page
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/LazyDuchess/TS2-Extender",
                        UseShellExecute = true
                    });
                }
            };
            sims2Item.Items.Add(sims2_rpc);
            contextMenu.Items.Add(sims2Item);

            // ── The Sims: Castaway Stories ────────────────────────────────────────────
            var castawayItem = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSCastaway.ico"), Header = "The Sims: Castaway Stories" };
            castawayItem.Click += (_, _) => OpenUrl("https://modthesims.info/t/513463");
            contextMenu.Items.Add(castawayItem);

            // ── The Sims 3 ────────────────────────────────────────────────────────────
            var sims3Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims3.ico"), Header = "The Sims 3" };

            // ── Best In-Game Settings for TS3 ─────────────────────────────────────────
            var ts3_bestSettings = new MenuItem { Header = "Best In-Game Settings" };
            ts3_bestSettings.Click += (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Messages", "BestSettingsDeprecated_Message", "The best settings article has been deprecated in favor of the use of Sims 3 Settings Setter. It has been included for posterity in the event users are having issues with S3SS. It may be removed in a future version of SimTools."),
                    LanguageManager.Get("Messages", "BestSettingsDeprecated_Title", "Best Settings Article Deprecated — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                {
                    OpenUrl("https://simtools-app.com/best-in-game-settings-ts3");
                };
            };
            
            sims3Item.Items.Add(ts3_bestSettings);

            // ── Game INI Tweaks ─────────────────────────────────────────────────
            var ts3_iniTweaks = new MenuItem { Header = "Game INI Tweaks" };

            var iniItems = new[]
            {
                ("Limit Game FPS", LanguageManager.Get("Tweaks", "IniTweaks_DeprecatedWarning", "The Game INI Tweak articles have been deprecated in favor of the use of Sims 3 Settings Setter. It has been included for posterity in the event users are having issues with S3SS. It may be removed in a future version of SimTools."), "https://simtools-app.com/limit-game-fps-ts3"),
                ("Allow More CPU Usage", LanguageManager.Get("Tweaks", "IniTweaks_DeprecatedWarning", "The Game INI Tweak articles have been deprecated in favor of the use of Sims 3 Settings Setter. It has been included for posterity in the event users are having issues with S3SS. It may be removed in a future version of SimTools."), "https://simtools-app.com/allow-more-cpu-usage-ts3"),
                ("Allow More GPU Usage", LanguageManager.Get("Tweaks", "IniTweaks_DeprecatedWarning", "The Game INI Tweak articles have been deprecated in favor of the use of Sims 3 Settings Setter. It has been included for posterity in the event users are having issues with S3SS. It may be removed in a future version of SimTools."), "https://simtools-app.com/allow-more-gpu-usage-ts3"),
                ("Clean DCBackup Cache", LanguageManager.Get("Tweaks", "IniTweaks_DeprecatedWarning", "The Game INI Tweak articles have been deprecated in favor of the use of Sims 3 Settings Setter. It has been included for posterity in the event users are having issues with S3SS. It may be removed in a future version of SimTools."), "https://simtools-app.com/clean-dcbackup-ts3"),
            };

            foreach (var (label, warning, url) in iniItems)
            {
                var item = new MenuItem { Header = label };
                item.Click += (_, _) =>
                {
                    var result = MessageBox.Show(warning, "SimTools — Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.OK)
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                };
                ts3_iniTweaks.Items.Add(item);
            }

            sims3Item.Items.Add(ts3_iniTweaks);

            // ── Intel Alder Lake Fix ──────────────────────────────────────────────────
            var ts3_alderLake = new MenuItem { Header = "Intel Alder Lake Fix" };
            ts3_alderLake.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "AlderLake_Info1", "Intel Alder Lake Fix for The Sims 3."),
                    LanguageManager.Get("Tweaks", "AlderLake_Title", "Intel Alder Lake Fix — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "AlderLake_Info2", "EA App version info for Alder Lake."),
                    LanguageManager.Get("Tweaks", "AlderLake_Title", "Intel Alder Lake Fix — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string gameDir = GamePaths.Sims3Game;
                string tempZip = Path.Combine(Path.GetTempPath(), "AlderLakePatch.zip");

                var (ok, _) = await DownloadFileOnly(
                    url: "%baseurl%/Sideload-Apps/x86/AlderLakePatch.zip",  // ← replace
                    destFilePath: tempZip);

                if (!ok) return;

                try { ZipFile.ExtractToDirectory(tempZip, gameDir, overwriteFiles: true); }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.Format("Tweaks", "AlderLake_ExtractFail", ex.Message),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string patchExe = Path.Combine(gameDir, "AlderLakePatch.exe");

                if (!File.Exists(patchExe))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Tweaks", "AlderLake_NotFound", "AlderLakePatch.exe could not be found."),
                        LanguageManager.Get("Messages", "Error_Title", "Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName         = patchExe,
                    WorkingDirectory = gameDir,
                    UseShellExecute  = true
                });
            };
            sims3Item.Items.Add(ts3_alderLake);

            // ── Ultimate ASI Loader ───────────────────────────────────
            var ts3_uasil = new MenuItem { Header = "Ultimate ASI Loader" };
            ts3_uasil.Click += async (_, _) =>
            {
                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    url: "%baseurl%/Sideload-Apps/x86/wininet.dll",  // ← replace
                    destFilePath: Path.Combine(GamePaths.Sims3Game, "wininet.dll"));
            };
            sims3Item.Items.Add(ts3_uasil);

            // ── Sims 3 Settings Setter ───────────────────────────────────
            var ts3_s3ss = new MenuItem { Header = "Sims 3 Settings Setter" };
            ts3_s3ss.Click += async (_, _) =>
            {
                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    url: "%baseurl%/Sideload-Apps/x86/S3SS/Sims3SettingsSetter.asi",  // ← replace
                    destFilePath: Path.Combine(GamePaths.Sims3Game, "Sims3SettingsSetter.asi"));
            };
            sims3Item.Items.Add(ts3_s3ss);

            // ── DXVK for Sims 3───────────────────────────────────
            var ts3_dxvk = new MenuItem { Header = "DXVK for Sims 3" };
            ts3_dxvk.Click += async (_, _) =>
            {
                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    url: "%baseurl%/Sideload-Apps/x86/d3d9.dll",  // ← replace
                    destFilePath: Path.Combine(GamePaths.Sims3Game, "d3d9.dll"));
            };
            sims3Item.Items.Add(ts3_dxvk);

            // ── LazyDuchess Launcher ──────────────────────────────────────────────────
            var ts3_ldLauncher = new MenuItem { Header = "LazyDuchess Launcher" };
            ts3_ldLauncher.Click += (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "LDLauncher_Info1", "LazyDuchess Launcher info."),
                    LanguageManager.Get("Tweaks", "LDLauncher_Title", "LazyDuchess Launcher — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "LDLauncher_Info2", "LazyDuchess Launcher install info."),
                    LanguageManager.Get("Tweaks", "LDLauncher_Title", "LazyDuchess Launcher — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ── Silently back up the existing launcher before overwriting ─────────
                string launcherPath = Path.Combine(GamePaths.Sims3Game, "Game", "Bin", "Sims3Launcher.exe");
                string backupPath = launcherPath + ".bak";

                if (File.Exists(launcherPath))
                {
                    try
                    {
                        if (File.Exists(backupPath))
                            File.Delete(backupPath);
                        File.Move(launcherPath, backupPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Could not back up the existing Sims3Launcher.exe:\n{ex.Message}\n\n" +
                            "The installation will not proceed.",
                            "SimTools — Backup Failed",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/Sims3Launcher.exe",
                    fileName: "Game/Bin/Sims3Launcher.exe",
                    downloadDirectory: GamePaths.Sims3Game
                );
            };
            sims3Item.Items.Add(ts3_ldLauncher);

            // ── Mono Patcher Library ──────────────────────────────────────────────────
            var ts3_monoPatcher = new MenuItem { Header = "Mono Patcher Library" };
            ts3_monoPatcher.Click += (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "MonoPatcher_Info1", "Mono Patcher info."),
                    LanguageManager.Get("Tweaks", "MonoPatcher_Title", "Mono Patcher Library — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "MonoPatcher_Info2", "Current Version: 0.2.2"),
                    LanguageManager.Get("Tweaks", "MonoPatcher_Title", "Mono Patcher Library — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/ld_mpl.exe",
                    fileName: "ld_mpl.exe",
                    downloadDirectory: GamePaths.Sims3Game
                );
            };
            sims3Item.Items.Add(ts3_monoPatcher);

            // ── TinyUI Fix ────────────────────────────────────────────────────────────
            var ts3_tinyUI = new MenuItem { Header = "TinyUI Fix" };
            ts3_tinyUI.Click += (_, _) =>
            {
                if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/tiny-ui-fix-for-ts3.bat",
                    fileName: "tiny-ui-fix-for-ts3.bat",
                    downloadDirectory: GamePaths.Sims3Game
                );
            };
            sims3Item.Items.Add(ts3_tinyUI);

            // ── Sweet Treats Conversion Guide ─────────────────────────────────────────
            var ts3_sweetTreats = new MenuItem { Header = "Sweet Treats Conversion Guide" };
            ts3_sweetTreats.Click += (_, _) => OpenUrl("%baseurl%/guides/sweet-treats");  // ← replace with actual URL
            sims3Item.Items.Add(ts3_sweetTreats);

            // ── nRaas Core Mods (sub-menu) ────────────────────────────────────────────
            var ts3_nraas = new MenuItem { Header = "nRaas Core Mods" };

            // Local helper — creates a package download item targeting Sims3Mods/SimTools/Packages
            MenuItem NRaasPackageItem(string header, string fileName)
            {
                var item = new MenuItem { Header = header };
                item.Click += async (_, _) =>
                {
                    if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                    {
                        MessageBox.Show(
                            LanguageManager.Get("Paths", "Sims3Mods", "Your Sims 3 Mods directory is not configured."),
                            LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!ModFrameworkHelper.EnsureInstalled(GamePaths.Sims3Mods)) return;
                    await DownloadFileOnly(
                        $"%baseurl%/Mods/Sims3/nRaas/{fileName}",
                        Path.Combine(GamePaths.Sims3Mods, "SimTools", "Packages", fileName));
                };
                return item;
            }

            ts3_nraas.Items.Add(NRaasPackageItem("ErrorTrap for EA (1.69)", "NRaas_ErrorTrap_EA.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("ErrorTrap for Steam (1.67)", "NRaas_ErrorTrap_Steam.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("ErrorTrap for Retail (1.67)", "NRaas_ErrorTrap_Retail.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("Overwatch", "NRaas_Overwatch.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("Master Controller", "NRaas_MasterController.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("Register", "NRaas_Register.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("Saver", "NRaas_Saver.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("Debug Enabler", "NRaas_DebugEnabler.package"));
            ts3_nraas.Items.Add(NRaasPackageItem("nRaas No-CD", "NRaas_NoCD.package"));

            var nraas_more = new MenuItem { Header = "More nRaas Mods" };
            nraas_more.Click += (_, _) => OpenUrl("https://www.nraas.net/community/Mods-List");
            ts3_nraas.Items.Add(nraas_more);

            sims3Item.Items.Add(ts3_nraas);
            contextMenu.Items.Add(sims3Item);

            // ── The Sims Medieval ─────────────────────────────────────────────────────
            var medievalItem = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSM.ico"), Header = "The Sims Medieval" };

            var medieval_smoothPatch = new MenuItem { Header = "LazyDuchess Smooth Patch" };
            medieval_smoothPatch.Click += (_, _) =>
            {
                if (!GamePaths.IsConfigured(GamePaths.SimsMedievalGame))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "SimsMedieval", "Your Sims Medieval Game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/TSM_SP/TS3Patch.asi",
                    fileName: "TS3Patch.asi",
                    downloadDirectory: GamePaths.SimsMedievalGame);
                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/TSM_SP/TS3Patch.txt",
                    fileName: "TS3Patch.txt",
                    downloadDirectory: GamePaths.SimsMedievalGame);
                DownloadAndOpenExe(
                    url: "%baseurl%/Sideload-Apps/x86/TSM_SP/wininet.dll",
                    fileName: "wininet.dll",
                    downloadDirectory: GamePaths.SimsMedievalGame);
            };
            medievalItem.Items.Add(medieval_smoothPatch);

            contextMenu.Items.Add(medievalItem);

            TweakButton.ContextMenu = contextMenu;
        }

        // Handler is now empty — menu is pre-built, nothing to do here
        private void TweakButton_Context(object sender, ContextMenuEventArgs e) { e.Handled = true; }

        // ── Mod Framework Button ───────────────────────────────────────────────────
        // Installs Resource.cfg and the standard folder structure into %Sims3Mods%.
        // If the path is not configured, attempts to auto-detect and offers to create
        // the Mods folder under Documents\Electronic Arts\The Sims 3.
        private async void ModFrameworkButton_Click(object sender, RoutedEventArgs e)
        {
            string modsPath = GamePaths.Sims3Mods;

            if (!GamePaths.IsConfigured(modsPath))
            {
                // ── Auto-detect: Documents\Electronic Arts\The Sims 3 ────────────
                string docs    = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string ts3Root = Path.Combine(docs, "Electronic Arts", "The Sims 3");
                string candidate = Path.Combine(ts3Root, "Mods");

                if (Directory.Exists(ts3Root))
                {
                    var answer = MessageBox.Show(
                        LanguageManager.Format("Framework", "ModsNotSet_Ask", ts3Root),
                        LanguageManager.Get("Framework", "ModsNotSet_Title", "SimTools — Mods Directory Not Set"),
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (answer != MessageBoxResult.Yes) return;

                    Directory.CreateDirectory(candidate);
                    IniHelper.Write("Directories", "Sims3_Mods", candidate);
                    modsPath = candidate;
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.Get("Framework", "ModsNotSet_Manual", "Mods directory not set - manual instructions."),
                        LanguageManager.Get("Framework", "ModsNotSet_Title", "SimTools — Mods Directory Not Set"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (!InstallModFramework(modsPath)) return;

            string simtoolsPkgDir = Path.Combine(modsPath, "SimTools", "Packages");

            // ── Silent: nobuildsparkles.package ───────────────────────────────────
            var (ok1, _) = await DownloadFileOnly(
                url: "%baseurl%/Framework/NoBuildSparkles.package",  // ← replace
                destFilePath: Path.Combine(simtoolsPkgDir, "NoBuildSparkles.package"));
            if (!ok1) return;

            // ── Silent: nointro.package ───────────────────────────────────────────
            var (ok2, _) = await DownloadFileOnly(
                url: "%baseurl%/Framework/nointromaxis.package",          // ← replace
                destFilePath: Path.Combine(simtoolsPkgDir, "nointromaxis.package"));
            if (!ok2) return;

            // ── Optional: SimTools custom game intro ──────────────────────────────
            if (MessageBox.Show(
                    LanguageManager.Get("Framework", "GameIntro_Ask", "Install SimTools custom game intro?"),
                    LanguageManager.Get("Framework", "GameIntro_Title", "SimTools — Custom Game Intro"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var (okIntro, _) = await DownloadFileOnly(
                    url: "%baseurl%/Framework/SimToolsIntro.package",        // ← replace
                    destFilePath: Path.Combine(simtoolsPkgDir, "SimToolsIntro.package"));
                if (!okIntro) return;
            }

            // ── Optional: SimTools custom splashscreen ────────────────────────────
            if (MessageBox.Show(
                    LanguageManager.Get("Framework", "Splashscreen_Ask", "Install SimTools custom splashscreen?"),
                    LanguageManager.Get("Framework", "Splashscreen_Title", "SimTools — Custom Splashscreen"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var (okSplash, _) = await DownloadFileOnly(
                    url: "%baseurl%/Mod-Framework/SimToolsSplashscreen.package",    // ← replace
                    destFilePath: Path.Combine(simtoolsPkgDir, "SimToolsSplashscreen.package"));
                if (!okSplash) return;
            }

            // ── Unified success message ───────────────────────────────────────────
            MessageBox.Show(
                LanguageManager.Format("Framework", "Installed", modsPath),
                LanguageManager.Get("Framework", "Installed_Title", "SimTools — Mod Framework Installed"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static bool InstallModFramework(string modsPath)
        {
            try
            {
                // ── 1. Extract Resource.cfg from compiled resources (pack URI) ──────
                string destCfg = Path.Combine(modsPath, "Resource.cfg");
                var sri = System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/Resources/Resource.cfg"));
                if (sri == null)
                {
                    MessageBox.Show(
                        LanguageManager.Get("Framework", "MissingResource", "Resource.cfg could not be found."),
                        LanguageManager.Get("Framework", "MissingResource_Title", "SimTools — Missing Resource"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                using (var inStream  = sri.Stream)
                using (var outStream = File.Create(destCfg))
                    inStream.CopyTo(outStream);

                // ── 2. Create folder structure ────────────────────────────────────
                string[] subfolders =
                {
                    "Disabled",
                    Path.Combine("Disabled", "Overrides"),
                    Path.Combine("Disabled", "Packages"),
                    Path.Combine("Disabled", "Probation"),
                    Path.Combine("Disabled", "Test"),
                    "Overrides",
                    "Packages",
                    "Probation",
                    Path.Combine("Probation", "Overrides"),
                    Path.Combine("Probation", "Packages"),
                    "SimTools",
                    Path.Combine("SimTools", "Overrides"),
                    Path.Combine("SimTools", "Packages"),
                    "Test",
                    Path.Combine("Test", "Overrides"),
                    Path.Combine("Test", "Packages"),
                };

                foreach (var sub in subfolders)
                    Directory.CreateDirectory(Path.Combine(modsPath, sub));

                // ── 3. Report success ─────────────────────────────────────────────
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.Format("Framework", "InstallFailed", ex.Message),
                    LanguageManager.Get("Framework", "InstallFailed_Title", "SimTools — Installation Failed"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Opens the About SimTools window
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutSimTools { Owner = this };
            about.ShowDialog();
        }

        private void ThanksButton_Click(object sender, RoutedEventArgs e)
        {
            new SpecialThanks { Owner = this }.ShowDialog();
        }

        private void GenericKeysButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                LanguageManager.Get("GenericKeys", "Info", "Generic product keys info."),
                LanguageManager.Get("GenericKeys", "Info_Title", "SimTools — Generic Keys"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            new GenericKeys { Owner = this }.ShowDialog();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // BUGFIX BUTTON
        // ═══════════════════════════════════════════════════════════════════════

        // Opens the context menu on left-click (same pattern as GPU / Tweaks buttons)
        private void BugFixButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Suppress the native right-click popup — menu is pre-built
        private void BugFixButton_Context(object sender, ContextMenuEventArgs e) { e.Handled = true; }

        // ── SetupBugFixContextMenu ─────────────────────────────────────────────
        // Builds the Bugfix Central context menu. Called from ApplyLanguage() so
        // it rebuilds automatically after a language change.
        //
        // Structure:
        //   The Sims 1       → warning message (use Simitone)
        //   The Sims 2
        //     Sim Shadow Fix → info msg → download .package to %Sims2Mods%
        //     Bright CAS Fix → info msg → download .package to %Sims2Mods%
        //   The Sims 3
        //     Patch Downloader → 2× info msg → download TS3Lib.dll + TS3PD.exe → run TS3PD.exe
        //     Simler90's Fixes → info msg → download .package to %Sims3Mods%
        //     Gameplay Fixes   → info msg → open GameplayFixesWindow
        //   The Sims 4       → warning message (placeholder)
        //   SimCopter
        //     SimCopterX → info msg → download SimCopterX.exe to %SimCopter% → run
        //   Streets of SimCity
        //     SimStreetsX → info msg → download SSXLauncher.exe to %StreetsOfSimCity% → run
        //   SimCity 2000
        //     SC2kFix  → info msg → download + extract sc2kfix-r10.zip to %SimCity2000%
        //     SC2000X  → info msg → download SC2000X.exe to %SimCity2000% → run
        // ──────────────────────────────────────────────────────────────────────
        private void SetupBugFixContextMenu()
        {
            var contextMenu = new ContextMenu();

            // Shared local Install dir — used for tools that live next to the app
            string installDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install");

            // ─────────────────────────────────────────────────────────────────
            // The Sims 1 — no fixes available; redirect user to Simitone
            // ─────────────────────────────────────────────────────────────────
            var sims1Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims1.ico"), Header = "The Sims 1" };
            sims1Item.Click += (_, _) =>
                MessageBox.Show(
                    LanguageManager.Get("BugFixes", "Sims1_Info", "Please use Simitone under Game Tweaks for Sims 1 bugs."),
                    LanguageManager.Get("BugFixes", "Sims1_Title", "The Sims 1 — Bug Fixes"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            contextMenu.Items.Add(sims1Item);

            // ─────────────────────────────────────────────────────────────────
            // The Sims 2
            // ─────────────────────────────────────────────────────────────────
            var sims2Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims2.ico"), Header = "The Sims 2" };

            // ── Sim Shadow Fix ────────────────────────────────────────────────
            var sims2_shadowFix = new MenuItem { Header = "Sim Shadow Fix" };
            sims2_shadowFix.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("BugFixes", "ShadowFix_Info", "Sim Shadow Fix info."),
                    LanguageManager.Get("BugFixes", "ShadowFix_Title", "Sim Shadow Fix — The Sims 2"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims2Mods))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims2Mods", "Your Sims 2 Mods directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    "%baseurl%/Mods/Sims2/Downloads/simNopke-simShadowFix-maxisMatch.package",
                    Path.Combine(GamePaths.Sims2Mods, "simNopke-simShadowFix-maxisMatch.package"));
            };
            sims2Item.Items.Add(sims2_shadowFix);

            // ── Bright CAS Fix ────────────────────────────────────────────────
            var sims2_brightCas = new MenuItem { Header = "Bright CAS Fix" };
            sims2_brightCas.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("BugFixes", "BrightCAS_Info", "Bright CAS Fix info."),
                    LanguageManager.Get("BugFixes", "BrightCAS_Title", "Bright CAS Fix — The Sims 2"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims2Mods))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims2Mods", "Your Sims 2 Mods directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    "%baseurl%/Mods/Sims2/Downloads/ld_BrightCASFix.package",
                    Path.Combine(GamePaths.Sims2Mods, "ld_BrightCASFix.package"));
            };
            sims2Item.Items.Add(sims2_brightCas);

            contextMenu.Items.Add(sims2Item);

            // ─────────────────────────────────────────────────────────────────
            // The Sims 3
            // ─────────────────────────────────────────────────────────────────
            var sims3Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims3.ico"), Header = "The Sims 3" };

            // ── Patch Downloader ──────────────────────────────────────────────
            // Shows two info messages, downloads TS3Lib.dll + TS3PD.exe to /Install,
            // then launches TS3PD.exe.  Both files use HEAD-check / skip-if-same logic.
            var ts3_patchDl = new MenuItem { Header = "Patch Downloader" };
            ts3_patchDl.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "PatchDL_Info1", "Patch Downloader info."),
                    LanguageManager.Get("Tweaks", "PatchDL_Title", "Patch Downloader — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "PatchDL_Info2", "Patch Downloader usage info."),
                    LanguageManager.Get("Tweaks", "PatchDL_Title", "Patch Downloader — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "PatchDL_Info3", "Patch each title recommendation."),
                    LanguageManager.Get("Tweaks", "PatchDL_Title", "Patch Downloader — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Directory.CreateDirectory(installDir);

                var (libOk, _) = await DownloadFileOnly(
                    "%baseurl%/Sideload-Apps/x86/TS3PD/TS3Lib.dll",
                    Path.Combine(installDir, "TS3Lib.dll"));
                if (!libOk) return;

                var (exeOk, _) = await DownloadFileOnly(
                    "%baseurl%/Sideload-Apps/x86/TS3PD/TS3PD.exe",
                    Path.Combine(installDir, "TS3PD.exe"));
                if (!exeOk) return;

                Process.Start(new ProcessStartInfo(
                    Path.Combine(installDir, "TS3PD.exe"))
                { UseShellExecute = true });
            };
            sims3Item.Items.Add(ts3_patchDl);

            // ── Simler90's Fixes ──────────────────────────────────────────────
            var ts3_simler90 = new MenuItem { Header = "Simler90's Fixes" };
            ts3_simler90.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "Simler90_Info", "Simler90 fixes info."),
                    LanguageManager.Get("Tweaks", "Simler90_Title", "Simler90's Fixes — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Mods", "Your Sims 3 Mods directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!ModFrameworkHelper.EnsureInstalled(GamePaths.Sims3Mods)) return;

                await DownloadFileOnly(
                    "%baseurl%/Mods/Sims3/Fixes/Packages/simler90GameplayCoreMod.package",
                    Path.Combine(GamePaths.Sims3Mods, "SimTools/Packages/simler90GameplayCoreMod-UPDATE206.package"));
            };
            sims3Item.Items.Add(ts3_simler90);

            // ── LazyDuchess' Mono Patcher ──────────────────────────────────────────────
            var ts3_mono_patcher = new MenuItem { Header = "Mono Patch" };
            ts3_mono_patcher.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "MonoPatch_Info1", "Mono Patcher info."),
                    LanguageManager.Get("Tweaks", "MonoPatch_Title", "LazyDuchess Mono Patcher — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "MonoPatch_Info2", "Mono Patcher is required for some SimTools mods."),
                    LanguageManager.Get("Tweaks", "MonoPatch_Title", "LazyDuchess Mono Patcher — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Mods", "Your Sims 3 Mods directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!ModFrameworkHelper.EnsureInstalled(GamePaths.Sims3Mods)) return;

                await DownloadFileOnly(
                    "%baseurl%/Mods/Sims3/Packages/Fixes/base/ld_MonoPatcher.package",
                    Path.Combine(GamePaths.Sims3Mods, "SimTools/Packages/ld_MonoPatcher.package"));
                await DownloadFileOnly(
                    "%baseurl%/Sideload-Apps/x86/MonoPatcher.asi",
                    Path.Combine(GamePaths.Sims3Game, "Game/Bin/MonoPatcher.asi"));
                await DownloadFileOnly(
                    "%baseurl%/Sideload-Apps/x86/wininet.dll",
                    Path.Combine(GamePaths.Sims3Game, "Game/Bin/wininet.dll"));
            };
            sims3Item.Items.Add(ts3_mono_patcher);

            // ── Gameplay Fixes ────────────────────────────────────────────────
            // Opens the multi-section AIO checkbox installer window.
            var ts3_gameplayFixes = new MenuItem { Header = "Gameplay Fixes" };
            ts3_gameplayFixes.Click += (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "GameplayFixes_Info", "Gameplay Fixes info."),
                    LanguageManager.Get("Tweaks", "GameplayFixes_Title", "Gameplay Fixes — The Sims 3"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "Sims3Mods", "Your Sims 3 Mods directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                new GameplayFixesWindow(GamePaths.Sims3Mods) { Owner = this }.ShowDialog();
            };
            sims3Item.Items.Add(ts3_gameplayFixes);

            contextMenu.Items.Add(sims3Item);

            // ─────────────────────────────────────────────────────────────────
            // The Sims 4 — placeholder
            // ─────────────────────────────────────────────────────────────────
            var sims4Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims4.ico"), Header = "The Sims 4" };
            sims4Item.Click += (_, _) =>
                MessageBox.Show(
                    LanguageManager.Get("BugFixes", "Sims4_Info", "To be implemented at a later time."),
                    LanguageManager.Get("BugFixes", "Sims4_Title", "The Sims 4 — Bug Fixes"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            contextMenu.Items.Add(sims4Item);

            // ─────────────────────────────────────────────────────────────────
            // SimCopter → SimCopterX
            // ─────────────────────────────────────────────────────────────────
            var simCopterItem = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Copter.ico"), Header = "SimCopter" };
            var simCopterX = new MenuItem { Header = "SimCopterX" };
            simCopterX.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "SimCopterX_Info", "SimCopterX info."),
                    LanguageManager.Get("Tweaks", "SimCopterX_Title", "SimCopterX"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.SimCopterGame))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "SimCopter", "Your SimCopter game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dest = Path.Combine(GamePaths.SimCopterGame, "SimCopterX.exe");
                var (ok, _) = await DownloadFileOnly(
                    "http://krimsky.net/patchers/download/SimCopterX.exe", dest);
                if (!ok) return;

                Process.Start(new ProcessStartInfo(dest) { UseShellExecute = true });
            };
            simCopterItem.Items.Add(simCopterX);
            contextMenu.Items.Add(simCopterItem);

            // ─────────────────────────────────────────────────────────────────
            // Streets of SimCity → SimStreetsX
            // ─────────────────────────────────────────────────────────────────
            var streetsItem = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Streets.ico"), Header = "Streets of SimCity" };
            var simStreetsX = new MenuItem { Header = "SimStreetsX" };
            simStreetsX.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "SimStreetsX_Info", "SimStreetsX info."),
                    LanguageManager.Get("Tweaks", "SimStreetsX_Title", "SimStreetsX"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.StreetsOfSimCityGame))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "SimStreets", "Your Streets of SimCity game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dest = Path.Combine(GamePaths.StreetsOfSimCityGame, "SSXLauncher.exe");
                var (ok, _) = await DownloadFileOnly(
                    "http://krimsky.net/patchers/download/SSXLauncher.exe", dest);
                if (!ok) return;

                Process.Start(new ProcessStartInfo(dest) { UseShellExecute = true });
            };
            streetsItem.Items.Add(simStreetsX);
            contextMenu.Items.Add(streetsItem);

            // ─────────────────────────────────────────────────────────────────
            // SimCity 2000 → SC2kFix + SC2000X
            // ─────────────────────────────────────────────────────────────────
            var sc2000Item = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/SC2K.ico"), Header = "SimCity 2000" };

            // ── SC2kFix (ZIP download + extract) ──────────────────────────────
            var sc2kFix = new MenuItem { Header = "SC2kFix" };
            sc2kFix.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "SC2kFix_Info", "SC2kFix info."),
                    LanguageManager.Get("Tweaks", "SC2kFix_Title", "SC2kFix — SimCity 2000"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.SimCity2000Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "SimCity2000", "Your SimCity 2000 game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadZipAndExtract(
                    "https://github.com/sc2kfix/sc2kfix/releases/download/r10/sc2kfix-r10.zip",
                    "sc2kfix-r10.zip",
                    GamePaths.SimCity2000Game);
            };
            sc2000Item.Items.Add(sc2kFix);

            // ── SC2000X (EXE download + run) ──────────────────────────────────
            var sc2000X = new MenuItem { Header = "SC2000X" };
            sc2000X.Click += async (_, _) =>
            {
                MessageBox.Show(
                    LanguageManager.Get("Tweaks", "SC2000X_Info", "SC2000X info."),
                    LanguageManager.Get("Tweaks", "SC2000X_Title", "SC2000X — SimCity 2000"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.SimCity2000Game))
                {
                    MessageBox.Show(
                        LanguageManager.Get("Paths", "SimCity2000", "Your SimCity 2000 game directory is not configured."),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dest = Path.Combine(GamePaths.SimCity2000Game, "SC2000X.exe");
                var (ok, _) = await DownloadFileOnly(
                    "https://github.com/alekasm/SC2000X/releases/download/1.01/SC2000X.exe", dest);
                if (!ok) return;

                Process.Start(new ProcessStartInfo(dest) { UseShellExecute = true, Verb = "runas" });
            };
            sc2000Item.Items.Add(sc2000X);

            contextMenu.Items.Add(sc2000Item);

            // ── Assign to button ───────────────────────────────────────────────
            BugFixButton.ContextMenu = contextMenu;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SHARED DOWNLOAD HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        // ── DownloadFileOnly ───────────────────────────────────────────────────
        // Downloads a file to an exact destination path.
        //   • Resolves %baseurl% in the url parameter.
        //   • Creates the destination directory if it doesn't exist.
        //   • Skips the download when the file already exists AND the server's
        //     Last-Modified header is not newer than the local write time (with
        //     a 5-second clock-skew tolerance).
        //   • Shows DownloadProgressWindow during the transfer.
        //
        // Returns:
        //   (true,  true)  — freshly downloaded
        //   (true,  false) — file was already current; download skipped
        //   (false, false) — download error (error MessageBox already shown)
        private async Task<(bool Success, bool IsNew)> DownloadFileOnly(string url, string destFilePath)
        {
            url = AppSettings.ResolveUrl(url);
            await RepoValidator.ValidateAndWarnAsync();

            // Ensure destination directory exists
            var dir = Path.GetDirectoryName(destFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.GetFileName(destFilePath);
            bool needsDownload = !File.Exists(destFilePath);

            // ── HEAD check — skip download if local file is already current ────
            if (!needsDownload)
            {
                try
                {
                    using var headClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    using var headResp = await headClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, url));

                    if (headResp.IsSuccessStatusCode)
                    {
                        var remoteDate = headResp.Content.Headers.LastModified;
                        if (remoteDate.HasValue)
                        {
                            var localWrite = File.GetLastWriteTimeUtc(destFilePath);
                            needsDownload = remoteDate.Value.UtcDateTime > localWrite.AddSeconds(5);
                        }
                        // No Last-Modified header → can't compare → keep local copy
                    }
                }
                catch
                {
                    // Network unavailable or HEAD not supported — keep local copy silently
                }
            }

            if (!needsDownload) return (true, false);

            // ── Download ──────────────────────────────────────────────────────
            var progressWindow = new DownloadProgressWindow(fileName)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            progressWindow.Show();

            try
            {
                using var http = new HttpClient();
                using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var remoteLastModified = response.Content.Headers.LastModified;
                long? totalBytes = response.Content.Headers.ContentLength;

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(
                    destFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long bytesRead = 0;
                int lastPercent = 0, chunk;

                while ((chunk = await contentStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, chunk));
                    bytesRead += chunk;

                    if (totalBytes.HasValue)
                    {
                        int pct = (int)(bytesRead * 100 / totalBytes.Value);
                        if (pct != lastPercent)
                        {
                            lastPercent = pct;
                            progressWindow.UpdateProgress(pct);
                        }
                    }
                    else
                    {
                        progressWindow.SetIndeterminate();
                    }
                }

                // Stamp the local file with the server's Last-Modified so the next
                // HEAD comparison works correctly even after an app restart.
                if (remoteLastModified.HasValue)
                    File.SetLastWriteTimeUtc(destFilePath, remoteLastModified.Value.UtcDateTime);

                return (true, true);
            }
            catch (Exception ex)
            {
                if (File.Exists(destFilePath)) File.Delete(destFilePath);

                MessageBox.Show(
                    LanguageManager.Format("Messages", "Error_DownloadFailed", fileName) + "\n" + ex.Message,
                    LanguageManager.Get("Messages", "Error_DownloadTitle", "Download Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return (false, false);
            }
            finally
            {
                progressWindow.Close();
            }
        }

        // ── DownloadZipAndExtract ──────────────────────────────────────────────
        // Downloads a ZIP archive to destDir/<zipName> using DownloadFileOnly
        // (HEAD-check, skip-if-same), then extracts its contents into destDir.
        //
        // Extraction is only performed when the ZIP was freshly downloaded —
        // if the HEAD check determined the local copy is already current the
        // extracted files are assumed to be current too, so no extraction runs.
        //
        // Returns true on success (including the already-current skip case).
        private async Task<bool> DownloadZipAndExtract(string url, string zipName, string destDir)
        {
            var zipPath = Path.Combine(destDir, zipName);
            var (ok, isNew) = await DownloadFileOnly(url, zipPath);

            if (!ok) return false;   // download failed — error already shown
            if (!isNew) return true;    // ZIP already current — extraction already done previously

            try
            {
                // overwriteFiles: true prevents exceptions on re-extraction
                ZipFile.ExtractToDirectory(zipPath, destDir, overwriteFiles: true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.Format("Framework", "ExtractFail", zipName, ex.Message),
                    LanguageManager.Get("Framework", "ExtractFail_Title", "Extract Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Settings Handler
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow { Owner = this };
            settings.ShowDialog();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }


        [SupportedOSPlatform("windows")]
        private async void SaveCleanerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
            {
                MessageBox.Show(
                    LanguageManager.Get("Paths", "Sims3Game", "Your Sims 3 Game directory is not configured."),
                    LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            const string zipName = "RegulSaveCleaner-v4.0.2-win.zip";
            const string url = "https://github.com/Onebeld/RegulSaveCleaner/releases/download/v4.0.2/RegulSaveCleaner-v4.0.2-win.zip";
            var destDir = GamePaths.Sims3Game;
            var exePath = Path.Combine(destDir, "RegulSaveCleaner-v4.0.2-win", "RegulSaveCleaner.exe");

            var download = !File.Exists(exePath)
    ? MessageBox.Show(
        LanguageManager.Get("ModResources", "SaveCleaner_Download", "Do you want to download the Regul Save Cleaner?"),
        LanguageManager.Get("ModResources", "SaveCleaner_Title", "Regul Save Cleaner"), MessageBoxButton.YesNo, MessageBoxImage.Question)
    : MessageBoxResult.No;

            if (download == MessageBoxResult.Yes)
            {
                bool ok = await DownloadZipAndExtract(url, zipName, destDir);
                if (!ok) return;

                var zipPath = Path.Combine(destDir, zipName);
                try { if (File.Exists(zipPath)) File.Delete(zipPath); } catch { }

                // Offer shortcut only if one doesn't already exist
                var lnkPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "Sims 3 Regul Save Cleaner.lnk");

                if (!File.Exists(lnkPath))
                {
                    var choice = MessageBox.Show(
                        LanguageManager.Get("ModResources", "SaveCleaner_Shortcut", "Would you like to create a desktop shortcut to Regul Save Cleaner?"),
                        LanguageManager.Get("ModResources", "SaveCleaner_ShortcutTitle", "Create Shortcut"), MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (choice == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var shell = (dynamic)Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")!)!;
                            var shortcut = shell.CreateShortcut(lnkPath);
                            shortcut.TargetPath = exePath;
                            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty;
                            shortcut.Description = "Regul Save Cleaner for The Sims 3";
                            shortcut.Save();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                LanguageManager.Format("ModResources", "SaveCleaner_ShortcutFail", ex.Message),
                                LanguageManager.Get("ModResources", "SaveCleaner_ShortcutFailTitle", "Shortcut Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }

            if (!File.Exists(exePath))
            {
                MessageBox.Show(
                    LanguageManager.Format("ModResources", "SaveCleaner_NotFound", exePath),
                    LanguageManager.Get("ModResources", "SaveCleaner_NotFoundTitle", "SimTools — File Not Found"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // MOD BUTTON
        // ═══════════════════════════════════════════════════════════════════════

        private void ModButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ModButton_Context(object sender, ContextMenuEventArgs e) { e.Handled = true; }

        private void SetupModContextMenu()
        {
            var contextMenu = new ContextMenu();

            // Local helpers
            void Browse(string url)
                => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

            void InfoThenBrowse(string message, string url)
            {
                MessageBox.Show(message, "SimTools — Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Browse(url);
            }

            void AskThenBrowse(string message, string title, string url)
            {
                var r = MessageBox.Show(message, title,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                    Browse(url);
            }

            // ── The Sims 1 ────────────────────────────────────────────────────
            var sims1 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims1.ico"), Header = "The Sims 1" };
            var s1_corylea = new MenuItem { Header = "Corylea Sims 1 Mods" };
            var s1_tsr = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSR.ico"), Header = "The Sims 1 on TSR" };
            var s1_mts = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "The Sims 1 on MTS" };
            s1_corylea.Click += (_, _) => Browse("http://corylea.com/Sims1ModsByCorylea.html");
            s1_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims1/skipsetitems/1/");
            s1_mts.Click += (_, _) => Browse("https://modthesims.info/f/562/");
            sims1.Items.Add(s1_corylea);
            sims1.Items.Add(s1_tsr);
            sims1.Items.Add(s1_mts);
            contextMenu.Items.Add(sims1);

            // ── The Sims 2 ────────────────────────────────────────────────────
            var sims2 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims2.ico"), Header = "The Sims 2" };
            var s2_tsr = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSR.ico"), Header = "The Sims 2 on TSR" };
            var s2_mts = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "The Sims 2 on MTS" };
            s2_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims2/skipsetitems/1/");
            s2_mts.Click += (_, _) => Browse("https://modthesims.info/downloads/ts2/");
            sims2.Items.Add(s2_tsr);
            sims2.Items.Add(s2_mts);
            contextMenu.Items.Add(sims2);

            // ── The Sims 3 ────────────────────────────────────────────────────
            var sims3 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims3.ico"), Header = "The Sims 3" };

            var s3_tsr = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSR.ico"), Header = "The Sims 3 on TSR" };
            var s3_mts = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "The Sims 3 on MTS" };
            s3_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims3/skipsetitems/1/");
            s3_mts.Click += (_, _) => Browse("https://modthesims.info/downloads/ts3/");
            sims3.Items.Add(s3_tsr);
            sims3.Items.Add(s3_mts);
            sims3.Items.Add(new Separator());

            var s3_autoTC = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Auto TestingCheats" };
            s3_autoTC.Click += (_, _) => AskThenBrowse(
                "This mod will automatically activate the cheat \"testingcheatsenabled true\" every time you run the game. No more, no less. It's a very simple mod.\n\nWould you like to visit this mod's download page?",
                "Auto TestingCheats", "https://modthesims.info/download.php?t=387543");

            var s3_bbi = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Bigger Builder's Island" };
            s3_bbi.Click += (_, _) => AskThenBrowse(
                "A completely empty map with plenty of space for you to put all of your lots and housing onto. Good for projects you plan to build and share.\n\nWould you like to visit this mod's download page?",
                "Bigger Builder's Island", "https://modthesims.info/d/546927");

            var s3_cso = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Can Station Overhaul" };
            s3_cso.Click += (_, _) => AskThenBrowse(
                "The Canning Station Overhaul introduces new features to Grandma's Canning Station from the Sims 3 Store. It also fixes a number of existing bugs in the item. REQUIRES this Store item to function.\n\nWould you like to visit this mod's download page?",
                "Can Station Overhaul", "https://modthesims.info/download.php?t=580462");

            var s3_carpool = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Carpool Disabler" };
            s3_carpool.Click += (_, _) => AskThenBrowse(
                "This mod allows you to stop the honking lunatics at least in TS3. Click on the mailbox of your current lot to find the interactions.\n\nWould you like to visit this mod's download page?",
                "Carpool Disabler", "https://modthesims.info/download.php?t=410296");

            var s3_cotm = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Children of the Moon" };
            s3_cotm.Click += (_, _) => AskThenBrowse(
                "When your sim gives birth during the full moon, at night, the birth now will have a chance of being a supernatural one. That means the newborn will be assigned a random supernatural type if so.\n\nWould you like to visit this mod's download page?",
                "Children of the Moon", "https://modthesims.info/download.php?t=520400");

            var s3_skills = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Faster / Slower Skills" };
            s3_skills.Click += (_, _) => AskThenBrowse(
                "This is a tuning mod that can do two things. It decreases or increases the amount of time it takes for Sims to learn all Skills.\n\nWould you like to visit this mod's download page?",
                "Faster / Slower Skills", "https://modthesims.info/download.php?t=486247");

            var s3_upgrades = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Faster Upgrade Times" };
            s3_upgrades.Click += (_, _) => AskThenBrowse(
                "This mod makes all upgrade times shorter.\n\nWould you like to visit this mod's download page?",
                "Faster Upgrade Times", "https://modthesims.info/download.php?t=526870");

            var s3_gardener = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Gardener Service" };
            s3_gardener.Click += (_, _) => AskThenBrowse(
                "Allows you to request a Gardener from any Telephone (not Mobile) to take care of your Garden.\n\nWould you like to visit this mod's download page?",
                "Gardener Service", "https://modthesims.info/download.php?t=459967");

            var s3_grow = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Grow" };
            s3_grow.Click += (_, _) => AskThenBrowse(
                "This mod attempts to give the impression of your Sim kids growing up.\n\nWould you like to visit this mod's download page?",
                "Grow", "https://modthesims.info/download.php?t=490536");

            var s3_banking = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Online Banking Mod" };
            s3_banking.Click += (_, _) => AskThenBrowse(
                "This mod adds an online banking system to all computers in the world. Sims can open an account and make deposits and withdrawals separate from their household funds.\n\nWould you like to visit this mod's download page?",
                "Online Banking Mod", "https://modthesims.info/download.php?t=438835");

            var s3_party = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Party Pooper Mod" };
            s3_party.Click += (_, _) => AskThenBrowse(
                "If your sim is a social recluse whose phone is ringing off the hook with incessant party invitations AND you don't plan on attending any parties, then this is the mod for you.\n\nWould you like to visit this mod's download page?",
                "Party Pooper Mod", "https://modthesims.info/download.php?t=604846");

            var s3_study = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Study Skills Online" };
            s3_study.Click += (_, _) => AskThenBrowse(
                "This mod simply adds a \"Study Skills Online\" menu to all computers, smartphones and tablets, where you can choose skills for your sims to learn. Once a skill has been chosen, your sim will start studying it.\n\nWould you like to visit this mod's download page?",
                "Study Skills Online", "https://modthesims.info/download.php?t=478271");

            var s3_washburn = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/sheets.ico"), Header = "TS3 Washburn Edition" };
            s3_washburn.Click += (_, _) => AskThenBrowse(
                "Curated by Golden Ennina. This collection of mods overhauls many aspects of the game, including fixes, tweaks and graphical overhauls. Please note some mods on the list are also included as part of SimTools.\n\nWould you like to visit this mod's download page?",
                "TS3 Washburn Edition", "https://docs.google.com/spreadsheets/d/1my3HxmlWeIEQZ69BwAWHZ6-89mMcOQck/edit?gid=1712644223#gid=1712644223");

            var s3_careers = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "Ultimate Careers" };
            s3_careers.Click += (_, _) => AskThenBrowse(
                "The end of career rabbitholes is here! With Ultimate Careers, instead of going into a rabbithole to work, Sims will spend the work day at a community lot, doing work-related interactions - or slacking off. The mod scans the Sim's interaction queue, and increases performance depending on which interactions are being used.\n\nWould you like to visit this mod's download page?",
                "Ultimate Careers", "https://modthesims.info/download.php?t=517911");

            var s3_skins = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/sfs.ico"), Header = "You Are Real Skins" };
            s3_skins.Click += (_, _) => AskThenBrowse(
                "Originally posted by Buhudain on his blog. The blog has since been removed. This is a mirror of the last release of his mesh and skins.\n\nWould you like to visit this mod's download page?",
                "You Are Real Skins", "https://simfileshare.net/folder/146836/");

            foreach (var item in new[] {
        s3_autoTC, s3_bbi, s3_cso, s3_carpool, s3_cotm, s3_skills, s3_upgrades,
        s3_gardener, s3_grow, s3_banking, s3_party, s3_study, s3_washburn,
        s3_careers, s3_skins })
                sims3.Items.Add(item);

            contextMenu.Items.Add(sims3);

            // ── The Sims 4 ────────────────────────────────────────────────────
            var sims4 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Sims4.ico"), Header = "The Sims 4" };
            var s4_tsr = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/TSR.ico"), Header = "The Sims 4 on TSR" };
            var s4_mts = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/MTS.ico"), Header = "The Sims 4 on MTS" };
            s4_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims4/skipsetitems/1/");
            s4_mts.Click += (_, _) => Browse("https://modthesims.info/downloads/ts4/");
            sims4.Items.Add(s4_tsr);
            sims4.Items.Add(s4_mts);
            contextMenu.Items.Add(sims4);

            // ── SimCity 2000 ──────────────────────────────────────────────────
            var sc2000 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/simtropolis.ico"), Header = "SimCity 2000" };
            sc2000.Click += (_, _) => Browse("https://community.simtropolis.com/clubs/30-simcity-2000-resource-page/");
            contextMenu.Items.Add(sc2000);

            // ── SimCity 3000 ──────────────────────────────────────────────────
            var sc3000 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/simtropolis.ico"), Header = "SimCity 3000" };
            sc3000.Click += (_, _) => Browse("https://community.simtropolis.com/files/category/41-simcity-3000-files/");
            contextMenu.Items.Add(sc3000);

            // ── SimCity 4 ─────────────────────────────────────────────────────
            var sc4 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/simtropolis.ico"), Header = "SimCity 4" };
            sc4.Click += (_, _) => Browse("https://community.simtropolis.com/forums/topic/762126-the-ultimate-guide-to-simcity-4-mods-for-new-players/");
            contextMenu.Items.Add(sc4);

            // ── SimCity 2013 ──────────────────────────────────────────────────
            var sc2013 = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/SC2013.ico"), Header = "SimCity 2013" };
            string sc2013Warning = LanguageManager.Get("AboutSimTools", "SC2013Warning", "A majority of SimCity 2013 mods will only work in Offline Mode.");

            var sc2013_twinzens = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/simtropolis.ico"), Header = "Mod Collection by Twinzens" };
            sc2013_twinzens.Click += (_, _) =>
            {
                MessageBox.Show(sc2013Warning,
                    LanguageManager.Get("AboutSimTools", "SC2013Warning_Title", "SimCity 2013 — Warning"), MessageBoxButton.OK, MessageBoxImage.Information);
                InfoThenBrowse(
                    "Let me introduce you to the most up-to-date and tested mod collection covering the whole ST exchange for SC13k!",
                    "https://community.simtropolis.com/files/file/33611-sc13-2020-mod-collection-by-twinzens/");
            };

            var sc2013_more = new MenuItem { Header = "More..." };
            sc2013_more.Click += (_, _) => InfoThenBrowse(sc2013Warning,
                "https://community.simtropolis.com/files/category/19-sc13-game-mods/");

            sc2013.Items.Add(sc2013_twinzens);
            sc2013.Items.Add(sc2013_more);
            contextMenu.Items.Add(sc2013);

            // ── SimCopter ─────────────────────────────────────────────────────
            var simCopter = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Copter.ico"), Header = "SimCopter" };
            var simCopter_maxis = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/github.ico"), Header = "Maxis Mods" };
            simCopter_maxis.Click += (_, _) => InfoThenBrowse(
                "Silly little mods for SimCopter.",
                "https://github.com/CahootsMalone/maxis-mods");
            simCopter.Items.Add(simCopter_maxis);
            contextMenu.Items.Add(simCopter);

            // ── Streets of SimCity ────────────────────────────────────────────
            var streets = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/Streets.ico"), Header = "Streets of SimCity" };
            var streets_maxis = new MenuItem { Icon = MenuIcon("pack://application:,,,/Images/Icons/github.ico"), Header = "Maxis Mods" };
            streets_maxis.Click += (_, _) => InfoThenBrowse(
                "Silly little mods for Streets of SimCity.",
                "https://github.com/CahootsMalone/maxis-mods");
            streets.Items.Add(streets_maxis);
            contextMenu.Items.Add(streets);

            // ── Assign to button ──────────────────────────────────────────────
            ModButton.ContextMenu = contextMenu;
        }
        // ═══════════════════════════════════════════════════════════════════════
        // STORE BUTTON
        // ═══════════════════════════════════════════════════════════════════════

        private void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void StoreButton_Context(object sender, ContextMenuEventArgs e) { e.Handled = true; }

        private void SetupStoreContextMenu()
        {
            var contextMenu = new ContextMenu();

            void Browse(string url)
                => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

            // ── The Sims 3 Store ──────────────────────────────────────────────
            var store = new MenuItem { Header = "The Sims 3 Store" };
            store.Click += (_, _) => Browse("https://store.thesims3.com/");
            contextMenu.Items.Add(store);

            // ── Daily Deal ────────────────────────────────────────────────────
            var dailyDeal = new MenuItem { Header = "Daily Deal" };
            dailyDeal.Click += (_, _) => Browse("https://store.thesims3.com/dailyDeal.html");
            contextMenu.Items.Add(dailyDeal);

            // ── Daily Deal Rotation ───────────────────────────────────────────
            var dealRotation = new MenuItem { Header = "Daily Deal Rotation" };
            dealRotation.Click += (_, _) => Browse("https://docs.google.com/spreadsheets/d/1NIeS9yIMAw-fA7VhseLilqCOV5XfKoSJXwbyyKQLJP4/edit?gid=882235701#gid=882235701");
            contextMenu.Items.Add(dealRotation);

            // ── Free Store Items ──────────────────────────────────────────────
            var freeItems = new MenuItem { Header = "Free Store Items" };
            freeItems.Click += (_, _) => Browse("https://docs.google.com/document/d/1Rf89z61M8Ah7a-xf15GNKVa8ZzGgz92d1oXm9XZUxso/edit?tab=t.0");
            contextMenu.Items.Add(freeItems);

            // ── Store Video Guide ─────────────────────────────────────────────
            var videoGuide = new MenuItem { Header = "Store Video Guide" };
            videoGuide.Click += (_, _) => Browse("https://youtu.be/OPgoRiQ9Fq8");
            contextMenu.Items.Add(videoGuide);

            // ── Buy TS3 Games (placeholder — window not yet implemented) ──────
            var buyGames = new MenuItem { Header = "Buy TS3 Games" };
            buyGames.Click += (_, _) => new BuyTS3 { Owner = this }.ShowDialog();
            contextMenu.Items.Add(buyGames);

            // ── Assign to button ──────────────────────────────────────────────
            StoreButton.ContextMenu = contextMenu;
        }

        private void EasterEgg1_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://archive.org/details/the-minds-eye-raw-dv-captures/The+Mind's+Eye+(1990%2C+original%2C+LaserDisc).avi");
            App.NotifyEasterEggFound(1);
        }

        private void BuySimsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new BuyTS3();
            window.Owner = this;
            window.Show();
        }

        // Button to join SimTools Discord
        private void DiscordButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "DiscordButton")
            {
                OpenUrl("https://discord.gg/GYJPSM9EdR");
            }
        }

        private void FacebookButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "FacebookButton")
            {
                OpenUrl("https://www.facebook.com/groups/1856454164865995");
            }
        }

        private void YoutubeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "YoutubeButton")
            {
                OpenUrl("https://www.youtube.com/@TS3Tools");
            }
        }

        private void SteamButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "SteamButton")
            {
                OpenUrl("https://steamcommunity.com/id/DJH3L10S");
            }
        }

        private void SimPortButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "SimPortButton")
            {
                OpenUrl("https://mypage.thesims3.com/mypage/RoflCopter69696969");
            }
        }

        private void WarningButton_Click(object sender, RoutedEventArgs e)
        {
            {
                MessageBox.Show(
                    LanguageManager.Get("AboutSimTools", "WarningMessage", "A great deal has changed between versions."),
                    LanguageManager.Get("AboutSimTools", "WarningMessage_Title", "Watch the Video Guide!"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // Opens the support window
        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            {
                var about = new SupportSimTools { Owner = this };
                about.ShowDialog();
            }
        }

        // ── Desktop shortcut helper ───────────────────────────────────────────────
        /// <summary>
        /// Creates a .lnk shortcut on the user's Desktop using WScript.Shell COM.
        /// </summary>
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private static void CreateDesktopShortcut(string targetExe, string shortcutName, string description = "")
        {
            string desktop     = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktop, shortcutName + ".lnk");

            Type   shellType = Type.GetTypeFromProgID("WScript.Shell")
                               ?? throw new InvalidOperationException("WScript.Shell is not available.");
            dynamic shell    = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath       = targetExe;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetExe) ?? string.Empty;
            shortcut.Description      = description;
            shortcut.Save();
        }


        // ═══════════════════════════════════════════════════════════════════════
        // MOD TOOLS BUTTON
        // ═══════════════════════════════════════════════════════════════════════

        private void SetupModToolsContextMenu()
        {
            var contextMenu = new ContextMenu();
            string binDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries");

            // Local helper: verify the required game path is set, then download and run.
            void RunTool(string requiredPath, string gameName, string url, string fileName)
            {
                if (!GamePaths.IsConfigured(requiredPath))
                {
                    MessageBox.Show(
                        LanguageManager.Format("Paths", "Generic", gameName),
                        LanguageManager.Get("Paths", "Title", "SimTools — Path Not Set"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DownloadAndOpenExe(url, fileName, binDir);
            }

            // ── The Sims (disabled) ──────────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "The Sims", IsEnabled = false });

            // ── The Sims 2 (disabled) ───────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "The Sims 2", IsEnabled = false });

            // ── The Sims 3 ───────────────────────────────────────────────────────────
            var sims3 = new MenuItem { Header = "The Sims 3" };

            // Create-A-World sub-menu
            var caw = new MenuItem { Header = "Create-A-World" };

            var caw167 = new MenuItem { Header = "CAW for 1.67" };
            caw167.Click += (_, _) => RunTool(
                GamePaths.Sims3Game, "Sims 3",
                "%baseurl%/Mod-Tools/Sims3/CAW_1.67.exe",  // ← replace
                "CAW_1.67.exe");

            var caw169 = new MenuItem { Header = "CAW for 1.69" };
            caw169.Click += (_, _) => RunTool(
                GamePaths.Sims3Game, "Sims 3",
                "%baseurl%/Mod-Tools/Sims3/CAW_1.69.exe",  // ← replace
                "CAW_1.69.exe");

            caw.Items.Add(caw167);
            caw.Items.Add(caw169);
            sims3.Items.Add(caw);

            var s3pe = new MenuItem { Header = "Sims 3 Package Editor" };
            s3pe.Click += (_, _) => RunTool(
                GamePaths.Sims3Game, "Sims 3",
                "%baseurl%/Mod-Tools/Sims3/S3PE.exe",        // ← replace
                "S3PE.exe");

            var s3pack = new MenuItem { Header = "Sims 3 Pack Extractor" };
            s3pack.Click += (_, _) => RunTool(
                GamePaths.Sims3Game, "Sims 3",
                "%baseurl%/Mod-Tools/Sims3/S3PackExtractor.exe",  // ← replace
                "S3PackExtractor.exe");

            var s3dash = new MenuItem { Header = "Sims 3 Dashboard" };
            s3dash.Click += (_, _) => RunTool(
                GamePaths.Sims3Game, "Sims 3",
                "%baseurl%/Mod-Tools/Sims3/S3Dashboard.exe",  // ← replace
                "S3Dashboard.exe");

            var showtime = new MenuItem { Header = "Convert Showtime to Collector's Edition" };
            showtime.Click += (_, _) => RunTool(
                GamePaths.Sims3Game, "Sims 3",
                "%baseurl%/Mod-Tools/Sims3/ShowtimeConverter.exe",  // ← replace
                "ShowtimeConverter.exe");

            // CCMagic is a standalone installer; no game-path check required.
            var ccmagic = new MenuItem { Header = "Install CCMagic" };
            ccmagic.Click += (_, _) => DownloadAndOpenExe(
                url: "%baseurl%/Mod-Tools/Sims3/CCMagicInstaller.exe",  // ← replace
                fileName: "CCMagicInstaller.exe",
                downloadDirectory: binDir);

            foreach (var item in new[] { s3pe, s3pack, s3dash, showtime, ccmagic })
                sims3.Items.Add(item);

            contextMenu.Items.Add(sims3);

            // ── The Sims 4 ───────────────────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "The Sims 4", IsEnabled = false });

            // ── The Sims Medieval (disabled) ──────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "The Sims Medieval", IsEnabled = false });

            // ── The Sims Stories (disabled) ───────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "The Sims Stories", IsEnabled = false });

            // ── SimCity 2000 (disabled) ───────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "SimCity 2000", IsEnabled = false });

            // ── SimCity 3000 (disabled) ───────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "SimCity 3000", IsEnabled = false });

            // ── SimCity 4 (disabled) ──────────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "SimCity 4", IsEnabled = false });

            // ── SimCity 2013 (disabled) ───────────────────────────────────────────────
            contextMenu.Items.Add(new MenuItem { Header = "SimCity 2013", IsEnabled = false });

            // ── Assign to button ─────────────────────────────────────────────────────
            ModToolsButton.ContextMenu = contextMenu;
        }

        private void ModToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}