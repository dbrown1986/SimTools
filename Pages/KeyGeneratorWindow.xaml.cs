using System.Windows;
using System.Windows.Controls;

using Clipboard  = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    /// <summary>
    /// Developer-only donor key generator.
    /// Opened via Ctrl+Shift+Alt+G from the main window — not exposed in normal UI.
    /// </summary>
    public partial class KeyGeneratorWindow : Window
    {
        public KeyGeneratorWindow()
        {
            InitializeComponent();
        }

        // ── Live key generation as the user types ─────────────────────────────
        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GeneratedKeyBox == null) return;

            string first = FirstNameBox?.Text.Trim()  ?? string.Empty;
            string last  = LastNameBox?.Text.Trim()   ?? string.Empty;

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
            {
                GeneratedKeyBox.Text = "(enter a name above)";
                return;
            }

            GeneratedKeyBox.Text = DonorKeyHelper.GenerateKey(first, last);
        }

        // ── Copy to clipboard ─────────────────────────────────────────────────
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string key = GeneratedKeyBox.Text;
            if (string.IsNullOrWhiteSpace(key) || key.StartsWith('(')) return;

            Clipboard.SetText(key);
            MessageBox.Show(
                LanguageManager.Get("KeyGen", "Copied", "Key copied to clipboard."),
                LanguageManager.Get("KeyGen", "Copied_Title", "SimTools — Key Generator"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
