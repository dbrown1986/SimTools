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
        private readonly string              _musicFolder;
        private readonly CancellationTokenSource _cts = new();

        public MusicDownloadWindow(string musicFolder)
        {
            _musicFolder = musicFolder;
            InitializeComponent();
            ContentRendered += async (_, _) => await RunDownloadAsync();
        }

        // ── Download logic ────────────────────────────────────────────────
        private async System.Threading.Tasks.Task RunDownloadAsync()
        {
            try
            {
                Directory.CreateDirectory(_musicFolder);

                string manifestUrl = AppSettings.ResolveUrl("%baseurl%/res/music/manifest.txt");
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                // Fetch manifest
                StatusLabel.Text = "Fetching track list…";
                using var manifestResp = await http.GetAsync(manifestUrl, _cts.Token);
                if (!manifestResp.IsSuccessStatusCode)
                {
                    Close();
                    return;
                }

                string manifest = await manifestResp.Content.ReadAsStringAsync(_cts.Token);

                List<string> tracks = manifest
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l) &&
                                !l.StartsWith('<') &&
                                !l.StartsWith('#'))
                    .ToList();

                if (tracks.Count == 0) { Close(); return; }

                TrackProgressBar.Maximum = tracks.Count;
                int completed = 0;

                foreach (string name in tracks)
                {
                    if (_cts.Token.IsCancellationRequested) break;

                    // Update UI
                    StatusLabel.Text = name;
                    CountLabel.Text  = $"{completed} of {tracks.Count} tracks";

                    string dest = Path.Combine(_musicFolder, name);

                    if (!File.Exists(dest))
                    {
                        try
                        {
                            string fileUrl = AppSettings.ResolveUrl(
                                $"%baseurl%/res/music/{Uri.EscapeDataString(name)}");

                            using var resp = await http.GetAsync(fileUrl, _cts.Token);
                            if (resp.IsSuccessStatusCode)
                            {
                                await using var fs = new FileStream(
                                    dest, FileMode.Create, FileAccess.Write);
                                await resp.Content.CopyToAsync(fs, _cts.Token);
                            }
                        }
                        catch (OperationCanceledException) { break; }
                        catch { /* skip unreadable/missing track */ }
                    }

                    completed++;
                    TrackProgressBar.Value = completed;
                    CountLabel.Text = $"{completed} of {tracks.Count} tracks";
                }

                StatusLabel.Text = _cts.Token.IsCancellationRequested
                    ? "Download cancelled."
                    : "Download complete!";
            }
            catch (OperationCanceledException)
            {
                StatusLabel.Text = "Download cancelled.";
            }
            catch
            {
                StatusLabel.Text = "Could not reach the music server.";
            }
            finally
            {
                // Brief pause so the user can see the final status before close
                await System.Threading.Tasks.Task.Delay(
                    _cts.Token.IsCancellationRequested ? 0 : 800);
                Close();
            }
        }

        // ── Cancel button ─────────────────────────────────────────────────
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CancelBtn.IsEnabled = false;
            CancelBtn.Content   = "Cancelling…";
            _cts.Cancel();
        }
    }
}
