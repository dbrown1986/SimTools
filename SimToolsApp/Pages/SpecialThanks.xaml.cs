// SimTools
// Main Application
// Special Thanks Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Collections.Generic;
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

namespace SimTools
{
    /// <summary>
    /// Interaction logic for SpecialThanks.xaml
    /// </summary>
    public partial class SpecialThanks : Window
    {
        public SpecialThanks()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ViewDonors_Click(object sender, RoutedEventArgs e)
        {
            new Donors { Owner = this }.ShowDialog();
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
            MuchThanksText.Text = LanguageManager.Get("SpecialThanks", "ThanksMessage", MuchThanksText.Text);
        }

    }
}
