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
            Console.WriteLine("Running RabbitMqAppHost...");

            try
            {
                var appHost = new RabbitMqAppHost();
                appHost.Init();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message);
            }

            Console.WriteLine("Press a key to quit...");
            Console.ReadKey();
        }
    }

}
