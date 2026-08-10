using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimTools
{
    public class RepoSyncWorker : BackgroundService
    {
        // Set your desired sync interval (e.g., check for updates every 15 minutes)
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string targetDir = Path.Combine(AppContext.BaseDirectory, "repo");
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    // Execute the download logic silently, pulling from the default Base URL
                    await Program.DownloadRepositoryFiles(targetDir, Program.DefaultBaseRepoUrl, isSilent: true, stoppingToken);
                }
                catch (Exception)
                {
                    // In a production service, you might want to log to a file or Event Viewer here
                }

                // Wait for the next cycle, but wake up immediately if the OS stops the service
                await Task.Delay(_syncInterval, stoppingToken);
            }
        }
    }
}