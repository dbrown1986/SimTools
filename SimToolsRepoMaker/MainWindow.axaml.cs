using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

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

        public MainWindow()
        {
            InitializeComponent();

            // Set a default output directory on startup
            TxtOutputDir.Text = Path.Combine(Environment.CurrentDirectory, "repo");
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
                BtnBrowse.IsEnabled = isEnabled;
            });
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

            // Hijack the console output
            var originalConsoleOut = Console.Out;
            Console.SetOut(new GuiConsoleWriter(LogToGui));

            try
            {
                await Task.Run(async () =>
                {
                    // isSilent is now FALSE so the output flows into our custom GuiConsoleWriter
                    await Program.DownloadRepositoryFiles(targetDir, isSilent: false, token: _cancellationTokenSource.Token);
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

            _cancellationTokenSource = new CancellationTokenSource();

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

                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    LogToGui("[*] Commencing repository downloads...\n");
                    await Program.DownloadRepositoryFiles(targetDir, isSilent: false, token: _cancellationTokenSource.Token);
                });

                LogToGui("[*] Downloads complete. To launch the Apache server, please run the application in CLI mode.\n");
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

        private void BtnService_Click(object? sender, RoutedEventArgs e)
        {
            LogToGui("\n[*] Attempting to install background auto-sync service...\n");

            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppContext.BaseDirectory, "SimToolsRepoMaker.exe");
            string serviceName = "SimToolsRepoSync";

            try
            {
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
                else
                {
                    LogToGui("[!] Background service installation is only supported on Windows and Linux.\n");
                }
            }
            catch (Exception ex)
            {
                LogToGui($"[!] Error managing service. Ensure you are running as Administrator/Root: {ex.Message}\n");
            }
        }
    }
}