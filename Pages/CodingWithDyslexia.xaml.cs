using System;
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
                new KeyGeneratorWindow { Owner = this }.ShowDialog();
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
