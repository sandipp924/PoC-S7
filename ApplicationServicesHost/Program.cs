using ServiceStack;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationServicesHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var appHost = new RabbitMqAppHost();
            appHost.Init();

            Console.WriteLine("Running RabbitMqAppHost.");
            Console.WriteLine("Press a key to quit...");
            Console.ReadKey();
        }
    }

}
