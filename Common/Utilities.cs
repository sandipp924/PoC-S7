using ServiceStack.Configuration;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Utilities
    {
        public static void InitializeLogFactory(IAppSettings settings)
        {
            ILogFactory logFactory = null;
            switch (settings.Get("LogFactory", "Console").ToLower())
            {
                default:
                case "console":
                    logFactory = new ConsoleLogFactory();
                    break;
                case "debug":
                    logFactory = new DebugLogFactory();
                    break;
            }

            LogManager.LogFactory = logFactory;
        }

        public static RabbitMQ.Client.ConnectionFactory CreateMqConnectionFactory(IAppSettings settings)
        {
            return new RabbitMQ.Client.ConnectionFactory
            {
                Uri = settings.Get<string>("AmqpServer", null),
            };
        }

        public static MqConfiguration GetMqConfiguration(IAppSettings settings)
        {
            return settings.Get<MqConfiguration>("AmqpConfiguration", new MqConfiguration
            {
                ExchangeDurable = true,
                QueueDurable = true,
                MessageDurable = true,
                NoAck = false
            });
        }

    }
}
