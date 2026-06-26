// SimTools
// Main Application
// Download Helper Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System.Windows;

namespace SimTools;

public partial class DownloadProgressWindow : Window
{
    public DownloadProgressWindow(string fileName = "")
    {
        InitializeComponent();
        ApplyLanguage();

        if (!string.IsNullOrEmpty(fileName))
        {
            string downloadMessage = LanguageManager.Format("DownloadProgress", "DownloadingFile", fileName);

            Title = downloadMessage;
            StatusText.Text = downloadMessage;
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
            PercentText.Text = string.Empty;
        });
    }

        // Called on load and by SettingsWindow after a language change
    public void ApplyLanguage()
    {
        // ── RTL / LTR layout direction ─────────────────────────────────────
        var lang = IniHelper.Read("Language", "SelectedLanguage", "en");

        // Add any future RTL languages to this set
        var rtlLanguages = new HashSet<string> { "ar" };
        FlowDirection = rtlLanguages.Contains(lang)
        ? System.Windows.FlowDirection.RightToLeft
        : System.Windows.FlowDirection.LeftToRight;

        // ── Text strings ───────────────────────────────────────────────────
        StatusText.Text = LanguageManager.Get("DownloadProgress", "StatusText1", StatusText.Text);
    }
}
