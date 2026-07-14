using System;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SimTools
{
    public partial class KPSTConversionWindow : Window
    {
        private SimsRegistryManager _registryManager;

        public KPSTConversionWindow()
        {
            InitializeComponent();
            _registryManager = new SimsRegistryManager();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await RunConversion();
        }

        private async Task RunConversion()
        {
            try
            {
                StatusText.Text = "Locating The Sims 3 installation...";

                // Try to find the installation automatically
                string steamInstallPath = _registryManager.FindSteamInstallation();

                // If not found, prompt user to browse for it
                if (string.IsNullOrEmpty(steamInstallPath))
                {
                    StatusText.Text = "Steam installation not found. Please select the folder manually.";
                    steamInstallPath = PromptUserForDirectory();

                    if (string.IsNullOrEmpty(steamInstallPath))
                    {
                        StatusText.Text = "Conversion cancelled: No installation directory selected.";
                        CloseButton.IsEnabled = true;
                        return;
                    }
                }

                StatusText.Text = "Starting conversion process...";

                // Run the conversion on a background thread
                // This initiates the registry changes outlined in the SimsRegistryManager class in KPTSAutoConvert.cs
                (bool success, string message) = await _registryManager.ConvertToSteamAsync(
                    steamInstallPath,
                    "your-ergc-key");

                StatusText.Text = success
                    ? "Conversion completed successfully!"
                    : $"Error during conversion: {message}";

                CloseButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error during conversion: {ex.Message}";
                CloseButton.IsEnabled = true;
            }
        }

        private string PromptUserForDirectory()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Locate The Sims 3 Installation Directory",
                Filter = "Executable files (TS3.exe)|TS3.exe|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                // Return the directory containing the selected file
                return System.IO.Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
            }

            return string.Empty;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}