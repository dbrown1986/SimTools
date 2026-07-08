// SimTools
// Main Application
// Support SimTools Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

// Resolve type ambiguities between WPF UI elements and Backend namespaces
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

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
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── XAML Expected Event Handlers ─────────────────────────────────────

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PayPalButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://simtools-app.com/donate");
        }

        private void CashAppButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://simtools-app.com/donate"); // Replace with specific CashApp URL if necessary
        }

        private void BMACButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://simtools-app.com/donate"); // Replace with specific BuyMeACoffee URL if necessary
        }

        private void PatreonButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.patreon.com/SimTools");
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Archeon-Industries"); // Replace with specific repository/sponsor URL if necessary
        }

        // ── Original Utility Feature Handlers ────────────────────────────────

        private void UnlockPersonalizationButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://simtools-app.com/unlock-personalization");
        }

        private void RepoMakerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = Path.Combine(baseDir, "SimToolsRepoMaker.exe");

                if (File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = baseDir,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.Get("SupportPage", "RepoMakerNotFound", "The Repo Maker Utility was not found in your installation directory.\n\nPlease re-run the SimTools installer and verify that the 'Also Install Repo Maker Utility' option is checked."),
                        LanguageManager.Get("SupportPage", "ToolNotFound_Title", "Tool Not Found"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.Get("SupportPage", "UnableToLaunchRM", $"Could not launch Repo Maker: {ex.Message}"),
                    LanguageManager.Get("SupportPage", "UnableToLaunchRM_Title", "Execution Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string exePath = Path.Combine(baseDir, "SimToolsLanguageTool.exe");

                if (File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = baseDir,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.Get("SupportPage", "LanguageToolNotFound", "The Language Tool was not found in your installation directory.\n\nPlease re-run the SimTools installer and verify that the 'Also Install Language Tool' option is checked."),
                        LanguageManager.Get("SupportPage", "ToolNotFound_Title", "Tool Not Found"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LanguageManager.Get("SupportPage", "UnableToLaunchLT", $"Could not launch Language Tool: {ex.Message}"),
                    LanguageManager.Get("SupportPage", "UnableToLaunchRM_Title", "Execution Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ApplyLanguage()
        {
            // Read active configuration key directly from initialization structure matching LanguageManager
            string lang = IniHelper.Read("Language", "SelectedLanguage", "en").ToLowerInvariant();

            // Handle Right-to-Left layout updates dynamically if Arabic language profiles are selected
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