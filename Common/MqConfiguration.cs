using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Util;
using ServiceStack;
using ServiceStack.Messaging;
using RabbitMQ.Client.Events;
using System;

namespace Common
{
    /// <summary>
    /// Used to specify parameters used for declaring RabbitMq entities such as exchanges, queues and messages.
    /// </summary>
    public class MqConfiguration
    {
        public bool ExchangeDurable { get; set; }
        public bool QueueDurable { get; set; }
        public bool MessageDurable { get; set; }
        public bool NoAck { get; set; }

        /// <summary>
        /// Amount of time in milliseconds to use for setting expiration on AMQP messages.
        /// </summary>
        public int? MessageExpiration { get; set; }

        /// <summary>
        /// Default amount of time in milliseconds to wait for a service request to return through AMQP broker.
        /// </summary>
        public int? DefaultAmqpResponseTimeOut { get; set; }
    }
}
