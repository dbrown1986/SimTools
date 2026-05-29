using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
// Add these two aliases to resolve the ambiguity:
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    /// <summary>
    /// Interaction logic for SupportSimTools.xaml
    /// </summary>
    public partial class SupportSimTools : Window
    {
        public SupportSimTools()
        {
            InitializeComponent();
        }

        // Helper method to open URLs in the default web browser
        private static void OpenUrl(string url)
        {
            try
            {
                // Let's load the URL
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            // URL not found or there was an error contacting the internet.
            catch (Exception ex)
            {
                MessageBox.Show(
                LanguageManager.Format("Messages", "Error_OpenLink", ex.Message),
                LanguageManager.Get("Messages", "Error_Title", "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PayPalButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "PayPalButton")
            {
                OpenUrl("https://www.paypal.com/donate/?hosted_button_id=VJZMHREBUFSC4");
            }
        }

        private void CashAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "CashAppButton")
            {
                OpenUrl("https://cash.app/f/POOL?id=mu2otu1w");
            }
        }

        private void BMACButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "BMACButton")
            {
                OpenUrl("https://buymeacoffee.com/dbrown1986");
            }
        }

        private void PatreonButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "PatreonButton")
            {
                OpenUrl("https://www.patreon.com/cw/SimTools");
            }
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "GitHubButton")
            {
                OpenUrl("https://github.com/dbrown1986/SimTools");
            }
        }

        private void RepoMakerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "RepoMakerButton")
            {
                OpenUrl("https://simtools-app.com/st-repo-maker");
            }
        }

        private void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "TranslationButton")
            {
                OpenUrl("https://simtools-app.com/st-translator");
            }
        }
    }
}
