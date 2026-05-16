using System.Windows;
using WpfButton = System.Windows.Controls.Button;

namespace SimTools
{
    public partial class LanguageSelectionWindow : Window
    {
        public LanguageSelectionWindow()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            TitleText.Text = LanguageManager.Get("Language", "Window_Title", TitleText.Text);
            SubtitleText.Text = LanguageManager.Get("Language", "Window_Subtitle", SubtitleText.Text);
            DoNotAskCheckBox.Content = LanguageManager.Get("Language", "Checkbox_DoNotAsk", "Do not ask me again");
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not WpfButton btn) return;

            var langCode = btn.Tag?.ToString() ?? "en";
            IniHelper.Write("Language", "SelectedLanguage", langCode);

            if (DoNotAskCheckBox.IsChecked == true)
                IniHelper.WriteBool("Language", "DoNotAskAgain", true);

            Close();
        }
    }
}