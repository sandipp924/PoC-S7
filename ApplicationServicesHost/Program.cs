using Common;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Logging;
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
            try
            {
                var appHost = new RabbitMqAppHost();
                appHost.Init();
            }
            catch (Exception exception)
            {
                typeof(Program).ErrorFormat("Error: {0}", exception.Message);
            }

            typeof(Program).Debug("Running RabbitMqAppHost...");

            Console.WriteLine("Press a key to quit...");

            // Read-key is necessary unless program will end. When using service shells, this will not be necessary.
            Console.ReadKey();
        }
    }

}
