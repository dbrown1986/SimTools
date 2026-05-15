using SimTools_v4;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

// Add these two aliases to resolve the ambiguity:
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools_v4
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
            MessageBox.Show(
                LanguageManager.Get("Messages", "Warn_MenuConverted"),
                LanguageManager.Get("Messages", "Warn_Title", "Warning"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install")
            );

            var sims2_64 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_64", "64-Bit") };
            sims2_64.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-64bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-64bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install")
            );

            sims2Item.Items.Add(sims2_32);
            sims2Item.Items.Add(sims2_64);

            // ── The Sims Stories (sub-menu) ────────────────────────────────────
            var simsStoriesItem = new MenuItem { Header = LanguageManager.Get("ContextMenu", "GPU_SimsStories", "The Sims Stories") };

            var simsStories_32 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_32", "32-Bit") };
            simsStories_32.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-32bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-32bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install")
            );

            var simsStories_64 = new MenuItem { Header = LanguageManager.Get("ContextMenu", "Bit_64", "64-Bit") };
            simsStories_64.Click += (s, args) => DownloadAndOpenExe(
                url: "https://www.simsnetwork.com/files/graphicsrulesmaker/graphicsrulesmaker-2.3.0-64bit.exe",  // ← replace
                fileName: "graphicsrulesmaker-2.3.0-64bit.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install")
            );

            simsStoriesItem.Items.Add(simsStories_32);
            simsStoriesItem.Items.Add(simsStories_64);

            // ── The Sims 3 (no sub-menu) ───────────────────────────────────────
            var sims3Item = new MenuItem { Header = LanguageManager.Get("ContextMenu", "GPU_Sims3", "The Sims 3") };
            sims3Item.Click += (s, args) => DownloadAndOpenExe(
                url: "https://repo.ts3tools.com/bin/x86/TS3_GPU_Addon.exe",  // ← replace
                fileName: "TS3_GPU_Addon.exe",
                downloadDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install")
            );

            contextMenu.Items.Add(sims2Item);
            contextMenu.Items.Add(simsStoriesItem);
            contextMenu.Items.Add(sims3Item);

            NewGPUButton.ContextMenu = contextMenu;
        }

        // Handler is now empty — menu is pre-built, nothing to do here
        private void NewGPUButton_Context(object sender, ContextMenuEventArgs e) { }

        // ── Helper: download only if missing, then launch ─────────────────────────
        private async void DownloadAndOpenExe(string url, string fileName, string downloadDirectory)
        {
            // Create the directory if it doesn't exist yet
            Directory.CreateDirectory(downloadDirectory);

            string exePath = Path.Combine(downloadDirectory, fileName);

            if (!File.Exists(exePath))
            {
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
            MessageBox.Show(
                LanguageManager.Get("Messages", "Warn_MenuConverted"),
                LanguageManager.Get("Messages", "Warn_Title", "Warning"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
        private void TweakButton_Context(object sender, ContextMenuEventArgs e) { }

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
    }
}