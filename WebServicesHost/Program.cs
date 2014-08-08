﻿using WebServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServicesHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var webServicesHost = new WebServicesAppHost();
            webServicesHost.Init().Start("http://localhost:8080/");

            Task.Delay(500).ContinueWith(t =>
            {
                Enumerable.Range(0, 1).ToList().ForEach(i => Process.Start("http://localhost:8080/SPA.html"));
            });

            Console.WriteLine("Running WebServicesAppHost.");
            Console.WriteLine("Press a key to quit...");
            Console.ReadKey();
        }
    }
}
