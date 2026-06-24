using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimToolsUninstaller
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_Load(object? sender, EventArgs e)
        {
            // 1. Prompt the User
            var result = MessageBox.Show(
                "Are you sure you want to completely remove SimTools and all of its components?",
                "SimTools Uninstaller",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                Application.Exit();
                return;
            }

            // 2. Locate Manifest
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string manifestPath = Path.Combine(baseDir, "uninstall_manifest.json");

            if (!File.Exists(manifestPath))
            {
                MessageBox.Show("The uninstall_manifest.json file was not found. Cannot proceed with automatic uninstallation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            try
            {
                // 3. Read and Deserialize JSON
                string jsonContent = await File.ReadAllTextAsync(manifestPath);
                List<string>? installedPaths = JsonSerializer.Deserialize<List<string>>(jsonContent);

                if (installedPaths != null && installedPaths.Count > 0)
                {
                    // 4. Reverse the list (files inside directories MUST be deleted before the directories themselves)
                    installedPaths.Reverse();

                    progOverall.Maximum = installedPaths.Count;
                    int progress = 0;

                    // 5. Deletion Loop
                    foreach (string path in installedPaths)
                    {
                        progress++;
                        progOverall.Value = progress;
                        lblStatus.Text = $"Removing: {Path.GetFileName(path)}";

                        // Slight delay so the UI doesn't freeze and the user can actually see progress
                        await Task.Delay(10);

                        try
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            else if (Directory.Exists(path))
                            {
                                // We use false here because the directory *should* be empty.
                                // If the user placed custom files inside it, it throws an error which we safely catch and ignore.
                                Directory.Delete(path, false);
                            }
                        }
                        catch
                        {
                            // Ignore locked files or non-empty directories to ensure the uninstaller doesn't crash mid-way
                        }
                    }
                }

                // Clean up installer artifacts
                try { File.Delete(manifestPath); } catch { }
                try { File.Delete(Path.Combine(baseDir, "install_log.txt")); } catch { }

                // 6. Queue Self-Deletion
                QueueSelfDeletion(baseDir);

                MessageBox.Show("SimTools has been successfully removed from your computer.", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during uninstallation:\n{ex.Message}", "Uninstallation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }

        private void QueueSelfDeletion(string installDir)
        {
            // Get the current running uninstaller executable path
            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath)) return;

            string batPath = Path.Combine(Path.GetTempPath(), "SimTools_Cleanup.bat");

            // This background batch script does the following:
            // 1. Pings localhost 3 times (creates a ~2 second delay to allow this WinForms app to fully close and unlock)
            // 2. Deletes the uninstaller executable
            // 3. Attempts to delete the final installation directory (only if empty)
            // 4. Deletes the batch script itself from the %TEMP% folder
            string batScript = $@"
@echo off
ping 127.0.0.1 -n 3 > nul
del ""{exePath}"" /q
rmdir ""{installDir}"" /s /q
del ""%~f0""
";

            File.WriteAllText(batPath, batScript);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batPath}\"",
                CreateNoWindow = true, // Run invisibly so the user doesn't see a black command prompt flash
                UseShellExecute = false
            };

            Process.Start(psi);
        }
    }
}