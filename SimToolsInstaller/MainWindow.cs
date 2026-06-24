using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SimToolsInstaller
{
    public partial class MainWindow : Form
    {
        private void MainWindow_Load(object? sender, EventArgs e)
        {
            // The designer is looking for this method! 
            // You can leave it completely empty.
        }

        // State
        private int _currentStep = 0;
        private CancellationTokenSource? _cancelTokenSource;
        private List<string> _installedFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent(); // Wakes up the Visual Designer!
            HookUpEvents();        // Attaches our logic to the buttons
            DetectArchitecture();
            LoadLicenseText();     // Loads Creative Commons text file into license agreement tab.

            // This hides the tabs at runtime so it looks like a wizard, 
            // but keeps them visible when you are working in the Visual Designer!
            wizardTabs.Appearance = TabAppearance.FlatButtons;
            wizardTabs.ItemSize = new Size(0, 1);
            wizardTabs.SizeMode = TabSizeMode.Fixed;

            UpdateUI();
        }

        private void LoadLicenseText()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                // Resource name is "Namespace.FileName"
                string resourceName = "SimToolsInstaller.legal.txt";

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            // Reads the whole file and pushes it into the text box
                            txtLicense.Text = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        txtLicense.Text = "Error: legal.txt resource not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                txtLicense.Text = $"Failed to load license: {ex.Message}";
            }
        }

        private void HookUpEvents()
        {
            btnCancelExit.Click += BtnCancelExit_Click;
            btnNext.Click += BtnNext_Click;
            btnPrev.Click += BtnPrev_Click;
            btnBrowse.Click += BtnBrowse_Click;

            radAgree.CheckedChanged += (s, e) => UpdateUI();
            rad64Bit.CheckedChanged += (s, e) => UpdateUI();
            txtInstallPath.TextChanged += (s, e) => ValidateDirectory();
        }

        private void DetectArchitecture()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                rad64Bit.Checked = true;
            }
            else
            {
                rad32Bit.Checked = true;
                rad64Bit.Enabled = false;
            }
        }

        private void UpdateUI()
        {
            btnPrev.Enabled = _currentStep > 0 && _currentStep < 5;
            btnNext.Text = _currentStep == 4 ? "Install" : "Next >";

            // Instantly switch to the correct tab!
            wizardTabs.SelectedIndex = _currentStep;

            // Handle the specific button logic for the current page
            switch (_currentStep)
            {
                case 1:
                    btnNext.Enabled = radAgree.Checked;
                    break;
                case 2:
                    btnNext.Enabled = rad32Bit.Checked || rad64Bit.Checked;
                    break;
                case 3:
                    if (string.IsNullOrEmpty(txtInstallPath.Text))
                    {
                        string progFiles = rad64Bit.Checked
                            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                            : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        txtInstallPath.Text = Path.Combine(progFiles, "SimTools");
                    }
                    ValidateDirectory();
                    break;
                case 5:
                    btnPrev.Enabled = false;
                    btnNext.Enabled = false;
                    _ = PerformInstallationAsync();
                    break;
                case 6: // Interrupt
                    btnPrev.Visible = false;
                    btnNext.Visible = false;
                    btnCancelExit.Text = "Exit";
                    break;
                case 7: // Complete
                    btnPrev.Visible = false;
                    btnNext.Visible = false;
                    btnCancelExit.Text = "Finish";
                    break;
            }
        }

        private void ValidateDirectory()
        {
            btnNext.Enabled = !string.IsNullOrWhiteSpace(txtInstallPath.Text) && Path.IsPathRooted(txtInstallPath.Text);
        }

        private void BtnNext_Click(object? sender, EventArgs e) { _currentStep++; UpdateUI(); }
        private void BtnPrev_Click(object? sender, EventArgs e) { _currentStep--; UpdateUI(); }

        private void BtnCancelExit_Click(object? sender, EventArgs e)
        {
            if (btnCancelExit.Text == "Exit" || btnCancelExit.Text == "Finish")
            {
                if (chkRun.Checked && _currentStep == 7)
                {
                    System.Diagnostics.Process.Start(Path.Combine(txtInstallPath.Text, "SimTools.exe"));
                }
                Application.Exit();
                return;
            }

            var result = MessageBox.Show("Are you sure you want to cancel the installation?", "Cancel Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (_currentStep == 5) _cancelTokenSource?.Cancel();
                else Application.Exit();
            }
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Select Install Folder" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                    txtInstallPath.Text = Path.Combine(dialog.SelectedPath, "SimTools");
            }
        }

        private async Task PerformInstallationAsync()
        {
            _cancelTokenSource = new CancellationTokenSource();
            string installDir = txtInstallPath.Text;
            bool is64Bit = rad64Bit.Checked;
            string xmlUrl = is64Bit
                ? "https://us1-repo.simtools-app.com/App/SimTools-x64.xml"
                : "https://us1-repo.simtools-app.com/App/SimTools-x86.xml";

            //      These are left intentionally disabled. They will be designed as
            //      side-by-side checkboxes to accompany the radio buttons for the
            //      installation of additional SimTools utilities. These utilities
            //      have not yet been designed fully at the time of this release.
            //    : "https://us1-repo.simtools-app.com/App/SimTools-Repo-Maker.xml";
            //    : "https://us1-repo.simtools-app.com/App/SimTools-Language-Tool.xml";

            try
            {
                if (!Directory.Exists(installDir)) Directory.CreateDirectory(installDir);

                using HttpClient client = new HttpClient();
                lblCurrentFile.Text = "Fetching manifest...";

                string xmlContent = await client.GetStringAsync(xmlUrl, _cancelTokenSource.Token);
                XDocument doc = XDocument.Parse(xmlContent);

                if (doc.Root == null) throw new InvalidDataException("Manifest missing root element.");

                var files = doc.Root.Elements("File");
                int totalFiles = System.Linq.Enumerable.Count(files);
                int currentFileCount = 0;

                progOverall.Maximum = totalFiles;
                progOverall.Value = 0;

                foreach (var file in files)
                {
                    _cancelTokenSource.Token.ThrowIfCancellationRequested();

                    string? fileName = file.Attribute("name")?.Value;
                    string? url = file.Attribute("url")?.Value;

                    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(url)) continue;

                    string localRelativePath = fileName.Substring(fileName.IndexOf('/') + 1);
                    string destPath = Path.Combine(installDir, localRelativePath);

                    lblCurrentFile.Text = $"Downloading: {localRelativePath}";

                    string? destDir = Path.GetDirectoryName(destPath);
                    if (destDir != null) Directory.CreateDirectory(destDir);

                    await DownloadFileAsync(client, url, destPath, _cancelTokenSource.Token);
                    _installedFiles.Add(destPath);

                    currentFileCount++;
                    progOverall.Value = currentFileCount;
                }

                if (chkDesktop.Checked || chkStartMenu.Checked) CreateShortcuts(installDir);
                CreateUninstallManifest(installDir);
                System.IO.File.WriteAllText(Path.Combine(installDir, "install_log.txt"), $"Installed on {DateTime.Now}");

                _currentStep = 7; UpdateUI();
            }
            catch (OperationCanceledException)
            {
                Rollback(installDir);
                _currentStep = 6; UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Rollback(installDir);
                _currentStep = 6; UpdateUI();
            }
        }

        private async Task DownloadFileAsync(HttpClient client, string url, string destPath, CancellationToken token)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            if (totalBytes != -1) progCurrent.Maximum = (int)totalBytes;

            using var stream = await response.Content.ReadAsStreamAsync(token);
            using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                totalRead += bytesRead;
                if (totalBytes != -1) progCurrent.Value = (int)totalRead;
            }
        }

        private void CreateShortcuts(string installDir)
        {
            string targetExe = Path.Combine(installDir, "SimTools.exe");
            WshShell shell = new WshShell();

            if (chkDesktop.Checked)
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SimTools.lnk");
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(path);
                shortcut.TargetPath = targetExe; shortcut.WorkingDirectory = installDir; shortcut.Save();
                _installedFiles.Add(path);
            }
            if (chkStartMenu.Checked)
            {
                string menuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "SimTools");
                Directory.CreateDirectory(menuPath);
                string path = Path.Combine(menuPath, "SimTools.lnk");
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(path);
                shortcut.TargetPath = targetExe; shortcut.WorkingDirectory = installDir; shortcut.Save();
                _installedFiles.Add(path);
                _installedFiles.Add(menuPath);
            }
        }

        private void CreateUninstallManifest(string installDir)
        {
            string path = Path.Combine(installDir, "uninstall_manifest.json");
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(_installedFiles, new JsonSerializerOptions { WriteIndented = true }));
            _installedFiles.Add(path);
        }

        private void Rollback(string installDir)
        {
            foreach (var file in _installedFiles)
            {
                try { if (System.IO.File.Exists(file)) System.IO.File.Delete(file); if (Directory.Exists(file)) Directory.Delete(file, true); } catch { }
            }
            try { if (Directory.Exists(installDir) && Directory.GetFiles(installDir).Length == 0) Directory.Delete(installDir, true); } catch { }
        }

        private void lblCompleteTitle_Click(object sender, EventArgs e)
        {

        }
    }
}