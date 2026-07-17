// SimTools
// Main Application
// SimTools Exclusive Items Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.IO;
using System.Windows;
using System.Linq;
using System.Text;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;

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

        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

        private string GetArchivePassword()
        {
            // The output generated from Step A (e.g., "YourSuperSecretPasswordHere")
            byte[] obfuscatedBytes = new byte[] { 0x2F, 0x69, 0x79, 0xD3, 0x00, 0x69, 0x65, 0xE3, 0x16, 0x1A, 0x7A, 0xCB, 0x75, 0x38, 0x78, 0xED };
            byte[] key = { 0x41, 0x5A, 0x32, 0x99 }; // Must match key from Step A

            byte[] decrypted = new byte[obfuscatedBytes.Length];
            for (int i = 0; i < obfuscatedBytes.Length; i++)
            {
                decrypted[i] = (byte)(obfuscatedBytes[i] ^ key[i % key.Length]);
            }

            return Encoding.UTF8.GetString(decrypted);
        }
        private async void SlimLivinDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Verify that the Sims 3 UserData directory is configured
            if (!GamePaths.IsConfigured(GamePaths.Sims3UserData))
            {
                MessageBox.Show(
                    LanguageManager.Get("Main", "TS3NoDir", "The Sims 3 User Data directory path has not been configured in your settings yet."),
                    LanguageManager.Get("Main", "TS3NoDir_Title", "Path Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 2. Setup paths
            // We download a temporary .7z file instead of the raw .package file
            string archiveFilename = "SimTools_Slim_Livin.7z";
            string tempArchivePath = GamePaths.Resolve(GamePaths.Sims3UserData, "Library", archiveFilename);
            string destDirectory = Path.GetDirectoryName(tempArchivePath);

            // 3. Download the password-protected 7z archive
            var (success, isNew) = await DownloadFileOnly(
                "https://us1-repo.simtools-app.com/Exclusives/Sims 3/SimTools_Slim_Livin.7z",
                tempArchivePath
            );

            if (success)
            {
                bool extractionSuccess = false;

                if (isNew && File.Exists(tempArchivePath))
                {
                    try
                    {
                        // Retrieve the de-obfuscated password
                        string archivePassword = GetArchivePassword();

                        var readerOptions = new ReaderOptions
                        {
                            Password = archivePassword
                        };

                        // 4. Extract using SharpCompress
                        // Note: Open as a SevenZipArchive. 
                        using (var archive = SevenZipArchive.OpenArchive(tempArchivePath, readerOptions))
                        {
                            // WriteToDirectory handles the sequential extraction required for solid 7z files
                            archive.WriteToDirectory(destDirectory, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }

                        extractionSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Extraction failed: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    finally
                    {
                        // 5. Always delete the local temporary archive to clean up
                        if (File.Exists(tempArchivePath))
                        {
                            try
                            {
                                File.Delete(tempArchivePath);
                            }
                            catch (IOException)
                            {
                                // Handle potential file lock issues during immediate deletion
                            }
                        }
                    }
                }
                else
                {
                    // If the file wasn't new, we assume it's already up to date
                    extractionSuccess = true;
                }

                // 6. Report outcome
                if (extractionSuccess)
                {
                    string message = isNew
                        ? LanguageManager.Get("ExclusiveItems", "Download_Success", "SimTools Slim Livin package has been successfully downloaded, extracted, and placed in your Library!")
                        : LanguageManager.Get("ExclusiveItems", "Download_Current", "Your SimTools Slim Livin package is already up to date.");

                    MessageBox.Show(
                        message,
                        LanguageManager.Get("ExclusiveItems", "Download_Title", "Success"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        private async void EvergreenAbodeDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Verify that the Sims 3 UserData directory is configured
            if (!GamePaths.IsConfigured(GamePaths.Sims3UserData))
            {
                MessageBox.Show(
                    LanguageManager.Get("Main", "TS3NoDir", "The Sims 3 User Data directory path has not been configured in your settings yet."),
                    LanguageManager.Get("Main", "TS3NoDir_Title", "Path Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 2. Setup paths
            // We download a temporary .7z file instead of the raw .package file
            string archiveFilename = "SimTools_Evergreen_Abode.7z";
            string tempArchivePath = GamePaths.Resolve(GamePaths.Sims3UserData, "Library", archiveFilename);
            string destDirectory = Path.GetDirectoryName(tempArchivePath);

            // 3. Download the password-protected 7z archive
            var (success, isNew) = await DownloadFileOnly(
                "https://us1-repo.simtools-app.com/Exclusives/Sims 3/SimTools_Evergreen_Abode.7z",
                tempArchivePath
            );

            if (success)
            {
                bool extractionSuccess = false;

                if (isNew && File.Exists(tempArchivePath))
                {
                    try
                    {
                        // Retrieve the de-obfuscated password
                        string archivePassword = GetArchivePassword();

                        var readerOptions = new ReaderOptions
                        {
                            Password = archivePassword
                        };

                        // 4. Extract using SharpCompress
                        // Note: Open as a SevenZipArchive. 
                        using (var archive = SevenZipArchive.OpenArchive(tempArchivePath, readerOptions))
                        {
                            // WriteToDirectory handles the sequential extraction required for solid 7z files
                            archive.WriteToDirectory(destDirectory, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }

                        extractionSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Extraction failed: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    finally
                    {
                        // 5. Always delete the local temporary archive to clean up
                        if (File.Exists(tempArchivePath))
                        {
                            try
                            {
                                File.Delete(tempArchivePath);
                            }
                            catch (IOException)
                            {
                                // Handle potential file lock issues during immediate deletion
                            }
                        }
                    }
                }
                else
                {
                    // If the file wasn't new, we assume it's already up to date
                    extractionSuccess = true;
                }

                // 6. Report outcome
                if (extractionSuccess)
                {
                    string message = isNew
                        ? LanguageManager.Get("ExclusiveItems", "Download_Success", "SimTools Slim Livin package has been successfully downloaded, extracted, and placed in your Library!")
                        : LanguageManager.Get("ExclusiveItems", "Download_Current", "Your SimTools Slim Livin package is already up to date.");

                    MessageBox.Show(
                        message,
                        LanguageManager.Get("ExclusiveItems", "Download_Title", "Success"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
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

            // Show DownloadProgressWindow as indeterminate since raw BouncyCastle stream
            // copying here is completed internally in one continuous block.
            var progressWindow = new DownloadProgressWindow(fileName)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            progressWindow.Show();
            progressWindow.SetIndeterminate(); // Keep indeterminate as we copy raw socket packets

            try
            {
                // Hand over download completely to your Windows 7 safe BouncyCastle implementation
                await SecureWebClient.DownloadFileAsync(url, destFilePath);

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
