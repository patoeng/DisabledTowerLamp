using System;

using System.Globalization;
using System.IO;

using System.Threading;
using System.Windows.Forms;

namespace OLEDB35
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static string appGuid = "E3E7A2BC-7974-43C9-A2F7-D7638BDCEE52";
        [STAThread]        
        static void Main()
        {
           
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Instance already running");
                    return;
                }
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
           }
           
        }
        static void writefile(string s)
        {
            var file = new StreamWriter("Log.log", true);
            file.WriteLine(DateTime.Now.ToString("f") + " : " + s);
            file.Close();
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            writefile(e.Exception.Message+"Handled "+e.Exception.StackTrace + ", source: " + e.Exception.Source);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
                writefile(exception.Message +"UnHandled "+ exception.StackTrace+ " source:" + exception.Source.ToString(CultureInfo.InvariantCulture));
        }
    }
    
}
