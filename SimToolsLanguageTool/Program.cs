using System;
using System.Windows.Forms;
using SimToolsLanguageTool;

namespace SimToolsLanguageManager
{
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