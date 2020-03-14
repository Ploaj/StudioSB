using System;
using System.Windows.Forms;
using System.Threading;

namespace StudioSB
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var mf = MainForm.Instance;
            mf.ARGS = (args);
            Application.Run(mf);
        }
    }
}
