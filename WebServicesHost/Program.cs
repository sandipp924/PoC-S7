using WebServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using ServiceStack.Configuration;
using ServiceStack.Text.Support;

namespace WebServices
{
    class Program
    {
        static void Main(string[] args)
        {
            IAppSettings settings = new AppSettings();

            var webServicesHost = new WebServicesAppHost();
            var url = string.Format("http://localhost:{0}/", settings.Get<int>("HttpListenPort", 8080));
            webServicesHost.Init().Start(url);

            Task.Delay(500).ContinueWith(t =>
            {
                Process.Start(string.Format("{0}/SPA.html", url));
            });

            typeof(Program).Debug("Running WebServicesAppHost.");

            Console.WriteLine("Press a key to quit...");
            Console.ReadKey();
        }
    }
}
