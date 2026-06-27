// SimTools
// Main Application
// Support SimTools Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

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
            ApplyLanguage();
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
                LanguageManager.Format("Main", "Error_OpenLink", ex.Message),
                LanguageManager.Get("Main", "Error_Title", "Error"),
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

        private void UnlockPersonalizationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UnlockPersonalizationDialog { Owner = this };
            dialog.ShowDialog();
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
            SupportText.Text = LanguageManager.Get("SupportPage", "SupportText1", SupportText.Text);
            FiscalSupport.Text = LanguageManager.Get("SupportPage", "FiscalSupport", FiscalSupport.Text);
            FiscalSupport2.Text = LanguageManager.Get("SupportPage", "FiscalSupport2", FiscalSupport2.Text);
            Contribute.Text = LanguageManager.Get("SupportPage", "Contribute", Contribute.Text);
            Contribute2.Text = LanguageManager.Get("SupportPage", "Contribute2", Contribute2.Text);
            RepoMakerButton.Content = LanguageManager.Get("SupportPage", "RepoMaker", "Repo Maker");
            TranslationButton.Content = LanguageManager.Get("SupportPage", "TranslationTool", "Translation Tool");
            UnlockPersonalizationButton.Content = LanguageManager.Get("SupportPage", "Personalize", "Unlock Personalization");
        }

        private void AdvertiseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == "AdvertiseButton")
            {
                OpenUrl("https://simtools-app.com/advertise-through-simtools");
            }
        }
    }    
}
