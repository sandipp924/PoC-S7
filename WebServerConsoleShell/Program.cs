using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServices;
using Common;
using ServiceStack.Configuration;
using ServiceStack.Text.Support;

namespace WebServerConsoleShell
{
    class Program
    {
        static void Main(string[] args)
        {
            var webServicesHost = WebServicesAppHost.CreateAndStart();

            // Open SPA.html in browser for demonstration purposes.
            Task.Delay(500).ContinueWith(t =>
            {
                Process.Start(string.Format("{0}/SPA.html", webServicesHost.ListenUrl));
            });

            typeof(Program).Debug("Running WebServicesAppHost.");

            Console.WriteLine("Press a key to quit...");

            // Read-key is necessary unless program will end. When using service shells, this will not be necessary.
            Console.ReadKey();
        }
    }
}
