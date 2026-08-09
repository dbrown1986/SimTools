using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Avalonia;
using SimTools;

public static class SecurityProtocolHelper
{
    public static void EnableModernSecurityProtocols()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    }
}

class Program
{
    private const string BaseRepoUrl = "https://us1-repo.simtools-app.com/";
    private const string ApacheZipName = "apache-win.zip";
    private const string ApacheExtractDir = "apache-win";

    // Windows API hook to restore the console output for --cli mode when using WinExe OutputType
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);
    private const int ATTACH_PARENT_PROCESS = -1;

    [STAThread]
    static async Task Main(string[] args)
    {
        SecurityProtocolHelper.EnableModernSecurityProtocols();

        // MODE 1: Headless Daemon (Background Service)
        if (args.Length > 0 && args[0].Equals("--daemon", StringComparison.OrdinalIgnoreCase))
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options => options.ServiceName = "SimToolsRepoSync")
                .UseSystemd()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<RepoSyncWorker>();
                })
                .Build();

            await host.RunAsync();
            return;
        }

        // MODE 2: Interactive CLI
        if (args.Length > 0 && args[0].Equals("--cli", StringComparison.OrdinalIgnoreCase))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
            }

            await RunCliModeAsync();
            return;
        }

        // MODE 3: Avalonia GUI (Default)
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    // The interactive CLI loop moved into its own method
    private static async Task RunCliModeAsync()
    {
        Console.WriteLine("\n[ SimTools CLI Mode Started ]");

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=======================================================");
            Console.WriteLine("              SimTools Repository Builder              ");
            Console.WriteLine("=======================================================");
            Console.WriteLine();
            Console.WriteLine("[1] Fetch repo for mirroring (Downloads to .\\repo)");
            Console.WriteLine("[2] Fetch repo for local hosting (Downloads to htdocs)");
            Console.WriteLine("[3] Manage Background Auto-Sync Service");
            Console.WriteLine("[4] Exit");
            Console.WriteLine();
            Console.WriteLine("=======================================================");
            Console.Write("Enter your choice (1-4): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await HandleMirroringMode();
                    break;
                case "2":
                    await HandleLocalHostingMode();
                    break;
                case "3":
                    ManageServiceMode();
                    break;
                case "4":
                    return;
                default:
                    continue;
            }
        }
    }

    private static void ManageServiceMode()
    {
        Console.Clear();
        Console.WriteLine("=======================================================");
        Console.WriteLine("              Manage Background Service                ");
        Console.WriteLine("=======================================================");
        Console.WriteLine("[1] Install Auto-Sync Service");
        Console.WriteLine("[2] Uninstall Auto-Sync Service");
        Console.WriteLine("[3] Return to Menu");
        Console.WriteLine("=======================================================");
        Console.Write("Choice: ");

        string? choice = Console.ReadLine();
        string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(AppContext.BaseDirectory, "SimToolsRepoMaker.exe");
        string serviceName = "SimToolsRepoSync";

        try
        {
            if (choice == "1")
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("sc.exe", $"create {serviceName} binPath= \"{exePath} --daemon\" start= auto")?.WaitForExit();
                    Process.Start("sc.exe", $"start {serviceName}")?.WaitForExit();
                    Console.WriteLine("\n[*] Windows Service installed and started successfully.");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string svcContent = $"[Unit]\nDescription=SimTools Repo Sync\nAfter=network.target\n\n[Service]\nExecStart={exePath} --daemon\nRestart=always\n\n[Install]\nWantedBy=multi-user.target";
                    File.WriteAllText($"/etc/systemd/system/{serviceName}.service", svcContent);
                    Process.Start("systemctl", "daemon-reload")?.WaitForExit();
                    Process.Start("systemctl", $"enable {serviceName}")?.WaitForExit();
                    Process.Start("systemctl", $"start {serviceName}")?.WaitForExit();
                    Console.WriteLine("\n[*] systemd service installed and started successfully.");
                }
            }
            else if (choice == "2")
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("sc.exe", $"stop {serviceName}")?.WaitForExit();
                    Process.Start("sc.exe", $"delete {serviceName}")?.WaitForExit();
                    Console.WriteLine("\n[*] Windows Service uninstalled.");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("systemctl", $"stop {serviceName}")?.WaitForExit();
                    Process.Start("systemctl", $"disable {serviceName}")?.WaitForExit();
                    if (File.Exists($"/etc/systemd/system/{serviceName}.service"))
                        File.Delete($"/etc/systemd/system/{serviceName}.service");
                    Process.Start("systemctl", "daemon-reload")?.WaitForExit();
                    Console.WriteLine("\n[*] systemd service uninstalled.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[!] Error managing service. Ensure you are running as Administrator/Root: {ex.Message}");
            Console.ResetColor();
        }

        if (choice == "1" || choice == "2")
        {
            Console.WriteLine("\nPress Enter to return to the menu...");
            Console.ReadLine();
        }
    }

    private static async Task HandleMirroringMode()
    {
        Console.Clear();
        Console.WriteLine("=======================================================");
        Console.WriteLine("            Option 1: Fetch Repo for Mirroring          ");
        Console.WriteLine("=======================================================");
        Console.WriteLine();

        string targetDir = Path.Combine(Environment.CurrentDirectory, "repo");
        string zipPath = Path.Combine(Environment.CurrentDirectory, "repo.zip");

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        await DownloadRepositoryFiles(targetDir);

        Console.Write("\n[*] Do you want to compress the downloaded repo into a zip file? (Y/N): ");
        if (Console.ReadLine()?.Trim().ToUpper() == "Y")
        {
            Console.WriteLine("\n[*] Creating repo.zip using Store mode (No Compression)...");
            if (File.Exists(zipPath)) File.Delete(zipPath);

            ZipFile.CreateFromDirectory(targetDir, zipPath, CompressionLevel.NoCompression, false);
            Console.WriteLine("[*] Zip creation complete.");
        }

        Console.WriteLine("\n=======================================================");
        Console.WriteLine("                    SUCCESSFUL MIRROR                   ");
        Console.WriteLine("=======================================================");
        Console.WriteLine("[i] You can now upload 'repo.zip' (if created) or the");
        Console.WriteLine("     individual files to your hosting provider.");
        Console.WriteLine();
        Console.WriteLine("[i] Consider submitting your new mirror link to:");
        Console.WriteLine("     https://simtools-app.com/repo-submission");
        Console.WriteLine("=======================================================");
        Console.WriteLine("\nPress Enter to return to the menu...");
        Console.ReadLine();
    }

    private static async Task HandleLocalHostingMode()
    {
        Console.Clear();
        Console.WriteLine("=======================================================");
        Console.WriteLine("          Option 2: Fetch Repo for Local Hosting        ");
        Console.WriteLine("=======================================================");
        Console.WriteLine();

        string apachePath = Path.Combine(Environment.CurrentDirectory, ApacheExtractDir);
        string targetDir = Path.Combine(apachePath, "htdocs");

        if (!Directory.Exists(apachePath))
        {
            if (!File.Exists(ApacheZipName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Local hosting requires '{ApacheZipName}' to be present next to this app.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"[*] Extracting {ApacheZipName} to .\\{ApacheExtractDir}...");
            try
            {
                ZipFile.ExtractToDirectory(ApacheZipName, apachePath);
                Console.WriteLine("[*] Apache extraction complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to extract Apache: {ex.Message}");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }
        }
        else
        {
            Console.WriteLine($"[*] Found existing .\\{ApacheExtractDir} folder, skipping extraction.");
        }

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        Console.WriteLine("\n[*] Commencing repository downloads...");
        await DownloadRepositoryFiles(targetDir);
        Console.WriteLine("[*] All repository files have finished downloading.");

        Console.Write("\n[*] Do you want to run the local Apache server now? (Y/N): ");
        if (Console.ReadLine()?.Trim().ToUpper() == "Y")
        {
            Console.WriteLine("\n[*] Locating Apache binary (httpd.exe)...");

            string exeName = "httpd.exe";
            string[] foundFiles = Directory.GetFiles(apachePath, exeName, SearchOption.AllDirectories);

            if (foundFiles.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"ERROR: {exeName} was not found anywhere inside .\\{ApacheExtractDir}");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            string foundExePath = foundFiles[0];
            string? binWorkingDirectory = Path.GetDirectoryName(foundExePath);
            if (string.IsNullOrEmpty(binWorkingDirectory))
            {
                binWorkingDirectory = apachePath;
            }

            Console.WriteLine("\n[*] Launching Apache HTTP Server...");
            Console.WriteLine("=======================================================");
            Console.WriteLine("[i] SimTools Local Repo is active at http://127.0.0.1");
            Console.WriteLine("[i] Point your SimTools Repo URL settings to 127.0.0.1");
            Console.WriteLine();
            Console.WriteLine("[!] CLOSE THE WINDOW OR PRESS [Ctrl + C] TO STOP THE SERVER");
            Console.WriteLine("=======================================================");
            Console.WriteLine();

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = foundExePath;
            psi.WorkingDirectory = binWorkingDirectory;

            try
            {
                using (Process? apacheProcess = Process.Start(psi))
                {
                    if (apacheProcess != null)
                    {
                        await apacheProcess.WaitForExitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start Apache: {ex.Message}");
                Console.ReadLine();
            }
        }
    }

    public static async Task DownloadRepositoryFiles(string rootTargetDir, bool isSilent = false, CancellationToken token = default)
    {
        if (!isSilent) Console.WriteLine("\n[*] Fetching dynamic file manifest from the master server...");

        string manifestData = string.Empty;

        try
        {
            // Pull the raw text output from the PHP index file
            manifestData = await SecureWebClient.GetStringAsync(BaseRepoUrl);
        }
        catch (Exception ex)
        {
            if (!isSilent)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[!] Failed to download remote manifest: {ex.Message}");
                Console.ResetColor();
            }
            return;
        }

        // Split the plain text into individual URLs
        string[] lines = manifestData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (!isSilent) Console.WriteLine($"[*] Found {lines.Length} total files to process.\n");

        int downloadCount = 0;
        int skipCount = 0;
        int deleteCount = 0;

        // Keep track of all the files we actually expect to exist
        HashSet<string> expectedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Local helper function to constantly overwrite the same console line
        void UpdateStatusLine(string message)
        {
            if (isSilent) return;
            // Pad out to 79 characters to erase previous text, preventing visual ghosting 
            if (message.Length > 79) message = message.Substring(0, 76) + "...";
            Console.Write($"\r{message.PadRight(79)}");
        }

        foreach (string line in lines)
        {
            if (token.IsCancellationRequested) break;

            string urlStr = line.Trim();
            if (string.IsNullOrEmpty(urlStr) || urlStr.StartsWith("<")) continue; // Skip empty lines or stray HTML

            string escapedUrl = urlStr.Replace(" ", "%20");

            if (!Uri.TryCreate(escapedUrl, UriKind.Absolute, out Uri? validUri) || validUri == null)
            {
                skipCount++;
                continue;
            }

            string absolutePath = validUri.AbsolutePath;

            string localRelativePath = absolutePath.TrimStart('/');
            localRelativePath = Uri.UnescapeDataString(localRelativePath);

            // Normalize directory separators for the current OS so our Hashset matches later
            string normalizedRelativePath = localRelativePath.Replace('/', Path.DirectorySeparatorChar);
            expectedFiles.Add(normalizedRelativePath);

            string destinationPath = Path.Combine(rootTargetDir, normalizedRelativePath);
            string? destinationFolder = Path.GetDirectoryName(destinationPath);

            if (!string.IsNullOrEmpty(destinationFolder) && !Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // --- SYNCHRONIZATION CHECK ---
            if (File.Exists(destinationPath))
            {
                var localFile = new FileInfo(destinationPath);
                var meta = await SecureWebClient.GetFileMetadataAsync(urlStr);

                // If remote content length matches the local file exactly, skip the download
                if (meta.ContentLength.HasValue && meta.ContentLength.Value == localFile.Length)
                {
                    UpdateStatusLine($"Skipped (Up-to-date): {localRelativePath}");
                    skipCount++;
                    continue;
                }
            }

            try
            {
                if (isSilent)
                {
                    await SecureWebClient.DownloadFileAsync(urlStr, destinationPath);
                }
                else
                {
                    // Shorten the display name so it fits nicely on the screen with the bar
                    string displayPath = localRelativePath;
                    if (displayPath.Length > 30) displayPath = "..." + displayPath.Substring(displayPath.Length - 27);
                    else displayPath = displayPath.PadRight(30);

                    // Download utilizing the real-time progress callbacks
                    await SecureWebClient.DownloadFileWithProgressAsync(
                        urlStr,
                        destinationPath,
                        onProgress: (pct) =>
                        {
                            int filled = (pct * 20) / 100;
                            string bar = new string('█', filled).PadRight(20, '-');
                            UpdateStatusLine($"Downloading: {displayPath} [{bar}] {pct,3}%");
                        },
                        onIndeterminate: () =>
                        {
                            UpdateStatusLine($"Downloading: {displayPath} [   Processing...    ] ---%");
                        },
                        onHeadersParsed: (headers) => { }
                    );
                }
                downloadCount++;
            }
            catch (Exception ex)
            {
                if (!isSilent)
                {
                    Console.WriteLine(); // Drop down a line so the error doesn't get overwritten
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" [!] Failed to download {urlStr}: {ex.Message}");
                    Console.ResetColor();
                }
                skipCount++;
            }
        }

        if (!isSilent) Console.WriteLine(); // Drop down a line before cleanup begins

        // --- ORPHAN CLEANUP ---
        if (!token.IsCancellationRequested && Directory.Exists(rootTargetDir))
        {
            if (!isSilent) Console.WriteLine("[*] Checking for orphaned local files to remove...");

            string[] localFiles = Directory.GetFiles(rootTargetDir, "*", SearchOption.AllDirectories);

            foreach (string localFile in localFiles)
            {
                // Compare the local file path to the root target directory to get its relative path
                string relativeToRoot = Path.GetRelativePath(rootTargetDir, localFile);

                // If this file wasn't in the master PHP list, it's an orphan. Delete it.
                if (!expectedFiles.Contains(relativeToRoot))
                {
                    try
                    {
                        UpdateStatusLine($"Deleting orphaned file: {relativeToRoot}");
                        File.Delete(localFile);
                        deleteCount++;
                    }
                    catch (Exception ex)
                    {
                        if (!isSilent)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($" [!] Failed to delete orphaned file {relativeToRoot}: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        // --- GENERATE LOCAL PHP MANIFEST ---
        if (!token.IsCancellationRequested && Directory.Exists(rootTargetDir))
        {
            if (!isSilent) UpdateStatusLine("[*] Generating local index.php manifest script...");

            string phpScriptPath = Path.Combine(rootTargetDir, "index.php");
            string phpScriptContent = @"<?php
header('Content-Type: text/plain');
$iterator = new RecursiveIteratorIterator(new RecursiveDirectoryIterator(__DIR__));

foreach ($iterator as $file) {
    if ($file->isFile() && $file->getFilename() !== 'index.php' && $file->getFilename() !== '.htaccess') {
        $path = str_replace('\\', '/', $file->getPathname());
        $relativePath = str_replace(__DIR__ . '/', '', $path);
        
        $protocol = isset($_SERVER['HTTPS']) && $_SERVER['HTTPS'] === 'on' ? 'https' : 'http';
        $host = $_SERVER['HTTP_HOST'];
        $baseDir = rtrim(dirname($_SERVER['SCRIPT_NAME']), '/\\');
        
        echo $protocol . '://' . $host . $baseDir . '/' . $relativePath . ""\n"";
    }
}
?>";
            try
            {
                // Write the file (overwrites if it already exists to ensure it is up-to-date)
                File.WriteAllText(phpScriptPath, phpScriptContent);
                // We need to add this to our expected files so the Orphan Cleanup doesn't delete it next time!
                expectedFiles.Add("index.php");
            }
            catch (Exception ex)
            {
                if (!isSilent)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" [!] Failed to create local index.php: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        if (!isSilent)
        {
            Console.WriteLine(); // Final line drop
            Console.WriteLine($"[*] Sync phase complete. Downloaded: {downloadCount}, Skipped: {skipCount}, Deleted: {deleteCount}");
        }
    }
}