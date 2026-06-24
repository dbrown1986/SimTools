using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;

namespace SimToolsUpdater
{
    public partial class MainWindow : Form
    {
        private const string ManifestBaseUrl = "https://us1-repo.simtools-app.com/App";
        private const string TargetExe = "SimTools.exe";

        private bool _is64BitBinary = false;
        private HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnNext_Click(object sender, EventArgs e)
        {
            // Transition: Welcome -> Update
            if (WelcomeScreen.Visible)
            {
                WelcomeScreen.Visible = false;
                UpdateScreen.Visible = true;
                BtnNext.Enabled = false; // Disable during update processing

                bool verificationPassed = await RunUpdateEngineAsync();

                if (verificationPassed)
                {
                    // Transition: Update -> Complete (Only on complete success)
                    UpdateScreen.Visible = false;
                    CompleteScreen.Visible = true;
                    BtnNext.Text = "Finish";
                    BtnNext.Enabled = true;
                }
                else
                {
                    // The engine encountered an issue (Target missing, network down, bad manifest)
                    // Re-enable the next button so they aren't completely soft-locked if they want to retry
                    BtnNext.Enabled = true;
                    UpdateScreen.Visible = false;
                    WelcomeScreen.Visible = true;
                }
            }
            // Transition: Complete -> Exit
            else if (CompleteScreen.Visible)
            {
                FinalizeAndExit();
            }
        }

        private async Task<bool> RunUpdateEngineAsync()
        {
            try
            {
                // 1. Verify existence of TargetExe alongside updater
                string executionDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string localTargetExePath = Path.Combine(executionDirectory, TargetExe);

                if (!File.Exists(localTargetExePath))
                {
                    ShowError("Target Missing", $"Could not locate {TargetExe} inside the application folder. Please make sure the updater is located in the main directory.");
                    Application.Exit();
                    return false;
                }

                // 2. Read TargetExe PE configuration structure matching x86/x64 streams
                _is64BitBinary = Is64BitImage(localTargetExePath);
                string platformFolder = _is64BitBinary ? "x64" : "x86";

                // 3. Fetch update remote deployment manifest xml package mapping
                string xmlManifestUrl = $"{ManifestBaseUrl}/SimTools-Update-{platformFolder}.xml";
                string manifestData = await _httpClient.GetStringAsync(xmlManifestUrl);

                XDocument doc = XDocument.Parse(manifestData);
                var elements = doc.Descendants("File").ToList();
                int totalFiles = elements.Count;
                int processedFiles = 0;

                // 4. Update deployment synchronization execution routine loop
                foreach (var fileElement in elements)
                {
                    // Extract values from XML Attributes instead of Child Elements
                    string relativePath = fileElement.Attribute("name")?.Value ?? "";
                    string remoteFileHash = fileElement.Attribute("hash")?.Value?.ToLowerInvariant() ?? "";
                    string fileUrl = fileElement.Attribute("url")?.Value ?? "";

                    if (string.IsNullOrEmpty(relativePath)) continue;

                    // Use the direct URL provided in the manifest attribute if available, 
                    // otherwise fall back to building it manually using the UNSTRIPPED path
                    if (string.IsNullOrEmpty(fileUrl))
                    {
                        fileUrl = $"{ManifestBaseUrl}/{platformFolder}/{relativePath.Replace('\\', '/')}";
                    }

                    // STRIP PLATFORM PREFIXES FOR LOCAL PERSISTENCE
                    // Strips "x64/" or "x86/" (and backslash alternatives) from the beginning of the filename
                    if (relativePath.StartsWith("x64/", StringComparison.OrdinalIgnoreCase) ||
                        relativePath.StartsWith("x64\\", StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = relativePath.Substring(4);
                    }
                    else if (relativePath.StartsWith("x86/", StringComparison.OrdinalIgnoreCase) ||
                             relativePath.StartsWith("x86\\", StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = relativePath.Substring(4);
                    }

                    string targetFileName = Path.Combine(executionDirectory, relativePath);

                    // Check if file verification bypass is acceptable
                    if (File.Exists(targetFileName))
                    {
                        string currentLocalHash = GetFileHash(targetFileName);
                        if (currentLocalHash == remoteFileHash)
                        {
                            processedFiles++;
                            UpdateOverallProgress(processedFiles, totalFiles);
                            continue;
                        }
                    }

                    // Run custom file stream writer with automatic file-lock correction retry
                    await DownloadFileWithRetryAsync(fileUrl, targetFileName);
                    processedFiles++;
                    UpdateOverallProgress(processedFiles, totalFiles);
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowError("Update Failed", $"An unexpected error occurred during the update cycle:\n\n{ex.Message}\n\nPlease check your internet connection and try running the updater as an Administrator.");
                this.Close();
                return false;
            }
        }

        private async Task DownloadFileWithRetryAsync(string url, string destination)
        {
            int maxAttempts = 4;
            int delaySeconds = 2;

            // Ensure destination directory branch parameters exist
            string? directoryPath = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        long? totalBytes = response.Content.Headers.ContentLength;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                        {
                            byte[] buffer = new byte[8192];
                            long totalReadBytes = 0;
                            int readBytes;

                            while ((readBytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, readBytes);
                                totalReadBytes += readBytes;

                                if (totalBytes.HasValue)
                                {
                                    int progressPercentage = (int)((totalReadBytes * 100) / totalBytes.Value);
                                    UpdateFileProgress(progressPercentage);
                                }
                            }
                        }
                    }
                    return; // Succeeded!
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    // Handle runtime process tracking access collision issues gracefully
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }

        private void UpdateFileProgress(int percentage)
        {
            if (FileProgressBar.InvokeRequired)
            {
                FileProgressBar.BeginInvoke(new Action(() => FileProgressBar.Value = Math.Clamp(percentage, 0, 100)));
            }
            else
            {
                FileProgressBar.Value = Math.Clamp(percentage, 0, 100);
            }
        }

        private void UpdateOverallProgress(int current, int total)
        {
            int overallPercentage = (int)((current * 100) / total);
            if (OverallProgressBar.InvokeRequired)
            {
                OverallProgressBar.BeginInvoke(new Action(() => OverallProgressBar.Value = Math.Clamp(overallPercentage, 0, 100)));
            }
            else
            {
                OverallProgressBar.Value = Math.Clamp(overallPercentage, 0, 100);
            }
        }

        private void FinalizeAndExit()
        {
            if (ChkLaunch.Checked)
            {
                // Fix: Resolve directory identically to the update engine step
                string executionDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string targetExePath = Path.Combine(executionDirectory, TargetExe);

                if (File.Exists(targetExePath))
                {
                    Process.Start(new ProcessStartInfo(targetExePath)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = executionDirectory // Ensures target reads local assets properly
                    });
                }
                else
                {
                    // Optional diagnostic fallback to let you know if it's missing
                    ShowError("Launch Failed", $"Could not find {TargetExe} at:\n{targetExePath}");
                }
            }
            Application.Exit();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to cancel the SimTools update verification cycle?", "Cancel Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        #region Helper Functions (PE Architecture & Hashing Verification)
        private static bool Is64BitImage(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            stream.Seek(0x3c, SeekOrigin.Begin);
            int peOffset = reader.ReadInt32();
            stream.Seek(peOffset, SeekOrigin.Begin);

            uint peHead = reader.ReadUInt32(); // PE\0\0
            if (peHead != 0x00004550) throw new Exception("Invalid PE Header format.");

            ushort machine = reader.ReadUInt16();
            return machine == 0x8664; // AMD64 token standard
        }

        private static string GetFileHash(string filename)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
}