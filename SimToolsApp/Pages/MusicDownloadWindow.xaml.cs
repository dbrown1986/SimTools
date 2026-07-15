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

namespace SimTools
{
    public partial class MusicDownloadWindow : Window
    {
        private readonly string _musicFolder;
        private readonly List<string>? _explicitUrls;
        private readonly CancellationTokenSource _cts = new();

        public MusicDownloadWindow(string musicFolder)
        {
            _musicFolder = musicFolder;
            InitializeComponent();
            ContentRendered += async (_, _) => await RunDownloadAsync();
        }

        public MusicDownloadWindow(string musicFolder, IEnumerable<string> explicitUrls)
        {
            _musicFolder = musicFolder;
            _explicitUrls = explicitUrls?.ToList();
            InitializeComponent();
            ContentRendered += async (_, _) => await RunDownloadAsync();
        }

        private async System.Threading.Tasks.Task RunDownloadAsync()
        {
            try
            {
                Directory.CreateDirectory(_musicFolder);
                List<string>? tracks = _explicitUrls;

                if (tracks == null)
                {
                    StatusLabel.Text = LanguageManager.Get("Music", "Fetching", "Fetching track list…");
                    string manifestUrl = AppSettings.ResolveUrl("%baseurl%/Resources/Music/manifest.txt");

                    try
                    {
                        string manifestText = await SecureWebClient.GetStringAsync(manifestUrl);

                        tracks = manifestText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(line => line.Trim())
                                             .Where(line => !string.IsNullOrEmpty(line))
                                             .ToList();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Failed to retrieve manifest: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StatusLabel.Text = LanguageManager.Get("Music", "NoServer", "Could not reach the music server.");
                        return;
                    }
                }

                if (tracks == null || tracks.Count == 0)
                {
                    StatusLabel.Text = LanguageManager.Get("Music", "NoTracks", "No tracks available to download.");
                    return;
                }

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
                            absoluteUrl = trackUrl;
                            var uri = new Uri(absoluteUrl);
                            fileName = System.IO.Path.GetFileName(uri.LocalPath);
                            fileName = Uri.UnescapeDataString(fileName);
                            destinationPath = System.IO.Path.Combine(_musicFolder, fileName);
                        }
                        else
                        {
                            string combinedPath = $"%baseurl%/Resources/Music/{trackUrl}";
                            absoluteUrl = AppSettings.ResolveUrl(combinedPath);
                            fileName = trackUrl;
                            destinationPath = System.IO.Path.Combine(_musicFolder, fileName);
                            absoluteUrl = absoluteUrl.Replace(" ", "%20");
                        }

                        string? dir = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        // CRITICAL FIX: If the file is less than 1MB, we assume it is missing, corrupt, or an HTML redirect wrapper.
                        // This forces a clean redownload instead of silently skipping it!
                        bool fileIsMissingOrCorrupt = !File.Exists(destinationPath) || new FileInfo(destinationPath).Length < 1000000;

                        if (fileIsMissingOrCorrupt)
                        {
                            StatusLabel.Text = string.Format(LanguageManager.Get("Music", "DownloadingTrack", "Downloading: {0}"), fileName);

                            try
                            {
                                await SecureWebClient.DownloadFileAsync(absoluteUrl, destinationPath);

                                // Perform immediate file integrity check post-download
                                if (File.Exists(destinationPath))
                                {
                                    long length = new FileInfo(destinationPath).Length;
                                    if (length < 5000) // Way too small for an MP3 file
                                    {
                                        string preview = File.ReadAllText(destinationPath).Substring(0, Math.Min(100, (int)length));
                                        if (preview.Contains("<html") || preview.Contains("<!DOCTYPE") || preview.Contains("<HTML"))
                                        {
                                            throw new InvalidDataException("The downloaded file is an HTML block instead of binary audio data.");
                                        }
                                    }
                                }
                            }
                            catch (Exception downloadEx)
                            {
                                if (File.Exists(destinationPath))
                                {
                                    File.Delete(destinationPath);
                                }
                                System.Windows.MessageBox.Show($"Failed downloading {fileName}.\n\nError: {downloadEx.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception prepEx)
                    {
                        System.Windows.MessageBox.Show($"Error prepping path for {trackUrl}:\n{prepEx.Message}", "Prep Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            catch (Exception mainEx)
            {
                System.Windows.MessageBox.Show($"Download manager experienced a critical failure:\n{mainEx.Message}", "Critical Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = LanguageManager.Get("Music", "NoServer", "Could not reach the music server.");
            }
            finally
            {
                await System.Threading.Tasks.Task.Delay(_cts.Token.IsCancellationRequested ? 0 : 800);
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelBtn.IsEnabled = false;
            CancelBtn.Content = LanguageManager.Get("Music", "Cancelling", "Cancelling…");
            _cts.Cancel();
        }
    }
}