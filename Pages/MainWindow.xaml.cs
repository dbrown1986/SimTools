
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

// Add these two aliases to resolve the ambiguity:
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            // Context menu handling, so event handlers don't fire off early when the XAML is loading
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

            // ── Rebuild context menus with localised headers ────────────────────
            SetupGPUContextMenu();
            SetupTweakContextMenu();
            SetupBugFixContextMenu();
            SetupModContextMenu();
            SetupStoreContextMenu();
        }

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

        // User clicked the "New GPU" button, show a warning message box
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
            var sims2Item = new MenuItem { Header = LanguageManager.Get("ContextMenu", "GPU_Sims2", "The Sims 2") };

            var sims2_32 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_32", "32-Bit") };
            sims2_32.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-32bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-32bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            var sims2_64 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_64", "64-Bit") };
            sims2_64.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-64bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-64bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            sims2Item.Items.Add(sims2_32);
            sims2Item.Items.Add(sims2_64);

            // ── The Sims Stories (sub-menu) ────────────────────────────────────
            var simsStoriesItem = new MenuItem { Header = LanguageManager.Get("ContextMenu", "GPU_SimsStories", "The Sims Stories") };

            var simsStories_32 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_32", "32-Bit") };
            simsStories_32.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-32bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-32bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            var simsStories_64 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_64", "64-Bit") };
            simsStories_64.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-64bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-64bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Binaries")
            );

            simsStoriesItem.Items.Add(simsStories_32);
            simsStoriesItem.Items.Add(simsStories_64);

            // ── The Sims 3 (no sub-menu) ───────────────────────────────────────
            var sims3Item = new MenuItem { Header = LanguageManager.Get("ContextMenu", "GPU_Sims3", "The Sims 3") };
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

        // User clicked the Tweaks button, show a warning message box
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

            // ── The Sims 1 (sub-menu) ──────────────────────────────────────────
            var sims1Item = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Tweaks_Sims1", "The Sims 1") };

            // ── Simitone ──────────────────────────────────────────
            var sims1_simitone = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Tweaks_Simitone", "Simitone") };
            sims1_simitone.Click += (s, args) => DownloadAndOpenExe(
                url: "https://github.com/riperiperi/Simitone/releases/download/v0.8.12/SimitoneWindows.zip",  // ← replace
                fileName: "SimitoneWindows.zip",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install")
            );

            sims1Item.Items.Add(sims1_simitone);

            contextMenu.Items.Add(sims1Item);

            TweakButton.ContextMenu = contextMenu;
        }

        // Handler is now empty — menu is pre-built, nothing to do here
        private void TweakButton_Context(object sender, ContextMenuEventArgs e) { e.Handled = true; }

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
            var sims1Item = new MenuItem { Header = "The Sims 1" };
            sims1Item.Click += (_, _) =>
                MessageBox.Show(
                    "Please use Simitone under Game Tweaks to address The Sims 1 game bugs.",
                    "The Sims 1 — Bug Fixes",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            contextMenu.Items.Add(sims1Item);

            // ─────────────────────────────────────────────────────────────────
            // The Sims 2
            // ─────────────────────────────────────────────────────────────────
            var sims2Item = new MenuItem { Header = "The Sims 2" };

            // ── Sim Shadow Fix ────────────────────────────────────────────────
            var sims2_shadowFix = new MenuItem { Header = "Sim Shadow Fix" };
            sims2_shadowFix.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "With this mod, Sims' and pets' indoor shadows no longer appear as black rectangles. " +
                    "This problem occurs on many modern graphics cards.",
                    "Sim Shadow Fix — The Sims 2",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims2Mods))
                {
                    MessageBox.Show(
                        "Your Sims 2 Mods directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    "This mod fixes CAS parts that were made without Bump Map support looking glowy in CAS, " +
                    "or nearly everything being glowy for people playing with shaders disabled. LazyDuchess FTW!",
                    "Bright CAS Fix — The Sims 2",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims2Mods))
                {
                    MessageBox.Show(
                        "Your Sims 2 Mods directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var sims3Item = new MenuItem { Header = "The Sims 3" };

            // ── Patch Downloader ──────────────────────────────────────────────
            // Shows two info messages, downloads TS3Lib.dll + TS3PD.exe to /Install,
            // then launches TS3PD.exe.  Both files use HEAD-check / skip-if-same logic.
            var ts3_patchDl = new MenuItem { Header = "Patch Downloader" };
            ts3_patchDl.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "INFO: You should only need to update to 1.67 if you are running the game from the Retail discs. " +
                    "EA App and Steam versions will already be updated to the latest versions when installed.",
                    "Patch Downloader — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    "Use the following Patch Downloader to download patches for the base game " +
                    "and expansions you have installed.",
                    "Patch Downloader — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    "Furthermore, it is highly recommended that you patch each title after " +
                    "you have installed it. As an example, install base game, patch it, install " +
                    "World Adventures, patch it, and so on and so forth.",
                    "Patch Downloader — The Sims 3",
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
                    Path.Combine(installDir, "TS3PD.exe")) { UseShellExecute = true });
            };
            sims3Item.Items.Add(ts3_patchDl);

            // ── Simler90's Fixes ──────────────────────────────────────────────
            var ts3_simler90 = new MenuItem { Header = "Simler90's Fixes" };
            ts3_simler90.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "Simler90's Gameplay Systems Core Mod fixes a long list of issues and bugs with the base game " +
                    "and many of the expansion packs. STRONGLY recommended.",
                    "Simler90's Fixes — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                {
                    MessageBox.Show(
                        "Your Sims 3 Mods directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    "%baseurl%/Mods/Sims3/packages/simler90GameplayCoreMod-UPDATE206.package",
                    Path.Combine(GamePaths.Sims3Mods, "SimTools/Packages/simler90GameplayCoreMod-UPDATE206.package"));
            };
            sims3Item.Items.Add(ts3_simler90);

            // ── LazyDuchess' Mono Patcher ──────────────────────────────────────────────
            var ts3_mono_patcher = new MenuItem { Header = "Mono Patch" };
            ts3_mono_patcher.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "Mono Patcher is a library that allows Script Modders " +
                    "to replace Sims 3 methods with as much compatibility " +
                    "as possible - No need to create core mods anymore to " +
                    "replace game functions.",
                    "LazyDuchess Mono Patcher — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show(
                    "Mono Patcher is required for some of the mods featured " +
                    "by SimTools. It's installation is highly suggested.",
                    "LazyDuchess Mono Patcher — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                {
                    MessageBox.Show(
                        "Your Sims 3 Mods directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await DownloadFileOnly(
                    "%baseurl%/Mods/Sims3/packages/base/ld_MonoPatcher.package",
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
                    "Launching Game Fixes AIO installer. " +
                    "Select fixes ONLY for the expansions that you have installed.",
                    "Gameplay Fixes — The Sims 3",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.Sims3Mods))
                {
                    MessageBox.Show(
                        "Your Sims 3 Mods directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                new GameplayFixesWindow(GamePaths.Sims3Mods) { Owner = this }.ShowDialog();
            };
            sims3Item.Items.Add(ts3_gameplayFixes);

            contextMenu.Items.Add(sims3Item);

            // ─────────────────────────────────────────────────────────────────
            // The Sims 4 — placeholder
            // ─────────────────────────────────────────────────────────────────
            var sims4Item = new MenuItem { Header = "The Sims 4" };
            sims4Item.Click += (_, _) =>
                MessageBox.Show(
                    "To be implemented at a later time.",
                    "The Sims 4 — Bug Fixes",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            contextMenu.Items.Add(sims4Item);

            // ─────────────────────────────────────────────────────────────────
            // SimCopter → SimCopterX
            // ─────────────────────────────────────────────────────────────────
            var simCopterItem = new MenuItem { Header = "SimCopter" };
            var simCopterX = new MenuItem { Header = "SimCopterX" };
            simCopterX.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "This is an alert from central dispatch - you're cleared for takeoff on Windows 10. " +
                    "You no longer need to use a virtual machine or CPU Killer, just nerves of steel. " +
                    "Additionally the patcher has optional higher-resolution modes to choose from including " +
                    "16:9 and 16:10 aspect ratios so you can fill up your screen; " +
                    "you'll feel like you're actually up in the sky saving shipwrecked Sims.",
                    "SimCopterX",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.SimCopterGame))
                {
                    MessageBox.Show(
                        "Your SimCopter game directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var streetsItem = new MenuItem { Header = "Streets of SimCity" };
            var simStreetsX = new MenuItem { Header = "SimStreetsX" };
            simStreetsX.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "Take your rage to the streets on Windows 10 by becoming a lunatic Sim driver. " +
                    "You no longer need to use a virtual machine or CPU Killer, just a lead foot. " +
                    "The patcher comes with a variety of options so you can either take a relaxing drive " +
                    "or battle up to seven other players over a network.",
                    "SimStreetsX",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.StreetsOfSimCityGame))
                {
                    MessageBox.Show(
                        "Your Streets of SimCity game directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var sc2000Item = new MenuItem { Header = "SimCity 2000" };

            // ── SC2kFix (ZIP download + extract) ──────────────────────────────
            var sc2kFix = new MenuItem { Header = "SC2kFix" };
            sc2kFix.Click += async (_, _) =>
            {
                MessageBox.Show(
                    "SC2KFix is a bugfix and modding plugin for SimCity 2000 Special Edition. " +
                    "Compatible with the Windows 95 version only. Use alongside SC2000X is untested.",
                    "SC2kFix — SimCity 2000",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.SimCity2000Game))
                {
                    MessageBox.Show(
                        "Your SimCity 2000 game directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    "SC2000X is an open-source installer and patcher for SimCity 2000 (Win95). " +
                    "In addition to being an all-in-one solution, it's completely open source and supports " +
                    "multiple versions of the game. The patcher fixes the “Save-As” crash bug, " +
                    "which is actually a general dialog bug extending to “Load Tile Set” as well.",
                    "SC2000X — SimCity 2000",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (!GamePaths.IsConfigured(GamePaths.SimCity2000Game))
                {
                    MessageBox.Show(
                        "Your SimCity 2000 game directory is not configured.\nPlease open Settings and set it first.",
                        "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    $"Download failed: {fileName}\n{ex.Message}",
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
                    $"Failed to extract {zipName}:\n{ex.Message}",
                    "Extract Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Settings Handler
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow { Owner = this };
            settings.ShowDialog();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void SaveCleanerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GamePaths.IsConfigured(GamePaths.Sims3Game))
            {
                MessageBox.Show(
                    "Your Sims 3 game directory is not configured.\nPlease open Settings and set it first.",
                    "SimTools — Path Not Set", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            const string zipName = "RegulSaveCleaner-v4.0.2-win.zip";
            const string url = "%baseurl%/Sideload-Apps/x64/RegulSaveCleaner-v4.0.2-win.zip";
            var destDir = GamePaths.Sims3Game;
            var exePath = Path.Combine(destDir, "RegulSaveCleaner-v4.0.2-win", "RegulSaveCleaner.exe");

            var download = !File.Exists(exePath)
    ? MessageBox.Show(
        "Do you want to download the Regul Save Cleaner?",
        "Regul Save Cleaner", MessageBoxButton.YesNo, MessageBoxImage.Question)
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
                        "Would you like to create a desktop shortcut to Regul Save Cleaner?",
                        "Create Shortcut", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (choice == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var shell = (dynamic)Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")!)!;
                            var shortcut = shell.CreateShortcut(lnkPath);
                            shortcut.TargetPath = exePath;
                            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                            shortcut.Save();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Could not create shortcut:\n{ex.Message}",
                                "Shortcut Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }

            if (!File.Exists(exePath))
            {
                MessageBox.Show(
                    $"RegulSaveCleaner.exe was not found at:\n{exePath}",
                    "SimTools — File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var sims1 = new MenuItem { Header = "The Sims 1" };
            var s1_corylea = new MenuItem { Header = "Corylea Sims 1 Mods" };
            var s1_tsr = new MenuItem { Header = "The Sims 1 on TSR" };
            var s1_mts = new MenuItem { Header = "The Sims 1 on MTS" };
            s1_corylea.Click += (_, _) => Browse("http://corylea.com/Sims1ModsByCorylea.html");
            s1_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims1/skipsetitems/1/");
            s1_mts.Click += (_, _) => Browse("https://modthesims.info/f/562/");
            sims1.Items.Add(s1_corylea);
            sims1.Items.Add(s1_tsr);
            sims1.Items.Add(s1_mts);
            contextMenu.Items.Add(sims1);

            // ── The Sims 2 ────────────────────────────────────────────────────
            var sims2 = new MenuItem { Header = "The Sims 2" };
            var s2_tsr = new MenuItem { Header = "The Sims 2 on TSR" };
            var s2_mts = new MenuItem { Header = "The Sims 2 on MTS" };
            s2_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims2/skipsetitems/1/");
            s2_mts.Click += (_, _) => Browse("https://modthesims.info/downloads/ts2/");
            sims2.Items.Add(s2_tsr);
            sims2.Items.Add(s2_mts);
            contextMenu.Items.Add(sims2);

            // ── The Sims 3 ────────────────────────────────────────────────────
            var sims3 = new MenuItem { Header = "The Sims 3" };

            var s3_tsr = new MenuItem { Header = "The Sims 3 on TSR" };
            var s3_mts = new MenuItem { Header = "The Sims 3 on MTS" };
            s3_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims3/skipsetitems/1/");
            s3_mts.Click += (_, _) => Browse("https://modthesims.info/downloads/ts3/");
            sims3.Items.Add(s3_tsr);
            sims3.Items.Add(s3_mts);
            sims3.Items.Add(new Separator());

            var s3_autoTC = new MenuItem { Header = "Auto TestingCheats" };
            s3_autoTC.Click += (_, _) => AskThenBrowse(
                "This mod will automatically activate the cheat \"testingcheatsenabled true\" every time you run the game. No more, no less. It's a very simple mod.\n\nWould you like to visit this mod's download page?",
                "Auto TestingCheats", "https://modthesims.info/download.php?t=387543");

            var s3_bbi = new MenuItem { Header = "Bigger Builder's Island" };
            s3_bbi.Click += (_, _) => AskThenBrowse(
                "A completely empty map with plenty of space for you to put all of your lots and housing onto. Good for projects you plan to build and share.\n\nWould you like to visit this mod's download page?",
                "Bigger Builder's Island", "https://modthesims.info/d/546927");

            var s3_cso = new MenuItem { Header = "Can Station Overhaul" };
            s3_cso.Click += (_, _) => AskThenBrowse(
                "The Canning Station Overhaul introduces new features to Grandma's Canning Station from the Sims 3 Store. It also fixes a number of existing bugs in the item. REQUIRES this Store item to function.\n\nWould you like to visit this mod's download page?",
                "Can Station Overhaul", "https://modthesims.info/download.php?t=580462");

            var s3_carpool = new MenuItem { Header = "Carpool Disabler" };
            s3_carpool.Click += (_, _) => AskThenBrowse(
                "This mod allows you to stop the honking lunatics at least in TS3. Click on the mailbox of your current lot to find the interactions.\n\nWould you like to visit this mod's download page?",
                "Carpool Disabler", "https://modthesims.info/download.php?t=410296");

            var s3_cotm = new MenuItem { Header = "Children of the Moon" };
            s3_cotm.Click += (_, _) => AskThenBrowse(
                "When your sim gives birth during the full moon, at night, the birth now will have a chance of being a supernatural one. That means the newborn will be assigned a random supernatural type if so.\n\nWould you like to visit this mod's download page?",
                "Children of the Moon", "https://modthesims.info/download.php?t=520400");

            var s3_skills = new MenuItem { Header = "Faster / Slower Skills" };
            s3_skills.Click += (_, _) => AskThenBrowse(
                "This is a tuning mod that can do two things. It decreases or increases the amount of time it takes for Sims to learn all Skills.\n\nWould you like to visit this mod's download page?",
                "Faster / Slower Skills", "https://modthesims.info/download.php?t=486247");

            var s3_upgrades = new MenuItem { Header = "Faster Upgrade Times" };
            s3_upgrades.Click += (_, _) => AskThenBrowse(
                "This mod makes all upgrade times shorter.\n\nWould you like to visit this mod's download page?",
                "Faster Upgrade Times", "https://modthesims.info/download.php?t=526870");

            var s3_gardener = new MenuItem { Header = "Gardener Service" };
            s3_gardener.Click += (_, _) => AskThenBrowse(
                "Allows you to request a Gardener from any Telephone (not Mobile) to take care of your Garden.\n\nWould you like to visit this mod's download page?",
                "Gardener Service", "https://modthesims.info/download.php?t=459967");

            var s3_grow = new MenuItem { Header = "Grow" };
            s3_grow.Click += (_, _) => AskThenBrowse(
                "This mod attempts to give the impression of your Sim kids growing up.\n\nWould you like to visit this mod's download page?",
                "Grow", "https://modthesims.info/download.php?t=490536");

            var s3_banking = new MenuItem { Header = "Online Banking Mod" };
            s3_banking.Click += (_, _) => AskThenBrowse(
                "This mod adds an online banking system to all computers in the world. Sims can open an account and make deposits and withdrawals separate from their household funds.\n\nWould you like to visit this mod's download page?",
                "Online Banking Mod", "https://modthesims.info/download.php?t=438835");

            var s3_party = new MenuItem { Header = "Party Pooper Mod" };
            s3_party.Click += (_, _) => AskThenBrowse(
                "If your sim is a social recluse whose phone is ringing off the hook with incessant party invitations AND you don't plan on attending any parties, then this is the mod for you.\n\nWould you like to visit this mod's download page?",
                "Party Pooper Mod", "https://modthesims.info/download.php?t=604846");

            var s3_study = new MenuItem { Header = "Study Skills Online" };
            s3_study.Click += (_, _) => AskThenBrowse(
                "This mod simply adds a \"Study Skills Online\" menu to all computers, smartphones and tablets, where you can choose skills for your sims to learn. Once a skill has been chosen, your sim will start studying it.\n\nWould you like to visit this mod's download page?",
                "Study Skills Online", "https://modthesims.info/download.php?t=478271");

            var s3_washburn = new MenuItem { Header = "TS3 Washburn Edition" };
            s3_washburn.Click += (_, _) => AskThenBrowse(
                "Curated by Golden Ennina. This collection of mods overhauls many aspects of the game, including fixes, tweaks and graphical overhauls. Please note some mods on the list are also included as part of SimTools.\n\nWould you like to visit this mod's download page?",
                "TS3 Washburn Edition", "https://docs.google.com/spreadsheets/d/1my3HxmlWeIEQZ69BwAWHZ6-89mMcOQck/edit?gid=1712644223#gid=1712644223");

            var s3_careers = new MenuItem { Header = "Ultimate Careers" };
            s3_careers.Click += (_, _) => AskThenBrowse(
                "The end of career rabbitholes is here! With Ultimate Careers, instead of going into a rabbithole to work, Sims will spend the work day at a community lot, doing work-related interactions - or slacking off. The mod scans the Sim's interaction queue, and increases performance depending on which interactions are being used.\n\nWould you like to visit this mod's download page?",
                "Ultimate Careers", "https://modthesims.info/download.php?t=517911");

            var s3_skins = new MenuItem { Header = "You Are Real Skins" };
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
            var sims4 = new MenuItem { Header = "The Sims 4" };
            var s4_tsr = new MenuItem { Header = "The Sims 4 on TSR" };
            var s4_mts = new MenuItem { Header = "The Sims 4 on MTS" };
            s4_tsr.Click += (_, _) => Browse("https://www.thesimsresource.com/downloads/browse/category/sims4/skipsetitems/1/");
            s4_mts.Click += (_, _) => Browse("https://modthesims.info/downloads/ts4/");
            sims4.Items.Add(s4_tsr);
            sims4.Items.Add(s4_mts);
            contextMenu.Items.Add(sims4);

            // ── SimCity 2000 ──────────────────────────────────────────────────
            var sc2000 = new MenuItem { Header = "SimCity 2000" };
            sc2000.Click += (_, _) => Browse("https://community.simtropolis.com/clubs/30-simcity-2000-resource-page/");
            contextMenu.Items.Add(sc2000);

            // ── SimCity 3000 ──────────────────────────────────────────────────
            var sc3000 = new MenuItem { Header = "SimCity 3000" };
            sc3000.Click += (_, _) => Browse("https://community.simtropolis.com/files/category/41-simcity-3000-files/");
            contextMenu.Items.Add(sc3000);

            // ── SimCity 4 ─────────────────────────────────────────────────────
            var sc4 = new MenuItem { Header = "SimCity 4" };
            sc4.Click += (_, _) => Browse("https://community.simtropolis.com/forums/topic/762126-the-ultimate-guide-to-simcity-4-mods-for-new-players/");
            contextMenu.Items.Add(sc4);

            // ── SimCity 2013 ──────────────────────────────────────────────────
            var sc2013 = new MenuItem { Header = "SimCity 2013" };
            const string sc2013Warning = "A majority of SimCity 2013 mods will only work in Offline Mode. You will experience disconnections if attempting to play with these mods in online mode.";

            var sc2013_twinzens = new MenuItem { Header = "Mod Collection by Twinzens" };
            sc2013_twinzens.Click += (_, _) =>
            {
                MessageBox.Show(sc2013Warning,
                    "SimCity 2013 — Warning", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var simCopter = new MenuItem { Header = "SimCopter" };
            var simCopter_maxis = new MenuItem { Header = "Maxis Mods" };
            simCopter_maxis.Click += (_, _) => InfoThenBrowse(
                "Silly little mods for SimCopter.",
                "https://github.com/CahootsMalone/maxis-mods");
            simCopter.Items.Add(simCopter_maxis);
            contextMenu.Items.Add(simCopter);

            // ── Streets of SimCity ────────────────────────────────────────────
            var streets = new MenuItem { Header = "Streets of SimCity" };
            var streets_maxis = new MenuItem { Header = "Maxis Mods" };
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
            // var buyGames = new MenuItem { Header = "Buy TS3 Games" };
            // buyGames.Click += (_, _) => new BuyGamesWindow { Owner = this }.ShowDialog();
            // contextMenu.Items.Add(buyGames);

            // ── Assign to button ──────────────────────────────────────────────
            StoreButton.ContextMenu = contextMenu;
        }
    }
}
