using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;

using WpfApplication = System.Windows.Application;

namespace SimTools
{
    public partial class UpdateDownloadWindow : Window
    {
        private readonly string               _downloadUrl;
        private readonly string               _destPath;
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// Creates the window and queues the download to start once the
        /// window is fully rendered.
        /// </summary>
        /// <param name="downloadUrl">Full URL of the installer EXE.</param>
        /// <param name="destPath">Local path where the EXE will be saved.</param>
        public UpdateDownloadWindow(string downloadUrl, string destPath)
        {
            _downloadUrl = downloadUrl;
            _destPath    = destPath;
            InitializeComponent();
            FileNameLabel.Text = Path.GetFileName(destPath);
            ContentRendered += async (_, _) => await RunDownloadAsync();
        }

        // ── Download logic ────────────────────────────────────────────────
        private async System.Threading.Tasks.Task RunDownloadAsync()
        {
            try
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(_destPath) ?? AppDomain.CurrentDomain.BaseDirectory);

                using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

                // Open a streaming response so we can track bytes
                using var response = await http.GetAsync(
                    _downloadUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    _cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    StatusLabel.Text = $"Server returned {(int)response.StatusCode}.";
                    await System.Threading.Tasks.Task.Delay(2000);
                    Close();
                    return;
                }

                long total    = response.Content.Headers.ContentLength ?? -1L;
                long received = 0L;

                if (total > 0)
                    DownloadProgressBar.Maximum = total;
                else
                    DownloadProgressBar.IsIndeterminate = true;

                await using var src  = await response.Content.ReadAsStreamAsync(_cts.Token);
                await using var dest = new FileStream(
                    _destPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize: 81920, useAsync: true);

                var buffer = new byte[81920];
                int bytesRead;

                while ((bytesRead = await src.ReadAsync(buffer, _cts.Token)) > 0)
                {
                    await dest.WriteAsync(buffer.AsMemory(0, bytesRead), _cts.Token);
                    received += bytesRead;

                    if (total > 0)
                    {
                        DownloadProgressBar.Value = received;
                        StatusLabel.Text = FormatBytes(received) + " / " + FormatBytes(total);
                        BytesLabel.Text  = $"{(int)(received * 100.0 / total)}%";
                    }
                    else
                    {
                        StatusLabel.Text = FormatBytes(received) + " downloaded";
                    }
                }

                // Success — launch installer and exit
                StatusLabel.Text                    = "Download complete! Launching installer…";
                DownloadProgressBar.Value           = DownloadProgressBar.Maximum;
                DownloadProgressBar.IsIndeterminate = false;
                CancelBtn.IsEnabled                 = false;

                await System.Threading.Tasks.Task.Delay(600, _cts.Token);

                Process.Start(new ProcessStartInfo
                {
                    FileName        = _destPath,
                    UseShellExecute = true   // required for EXE elevation prompts
                });

                WpfApplication.Current.Shutdown();
            }
            catch (OperationCanceledException)
            {
                StatusLabel.Text = "Download cancelled.";
                await System.Threading.Tasks.Task.Delay(400);
                Close();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Download failed: {ex.Message}";
                await System.Threading.Tasks.Task.Delay(2500);
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

        // ── Helper ────────────────────────────────────────────────────────
        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F1} MB";
            if (bytes >= 1_024)     return $"{bytes / 1_024.0:F1} KB";
            return $"{bytes} B";
        }
    }
}
