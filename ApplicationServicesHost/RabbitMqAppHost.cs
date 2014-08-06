using Funq;
using ServiceStack;
using ServiceStack.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Services;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;
using Services.Dtos;
using ServiceStack.Configuration;
using RabbitMQ.Client;
using System.Threading;
using Common;

namespace ApplicationServicesHost
{
    public class RabbitMqAppHost : BasicAppHost
    {
        private static readonly string AmqpServerUri = ConfigUtils.GetConnectionString("AmqpServer");

        /// <summary>
        /// Initializes a new instance of your RabbitMQ Services host application, with the specified name and assembly containing the services.
        /// </summary>
        public RabbitMqAppHost() : base("RabbitMQ Services Host", typeof(ReferenceService).Assembly) { }

        /// <summary>
        /// Configure the container with the necessary routes for your ServiceStack application.
        /// </summary>
        /// <param name="container">The built-in IoC used with ServiceStack.</param>
        public override void Configure(Container container)
        {
            //Set JSON web services to return idiomatic JSON camelCase properties   
            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

            //Change the default ServiceStack configuration
            this.SetConfig(new HostConfig
            {
                EnableFeatures = Feature.Json,
                DefaultContentType = MimeTypes.Json,
                PreferredContentTypes = new List<string> { MimeTypes.Json }
            });

            var connectionFactory = new ConnectionFactory
            {
                Uri = AmqpServerUri
            };
            
            var processor = new RabbitMqMessageProcessor<SymbologyInfoQuery, SymbologyInfo>(
                new RabbitMqReadMessageQueue<SymbologyInfoQuery>(connectionFactory, new MqConfiguration { ExchangeDurable = true, QueueDurable = true, MessageDurable = true, NoAck = false }),
                query => (SymbologyInfo)ServiceController.Execute(query));

            var serverThread = new Thread(processor.Run) { IsBackground = true };

            serverThread.Start();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
