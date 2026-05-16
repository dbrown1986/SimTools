using System.Windows;
using System.Windows.Controls;

// Aliases to resolve every ambiguity cleanly
using WpfColor = System.Windows.Media.Color;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfBrush = System.Windows.Media.SolidColorBrush;
using WpfPBar = System.Windows.Controls.ProgressBar;

namespace SimTools
{
    public class DownloadProgressWindow : Window
    {
        private readonly TextBlock _statusText;
        private readonly WpfPBar _progressBar;
        private readonly TextBlock _percentText;

        public DownloadProgressWindow(string fileName)
        {
            var downloading = LanguageManager.Get("Download", "Window_Title", "Downloading");
            Title = $"{downloading} {fileName}...";
            Height = 130;
            Width = 380;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new WpfBrush(WpfColor.FromRgb(30, 30, 30));

            _statusText = new TextBlock
            {
                Text = $"{downloading} {fileName}...",
                FontSize = 13,
                Foreground = WpfBrushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };

            _progressBar = new WpfPBar
            {
                Height = 22,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Margin = new Thickness(0, 0, 0, 6)
            };

            _percentText = new TextBlock
            {
                Text = "0%",
                FontSize = 11,
                Foreground = WpfBrushes.Gray,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            var panel = new StackPanel { Margin = new Thickness(16) };
            panel.Children.Add(_statusText);
            panel.Children.Add(_progressBar);
            panel.Children.Add(_percentText);

            Content = panel;
        }

        public void UpdateProgress(int percent)
        {
            _progressBar.IsIndeterminate = false;
            _progressBar.Value = percent;
            _percentText.Text = $"{percent}%";

            if (percent >= 100)
                _statusText.Text = LanguageManager.Get("Download", "Progress_Finalising", "Finalising...");
        }

        public void SetIndeterminate()
        {
            _progressBar.IsIndeterminate = true;
            _percentText.Text = "…";
        }
    }
}