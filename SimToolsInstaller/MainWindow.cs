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

        // Simplifies tracking: tracks if the user explicitly clicked Browse or customized the text box
        private bool _userManuallyChangedDirectory = false;
        private bool _isInitializing = true; // Initialization gate

        // Central Language State Backing Field
        private LanguageStrings _currentLanguage = new LanguageStrings();

        public MainWindow()
        {
            // =================================================================================
            // LANGUAGE TESTING SUITE (Uncomment ONE line below to force-test that localization)
            // =================================================================================
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ar"); // Arabic (RTL)
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de"); // German
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es"); // Spanish
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr"); // French
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja"); // Japanese
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt"); // Portuguese
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru"); // Russian
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh"); // Chinese
            // =================================================================================

            // 1. First wake up and populate all design components so they exist in memory
            InitializeComponent();

            // 2. IMMEDIATELY map the active culture strings onto the controls before showing the form
            InitializeLocalization();

            // 3. Complete the rest of the setup configuration
            HookUpEvents();
            LoadLicenseText();
            DetectArchitecture();

            // Hides tabs at runtime so it looks like a wizard
            wizardTabs.Appearance = TabAppearance.FlatButtons;
            wizardTabs.ItemSize = new Size(0, 1);
            wizardTabs.SizeMode = TabSizeMode.Fixed;

            // 4. Draw the current layout step text safely
            UpdateUI();

            _isInitializing = false; // Clears the gate here so events can safely listen now.
        }

        private void LoadLicenseText()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                // 1. Get the current active 2-letter system/testing language code (e.g., "de", "es", "zh")
                string langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

                // 2. Construct the localized resource name path
                string localizedResourceName = $"SimToolsInstaller.legal_{langCode}.txt";
                string defaultResourceName = "SimToolsInstaller.legal.txt";

                string resourceToLoad = defaultResourceName;

                // 3. Check if the localized resource exists in the assembly manifests; if it does, use it!
                if (assembly.GetManifestResourceInfo(localizedResourceName) != null)
                {
                    resourceToLoad = localizedResourceName;
                }

                // 4. Stream and read the chosen manifest text file
                using (Stream? stream = assembly.GetManifestResourceStream(resourceToLoad))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            txtLicense.Text = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        txtLicense.Text = $"Error: License resource ({resourceToLoad}) not found.";
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
            // Navigation Controls
            btnNext.Click += BtnNext_Click;
            btnPrev.Click += BtnPrev_Click;
            btnCancelExit.Click += BtnCancelExit_Click;
            btnBrowse.Click += BtnBrowse_Click;

            // License Agreement Handlers
            radAgree.CheckedChanged += (s, e) => UpdateUI();
            radDisagree.CheckedChanged += (s, e) => UpdateUI();

            // Architecture Selection Toggles
            rad32Bit.CheckedChanged += Architecture_CheckedChanged;
            rad64Bit.CheckedChanged += Architecture_CheckedChanged;

            // Directory Selection Changes
            txtInstallPath.TextChanged += TxtFolder_TextChanged;
        }

        private void TxtFolder_TextChanged(object? sender, EventArgs e)
        {
            // Ignore automatic changes during form generation
            if (_isInitializing) return;

            string path = txtInstallPath.Text;
            string default64 = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SimTools");
            string default32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SimTools");

            // It is ONLY a manual user override if it doesn't match either of the system calculated options
            if (path != default64 && path != default32 && !string.IsNullOrEmpty(path))
            {
                _userManuallyChangedDirectory = true;
            }
            else if (path == default64 || path == default32)
            {
                // If they toggle back to a default option, unlock dynamic updates
                _userManuallyChangedDirectory = false;
            }
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

        private void Architecture_CheckedChanged(object? sender, EventArgs e)
        {
            // Ignore layout string changes during initial app boot up
            if (_isInitializing) return;

            if (sender is RadioButton radio && radio.Checked)
            {
                // Don't overwrite if the user typed or browsed to a custom location
                if (_userManuallyChangedDirectory) return;

                string progFiles;
                if (rad64Bit.Checked)
                {
                    progFiles = Environment.GetEnvironmentVariable("ProgramW6432")
                                ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
                else
                {
                    progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                }

                txtInstallPath.Text = Path.Combine(progFiles, "SimTools");
            }
        }

        private void UpdateUI()
        {
            btnPrev.Enabled = _currentStep > 0 && _currentStep < 5;
            btnPrev.Text = _currentLanguage.BtnPrev;
            btnNext.Text = _currentStep == 4 ? _currentLanguage.BtnInstall : _currentLanguage.BtnNext;

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
                case 3: // Installation Directory Screen
                    if (!_userManuallyChangedDirectory)
                    {
                        string progFiles;

                        if (rad64Bit.Checked)
                        {
                            // Direct environment lookup bypasses 32-bit WOW64 redirection safely
                            progFiles = Environment.GetEnvironmentVariable("ProgramW6432")
                                        ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                        }
                        else
                        {
                            progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        }

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
                    btnCancelExit.Text = _currentLanguage.BtnExit;
                    break;
                case 7: // Complete
                    btnPrev.Visible = false;
                    btnNext.Visible = false;
                    btnCancelExit.Text = _currentLanguage.BtnFinish;
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
            if (btnCancelExit.Text == _currentLanguage.BtnExit || btnCancelExit.Text == _currentLanguage.BtnFinish)
            {
                if (chkRun.Checked && _currentStep == 7)
                {
                    System.Diagnostics.Process.Start(Path.Combine(txtInstallPath.Text, "SimTools.exe"));
                }
                Application.Exit();
                return;
            }

            var result = MessageBox.Show(_currentLanguage.MsgCancelQuestion, _currentLanguage.MsgCancelTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (_currentStep == 5) _cancelTokenSource?.Cancel();
                else Application.Exit();
            }
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = txtInstallPath.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtInstallPath.Text = dialog.SelectedPath;
                    _userManuallyChangedDirectory = true;
                }
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
        #region Localization Storage Infrastructure
        private void InitializeLocalization()
        {
            string langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

            if (LocalizationData.Translations.ContainsKey(langCode))
            {
                _currentLanguage = LocalizationData.Translations[langCode];
            }
            else
            {
                _currentLanguage = LocalizationData.Translations["en"]; // Fallback
            }

            // Apply to static UI elements across all TabPages
            lblWelcomeTitle.Text = _currentLanguage.WelcomeTitle;
            lblWelcomeDesc.Text = _currentLanguage.WelcomeDesc;

            lblLicenseTitle.Text = _currentLanguage.LicenseTitle;
            lblLicenseDesc.Text = _currentLanguage.LicenseDesc;
            radAgree.Text = _currentLanguage.RadAgree;
            radDisagree.Text = _currentLanguage.RadDisagree;

            lblArchTitle.Text = _currentLanguage.ArchTitle;
            lblArchDesc.Text = _currentLanguage.ArchDesc;
            rad32Bit.Text = _currentLanguage.Rad32Bit;
            rad64Bit.Text = _currentLanguage.Rad64Bit;
            richTextBox1.Text = _currentLanguage.RichText32;
            richTextBox2.Text = _currentLanguage.RichText64;

            lblDirTitle.Text = _currentLanguage.DirTitle;
            lblDirDesc.Text = _currentLanguage.DirDesc;
            label1.Text = _currentLanguage.DirLabel1;
            btnBrowse.Text = _currentLanguage.BtnBrowse;
            chkDesktop.Text = _currentLanguage.ChkDesktop;
            chkStartMenu.Text = _currentLanguage.ChkStartMenu;
            checkBox1.Text = _currentLanguage.ChkRepoUtility;
            checkBox2.Text = _currentLanguage.ChkLangTool;

            lblReadyTitle.Text = _currentLanguage.ReadyTitle;
            lblReadyDesc.Text = _currentLanguage.ReadyDesc;

            lblInstallTitle.Text = _currentLanguage.InstallTitle;
            lblOverall.Text = _currentLanguage.LblOverallProgress;

            lblInterruptTitle.Text = _currentLanguage.InterruptTitle;
            lblInterruptDesc.Text = _currentLanguage.InterruptDesc;

            lblCompleteTitle.Text = _currentLanguage.CompleteTitle;
            lblCompleteDesc.Text = _currentLanguage.CompleteDesc;
            chkRun.Text = _currentLanguage.ChkRun;

            btnCancelExit.Text = _currentLanguage.BtnCancel;
        }

        public class LanguageStrings
        {
            public string BtnPrev { get; set; } = "< Back";
            public string BtnNext { get; set; } = "Next >";
            public string BtnCancel { get; set; } = "Cancel";
            public string BtnInstall { get; set; } = "Install";
            public string BtnExit { get; set; } = "Exit";
            public string BtnFinish { get; set; } = "Finish";
            public string BtnBrowse { get; set; } = "Browse...";

            public string WelcomeTitle { get; set; } = "Welcome to the SimTools Setup Wizard";
            public string WelcomeDesc { get; set; } = "This wizard will guide you through the installation of SimTools.\n\nIt is recommended that you close all other applications before starting Setup.\n\nClick Next to continue.";

            public string LicenseTitle { get; set; } = "License Agreement";
            public string LicenseDesc { get; set; } = "Please review the license terms before installing SimTools.";
            public string RadAgree { get; set; } = "I agree to the terms";
            public string RadDisagree { get; set; } = "I do not agree";

            public string ArchTitle { get; set; } = "Select Architecture";
            public string ArchDesc { get; set; } = "Please select the version of SimTools you wish to install based on your OS.";
            public string Rad32Bit { get; set; } = "32-Bit (x86)";
            public string Rad64Bit { get; set; } = "64-Bit (x64)";
            public string RichText32 { get; set; } = "The 32-Bit version of SimTools is ideal for legacy systems running on a quad-core CPU or below with 4GB - 8GB of system memory. 32-Bit system executables are restricted to only 4GB~ max usage.";
            public string RichText64 { get; set; } = "The 64-Bit version of SimTools is optimized for modern high-performance gaming machines with 16GB+ of system memory and multi-core processors.";

            public string DirTitle { get; set; } = "Choose Install Location";
            public string DirDesc { get; set; } = "Choose the folder in which to install SimTools.";
            public string DirLabel1 { get; set; } = "Setup will install SimTools into the following folder. To continue, click Next. If you would like to select a different folder, click Browse.";
            public string ChkDesktop { get; set; } = "Create a desktop shortcut";
            public string ChkStartMenu { get; set; } = "Create a Start Menu shortcut";
            public string ChkRepoUtility { get; set; } = "Also Install Repo Maker Utility (In Development)";
            public string ChkLangTool { get; set; } = "Also Install Language Tool (In Development)";

            public string ReadyTitle { get; set; } = "Ready to Install";
            public string ReadyDesc { get; set; } = "Setup is now ready to begin installing SimTools on your computer.";

            public string InstallTitle { get; set; } = "Installing...";
            public string LblOverallProgress { get; set; } = "Overall Progress:";

            public string InterruptTitle { get; set; } = "Installation Interrupted";
            public string InterruptDesc { get; set; } = "The installation was cancelled or interrupted.";

            public string CompleteTitle { get; set; } = "Completing the SimTools Setup Wizard";
            public string CompleteDesc { get; set; } = "Setup has finished installing SimTools on your computer. The application may be launched by selecting the installed shortcuts.";
            public string ChkRun { get; set; } = "Launch SimTools now";

            public string MsgCancelQuestion { get; set; } = "Are you sure you want to cancel the installation?";
            public string MsgCancelTitle { get; set; } = "Cancel Setup";
        }

        public static class LocalizationData
        {
            public static readonly Dictionary<string, LanguageStrings> Translations = new Dictionary<string, LanguageStrings>
            {
                ["en"] = new LanguageStrings(),

                ["ar"] = new LanguageStrings
                {
                    BtnPrev = "السابق >",
                    BtnNext = "< التالي",
                    BtnCancel = "إلغاء الأمر",
                    BtnInstall = "تثبيت",
                    BtnExit = "خروج",
                    BtnFinish = "إنهاء",
                    BtnBrowse = "استعراض...",
                    WelcomeTitle = "مرحبًا بك في معالج إعداد SimTools",
                    WelcomeDesc = "سيقوم هذا المعالج بتوجيهك خلال تثبيت SimTools.\n\nيوصى بإغلاق جميع التطبيقات الأخرى قبل بدء الإعداد.\n\nانقر فوق التالي للمتابعة.",
                    LicenseTitle = "اتفاقية الترخيص",
                    LicenseDesc = "يرجى مراجعة شروط الترخيص قبل تثبيت SimTools.",
                    RadAgree = "أوافق على الشروط",
                    RadDisagree = "لا أوافق",
                    ArchTitle = "تحديد البنية الخاصة بالنظام",
                    ArchDesc = "يرجى تحديد إصدار SimTools الذي ترغب في تثبيته بناءً على نظام التشغيل لديك.",
                    Rad32Bit = "32 بت (x86)",
                    Rad64Bit = "64 بت (x64)",
                    RichText32 = "إصدار 32 بت من SimTools مثالي للأنظمة القديمة التي تعمل بمعالج رباعي النواة أو أقل مع ذاكرة نظام تتراوح بين 4 إلى 8 جيجابايت.",
                    RichText64 = "تم تحسين إصدار 64 بت من SimTools للأجهزة الحديثة وعالية الأداء التي تحتوي على ذاكرة نظام تبلغ 16 جيجابايت أو أكثر ومعالجات متعددة النواة.",
                    DirTitle = "تحديد موقع التثبيت",
                    DirDesc = "اختر المجلد الذي ترغب في تثبيت SimTools فيه.",
                    DirLabel1 = "سيقوم البرنامج بتثبيت SimTools في المجلد التالي. للمتابعة انقر فوق التالي. إذا كنت ترغب في اختيار مجلد آخر انقر فوق استعراض.",
                    ChkDesktop = "إنشاء اختصار على سطح المكتب",
                    ChkStartMenu = "إنشاء اختصار في قائمة ابدأ",
                    ChkRepoUtility = "تثبيت أداة Repo Maker أيضًا (قيد التطوير)",
                    ChkLangTool = "تثبيت أداة اللغة أيضًا (قيد التطوير)",
                    ReadyTitle = "جاهز للتثبيت",
                    ReadyDesc = "برنامج الإعداد جاهز الآن لبدء تثبيت SimTools على جهاز الكمبيوتر الخاص بك.",
                    InstallTitle = "جاري التثبيت...",
                    LblOverallProgress = "التقدم العام:",
                    InterruptTitle = "تم مقاطعة التثبيت",
                    InterruptDesc = "تم إلغاء التثبيت أو مقاطعته.",
                    CompleteTitle = "إكمال معالج إعداد SimTools",
                    CompleteDesc = "انتهى معالج الإعداد من تثبيت SimTools على جهاز الكمبيوتر الخاص بك. يمكن تشغيل التطبيق عن طريق تحديد الاختصارات المثبتة.",
                    ChkRun = "تشغيل SimTools الآن",
                    MsgCancelQuestion = "هل أنت متأكد من أنك تريد إلغاء التثبيت؟",
                    MsgCancelTitle = "إلغاء الإعداد"
                },

                ["de"] = new LanguageStrings
                {
                    BtnPrev = "< Zurück",
                    BtnNext = "Weiter >",
                    BtnCancel = "Abbrechen",
                    BtnInstall = "Installieren",
                    BtnExit = "Beenden",
                    BtnFinish = "Fertigstellen",
                    BtnBrowse = "Durchsuchen...",
                    WelcomeTitle = "Willkommen beim SimTools Installations-Assistenten",
                    WelcomeDesc = "Dieser Assistent wird Sie durch die Installation von SimTools führen.\n\nEs wird empfohlen, alle anderen Anwendungen zu schließen, bevor Sie das Setup starten.\n\nKlicken Sie auf Weiter, um fortzufahren.",
                    LicenseTitle = "Lizenzvereinbarung",
                    LicenseDesc = "Bitte lesen Sie die Lizenzbedingungen vor der Installation von SimTools.",
                    RadAgree = "Ich akzeptiere die Bedingungen",
                    RadDisagree = "Ich lehne ab",
                    ArchTitle = "Architektur auswählen",
                    ArchDesc = "Bitte wählen Sie die Version von SimTools, die Sie installieren möchten, basierend auf Ihrem Betriebssystem.",
                    Rad32Bit = "32-Bit (x86)",
                    Rad64Bit = "64-Bit (x64)",
                    RichText32 = "Die 32-Bit-Version von SimTools ist ideal für ältere Systeme mit einem Quad-Core-Prozessor oder darunter und 4 GB - 8 GB Arbeitsspeicher.",
                    RichText64 = "Die 64-Bit-Version von SimTools ist für moderne Hochleistungs-Gaming-PCs mit mehr als 16 GB Arbeitsspeicher und Multi-Core-Prozessoren optimiert.",
                    DirTitle = "Installationsordner wählen",
                    DirDesc = "Wählen Sie den Ordner, in dem SimTools installiert werden soll.",
                    DirLabel1 = "Das Setup installiert SimTools in den folgenden Ordner. Klicken Sie auf Weiter, um fortzufahren. Klicken Sie auf Durchsuchen, um einen anderen Ordner auszuwählen.",
                    ChkDesktop = "Desktop-Verknüpfung erstellen",
                    ChkStartMenu = "Startmenü-Verknüpfung erstellen",
                    ChkRepoUtility = "Auch Repo Maker Utility installieren (In Entwicklung)",
                    ChkLangTool = "Auch Language Tool installieren (In Entwicklung)",
                    ReadyTitle = "Bereit zur Installation",
                    ReadyDesc = "Das Setup ist nun bereit, SimTools auf Ihrem Computer zu installieren.",
                    InstallTitle = "Installation läuft...",
                    LblOverallProgress = "Gesamtfortschritt:",
                    InterruptTitle = "Installation abgebrochen",
                    InterruptDesc = "Die Installation wurde abgebrochen oder unterbrochen.",
                    CompleteTitle = "Fertigstellen des SimTools Installations-Assistenten",
                    CompleteDesc = "Das Setup hat die Installation von SimTools auf Ihrem Computer abgeschlossen. Die Anwendung kann über die Verknüpfungen gestartet werden.",
                    ChkRun = "SimTools jetzt starten",
                    MsgCancelQuestion = "Sind Sie sicher, dass Sie die Installation abbrechen möchten?",
                    MsgCancelTitle = "Setup abbrechen"
                },

                ["es"] = new LanguageStrings
                {
                    BtnPrev = "< Atrás",
                    BtnNext = "Siguiente >",
                    BtnCancel = "Cancelar",
                    BtnInstall = "Instalar",
                    BtnExit = "Salir",
                    BtnFinish = "Finalizar",
                    BtnBrowse = "Examinar...",
                    WelcomeTitle = "Bienvenido al Asistente de Instalación de SimTools",
                    WelcomeDesc = "Este asistente le guiará a través de la instalación de SimTools.\n\nSe recomienda cerrar todas las demás aplicaciones antes de iniciar.\n\nHaga clic en Siguiente para continuar.",
                    LicenseTitle = "Acuerdo de Licencia",
                    LicenseDesc = "Revise los términos de la licencia antes de instalar SimTools.",
                    RadAgree = "Acepto los términos",
                    RadDisagree = "No acepto",
                    ArchTitle = "Seleccionar Arquitectura",
                    ArchDesc = "Seleccione la versión de SimTools que desea instalar según su sistema operativo.",
                    Rad32Bit = "32-Bit (x86)",
                    Rad64Bit = "64-Bit (x64)",
                    RichText32 = "La versión de 32 bits de SimTools es ideal para sistemas antiguos que se ejecutan en una CPU de cuatro núcleos o menos con 4 GB a 8 GB de memoria.",
                    RichText64 = "La versión de 64 bits de SimTools está optimizada para máquinas de juego modernas de alto rendimiento con más de 16 GB de memoria y procesadores multinúcleo.",
                    DirTitle = "Elegir Ubicación de Instalación",
                    DirDesc = "Elija la carpeta en la que desea instalar SimTools.",
                    DirLabel1 = "El programa instalará SimTools en la siguiente carpeta. Para continuar, haga clic en Siguiente. Si desea seleccionar una carpeta diferente, haga clic en Examinar.",
                    ChkDesktop = "Crear un acceso directo en el escritorio",
                    ChkStartMenu = "Crear un acceso directo en el menú de inicio",
                    ChkRepoUtility = "Instalar también la utilidad Repo Maker (En desarrollo)",
                    ChkLangTool = "Instalar también la herramienta de idioma (En desarrollo)",
                    ReadyTitle = "Listo para Instalar",
                    ReadyDesc = "El asistente ya está listo para comenzar la instalación de SimTools en su computadora.",
                    InstallTitle = "Instalando...",
                    LblOverallProgress = "Progreso General:",
                    InterruptTitle = "Instalación Interrumpida",
                    InterruptDesc = "La instalación fue cancelada o interrumpida.",
                    CompleteTitle = "Completando el Asistente de Instalación de SimTools",
                    CompleteDesc = "El instalador ha terminado de instalar SimTools en su computadora. Puede iniciar la aplicación mediante los accesos directos creados.",
                    ChkRun = "Ejecutar SimTools ahora",
                    MsgCancelQuestion = "¿Está seguro de que desea cancelar la instalación?",
                    MsgCancelTitle = "Cancelar Instalación"
                },

                ["fr"] = new LanguageStrings
                {
                    BtnPrev = "< Précédent",
                    BtnNext = "Suivant >",
                    BtnCancel = "Annuler",
                    BtnInstall = "Installer",
                    BtnExit = "Quitter",
                    BtnFinish = "Terminer",
                    BtnBrowse = "Parcourir...",
                    WelcomeTitle = "Bienvenue dans le programme d'installation de SimTools",
                    WelcomeDesc = "Ce programme vous guidera tout au long de l'installation de SimTools.\n\nIl est recommandé de fermer toutes les autres applications avant de commencer.\n\nCliquez sur Suivant pour continuer.",
                    LicenseTitle = "Contrat de Licence",
                    LicenseDesc = "Veuillez lire les conditions de licence avant d'installer SimTools.",
                    RadAgree = "J'accepte les termes",
                    RadDisagree = "Je n'accepte pas",
                    ArchTitle = "Sélection de l'Architecture",
                    ArchDesc = "Veuillez sélectionner la version de SimTools à installer en fonction de votre système d'exploitation.",
                    Rad32Bit = "32-Bit (x86)",
                    Rad64Bit = "64-Bit (x64)",
                    RichText32 = "La version 32 bits de SimTools est idéale pour les anciens systèmes fonctionnant avec un processeur quadricœur ou inférieur avec 4 à 8 Go de mémoire.",
                    RichText64 = "La version 64 bits de SimTools est optimisée pour les configurations de jeu modernes à hautes performances avec plus de 16 Go de mémoire et des processeurs multicœurs.",
                    DirTitle = "Dossier de Destination",
                    DirDesc = "Choisissez le dossier dans lequel vous souhaitez installer SimTools.",
                    DirLabel1 = "Le programme installera SimTools dans le dossier suivant. Pour continuer, cliquez sur Suivant. Pour choisir un autre dossier, cliquez sur Parcourir.",
                    ChkDesktop = "Créer un raccourci sur le bureau",
                    ChkStartMenu = "Créer un raccourci dans le menu Démarrer",
                    ChkRepoUtility = "Installer également l'utilitaire Repo Maker (En développement)",
                    ChkLangTool = "Installer également l'outil de langue (En développement)",
                    ReadyTitle = "Prêt à Installer",
                    ReadyDesc = "Le programme est prêt à commencer l'installation de SimTools sur votre ordinateur.",
                    InstallTitle = "Installation en cours...",
                    LblOverallProgress = "Progression générale :",
                    InterruptTitle = "Installation Interrompue",
                    InterruptDesc = "L'installation a été annulée ou interrompue.",
                    CompleteTitle = "Fin de l'installation de SimTools",
                    CompleteDesc = "Le programme a terminé l'installation de SimTools sur votre ordinateur. L'application peut être lancée à l'aide des raccourcis installés.",
                    ChkRun = "Lancer SimTools maintenant",
                    MsgCancelQuestion = "Êtes-vous sûr de vouloir annuler l'installation ?",
                    MsgCancelTitle = "Annuler l'installation"
                },

                ["ja"] = new LanguageStrings
                {
                    BtnPrev = "< 戻る",
                    BtnNext = "次へ >",
                    BtnCancel = "キャンセル",
                    BtnInstall = "インストール",
                    BtnExit = "終了",
                    BtnFinish = "完了",
                    BtnBrowse = "参照...",
                    WelcomeTitle = "SimTools セットアップウィザードへようこそ",
                    WelcomeDesc = "このウィザードは、コンピューターへの SimTools のインストールを案内します。\n\nセットアップを開始する前に、他のすべてのアプリケーションを閉じることをお勧めします。\n\n続行するには「次へ」をクリックしてください。",
                    LicenseTitle = "ライセンス契約",
                    LicenseDesc = "SimTools をインストールする前に、ライセンス条項を確認してください。",
                    RadAgree = "規約に同意する",
                    RadDisagree = "同意しない",
                    ArchTitle = "アーキテクチャの選択",
                    ArchDesc = "お使いのOSに合わせて、インストールする SimTools のバージョンを選択してください。",
                    Rad32Bit = "32ビット (x86)",
                    Rad64Bit = "64ビット (x64)",
                    RichText32 = "SimTools の 32 ビットバージョンは、4GB〜8GBのシステムメモリを搭載したクアッドコアCPU以下のレガシーシステムに最適です。",
                    RichText64 = "SimTools の 64 ビットバージョンは、16GB以上のメモリとマルチコアプロセッサを備えた最新の高性能ゲーミングマシン向けに最適化されています。",
                    DirTitle = "インストール先の選択",
                    DirDesc = "SimTools をインストールするフォルダを選択してください。",
                    DirLabel1 = "セットアップは次のフォルダに SimTools をインストールします。続行するには「次へ」をクリック、別のフォルダを選択する場合は「参照」をクリックしてください。",
                    ChkDesktop = "デスクトップにショートカットを作成する",
                    ChkStartMenu = "スタートメニューにショートカットを作成する",
                    ChkRepoUtility = "Repo Maker ユーティリティもインストールする (開発中)",
                    ChkLangTool = "言語ツールもインストールする (開発中)",
                    ReadyTitle = "インストールの準備完了",
                    ReadyDesc = "セットアップは、コンピューターへの SimTools のインストールを開始する準備ができました。",
                    InstallTitle = "インストール中...",
                    LblOverallProgress = "全体の進行状況:",
                    InterruptTitle = "インストールの中断",
                    InterruptDesc = "インストールはキャンセルされるか、中断されました。",
                    CompleteTitle = "SimTools セットアップウィザードの完了",
                    CompleteDesc = "SimTools のインストールが完了しました。作成されたショートカットからアプリケーションを起動できます。",
                    ChkRun = "今すぐ SimTools を起動する",
                    MsgCancelQuestion = "インストールをキャンセルしてもよろしいですか？",
                    MsgCancelTitle = "セットアップのキャンセル"
                },

                ["pt"] = new LanguageStrings
                {
                    BtnPrev = "< Voltar",
                    BtnNext = "Avançar >",
                    BtnCancel = "Cancelar",
                    BtnInstall = "Instalar",
                    BtnExit = "Sair",
                    BtnFinish = "Concluir",
                    BtnBrowse = "Procurar...",
                    WelcomeTitle = "Bem-vindo ao Assistente de Instalação do SimTools",
                    WelcomeDesc = "Este assistente irá guiá-lo através da instalação do SimTools.\n\nRecomenda-se fechar todos os outros aplicativos antes de iniciar a configuração.\n\nClique em Avançar para continuar.",
                    LicenseTitle = "Contrato de Licença",
                    LicenseDesc = "Por favor, revise os termos da licença antes de instalar o SimTools.",
                    RadAgree = "Eu aceito os termos",
                    RadDisagree = "Não aceito",
                    ArchTitle = "Selecionar Arquitetura",
                    ArchDesc = "Selecione a versão do SimTools que deseja instalar com base no seu sistema operacional.",
                    Rad32Bit = "32-Bit (x86)",
                    Rad64Bit = "64-Bit (x64)",
                    RichText32 = "A versão de 32 bits do SimTools é ideal para sistemas antigos executados em uma CPU quad-core ou inferior com 4 GB a 8 GB de memória.",
                    RichText64 = "A versão de 64 bits do SimTools é otimizada para computadores de jogos modernos de alto desempenho com mais de 16 GB de memória e processadores multinúcleo.",
                    DirTitle = "Escolher Local de Instalação",
                    DirDesc = "Escolha a pasta na qual deseja instalar o SimTools.",
                    DirLabel1 = "O assistente vai instalar o SimTools na seguinte pasta. Para continuar, clique em Avançar. Se quiser selecionar uma pasta diferente, clique em Procurar.",
                    ChkDesktop = "Criar um atalho na área de trabalho",
                    ChkStartMenu = "Criar um atalho no menu Iniciar",
                    ChkRepoUtility = "Instalar também o utilitário Repo Maker (Em desenvolvimento)",
                    ChkLangTool = "Instalar também a ferramenta de idioma (Em desenvolvimento)",
                    ReadyTitle = "Pronto para Instalar",
                    ReadyDesc = "O assistente está pronto para iniciar a instalação do SimTools no seu computador.",
                    InstallTitle = "Instalando...",
                    LblOverallProgress = "Progresso Geral:",
                    InterruptTitle = "Instalação Interrompida",
                    InterruptDesc = "A instalação foi cancelada ou interrompida.",
                    CompleteTitle = "Concluindo o Assistente de Instalação do SimTools",
                    CompleteDesc = "O assistente terminou de instalar o SimTools no seu computador. O aplicativo pode ser iniciado pelos atalhos criados.",
                    ChkRun = "Iniciar o SimTools agora",
                    MsgCancelQuestion = "Tem certeza de que deseja cancelar a instalação?",
                    MsgCancelTitle = "Cancelar Instalação"
                },

                ["ru"] = new LanguageStrings
                {
                    BtnPrev = "< Назад",
                    BtnNext = "Далее >",
                    BtnCancel = "Отмена",
                    BtnInstall = "Установить",
                    BtnExit = "Выход",
                    BtnFinish = "Завершить",
                    BtnBrowse = "Обзор...",
                    WelcomeTitle = "Вас приветствует мастер установки SimTools",
                    WelcomeDesc = "Этот мастер поможет вам установить программу SimTools на ваш компьютер.\n\nПеред началом установки рекомендуется закрыть все другие приложения.\n\nНажмите «Далее», чтобы продолжить.",
                    LicenseTitle = "Лицензионное соглашение",
                    LicenseDesc = "Пожалуйста, ознакомьтесь с условиями лицензии перед установкой SimTools.",
                    RadAgree = "Я принимаю условия",
                    RadDisagree = "Я не принимаю",
                    ArchTitle = "Выбор архитектуры",
                    ArchDesc = "Пожалуйста, выберите версию SimTools для установки в соответствии с вашей операционной системой.",
                    Rad32Bit = "32-Bit (x86)",
                    Rad64Bit = "64-Bit (x64)",
                    RichText32 = "32-разрядная версия SimTools идеально подходит для устаревших систем с четырехъядерным процессором или ниже и 4–8 ГБ оперативной памяти.",
                    RichText64 = "64-разрядная версия SimTools оптимизирована для современных высокопроизводительных игровых систем с 16 ГБ+ оперативной памяти и многоядерными процессорами.",
                    DirTitle = "Выбор папки установки",
                    DirDesc = "Выберите папку, в которую вы хотите установить SimTools.",
                    DirLabel1 = "Программа установит SimTools в следующую папку. Для продолжения нажмите «Далее». Если вы хотите выбрать другую папку, нажмите «Обзор».",
                    ChkDesktop = "Создать ярлык на рабочем столе",
                    ChkStartMenu = "Создать ярлык в меню «Пуск»",
                    ChkRepoUtility = "Также установить утилиту Repo Maker (В разработке)",
                    ChkLangTool = "Также установить языковой инструмент (В разработке)",
                    ReadyTitle = "Все готово к установке",
                    ReadyDesc = "Программа установки готова начать копирование файлов SimTools на ваш компьютер.",
                    InstallTitle = "Установка...",
                    LblOverallProgress = "Общий прогресс:",
                    InterruptTitle = "Установка прервана",
                    InterruptDesc = "Установка была отменена или прервана.",
                    CompleteTitle = "Завершение работы мастера установки SimTools",
                    CompleteDesc = "Программа установки успешно завершила работу. Приложение может быть запущено с помощью созданных ярлыков.",
                    ChkRun = "Запустить SimTools сейчас",
                    MsgCancelQuestion = "Вы уверены, что хотите отменить установку?",
                    MsgCancelTitle = "Отмена установки"
                },

                ["zh"] = new LanguageStrings
                {
                    BtnPrev = "< 上一步",
                    BtnNext = "下一步 >",
                    BtnCancel = "取消",
                    BtnInstall = "安装",
                    BtnExit = "退出",
                    BtnFinish = "完成",
                    BtnBrowse = "浏览...",
                    WelcomeTitle = "欢迎使用 SimTools 安装向导",
                    WelcomeDesc = "本向导将引导您完成 SimTools 的安装。\n\n建议在启动安装程序之前关闭所有其他应用程序。\n\n点击“下一步”继续。",
                    LicenseTitle = "许可协议",
                    LicenseDesc = "在安装 SimTools 之前，请阅读许可条款。",
                    RadAgree = "我接受协议条款",
                    RadDisagree = "我拒绝",
                    ArchTitle = "选择系统架构",
                    ArchDesc = "请根据您的操作系统选择要安装的 SimTools 版本。",
                    Rad32Bit = "32位 (x86)",
                    Rad64Bit = "64位 (x64)",
                    RichText32 = "32位版本的 SimTools 非常适合运行在四核或更低配置CPU、4GB - 8GB内存的旧型系统上。",
                    RichText64 = "64位版本的 SimTools 专为配备 16GB+ 内存及多核处理器的现代高性能游戏电脑而深度优化。",
                    DirTitle = "选择安装位置",
                    DirDesc = "选择安装 SimTools 的目标文件夹。",
                    DirLabel1 = "安装程序会将 SimTools 安装到以下文件夹。要继续请点击“下一步”，如需选择其他文件夹请点击“浏览”。",
                    ChkDesktop = "创建桌面快捷方式",
                    ChkStartMenu = "创建 Plank/开始菜单快捷方式",
                    ChkRepoUtility = "同时安装 Repo Maker 工具 (正在开发中)",
                    ChkLangTool = "同时安装语言管理工具 (正在开发中)",
                    ReadyTitle = "准备安装",
                    ReadyDesc = "向导现在准备好开始在您的电脑上安装 SimTools。",
                    InstallTitle = "正在安装...",
                    LblOverallProgress = "总体进度:",
                    InterruptTitle = "安装已中断",
                    InterruptDesc = "安装已被取消或强行中断。",
                    CompleteTitle = "正在完成 SimTools 安装向导",
                    CompleteDesc = "安装程序已成功将 SimTools 安装到您的计算机上。您可以通过选择生成的快捷方式来启动程序。",
                    ChkRun = "立即运行 SimTools",
                    MsgCancelQuestion = "您确定要取消当前的安装进程吗？",
                    MsgCancelTitle = "取消安装"
                }
            };
        }
        #endregion
    }
}