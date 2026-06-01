using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    /// <summary>
    /// Allows a donor to enter their personalisation key.
    /// The key is validated, decoded, and if correct the name is persisted to
    /// the INI file so that IntroductoryPage can display it on next load.
    /// </summary>
    public partial class UnlockPersonalizationDialog : Window
    {
        // Set to true when the user successfully unlocks or removes a key,
        // so the caller (SupportSimTools) knows to refresh IntroductoryPage.
        public bool KeyChanged { get; private set; } = false;

        public UnlockPersonalizationDialog()
        {
            InitializeComponent();

            // Pre-populate with any existing key so the user can see / replace it
            string existingKey = IniHelper.Read("Personalization", "DonorKey", "");
            if (!string.IsNullOrWhiteSpace(existingKey))
                KeyTextBox.Text = existingKey;
        }

        // ── Unlock ────────────────────────────────────────────────────────────
        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                ShowStatus(LanguageManager.Get("Personalization", "EnterKey", "Please enter a key."));
                return;
            }

            if (!DonorKeyHelper.TryDecodeKey(key, out string firstName, out string lastName))
            {
                ShowStatus(LanguageManager.Get("Personalization", "InvalidKey", "That key is not valid. Please check it and try again."));
                return;
            }

            // Persist key and decoded names
            IniHelper.Write("Personalization", "DonorKey",   key);
            IniHelper.Write("Personalization", "FirstName",  firstName);
            IniHelper.Write("Personalization", "LastName",   lastName);

            KeyChanged = true;

            MessageBox.Show(
                LanguageManager.Format("Personalization", "Unlocked", firstName),
                LanguageManager.Get("Personalization", "Unlocked_Title", "SimTools — Personalization"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }

        // ── Remove existing key ───────────────────────────────────────────────
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                LanguageManager.Get("Personalization", "RemoveAsk", "Remove your personalisation key?"),
                LanguageManager.Get("Personalization", "RemoveAsk_Title", "SimTools — Remove Personalisation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IniHelper.Write("Personalization", "DonorKey",  "");
            IniHelper.Write("Personalization", "FirstName", "");
            IniHelper.Write("Personalization", "LastName",  "");

            KeyChanged = true;
            Close();
        }

        // ── Cancel ────────────────────────────────────────────────────────────
        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        // Allow pressing Enter in the text box to trigger Unlock
        private void KeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) UnlockButton_Click(sender, e);
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private void ShowStatus(string message)
        {
            StatusText.Text       = message;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}
