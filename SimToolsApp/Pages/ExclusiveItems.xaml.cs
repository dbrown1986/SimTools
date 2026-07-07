// SimTools
// Main Application
// SimTools Exclusive Items Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System.IO;
using System.Net.Http;
using System.Windows;

using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    public partial class ExclusiveItems : Window
    {
        public ExclusiveItems()
        {
            InitializeComponent();
            ApplyLanguage();

            ApplyHolidayThemes(); // <-- Holiday theme application
        }

        private void ApplyHolidayThemes()
        {
            // Hide all holiday elements by default
            Clover.Visibility = Visibility.Collapsed;
            Clover2.Visibility = Visibility.Collapsed;
            Clover3.Visibility = Visibility.Collapsed;
            Clover4.Visibility = Visibility.Collapsed;
            Clover5.Visibility = Visibility.Collapsed;
            Fireworks.Visibility = Visibility.Collapsed;
            SantaHat.Visibility = Visibility.Collapsed;

            DateTime today = DateTime.Today;
            int currentYear = today.Year;

            // 2. Check St. Patrick's Day (March 17)
            // Range: March 14 to March 20
            DateTime stPatricksDay = new DateTime(currentYear, 3, 17);
            if (today >= stPatricksDay.AddDays(-3) && today <= stPatricksDay.AddDays(3))
            {
                Clover.Visibility = Visibility.Visible;
                Clover2.Visibility = Visibility.Visible;
                Clover3.Visibility = Visibility.Visible;
                Clover4.Visibility = Visibility.Visible;
                Clover5.Visibility = Visibility.Visible;
            }

            // 3. Check Independence Day (July 4)
            // Range: July 1 to July 7
            DateTime independenceDay = new DateTime(currentYear, 7, 4);
            if (today >= independenceDay.AddDays(-3) && today <= independenceDay.AddDays(3))
            {
                Fireworks.Visibility = Visibility.Visible;
            }

            // 4. Check New Year's Eve (December 31 / January 1 window)
            // Range: Dec 28 to Jan 4. (Handles wrapping over the year bound perfectly)
            if (IsDateInNewYearsWindow(today))
            {
                Fireworks.Visibility = Visibility.Visible;
            }

            // 5. Check Christmas (December 25)
            // Range: December 22 to December 28
            DateTime christmas = new DateTime(currentYear, 12, 25);
            if (today >= christmas.AddDays(-3) && today <= christmas.AddDays(3))
            {
                SantaHat.Visibility = Visibility.Visible;
            }
        }

        private bool IsDateInNewYearsWindow(DateTime targetDate)
        {
            // Check if we are at the end of the current year (Dec 28 - Dec 31)
            DateTime nyeThisYear = new DateTime(targetDate.Year, 12, 31);
            if (targetDate >= nyeThisYear.AddDays(-3) && targetDate <= nyeThisYear)
            {
                return true;
            }

            // Check if we are at the very start of a new year (Jan 1 - Jan 4)
            DateTime nydThisYear = new DateTime(targetDate.Year, 1, 1);
            if (targetDate >= nydThisYear && targetDate <= nydThisYear.AddDays(3))
            {
                return true;
            }

            return false;
        }

        // ── Translation strings ───────────────────────────────────────────
        private void ApplyLanguage()
        {
            Title = LanguageManager.Get("ExclusiveItems", "Title", "SimTools — Exclusive Items");
            ExclusiveItemsText.Text = LanguageManager.Get("ExclusiveItems", "ContentText1",
                "Exclusive donor content will appear here.");
            ExclusiveItemsText2.Text = LanguageManager.Get("ExclusiveItems", "ContentText2",
                "Users of SimTools who have donated can share these items with family and friends, but do not upload them to any modding sites.");
        }

        // ── Close button ──────────────────────────────────────────────────
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BackButton_Clic(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void SlimLivinDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Verify that the Sims 3 UserData directory is configured before attempting a download
            if (!GamePaths.IsConfigured(GamePaths.Sims3UserData))
            {
                MessageBox.Show(
                    LanguageManager.Get("Main", "TS3NoDir", "The Sims 3 User Data directory path has not been configured in your settings yet. Please set it up in the configuration file first."),
                    LanguageManager.Get("Main", "TS3NoDir_Title", "Path Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 2. Resolve the full destination file path under Sims3UserData/Library
            // GamePaths.Resolve automatically creates any missing subdirectories along the way
            string filename = "SimTools_Slim_Livin.package";
            string destPath = GamePaths.Resolve(GamePaths.Sims3UserData, "Library", filename);

            // 3. Hand off the transfer to your native download worker
            // This handles the %baseurl% translation, repository validation, HEAD checks, and progress bar UI
            var (success, isNew) = await DownloadFileOnly("%baseurl%/Exclusives/Sims 3/SimTools_Slim_Livin.package", destPath);

            // 4. Report back to the user based on the operation outcome
            if (success)
            {
                string message = isNew
                    ? LanguageManager.Get("ExclusiveItems", "Download_Success", "SimTools Slim Livin package has been successfully downloaded and placed in your Library!")
                    : LanguageManager.Get("ExclusiveItems", "Download_Current", "Your SimTools Slim Livin package is already up to date.");

                MessageBox.Show(
                    message,
                    LanguageManager.Get("ExclusiveItems", "Download_Title", "Success"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
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
                    LanguageManager.Format("Main", "Error_DownloadFailed", fileName) + "\n" + ex.Message,
                    LanguageManager.Get("Main", "Error_DownloadTitle", "Download Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return (false, false);
            }
            finally
            {
                progressWindow.Close();
            }
        }

    }
}
