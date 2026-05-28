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
    /// Interaction logic for SpecialThanks.xaml
    /// </summary>
    public partial class SpecialThanks : Window
    {
        public SpecialThanks()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ViewDonors_Click(object sender, RoutedEventArgs e)
        {
            new Donors { Owner = this }.ShowDialog();
        }
    }
}
