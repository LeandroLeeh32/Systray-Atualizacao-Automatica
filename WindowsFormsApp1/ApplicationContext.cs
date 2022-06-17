using System;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApp1.Properties;
using System.Configuration;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.ServiceProcess;
using System.Diagnostics;
using System.Net.Http;
using static WindowsFormsApp1.Cryptography;

namespace WindowsFormsApp1
{
    public class ApplicationContext : System.Windows.Forms.ApplicationContext
    {
        private NotifyIcon trayIcon;
        string Download = ConfigurationManager.AppSettings["Download"];
        string ApplicationNameInstaller = ConfigurationManager.AppSettings["ApplicationNameInstaller"];
        string ServiceName = ConfigurationManager.AppSettings["ServiceName"];
        string ProxyHost = ConfigurationManager.AppSettings["ProxyHost"];
        string ProxyPort = ConfigurationManager.AppSettings["ProxyPort"];
        string ProxyUserName = ConfigurationManager.AppSettings["ProxyUserName"];
        string ProxyPassword = ConfigurationManager.AppSettings["ProxyPassword"];
        string ProxyProtocol = ConfigurationManager.AppSettings["ProxyProtocol"];
        string HasProxy = ConfigurationManager.AppSettings["HasProxy"];
        string StartAutomatic = ConfigurationManager.AppSettings["StartAutomatic"];
        Thread _Thread;
        bool IsAutomaticRunning = false;



        public static byte[] Key = { 124, 222, 121, 82, 172, 21, 185, 111, 228, 182, 72, 132, 233, 123, 80, 12 };
        public static byte[] IV = { 172, 111, 13, 42, 244, 102, 81, 211 };

        public ApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.llight_green,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit),
                new MenuItem("Download ", Get),
                new MenuItem("Stop ", Stop),
                new MenuItem("Start ", Start),
                new MenuItem("Quiet ", Quiet),
                new MenuItem("Automatic ", Automatic)
            }),
                Visible = true
            };
        }

        void Automatic(object sender, EventArgs e)
        {
            _Thread = new Thread(new ThreadStart(Sequence));
            _Thread.Start();
        }

        void Sequence()
        {
            createMenu(true);
            while (true)
            {
                IsAutomaticRunning = true;
                Get();
                Stop();
                Thread.Sleep((1000 * 60) * int.Parse(StartAutomatic));
            }
        }

        void Exit_Automatic(object sender, EventArgs e)
        {
            if (!IsAutomaticRunning)
            {
                _Thread.Abort();
                createMenu(false);
            }
        }

        void Exit(object sender, EventArgs e) => Exit();
        void Exit()
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        void Get(object sender, EventArgs e) => Get();
        void Get()
        {
            WebClient client = new WebClient();
            string PathResult = Path.Combine(Path.GetTempPath(), ApplicationNameInstaller);
            client.Proxy = Convert.ToBoolean(HasProxy) ? ResolveWebProxy() : new WebProxy();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            client.DownloadFileAsync(new Uri(Download), PathResult);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            trayIcon.Icon = Resources.light_red;
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (IsAutomaticRunning) { Quiet(); }
            trayIcon.Icon = Resources.llight_green;
        }

        void Stop(object sender, EventArgs e) => Stop();
        void Stop()
        {
            ServiceController service = new ServiceController(ServiceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }

        void Start(object sender, EventArgs e) => Start();
        void Start()
        {
            ServiceController service = new ServiceController(ServiceName);
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
        }

        void Quiet(object sender, EventArgs e) => Quiet();
        void Quiet()
        {
            trayIcon.Icon = Resources.light_red;
            string PathResult = Path.Combine(Path.GetTempPath(), ApplicationNameInstaller);
           Process process = new Process();
            {
                process.StartInfo.FileName = PathResult;
                process.StartInfo.Arguments = " /SILENT | /VERYSILENT [/SUPPRESSMSGBOXES]";
                process.EnableRaisingEvents = true;
                process.Exited += process_Exited;
                process.Start();
                process.WaitForExit();
            }

        }

        private void process_Exited(object sender, EventArgs e)
        {
            var myProcess = (Process)sender;
            if (myProcess.ExitCode == 0)
            {
                if (IsAutomaticRunning) { Start(); };
                IsAutomaticRunning = false;
                trayIcon.Icon = Resources.llight_green;
            }
        }

        void createMenu(bool IsAutomatic)
        {
            if (IsAutomatic)
            {
                trayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Stop Automatic", Exit_Automatic) });
            }
            else
            {
                trayIcon.Icon = Resources.llight_green;
                trayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit),
                    new MenuItem("Download ", Get),
                    new MenuItem("Stop ", Stop),
                    new MenuItem("Start ", Start),
                    new MenuItem("Quiet ", Quiet),
                    new MenuItem("Automatic ", Automatic)});
            }
        }

        private WebProxy ResolveWebProxy()
        {

            Cryptography.Key = Key; Cryptography.IV = IV;

            return new WebProxy
            {
            Address = new Uri($"{Cryptography.DecryptString(ProxyProtocol)}://{Cryptography.DecryptString(ProxyHost)}:{Cryptography.DecryptString(ProxyPort)}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                   userName: Cryptography.DecryptString(ProxyUserName),
                   password: Cryptography.DecryptString(ProxyPassword))
            };
    }

}
}
