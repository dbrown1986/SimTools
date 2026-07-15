// SimTools
// Uninstaller
// Main Program Definition File
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Net;
using System.Windows.Forms;

namespace SimToolsUninstaller
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
            Application.Run(new MainWindow());
        }
    }
}