// SimTools
// Main Application
// Sweet Treats Window Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SimTools
{
    public partial class SweetTreats : Window
    {
        public SweetTreats()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Convert2Steam_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
                "CAUTION: This may only work for the retail CD. This feature is experimental.\n\nBefore proceeding you must install the base game and Sweet Treats (preferebly from disc).\n\nAfter installation, you must copy the files to the Sims 3 Steamapps directory.\n\nPlease see the guides for more details before proceeding.\n\nThis tool only detects games and modifies the registry for you.",
                "Experimental Feature Warning",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                KPSTConversionWindow conversionWindow = new KPSTConversionWindow
                {
                    Owner = this
                };
                conversionWindow.ShowDialog();
            }
        }

        private void SetTextWithLineBreaks(TextBlock textBlock, string text)
        {
            textBlock.Inlines.Clear();
            var lines = text.Split(new[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                textBlock.Inlines.Add(new Run { Text = lines[i] });
                if (i < lines.Length - 1)
                {
                    textBlock.Inlines.Add(new LineBreak());
                }
            }
        }
    }
}



