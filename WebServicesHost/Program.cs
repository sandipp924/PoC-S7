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

            var listenUrl = string.Format("http://localhost:{0}/", settings.Get<int>("HttpListenPort", 8080));

            var webServicesHost = new WebServicesAppHost();
            webServicesHost.Init().Start(listenUrl);

            // Open SPA.html in browser for demonstration purposes.
            Task.Delay(500).ContinueWith(t =>
            {
                Process.Start(string.Format("{0}/SPA.html", listenUrl));
            });

            typeof(Program).Debug("Running WebServicesAppHost.");

            Console.WriteLine("Press a key to quit...");

            // Read-key is necessary unless program will end. When using service shells, this will not be necessary.
            Console.ReadKey();
        }
    }
}
