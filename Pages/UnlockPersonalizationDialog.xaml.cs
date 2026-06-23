using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

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
        // Note the addition of "async" here so that network requests can run on a background thread.
        private async void UnlockButton_Click(object sender, RoutedEventArgs e)
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

            // Get the local hardware identifier unique to this Windows installation
            string machineGuid = MachineIdentity.GetMachineGuid();
            if (string.IsNullOrWhiteSpace(machineGuid))
            {
                ShowStatus("Could not verify hardware identity.");
                return;
            }

            // Disable UI inputs so the user doesn't double-click while waiting on the server
            UnlockButton.IsEnabled = false;
            ShowStatus("Verifying activation online...");

            try
            {
                using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    // Compile the registration text data packet
                    var payload = new
                    {
                        donor_key = key,
                        machine_guid = machineGuid,
                        machine_name = Environment.MachineName
                    };

                    string json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Send to your online API Clerk (Be sure to replace with your live domain)
                    var response = await http.PostAsync("https://simtools-app.com/api/activate.php", content);

                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        // 409 Conflict = Server database says 2 machines are already registered
                        MessageBox.Show(
                            "This donor key has already reached its maximum activation limit (2 machines).\n\n" +
                            "Please revoke one of your existing active setups via your donor portal before activating this machine.",
                            "Activation Limit Reached",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        ShowStatus("Activation limit exceeded.");
                        return;
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        // Any unexpected server error like a 500, 404, or 400
                        ShowStatus("Server rejected the activation or database is offline.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Network error during online activation: {ex.Message}\n\nPlease check your internet connection and try again.",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ShowStatus("Connection failed.");
                return;
            }
            finally
            {
                // Always re-enable the button if an execution thread stops early
                UnlockButton.IsEnabled = true;
            }

            // ── Online check succeeded; proceed with writing the local token ──
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
            StatusText.Text = message;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}