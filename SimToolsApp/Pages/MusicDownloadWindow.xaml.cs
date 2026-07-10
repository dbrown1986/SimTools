// SimTools
// Main Application
// Music Download Window Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                List<string>? tracks = _explicitUrls;

                // If no hardcoded URLs were provided, fall back to downloading the manifest file
                if (tracks == null)
                {
                    StatusLabel.Text = LanguageManager.Get("Music", "Fetching", "Fetching track list…");
                    string manifestUrl = AppSettings.ResolveUrl("%baseurl%/Resources/Music/manifest.txt");

                    using var manifestResp = await http.GetAsync(manifestUrl, _cts.Token);
                    if (!manifestResp.IsSuccessStatusCode)
                    {
                        StatusLabel.Text = LanguageManager.Get("Music", "NoServer", "Could not reach the music server.");
                        return;
                    }

                    string manifestText = await manifestResp.Content.ReadAsStringAsync();
                    tracks = manifestText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(line => line.Trim())
                                         .Where(line => !string.IsNullOrEmpty(line))
                                         .ToList();
                }

                if (tracks == null || tracks.Count == 0)
                {
                    StatusLabel.Text = LanguageManager.Get("Music", "NoTracks", "No tracks available to download.");
                    return;
                }

                // Setup the Progress Bar metrics exactly as before
                TrackProgressBar.Minimum = 0;
                TrackProgressBar.Maximum = tracks.Count;
                TrackProgressBar.Value = 0;
                CountLabel.Text = $"0 of {tracks.Count} tracks";

                int completed = 0;

                foreach (string trackUrl in tracks)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    // Convert any relative paths or base percentages safely if using the manifest framework
                    string absoluteUrl = _explicitUrls != null ? trackUrl : AppSettings.ResolveUrl(trackUrl);
                    string fileName = System.IO.Path.GetFileName(new Uri(absoluteUrl).LocalPath);
                    string destinationPath = System.IO.Path.Combine(_musicFolder, fileName);

                    if (!File.Exists(destinationPath))
                    {
                        try
                        {
                            StatusLabel.Text = string.Format(LanguageManager.Get("Music", "DownloadingTrack", "Downloading: {0}"), fileName);

                            using var response = await http.GetAsync(absoluteUrl, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                            if (response.IsSuccessStatusCode)
                            {
                                using var streamToReadFrom = await response.Content.ReadAsStreamAsync(_cts.Token);
                                using var streamToWriteTo = File.Create(destinationPath);
                                await streamToReadFrom.CopyToAsync(streamToWriteTo, _cts.Token);
                            }
                        }
                        catch { /* skip unreadable/missing track */ }
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