using ApplicationServicesHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using ServiceStack.Logging;


namespace ApplicationServerConsoleShell
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var appHost = RabbitMqAppHost.CreateAndStart();
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
