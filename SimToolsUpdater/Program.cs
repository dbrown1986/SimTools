using System;
using System.Windows.Forms;

namespace SimToolsUpdater
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Starts the WinForms message loop and opens your converted updater form
            Application.Run(new MainWindow());
        }
    }
}