// SimTools
// Main Application
// SimTools Easter Egg (Coding With Dyslexia) Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.IO;
using System.Security.Cryptography;
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
    /// Interaction logic for CodingWithDyslexia.xaml
    /// </summary>
    public partial class CodingWithDyslexia : Window
    {
        public CodingWithDyslexia()
        {
            InitializeComponent();
            ApplyLanguage();
            PreviewKeyDown += CodingWithDyslexia_PreviewKeyDown;
        }

        // ── Secret developer shortcut: Ctrl+Shift+Alt+G → Key Generator ─────
        private void CodingWithDyslexia_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.G
                && (System.Windows.Input.Keyboard.Modifiers &
                    (System.Windows.Input.ModifierKeys.Control
                     | System.Windows.Input.ModifierKeys.Shift
                     | System.Windows.Input.ModifierKeys.Alt))
                == (System.Windows.Input.ModifierKeys.Control
                    | System.Windows.Input.ModifierKeys.Shift
                    | System.Windows.Input.ModifierKeys.Alt))
            {
                e.Handled = true;

                // 1. Define the local key file path alongside your application executable
                string keyFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "developer.key");

                if (!File.Exists(keyFilePath))
                {
                    // Fail silently so a curious user doesn't know a secret window exists here
                    return;
                }

                try
                {
                    // 2. Read the password from your secret local file
                    string fileContent = File.ReadAllText(keyFilePath).Trim();

                    // 3. Compute the SHA-256 hash of what was found inside your file
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fileContent));
                        StringBuilder builder = new StringBuilder();
                        foreach (byte b in bytes)
                        {
                            builder.Append(b.ToString("x2"));
                        }
                        string computedHash = builder.ToString();

                        // 4. THE SECURITY GATE: Paste your pre-computed 64-character hash here.
                        // This is a dummy example. You must generate your own hash (see Step 3).
                        string masterDeveloperHash = "a5f63f429490c977313871e35c1122987347491fec6256ea132be3a3d3b9f766";

                        // Cryptographic comparison
                        if (string.Equals(computedHash, masterDeveloperHash, StringComparison.OrdinalIgnoreCase))
                        {
                            new KeyGeneratorWindow { Owner = this }.ShowDialog();
                        }
                    }
                }
                catch
                {
                    // Fail completely silently on file read or format errors
                }
            }
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
            Coding.Text = LanguageManager.Get("EasterEgg1", "Coding", Coding.Text);
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
