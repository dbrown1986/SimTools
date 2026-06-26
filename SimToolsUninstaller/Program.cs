// SimTools
// Uninstaller
// Main Program Definition File
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Windows.Forms;

namespace SimToolsUninstaller
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow());
        }
    }
}