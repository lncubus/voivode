using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace voivode
{
    static class Program
    {
        public const string ErrorMessage = "Произошла ошибка";
        public const string PleaseReportMessage = "Пожалуйста, передайте информацию об ошибке разработчику.";
        public const string ReportingEMail = "randirbox" + "@" + "gmail.com";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.Run(new MainForm());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            DialogResult ok = MessageBox.Show(string.Join("\n", PleaseReportMessage, e.Exception.Message ),
                ErrorMessage, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            if (ok != DialogResult.OK)
                return;
            string report = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
            string error = e.Exception.ToString();
            File.WriteAllLines(report, new[] { ErrorMessage, PleaseReportMessage, ReportingEMail });
            File.AppendAllText(report, error);
            Process.Start(report);
        }
    }
}
