using System.Windows;

using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    public partial class ExclusiveItems : Window
    {
        public ExclusiveItems()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        // ── Translation strings ───────────────────────────────────────────
        private void ApplyLanguage()
        {
            Title = LanguageManager.Get("ExclusiveItems", "Title", "SimTools — Exclusive Items");
            ExclusiveItemsText.Text = LanguageManager.Get("ExclusiveItems", "ContentText",
                "Exclusive donor content will appear here.");
        }

        // ── Close button ──────────────────────────────────────────────────
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BackButton_Clic(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
