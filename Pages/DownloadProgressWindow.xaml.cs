using System.Windows;

namespace SimTools;

public partial class DownloadProgressWindow : Window
{
    public DownloadProgressWindow(string fileName = "")
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(fileName))
        {
            Title            = $"Downloading: {fileName}";
            StatusText.Text  = $"Downloading: {fileName}";
        }
    }

    /// <summary>Updates the progress bar and labels. Safe to call from any thread.</summary>
    public void UpdateProgress(int percent)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value           = percent;
            PercentText.Text            = $"{percent}%";
        });
    }

    /// <summary>Switches to indeterminate (marquee) mode while content-length is unknown.</summary>
    public void SetIndeterminate()
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBar.IsIndeterminate = true;
            PercentText.Text            = string.Empty;
        });
    }
}
