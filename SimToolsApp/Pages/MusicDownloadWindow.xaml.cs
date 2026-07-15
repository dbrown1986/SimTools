// SimTools
// Main Application
// Music Download Window Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using WpfMessageBox = System.Windows.MessageBox;

namespace SimTools
{
    public partial class MusicDownloadWindow : Window
    {
        private readonly string _musicFolder;
        private readonly List<string>? _explicitUrls; // <-- Added to hold direct URLs
        private readonly CancellationTokenSource _cts = new();

        // Original constructor for manifest files
        public MusicDownloadWindow(string musicFolder)
        {
            _musicFolder = musicFolder;
            InitializeComponent();
            ContentRendered += async (_, _) => await RunDownloadAsync();
        }

        // New constructor for direct URL arrays (used by MusicPacks)
        public MusicDownloadWindow(string musicFolder, IEnumerable<string> explicitUrls)
        {
            _musicFolder = musicFolder;
            _explicitUrls = explicitUrls?.ToList();
            InitializeComponent();
            ContentRendered += async (_, _) => await RunDownloadAsync();
        }

        // ── Download logic ────────────────────────────────────────────────
        private async System.Threading.Tasks.Task RunDownloadAsync()
        {
            try
            {
                Directory.CreateDirectory(_musicFolder);
                List<string>? tracks = _explicitUrls;

                // If no hardcoded URLs were provided, fall back to downloading the manifest file
                if (tracks == null)
                {
                    StatusLabel.Text = LanguageManager.Get("Music", "Fetching", "Fetching track list…");
                    string manifestUrl = AppSettings.ResolveUrl("%baseurl%/Resources/Music/manifest.txt");

                    try
                    {
                        // Bypasses Schannel entirely using our custom BouncyCastle engine!
                        string manifestText = await SecureWebClient.GetStringAsync(manifestUrl);

                        tracks = manifestText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(line => line.Trim())
                                             .Where(line => !string.IsNullOrEmpty(line))
                                             .ToList();
                    }
                    catch (Exception ex)
                    {
                        // Log/handle error details if needed
                        StatusLabel.Text = LanguageManager.Get("Music", "NoServer", "Could not reach the music server.");
                        return;
                    }
                }

                if (tracks == null || tracks.Count == 0)
                {
                    StatusLabel.Text = LanguageManager.Get("Music", "NoTracks", "No tracks available to download.");
                    return;
                }

                // ── Download tracks loop ─────────────────────────────────────────────
                // (Assuming you have a loop right below this that downloads the files)
                // You can now download each track file securely using:
                // await SecureWebClient.DownloadFileAsync(trackUrl, destinationPath);

                // Setup the Progress Bar metrics exactly as before
                TrackProgressBar.Minimum = 0;
                TrackProgressBar.Maximum = tracks.Count;
                TrackProgressBar.Value = 0;
                CountLabel.Text = $"0 of {tracks.Count} tracks";

                int completed = 0;

                foreach (string trackUrl in tracks)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    try
                    {
                        string absoluteUrl;
                        string destinationPath;
                        string fileName;

                        if (_explicitUrls != null)
                        {
                            // MusicPacks uses full URLs directly
                            absoluteUrl = trackUrl;
                            fileName = System.IO.Path.GetFileName(new Uri(absoluteUrl).LocalPath);
                            destinationPath = System.IO.Path.Combine(_musicFolder, fileName);
                        }
                        else
                        {
                            // 1. Get the unescaped absolute web URL via AppSettings
                            string combinedPath = $"%baseurl%/Resources/Music/{trackUrl}";
                            absoluteUrl = AppSettings.ResolveUrl(combinedPath);

                            // 2. Derive the local filename directly from the clean track name (preserving literal spaces)
                            fileName = trackUrl;
                            destinationPath = System.IO.Path.Combine(_musicFolder, fileName);

                            // 3. ONLY escape the web address spaces for the HTTP request itself
                            absoluteUrl = absoluteUrl.Replace(" ", "%20");
                        }

                        if (!File.Exists(destinationPath))
                        {
                            StatusLabel.Text = string.Format(LanguageManager.Get("Music", "DownloadingTrack", "Downloading: {0}"), fileName);

                            // The web request receives the %20 URL, while File.Create uses the clean Windows path
                            try
                            {
                                await SecureWebClient.DownloadFileAsync(absoluteUrl, destinationPath);
                            }
                            catch (Exception ex)
                            {
                                // Handle or log the exception if a track fails to download
                                // (e.g., track-specific error UI updates, or allowing the loop to continue to the next song)
                            }
                        }
                    }
                    catch
                    {
                        /* Safely skip genuinely missing files */
                    }

                    completed++;
                    TrackProgressBar.Value = completed;
                    CountLabel.Text = $"{completed} of {tracks.Count} tracks";
                }

                StatusLabel.Text = _cts.Token.IsCancellationRequested
                    ? LanguageManager.Get("Music", "Cancelled", "Download cancelled.")
                    : LanguageManager.Get("Music", "Complete", "Download complete!");
            }
            catch (OperationCanceledException)
            {
                StatusLabel.Text = LanguageManager.Get("Music", "Cancelled", "Download cancelled.");
            }
            catch
            {
                StatusLabel.Text = LanguageManager.Get("Music", "NoServer", "Could not reach the music server.");
            }
            finally
            {
                // Brief pause so the user can see the final status before close
                await System.Threading.Tasks.Task.Delay(_cts.Token.IsCancellationRequested ? 0 : 800);
                Close();
            }
        }

        // ── Cancel button ─────────────────────────────────────────────────
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelBtn.IsEnabled = false;
            CancelBtn.Content = LanguageManager.Get("Music", "Cancelling", "Cancelling…");
            _cts.Cancel();
        }
    }
}