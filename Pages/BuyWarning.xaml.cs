using System.Windows;

namespace SimTools;

public partial class BuyWarning : Window
{
    public BuyWarning()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
