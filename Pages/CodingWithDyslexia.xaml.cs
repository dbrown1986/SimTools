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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
