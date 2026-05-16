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
using System.Windows.Navigation;
using System.Windows.Shapes;

// Add these two aliases to resolve the ambiguity:
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace SimTools_v4
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SimToolsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "SimTools (previously TS3Tools) is still the same suite of tools previously developed, but now includes options for a variety of Maxis' sim genre of games. SimTools includes options for Sims 1, Sims 2, Sims Stories, Sims 3, Sims 4, Simcity 2000, Simcity 3000, Simcity 3000 Unlimited, Simcity 4, Simcity 2K13, SimCopter and Streets of Simcity, with more to come in later versions.",
                "About SimTools",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
        }
    }
}
