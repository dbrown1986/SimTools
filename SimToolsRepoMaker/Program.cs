using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using SimTools;

public static class SecurityProtocolHelper
{
    public static void EnableModernSecurityProtocols()
    {
        // 3072 is the numerical value for SecurityProtocolType.Tls12. 
        // Casting it allows old frameworks (.NET 4.0) to compile it even if the enum isn't defined natively.
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    }
}

class Program
{
    private const string ListFileName = "recursive_list.txt";
    private const string ApacheZipName = "apache-win.zip";
    private const string ApacheExtractDir = "apache-win";

    static async Task Main(string[] args)
    {
        if (!File.Exists(ListFileName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {ListFileName} not found in the current directory.");
            Console.ResetColor();
            Console.ReadLine();
            return;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=======================================================");
            Console.WriteLine("              SimTools Repository Builder              ");
            Console.WriteLine("=======================================================");
            Console.WriteLine();
            Console.WriteLine("[1] Fetch repo for mirroring (Downloads to .\\repo and zips)");
            Console.WriteLine("[2] Fetch repo for local hosting (Extracts Apache, downloads to htdocs)");
            Console.WriteLine("[3] Exit");
            Console.WriteLine();
            Console.WriteLine("=======================================================");
            Console.Write("Enter your choice (1-3): ");

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
                    return;
                default:
                    continue;
            }
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

        Console.WriteLine("\n[*] Creating repo.zip using Store mode (No Compression)...");
        if (File.Exists(zipPath)) File.Delete(zipPath);

        ZipFile.CreateFromDirectory(targetDir, zipPath, CompressionLevel.NoCompression, false);

        Console.WriteLine("\n=======================================================");
        Console.WriteLine("                    SUCCESSFUL MIRROR                   ");
        Console.WriteLine("=======================================================");
        Console.WriteLine("[i] You can now upload 'repo.zip' to your hosting ");
        Console.WriteLine("     provider and unzip it, or upload the individual ");
        Console.WriteLine("     files inside the 'repo' directory.");
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

    private static async Task DownloadRepositoryFiles(string rootTargetDir)
    {
        Console.WriteLine("[*] Reading manifest file...");
        string[] lines = await File.ReadAllLinesAsync(ListFileName);
        Console.WriteLine($"[*] Found {lines.Length} total lines to process.");

        int downloadCount = 0;
        int skipCount = 0;

        foreach (string line in lines)
        {
            string urlStr = line.Trim();

            if (string.IsNullOrEmpty(urlStr))
            {
                continue;
            }

            // Automatically repair any missing prefix scheme safely
            if (urlStr.StartsWith("://"))
            {
                urlStr = "https" + urlStr;
            }

            // Handle accidental space escaping formatting bugs cleanly
            string escapedUrl = urlStr.Replace(" ", "%20");

            if (!Uri.TryCreate(escapedUrl, UriKind.Absolute, out Uri? validUri) || validUri == null)
            {
                skipCount++;
                continue;
            }

            string absolutePath = validUri.AbsolutePath;

            if (absolutePath == "/" || absolutePath.EndsWith("/"))
            {
                skipCount++;
                continue;
            }

            // Isolate the pure asset filename
            string fileNamePart = Path.GetFileName(absolutePath);

            // Filter structural folders without extension types out of processing
            if (string.IsNullOrEmpty(fileNamePart) || !fileNamePart.Contains('.'))
            {
                skipCount++;
                continue;
            }

            string localRelativePath = absolutePath.TrimStart('/');
            localRelativePath = Uri.UnescapeDataString(localRelativePath);

            string destinationPath = Path.Combine(rootTargetDir, localRelativePath);
            string? destinationFolder = Path.GetDirectoryName(destinationPath);

            if (!string.IsNullOrEmpty(destinationFolder) && !Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            try
            {
                Console.WriteLine($"Downloading: {localRelativePath}");

                // Replace the HttpClient block with SecureWebClient
                await SecureWebClient.DownloadFileAsync(urlStr, destinationPath);

                downloadCount++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" [!] Failed to download {urlStr}: {ex.Message}");
                Console.ResetColor();
                skipCount++;
            }
        }

        Console.WriteLine($"\n[*] Download phase complete. Success: {downloadCount}, Skipped: {skipCount}");
    }
}