using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Runtime.InteropServices;
using Avalonia.Layout;

namespace SimTools
{
    // Custom TextWriter to redirect Console.Write to the Avalonia GUI
    public class GuiConsoleWriter : TextWriter
    {
        private readonly Action<string> _writeAction;
        public GuiConsoleWriter(Action<string> writeAction) => _writeAction = writeAction;

        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(char value) => _writeAction(value.ToString());
        public override void Write(string? value) { if (value != null) _writeAction(value); }
        public override void WriteLine(string? value) => _writeAction((value ?? "") + "\n");
        public override void WriteLine() => _writeAction("\n");
    }

    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Process? _apacheProcess;

        public MainWindow()
        {
            InitializeComponent();

            // Set a default output directory on startup
            TxtOutputDir.Text = Path.Combine(Environment.CurrentDirectory, "repo");
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // Kill Apache if it is running when the application closes
            if (_apacheProcess != null && !_apacheProcess.HasExited)
            {
                _apacheProcess.Kill();
                _apacheProcess.Dispose();
            }
            base.OnClosing(e);
        }

        private void LogToGui(string text)
        {
            // Ensure thread safety when updating the GUI from the background task
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                // The \r character is used by our CLI progress bar to overwrite the same line
                if (text.StartsWith("\r"))
                {
                    string cleanText = text.TrimStart('\r').TrimEnd();

                    // Strip out the text-based progress bar (e.g., [███---]) so only the GUI bar is visible
                    int bracketIndex = cleanText.IndexOf('[');
                    if (bracketIndex > 0)
                    {
                        TxtStatus.Text = cleanText.Substring(0, bracketIndex).Trim();
                    }
                    else
                    {
                        TxtStatus.Text = cleanText;
                    }

                    // Parse the text to find the percentage and update the actual GUI progress bar
                    if (text.Contains("%"))
                    {
                        string[] parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (part.EndsWith("%") && int.TryParse(part.TrimEnd('%'), out int pct))
                            {
                                SyncProgressBar.Value = pct;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    TxtLog.Text += text;
                    LogScrollViewer.ScrollToEnd();
                }
            });
        }

        private void SetControlsEnabled(bool isEnabled)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                BtnMirror.IsEnabled = isEnabled;
                BtnLocalHost.IsEnabled = isEnabled;
                BtnService.IsEnabled = isEnabled;
                BtnToggleApache.IsEnabled = isEnabled;
                BtnBrowse.IsEnabled = isEnabled;
                ChkCreateZip.IsEnabled = isEnabled;
                CmbServerSource.IsEnabled = isEnabled;
            });
        }

        private string GetSelectedRepoUrl()
        {
            // Map the friendly UI names back to the actual server URLs
            string defaultUrl = "https://us1-repo.simtools-app.com/";

            if (CmbServerSource.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
            {
                string selection = selectedItem.Content.ToString()!;
                return selection switch
                {
                    "United States (West)" => "https://us1-repo.simtools-app.com/",
                    "United States (East)" => "https://us2-repo.simtools-app.com/",
                    "Germany (Frankfurt)" => "https://de1-repo.simtools-app.com/",
                    "Japan (Tokyo)" => "https://jp1-repo.simtools-app.com/",
                    "Netherlands (Amsterdam)" => "https://nl1-repo.simtools-app.com/",
                    "Singapore" => "https://sg1-repo.simtools-app.com/",
                    _ => defaultUrl
                };
            }
            return defaultUrl;
        }

        // --- Custom Native Avalonia Confirmation Dialog ---
        private async Task<bool> ShowConfirmDialog(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 20 };
            panel.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 10 };
            var btnYes = new Button { Content = "Yes", Width = 75, HorizontalContentAlignment = HorizontalAlignment.Center };
            var btnNo = new Button { Content = "No", Width = 75, HorizontalContentAlignment = HorizontalAlignment.Center };

            bool result = false;

            btnYes.Click += (s, e) => { result = true; dialog.Close(); };
            btnNo.Click += (s, e) => { result = false; dialog.Close(); };

            btnPanel.Children.Add(btnYes);
            btnPanel.Children.Add(btnNo);
            panel.Children.Add(btnPanel);
            dialog.Content = panel;

            await dialog.ShowDialog(this);
            return result;
        }

        private async void BtnBrowse_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Output Directory",
                AllowMultiple = false
            });

            if (folders != null && folders.Count > 0)
            {
                TxtOutputDir.Text = folders[0].Path.LocalPath;
                LogToGui($"[*] Output directory changed to: {TxtOutputDir.Text}\n");
            }
        }

        private async void BtnMirror_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtOutputDir.Text))
            {
                LogToGui("[!] Please select an output directory first.\n");
                return;
            }

            LogToGui("\n[*] Starting Mirror Mode...\n");
            TxtStatus.Text = "Fetching manifest & downloading...";
            SetControlsEnabled(false);
            SyncProgressBar.Value = 0;

            _cancellationTokenSource = new CancellationTokenSource();
            string targetDir = TxtOutputDir.Text;
            bool doZip = ChkCreateZip.IsChecked == true;

            string selectedRepoUrl = GetSelectedRepoUrl();
            if (!selectedRepoUrl.EndsWith("/")) selectedRepoUrl += "/";

            // Hijack the console output
            var originalConsoleOut = Console.Out;
            Console.SetOut(new GuiConsoleWriter(LogToGui));

            try
            {
                await Task.Run(async () =>
                {
                    await Program.DownloadRepositoryFiles(targetDir, selectedRepoUrl, isSilent: false, token: _cancellationTokenSource.Token);

                    if (doZip && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        LogToGui("\n[*] Creating repo.zip using Store mode (No Compression)...");

                        string zipPath = Path.Combine(Directory.GetParent(targetDir)?.FullName ?? Environment.CurrentDirectory, "repo.zip");
                        if (File.Exists(zipPath)) File.Delete(zipPath);

                        // false explicitly zips the contents, not the root folder itself
                        ZipFile.CreateFromDirectory(targetDir, zipPath, CompressionLevel.NoCompression, false);
                        LogToGui("\n[*] Zip creation complete.\n");
                    }
                });

                LogToGui("[*] Mirror complete.\n");
                TxtStatus.Text = "Ready.";
                SyncProgressBar.Value = 100;
            }
            catch (Exception ex)
            {
                LogToGui($"[!] An error occurred: {ex.Message}\n");
                TxtStatus.Text = "Error.";
            }
            finally
            {
                // Always restore the original console output when finished
                Console.SetOut(originalConsoleOut);
                SetControlsEnabled(true);
            }
        }

        private async void BtnLocalHost_Click(object? sender, RoutedEventArgs e)
        {
            LogToGui("\n[*] Setting up Local Hosting...\n");
            SetControlsEnabled(false);
            TxtStatus.Text = "Extracting Apache & Syncing...";
            SyncProgressBar.Value = 0;

            string apacheZip = "apache-win.zip";
            string apachePath = Path.Combine(Environment.CurrentDirectory, "apache-win");
            string targetDir = Path.Combine(apachePath, "htdocs");
            string defaultRepoDir = Path.Combine(Environment.CurrentDirectory, "repo");

            _cancellationTokenSource = new CancellationTokenSource();

            string selectedRepoUrl = GetSelectedRepoUrl();
            if (!selectedRepoUrl.EndsWith("/")) selectedRepoUrl += "/";

            var originalConsoleOut = Console.Out;
            Console.SetOut(new GuiConsoleWriter(LogToGui));

            try
            {
                await Task.Run(async () =>
                {
                    if (!Directory.Exists(apachePath))
                    {
                        if (!File.Exists(apacheZip))
                        {
                            LogToGui($"[!] ERROR: Local hosting requires '{apacheZip}' to be present next to this app.\n");
                            return;
                        }

                        LogToGui($"[*] Extracting {apacheZip}...\n");
                        ZipFile.ExtractToDirectory(apacheZip, apachePath);
                        LogToGui("[*] Apache extraction complete.\n");
                    }
                    else
                    {
                        LogToGui("[*] Found existing apache-win folder, skipping extraction.\n");
                    }

                    // Smart Local Copy Check
                    if (Directory.Exists(defaultRepoDir))
                    {
                        LogToGui("[*] Found existing repo directory. Copying files to htdocs to speed up sync...\n");
                        Program.CopyDirectory(defaultRepoDir, targetDir);
                    }
                    else if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    LogToGui("[*] Commencing repository downloads...\n");
                    await Program.DownloadRepositoryFiles(targetDir, selectedRepoUrl, isSilent: false, token: _cancellationTokenSource.Token);

                    // Start Apache automatically for the user
                    LogToGui("\n[*] Starting Local Apache Server...\n");
                    string exeName = "httpd.exe";
                    string[] foundFiles = Directory.GetFiles(apachePath, exeName, SearchOption.AllDirectories);

                    if (foundFiles.Length > 0)
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = foundFiles[0],
                            WorkingDirectory = Path.GetDirectoryName(foundFiles[0]) ?? apachePath,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        _apacheProcess = Process.Start(psi);
                        LogToGui("[i] Apache Server is running at http://127.0.0.1\n");

                        // Switch the button text on the UI thread
                        Avalonia.Threading.Dispatcher.UIThread.Post(() => BtnToggleApache.Content = "Stop Apache Server");
                    }
                });

                LogToGui("[*] Setup complete.\n");
                TxtStatus.Text = "Ready.";
                SyncProgressBar.Value = 100;
            }
            catch (Exception ex)
            {
                LogToGui($"[!] An error occurred during Local Host setup: {ex.Message}\n");
                TxtStatus.Text = "Error.";
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
                SetControlsEnabled(true);
            }
        }

        private void BtnToggleApache_Click(object? sender, RoutedEventArgs e)
        {
            if (_apacheProcess != null && !_apacheProcess.HasExited)
            {
                // Toggle Off (Stop)
                _apacheProcess.Kill();
                _apacheProcess.Dispose();
                _apacheProcess = null;

                BtnToggleApache.Content = "Start Apache Server";
                LogToGui("\n[*] Apache server stopped.\n");
            }
            else
            {
                // Toggle On (Start)
                string apachePath = Path.Combine(Environment.CurrentDirectory, "apache-win");
                string exeName = "httpd.exe";
                string[] foundFiles = Directory.Exists(apachePath) ? Directory.GetFiles(apachePath, exeName, SearchOption.AllDirectories) : Array.Empty<string>();

                if (foundFiles.Length > 0)
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = foundFiles[0],
                        WorkingDirectory = Path.GetDirectoryName(foundFiles[0]) ?? apachePath,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    try
                    {
                        _apacheProcess = Process.Start(psi);
                        BtnToggleApache.Content = "Stop Apache Server";
                        LogToGui("\n[i] Apache Server is running at http://127.0.0.1\n");
                    }
                    catch (Exception ex)
                    {
                        LogToGui($"\n[!] Failed to start Apache: {ex.Message}\n");
                    }
                }
                else
                {
                    LogToGui("\n[!] Apache not found. Please run 'Fetch Repo for Local Hosting' first to download and extract Apache.\n");
                }
            }
        }

        private async void BtnService_Click(object? sender, RoutedEventArgs e)
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppContext.BaseDirectory, "SimToolsRepoMaker.exe");
            string serviceName = "SimToolsRepoSync";
            bool isInstalled = false;

            try
            {
                // 1. Scan to see if the service is already installed
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sc.exe",
                            Arguments = $"query {serviceName}",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    isInstalled = output.Contains(serviceName);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    isInstalled = File.Exists($"/etc/systemd/system/{serviceName}.service");
                }
                else
                {
                    LogToGui("\n[!] Background service management is only supported on Windows and Linux.\n");
                    return;
                }

                // 2. Act based on the scanned status
                if (isInstalled)
                {
                    bool remove = await ShowConfirmDialog("Service Installed", "The background auto-sync service is currently installed.\n\nWould you like to remove it?");

                    if (remove)
                    {
                        LogToGui("\n[*] Uninstalling background auto-sync service...\n");
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            Process.Start("sc.exe", $"stop {serviceName}")?.WaitForExit();
                            Process.Start("sc.exe", $"delete {serviceName}")?.WaitForExit();
                            LogToGui("[*] Windows Service uninstalled successfully.\n");
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Process.Start("systemctl", $"stop {serviceName}")?.WaitForExit();
                            Process.Start("systemctl", $"disable {serviceName}")?.WaitForExit();
                            File.Delete($"/etc/systemd/system/{serviceName}.service");
                            Process.Start("systemctl", "daemon-reload")?.WaitForExit();
                            LogToGui("[*] Linux systemd service uninstalled successfully.\n");
                        }
                    }
                    else
                    {
                        LogToGui("\n[*] Service removal cancelled by user.\n");
                    }
                }
                else
                {
                    LogToGui("\n[*] Attempting to install background auto-sync service...\n");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Process.Start("sc.exe", $"create {serviceName} binPath= \"{exePath} --daemon\" start= auto")?.WaitForExit();
                        Process.Start("sc.exe", $"start {serviceName}")?.WaitForExit();
                        LogToGui("[*] Windows Service installed and started successfully.\n");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        string svcContent = $"[Unit]\nDescription=SimTools Repo Sync\nAfter=network.target\n\n[Service]\nExecStart={exePath} --daemon\nRestart=always\n\n[Install]\nWantedBy=multi-user.target";
                        File.WriteAllText($"/etc/systemd/system/{serviceName}.service", svcContent);
                        Process.Start("systemctl", "daemon-reload")?.WaitForExit();
                        Process.Start("systemctl", $"enable {serviceName}")?.WaitForExit();
                        Process.Start("systemctl", $"start {serviceName}")?.WaitForExit();
                        LogToGui("[*] Linux systemd service installed and started successfully.\n");
                    }
                }
            }
            catch (Exception ex)
            {
                LogToGui($"\n[!] Error managing service. Ensure you are running the application as Administrator/Root: {ex.Message}\n");
            }
        }
    }
}