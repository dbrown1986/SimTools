using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox   = System.Windows.MessageBox;

namespace SimTools
{
    /// <summary>
    /// Allows a donor to enter their personalisation key.
    ///
    /// On success the key is validated in memory, the decoded names are written
    /// to the machine-locked token file (SimTools.token), and the key is then
    /// discarded — it is never stored to disk in any form.
    /// </summary>
    public partial class UnlockPersonalizationDialog : Window
    {
        // Set to true when the user successfully unlocks or removes a key,
        // so the caller (SupportSimTools) knows to refresh IntroductoryPage.
        public bool KeyChanged { get; private set; } = false;

        public UnlockPersonalizationDialog()
        {
            InitializeComponent();
            // Do NOT pre-populate the text box — the key is no longer stored
            // anywhere on disk, so there is nothing to show.
        }

        // ── Unlock ────────────────────────────────────────────────────────────
        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                ShowStatus(LanguageManager.Get("Personalization", "EnterKey",
                    "Please enter a key."));
                return;
            }

            if (!DonorKeyHelper.TryDecodeKey(key, out string firstName, out _))
            {
                ShowStatus(LanguageManager.Get("Personalization", "InvalidKey",
                    "That key is not valid. Please check it and try again."));
                return;
            }

            // Write the machine-locked token file.
            // The key itself is NOT written anywhere — only the decoded names
            // are stored, encrypted and bound to this machine's GUID.
            try
            {
                DonorKeyHelper.WriteTokenFile(key);
            }
            catch
            {
                MessageBox.Show(
                    LanguageManager.Get("Personalization", "TokenWriteFailed",
                        "Your key was accepted, but the machine-lock file could not be " +
                        "written. You may be prompted to re-enter your key on the next launch."),
                    LanguageManager.Get("Personalization", "Unlocked_Title",
                        "SimTools — Personalization"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            KeyChanged = true;

            MessageBox.Show(
                LanguageManager.Format("Personalization", "Unlocked", firstName),
                LanguageManager.Get("Personalization", "Unlocked_Title",
                    "SimTools — Personalization"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }

        // ── Remove existing key ───────────────────────────────────────────────
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                LanguageManager.Get("Personalization", "RemoveAsk",
                    "Remove your personalisation key?"),
                LanguageManager.Get("Personalization", "RemoveAsk_Title",
                    "SimTools — Remove Personalisation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            DonorKeyHelper.ClearPersonalization();
            KeyChanged = true;
            Close();
        }

        // ── Cancel ────────────────────────────────────────────────────────────
        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private void KeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) UnlockButton_Click(sender, e);
        }

        private void ShowStatus(string message)
        {
            StatusText.Text       = message;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}
