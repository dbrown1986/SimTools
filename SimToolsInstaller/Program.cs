using System;
using System.Windows.Forms; // This fixes the 'Application' error

namespace SimToolsInstaller
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Note: Make sure this matches the name of your form class!
            // Based on your error log, it looks like it is named MainWindow
            Application.Run(new MainWindow());
        }
    }
}