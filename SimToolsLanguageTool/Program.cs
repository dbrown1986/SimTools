using SimToolsLanguageTool;
using System;
using System.Net;
using System.Windows.Forms;

namespace SimToolsLanguageManager
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

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // If you kept the name MainForm:
            Application.Run(new MainForm());

            // If your form is named Form1, it would be:
            // Application.Run(new Form1());
        }
    }
}