// SimTools
// Main Application
// Unlock Personalization Dialog Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
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
    /// to the machine-locked token file (donor.token), and the key is then
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
        private async void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim(); // Grab the email input from the new text box

            // Validate that BOTH fields are filled in
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(email))
            {
                ShowStatus(LanguageManager.Get("Personalization", "EnterKeyAndEmail",
                    "Please enter both your email address and donor key."));
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
                // Compile the registration text data packet
                var payload = new
                {
                    donor_key = key,
                    email = email,
                    machine_guid = machineGuid,
                    machine_name = Environment.MachineName
                };

                string json = JsonSerializer.Serialize(payload);

                // Replaced HttpClient POST request with secure BouncyCastle alternative (fixes Win 7)
                await SecureWebClient.PostJsonAsync("https://simtools-app.com/api/activate.php", json);
            }
            catch (System.Net.Http.HttpRequestException hex) when (hex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Server database returned a 409 conflict
                MessageBox.Show(
                    "This donor key has already reached its maximum activation limit (5 machines).\n\n" +
                    "Please revoke one of your existing active setups via your donor portal before activating this machine.",
                    "Activation Limit Reached",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ShowStatus("Activation limit exceeded.");
                return;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                // Server database returned an activation failure (e.g., 400 or 500)
                ShowStatus("Server rejected the activation. Check that your key and email match.");
                return;
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

        // ── Remove existing key ──────────────────
        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                LanguageManager.Get("Personalization", "RemoveAsk",
                    "Remove your personalisation key?"),
                LanguageManager.Get("Personalization", "RemoveAsk_Title",
                    "SimTools — Remove Personalisation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Get the unique identifier for this computer
            string machineGuid = MachineIdentity.GetMachineGuid();

            if (!string.IsNullOrWhiteSpace(machineGuid))
            {
                // Change button status temporarily so they know it is communicating
                ClearButton.IsEnabled = false;
                ShowStatus("Deactivating device registration online...");

                try
                {
                    var payload = new { machine_guid = machineGuid };
                    string json = JsonSerializer.Serialize(payload);

                    // Replaced HttpClient with our secure BouncyCastle alternative (fixes Win 7)
                    await SecureWebClient.PostJsonAsync("https://simtools-app.com/api/deactivate.php", json);
                }
                catch
                {
                    // Catch failures silently so offline state transitions are not blocked
                }
                finally
                {
                    ClearButton.IsEnabled = true;
                }
            }

            // Always wipe the local token files out regardless of network state
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