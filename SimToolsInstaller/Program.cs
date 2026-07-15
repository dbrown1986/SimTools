// SimTools
// Installer
// Main Program Definition File
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Windows.Forms; // This fixes the 'Application' error
using System.Net;

namespace SimToolsInstaller
{
    public static class SecurityProtocolHelper
    {
        public static void EnableModernSecurityProtocols()
        {
            // 3072 is the numerical value for SecurityProtocolType.Tls12. 
            // Casting it allows old frameworks (.NET 4.0) to compile it even if the enum isn't defined natively.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }
    }

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